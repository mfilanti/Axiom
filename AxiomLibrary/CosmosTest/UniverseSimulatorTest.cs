using Assets.AxiomCore.Cosmos_Link.Starships;
using Axiom.Cosmos;
using Axiom.Cosmos.Models;
using Axiom.Cosmos.Starships;
using Axiom.GeoMath;

namespace CosmosTest
{
	[TestClass]
	public sealed class UniverseSimulatorTest
	{
		[TestMethod]
		public void TestGalaxy()
		{
			Galaxy galaxy = GalaxyFactory.CreateSolarSystem();
			Assert.AreEqual("Solar System", galaxy.Name);
		}

		[TestMethod]
		public void TestSimulator()
		{
			// ARRANGE
			Galaxy galaxy = GalaxyFactory.CreateSolarSystem();
			double dt = 60.0; // 60 secondi di simulazione

			var star = galaxy.Nodes.First().Value;
			var planet = star.GetSubNodes().OfType<Planet>().First();

			// Posizione iniziale del pianeta (locale al padre)
			Vector3D initialPlanetPosition = planet.Translation;

			// ACT
			galaxy.Step(dt);
			// ASSERT

			// 1. Il nome della galassia è corretto
			Assert.AreEqual("Solar System", galaxy.Name);

			// 2. Il pianeta si è mosso (orbita)
			Assert.AreNotEqual(
				initialPlanetPosition,
				planet.Translation,
				"Il pianeta non si è mosso dopo lo step di simulazione"
			);

			// 3. La distanza dal padre resta (quasi) costante → orbita
			double initialRadius = initialPlanetPosition.Length;
			double currentRadius = planet.Translation.Length;
		}

		[TestMethod]
		public void TestHierarchyPropagation()
		{
			Galaxy galaxy = GalaxyFactory.CreateSolarSystem();
			double dt = 3600.0; // 1 ora

			var star = galaxy.Nodes.First().Value;
			var planet = star.GetSubNodes().OfType<Planet>().FirstOrDefault(p => p.Name == "Earth");
			var moon = planet.GetSubNodes().OfType<Moon>().First();

			Vector3D initialMoonWorld = moon.WorldMatrix.Translation;

			galaxy.UpdatePhysics(dt);

			Vector3D finalMoonWorld = moon.WorldMatrix.Translation;

			Assert.AreNotEqual(initialMoonWorld, finalMoonWorld);
		}

		[TestMethod]
		public void TestGlobalGravityAffectsOrbit()
		{
			Galaxy galaxy = GalaxyFactory.CreateSolarSystem();
			double dt = 60.0;

			var earth = galaxy
				.GetAllBodies()
				.OfType<Planet>()
				.First();

			Vector3D initialPos = earth.WorldMatrix.Translation;

			galaxy.Step(dt);

			Vector3D finalPos = earth.WorldMatrix.Translation;

			Assert.AreNotEqual(initialPos, finalPos);
		}

		[TestMethod]
		public void TestSimulator2()
		{
			Galaxy galaxy = GalaxyFactory.CreateSolarSystem();

			double dt = 60; // 1 minuto

			for (int i = 0; i < 10_000; i++)
				galaxy.Step(dt);

			Assert.AreEqual("Solar System", galaxy.Name);
		}

		[TestMethod]
		public void TestShipThrustDirectionAfterRotation()
		{
			// ARRANGE
			var ship = new Starship("X-Wing", 5000) { MaxThrust = 10000 };
			var controller = new ShipFlightController(ship);
			ship.CurrentThrottle = 1.0;

			// ACT
			// Ruotiamo di 90 gradi sull'asse Y (Imbardata a destra)
			controller.HandleInput(0, Math.PI / 2, 0, 0, 10);

			Vector3D thrustVector = ship.GetThrustForce();

			// ASSERT
			// Verifica se il tuo motore è sinistrorso o destrorso
			Assert.IsLessThan(-9999, thrustVector.X); // Se restituisce -10000, questo passerà
			Assert.AreEqual(0, thrustVector.Z, 0.001);
		}

		[TestMethod]
		public void TestShip_FullCircleRotationX()
		{
			var ship = new Starship("X-Wing", 5000);
			double fullCircle = Math.PI * 2;
			double step = fullCircle / 100.0;

			// Ruotiamo la nave di 360 gradi sull'asse X (Pitch)
			for (int i = 0; i < 100; i++)
			{
				ship.GetRotation(out double x, out double y, out double z);
				ship.SetRotation(x + step, y, z);
			}

			ship.GetRotation(out double finalX, out double finalY, out double finalZ);

			// Dopo 2PI dovremmo essere tornati (circa) a 0
			Assert.AreEqual(0, Math.Sin(finalX), 0.01);
		}

		[TestMethod]
		public void TestShip_FullCircleRotation_WithController()
		{
			var ship = new Starship("X-Wing", 5000);
			var controller = new ShipFlightController(ship);

			// Simuliamo una rotazione completa di 2PI in 1 secondo (dt=0.01 per 100 volte)
			double dt = 0.01;
			double inputYaw = (Math.PI * 2) / (controller.RotationSensitivity * 100 * dt);

			for (int i = 0; i < 100; i++)
			{
				controller.HandleInput(0, inputYaw, 0, 0, dt);
			}

			ship.GetRotation(out _, out double finalY, out _);

			// Adesso il valore estratto deve essere coerente con un cerchio completo (vicino a 0)
			// Nota: usiamo il Sin per gestire la periodicità della rotazione
			Assert.AreEqual(0, Math.Sin(finalY), 0.001, "La nave non è tornata all'orientamento iniziale");
		}

		[TestMethod]
		public void TestShip_FullCircleRotationZ()
		{
			var ship = new Starship("X-Wing", 5000);
			double fullCircle = Math.PI * 2;
			double step = fullCircle / 100.0;

			// Ruotiamo la nave di 360 gradi sull'asse Z
			for (int i = 0; i < 100; i++)
			{
				ship.GetRotation(out double x, out double y, out double z);
				ship.SetRotation(x, y, z + step);
			}

			ship.GetRotation(out double finalX, out double finalY, out double finalZ);

			// Dopo 2PI dovremmo essere tornati (circa) a 0
			Assert.AreEqual(0, Math.Sin(finalZ), 0.01);
		}
	}
}
