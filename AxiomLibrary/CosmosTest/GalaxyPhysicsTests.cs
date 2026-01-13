using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosTest
{
	[TestClass]
    public class GalaxyPhysicsTests
    {
		private const double G = 6.67430e-11;

		[TestMethod]
		public void Step_ShouldUpdatePositions_WhenMultipleBodiesExist()
		{
			// ARRANGE
			// Creiamo una galassia con una stella massiccia e un pianeta
			var galaxy = new Galaxy("Test Galaxy");
			galaxy.GravityField = new NewtonianGravity(); // Assicurati che punti all'Octree internamente

			var sun = new Star("Sun", 1.989e30, 696340, Vector3D.Zero, 3.828e26);
			sun.Dynamics = new DynamicsState { Velocity = Vector3D.Zero, Acceleration = Vector3D.Zero };
			sun.Motion = new VelocityVerletMotion();

			// Pianeta a 1 Unità Astronomica
			double distance = 1.496e11;
			var earth = new Planet("Earth", 5.972e24, 6371, new Vector3D(distance, 0, 0));
			earth.Dynamics = new DynamicsState
			{
				Velocity = new Vector3D(0, 29780, 0), // Velocità orbitale media terra
				Acceleration = Vector3D.Zero
			};
			earth.Motion = new VelocityVerletMotion();

			sun.AddNode(earth);
			galaxy.AddStar(sun);

			double initialX = earth.X;
			double deltaTime = 3600; // 1 ora di simulazione

			// ACT
			galaxy.Step2(deltaTime);

			// ASSERT
			// Verifichiamo che la posizione X sia cambiata (il pianeta si è mosso nell'orbita)
			Assert.AreNotEqual(initialX, earth.X, "La posizione X del pianeta dovrebbe essere cambiata dopo lo Step.");

			// Verifichiamo che l'accelerazione sia stata calcolata (non più zero)
			Assert.IsGreaterThan(0, earth.Dynamics.Acceleration.Length, "L'accelerazione dovrebbe essere stata calcolata dall'Octree.");
		}

		[TestMethod]
		public void Step_WithLargeNumberOfBodies_ShouldNotCrash()
		{
			// ARRANGE
			var galaxy = new Galaxy("Performance Test");
			galaxy.GravityField = new NewtonianGravity();

			var sun = new Star("Central Sun", 1.989e30, 700000, Vector3D.Zero, 1.0);
			sun.Dynamics = new DynamicsState();
			sun.Motion = new VelocityVerletMotion();
			galaxy.AddStar(sun);

			// Aggiungiamo 100 asteroidi casuali per testare la stabilità dell'Octree
			Random rng = new Random(42);
			for (int i = 0; i < 100; i++)
			{
				var pos = new Vector3D(rng.NextDouble() * 1e11, rng.NextDouble() * 1e11, rng.NextDouble() * 1e11);
				var asteroid = new Planet($"Ast-{i}", 1e10, 100, pos);
				asteroid.Dynamics = new DynamicsState();
				asteroid.Motion = new VelocityVerletMotion();
				sun.AddNode(asteroid);
			}

			// ACT
			try
			{
				galaxy.Step2(1.0);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Lo Step ha generato un'eccezione: {ex.Message}");
			}

			// ASSERT
			Assert.IsTrue(true); // Se arriva qui, l'Octree ha retto
		}

		[TestMethod]
		public void Octree_BoundaryExpansion_ShouldContainAllBodies()
		{
			// Questo test verifica indirettamente se la tua logica di Enlarge/Offset funziona
			var galaxy = new Galaxy("Boundary Test");
			galaxy.GravityField = new NewtonianGravity();

			// Mettiamo due corpi molto distanti
			var star1 = new Star("S1", 1e30, 10, new Vector3D(-1e15, -1e15, -1e15), 0);
			var star2 = new Star("S2", 1e30, 10, new Vector3D(1e15, 1e15, 1e15), 0);

			star1.Dynamics = new DynamicsState(); star1.Motion = new VelocityVerletMotion();
			star2.Dynamics = new DynamicsState(); star2.Motion = new VelocityVerletMotion();

			galaxy.AddStar(star1);
			galaxy.AddStar(star2);

			// ACT
			galaxy.Step2(0.1);

			// ASSERT
			// Se lo step completa senza errori di "Point out of bounds" nell'Octree, 
			// significa che l'espansione del box ha funzionato correttamente.
			Assert.IsNotNull(galaxy.Stars);
		}
	}
}
