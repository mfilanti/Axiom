using Axiom.Cosmos;
using Axiom.Physics;
using Axiom.Cosmos.Model;
using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using System.Collections.Generic;
using System.Linq;

namespace CosmosTest
{
    /// <summary>
    /// Test di regressione per i bug corretti nella libreria Cosmos.
    /// </summary>
    [TestClass]
    public class BugRegressionTests
    {
        private const double G = 6.67430e-11;

        /// <summary>
        /// Regressione BUG-01: NewtonianGravity.ComputeForce non deve lanciare NullReferenceException
        /// (il campo _bodies era mai inizializzato) e deve produrre un'attrazione verso l'altro corpo.
        /// </summary>
        [TestMethod]
        public void NewtonianGravity_ComputeForce_DoesNotThrow_AndAttracts()
        {
            var a = new Planet("A", 1e24, 1, new Vector3D(0, 0, 0));
            var b = new Planet("B", 1e24, 1, new Vector3D(10, 0, 0));

            // Costruttore di default: nessun corpo -> forza nulla, ma NESSUNA eccezione.
            var emptyField = new NewtonianGravity();
            Vector3D noForce = emptyField.ComputeForce(a);
            Assert.IsTrue(noForce.IsEquals(Vector3D.Zero), "Senza corpi la forza deve essere nulla.");

            // Costruttore con i corpi: la forza su A deve puntare verso B (+X).
            var field = new NewtonianGravity(new List<CelestialBody> { a, b });
            Vector3D force = field.ComputeForce(a);

            Assert.IsGreaterThan(0, force.Length, "La forza gravitazionale dovrebbe essere non nulla.");
            Assert.IsGreaterThan(0, force.X, "La forza su A dovrebbe puntare verso B (direzione +X).");
        }

        /// <summary>
        /// Regressione BUG-06: la simulazione di default deve contenere una nave (la riga AddShip era
        /// commentata, quindi la nave veniva creata ma mai aggiunta).
        /// </summary>
        [TestMethod]
        public void CreateDefaultSimulation_ShouldContainAShip()
        {
            var simulation = SpaceSimulationFactory.CreateDefaultSimulation();
            Assert.IsNotNull(simulation.CurrentShip, "La simulazione di default dovrebbe avere una nave corrente.");
            Assert.AreEqual("X-Wing Red 5", simulation.CurrentShip.Ship.Name);
        }

        /// <summary>
        /// Regressione BUG-07: il raggio di Giove deve essere coerente (69 911 000 m) in entrambe le
        /// varianti di factory (nella variante Debug era erroneamente uguale al raggio del Sole).
        /// </summary>
        [TestMethod]
        public void JupiterRadius_ShouldBeConsistentAcrossFactories()
        {
            const double expectedJupiterRadius = 69911000.0;

            var jupiterStd = GalaxyFactory.CreateSolarSystem().GetAllBodies().First(b => b.Name == "Jupiter");
            var jupiterDbg = GalaxyFactory.CreateSolarSystemDebug().GetAllBodies().First(b => b.Name == "Jupiter");

            Assert.AreEqual(expectedJupiterRadius, jupiterStd.Radius, 1e-3, "Raggio di Giove errato in CreateSolarSystem.");
            Assert.AreEqual(expectedJupiterRadius, jupiterDbg.Radius, 1e-3, "Raggio di Giove errato in CreateSolarSystemDebug.");
        }

