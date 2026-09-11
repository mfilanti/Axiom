using Axiom.Cosmos.Model;
using Axiom.GeoMath;
using Axiom.Physics;
using System;
using System.Linq;

namespace Axiom.Cosmos.Model.Tests
{
	[TestClass]
	public class GalaxyTests
	{
		[TestMethod]
		public void Constructor_SetsName()
		{
			var galaxy = new Galaxy("Milky Way");
			Assert.AreEqual("Milky Way", galaxy.Name);
		}

		[TestMethod]
		public void AddCelestialBody_AddsTopLevelNode()
		{
			var galaxy = new Galaxy("G");
			Assert.IsEmpty(galaxy.Nodes);
			galaxy.AddCelestialBody(new Star("Sun", 1e30, 1, Vector3D.Zero, 1));
			Assert.HasCount(1, galaxy.Nodes);
		}

		[TestMethod]
		public void GetAllBodies_FlattensHierarchyRecursively()
		{
			var galaxy = ModelSystems.SunAndPlanet(out var sun, out var planet);
			var moon = new Moon("Moon", 7.3e22, 1.7e6, new Vector3D(3.8e8, 0, 0))
			{
				Dynamics = new DynamicsState(),
				Motion = new VelocityVerletMotion()
			};
			planet.AddNode(moon);

			var names = galaxy.GetAllBodies().Select(b => b.Name).ToList();

			Assert.HasCount(3, names);
			CollectionAssert.AreEquivalent(new[] { "Sun", "Earth", "Moon" }, names);
		}

		[TestMethod]
		public void Step_MovesPlanet_AndComputesAcceleration()
		{
			var galaxy = ModelSystems.SunAndPlanet(out _, out var planet);
			double x0 = planet.WorldMatrix.Translation.X;

			galaxy.Step(3600.0);

			Assert.AreNotEqual(x0, planet.WorldMatrix.Translation.X, "il pianeta si è mosso");
			Assert.IsGreaterThan(0, planet.Dynamics.Acceleration.Length, "accelerazione gravitazionale calcolata");
		}

		[TestMethod]
		public void Step_CircularOrbit_ConservesRadius()
		{
			double r0 = 1.496e11;
			var galaxy = ModelSystems.SunAndPlanet(out _, out var planet, r0);

			for (int i = 0; i < 200; i++)
				galaxy.Step(3600.0);

			double rFinal = planet.WorldMatrix.Translation.Length;
			double relError = Math.Abs(rFinal - r0) / r0;
			Assert.IsLessThan(0.05, relError, $"raggio orbitale non conservato (errore {relError:P2})");
		}

		[TestMethod]
		public void UpdatePhysics_ReturnsOctreeConsistentWithUpdatedPositions()
		{
			var galaxy = ModelSystems.SunAndPlanet(out _, out var planet);

			var octree = galaxy.UpdatePhysics(3600.0);

			Assert.IsNotNull(octree);
			var detected = octree.QueryRange((Point3D)planet.WorldMatrix.Translation, 1.0);
			Assert.IsTrue(detected.Any(b => b.Name == "Earth"),
				"l'Octree restituito deve essere coerente con le posizioni aggiornate");
		}

		[TestMethod]
		public void UpdatePhysics_EmptyGalaxy_ReturnsNull()
		{
			Assert.IsNull(new Galaxy("Empty").UpdatePhysics(1.0));
		}
	}
}
