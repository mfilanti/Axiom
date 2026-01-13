using Axiom.Cosmos;
using Axiom.Cosmos.Models;
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

			var star = galaxy.Stars.First();
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

			var star = galaxy.Stars.First();
			var planet = star.GetSubNodes().OfType<Planet>().FirstOrDefault(p=>p.Name == "Earth");
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
	}
}