        /// <summary>
        /// Regressione BUG-08: UpdatePhysics deve restituire un Octree coerente con le posizioni
        /// AGGIORNATE dei corpi (prima integrava solo il primo half-step e restituiva l'Octree
        /// costruito sulle posizioni vecchie).
        /// </summary>
        [TestMethod]
        public void UpdatePhysics_ShouldReturnOctreeConsistentWithUpdatedPositions()
        {
            var galaxy = new Galaxy("Test") { GravityField = new NewtonianGravity() };

            var sun = new Star("Sun", 1.989e30, 6.9e8, Vector3D.Zero, 1.0)
            {
                Dynamics = new DynamicsState(),
                Motion = null // il Sole resta fermo
            };
            double r = 1.496e11;
            double v = System.Math.Sqrt(G * sun.Mass / r);
            var planet = new Planet("Earth", 5.972e24, 6.4e6, new Vector3D(r, 0, 0))
            {
                Dynamics = new DynamicsState { Velocity = new Vector3D(0, v, 0) },
                Motion = new VelocityVerletMotion()
            };
            sun.AddNode(planet);
            galaxy.AddCelestialBody(sun);

            var octree = galaxy.UpdatePhysics(3600.0);

            Assert.IsNotNull(octree);

            // Il pianeta si è mosso; l'Octree restituito deve "vederlo" nella sua nuova posizione.
            Point3D newPlanetWorld = (Point3D)planet.WorldMatrix.Translation;
            var detected = octree.QueryRange(newPlanetWorld, 1.0);
            Assert.IsTrue(detected.Any(b => b.Name == "Earth"),
                "L'Octree restituito da UpdatePhysics deve essere coerente con le posizioni aggiornate.");
        }

        /// <summary>
        /// Regressione BUG-05/BUG-08 (correttezza fisica): un'orbita circolare a due corpi (Sole fisso)
        /// deve conservare il raggio orbitale entro pochi punti percentuali su molti passi.
        /// </summary>
        [TestMethod]
        public void TwoBodyCircularOrbit_ShouldConserveRadius()
        {
            var galaxy = new Galaxy("Orbit") { GravityField = new NewtonianGravity() };

            var sun = new Star("Sun", 1.989e30, 6.9e8, Vector3D.Zero, 1.0)
            {
                Dynamics = new DynamicsState(),
                Motion = null
            };
            double r0 = 1.496e11;
            double v = System.Math.Sqrt(G * sun.Mass / r0);
            var planet = new Planet("Earth", 5.972e24, 6.4e6, new Vector3D(r0, 0, 0))
            {
                Dynamics = new DynamicsState { Velocity = new Vector3D(0, v, 0) },
                Motion = new VelocityVerletMotion()
            };
            sun.AddNode(planet);
            galaxy.AddCelestialBody(sun);

            double dt = 3600.0; // 1 ora
            for (int i = 0; i < 200; i++)
                galaxy.Step(dt);

            double rFinal = planet.WorldMatrix.Translation.Length;
            double relativeError = System.Math.Abs(rFinal - r0) / r0;

            Assert.IsLessThan(0.05, relativeError,
                $"Il raggio orbitale non è conservato (errore relativo {relativeError:P2}).");
        }

        /// <summary>
        /// Regressione BUG-05: una luna (corpo di secondo livello) deve restare legata al proprio
        /// pianeta e non "volare via" per incoerenza tra i sistemi di riferimento.
        /// </summary>
        [TestMethod]
        public void Moon_ShouldStayBoundToItsPlanet()
        {
            var galaxy = GalaxyFactory.CreateSolarSystem();

            var earth = galaxy.GetAllBodies().First(b => b.Name == "Earth");
            var moon = galaxy.GetAllBodies().First(b => b.Name == "Moon");

            double initialDistance = (moon.WorldMatrix.Translation - earth.WorldMatrix.Translation).Length;

            double dt = 3600.0; // 1 ora
            for (int i = 0; i < 100; i++)
                galaxy.UpdatePhysics(dt);

            double finalDistance = (moon.WorldMatrix.Translation - earth.WorldMatrix.Translation).Length;

            // La distanza Luna-Terra deve restare dello stesso ordine di grandezza (orbita legata),
            // non divergere: prima della correzione la luna poteva allontanarsi indefinitamente.
            Assert.IsGreaterThan(initialDistance * 0.5, finalDistance, "La luna si è avvicinata troppo (instabilità).");
            Assert.IsLessThan(initialDistance * 2.0, finalDistance, "La luna si è allontanata troppo dal pianeta (non più legata).");
        }
    }
}
