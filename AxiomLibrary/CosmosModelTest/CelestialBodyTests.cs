using Axiom.Cosmos.Model;
using Axiom.GeoMath;
using Axiom.Physics;

namespace Axiom.Cosmos.Model.Tests
{
	[TestClass]
	public class CelestialBodyTests
	{
		[TestMethod]
		public void Constructors_SetCoreProperties()
		{
			var star = new Star("Sun", 1.989e30, 6.9e8, new Vector3D(1, 2, 3), 3.8e26);
			Assert.AreEqual("Sun", star.Name);
			Assert.AreEqual(1.989e30, star.Mass);
			Assert.AreEqual(6.9e8, star.Radius);
			Assert.AreEqual(3.8e26, star.Luminosity);
			Assert.IsTrue(star.Position.IsEquals(new Vector3D(1, 2, 3)), "Position = traslazione di mondo");

			var planet = new Planet("Earth", 5.972e24, 6.4e6, new Vector3D(10, 0, 0));
			Assert.AreEqual("Earth", planet.Name);

			var moon = new Moon("Moon", 7.3e22, 1.7e6, new Vector3D(0, 5, 0));
			Assert.AreEqual("Moon", moon.Name);
		}

		[TestMethod]
		public void Weight_EqualsMass_ViaPhysicsBody()
		{
			var planet = new Planet("P", 4.2e23, 1, Vector3D.Zero);
			Assert.AreEqual(planet.Mass, planet.Weight);
		}

		[TestMethod]
		public void Step_IntegratesOwnMotion_AndPropagatesRecursivelyToChildren()
		{
			// Sole fermo (Motion = null) con pianeta figlio e luna nipote, entrambi con moto proprio:
			// Step del sole NON muove il sole ma deve propagarsi a pianeta e luna.
			var sun = new Star("Sun", 1e30, 1, Vector3D.Zero, 1) { Motion = null, Dynamics = new DynamicsState() };
			var planet = new Planet("Earth", 1e24, 1, new Vector3D(100, 0, 0))
			{
				Motion = new VelocityVerletMotion(),
				Dynamics = new DynamicsState { Velocity = new Vector3D(1, 0, 0) }
			};
			var moon = new Moon("Moon", 1e22, 1, new Vector3D(0, 10, 0))
			{
				Motion = new VelocityVerletMotion(),
				Dynamics = new DynamicsState { Velocity = new Vector3D(0, 1, 0) }
			};
			planet.AddNode(moon);
			sun.AddNode(planet);

			double planetX0 = planet.Translation.X;
			double moonY0 = moon.Translation.Y;

			sun.Step(1.0);

			Assert.IsTrue(sun.Translation.IsEquals(Vector3D.Zero), "il sole (Motion=null) non si muove");
			Assert.IsGreaterThan(planetX0, planet.Translation.X, "il pianeta figlio si è mosso");
			Assert.IsGreaterThan(moonY0, moon.Translation.Y, "la luna nipote si è mossa (ricorsione)");
		}

		[TestMethod]
		public void DisplayInfo_DoesNotThrow()
		{
			// Copertura di fumo dei metodi di dominio (scrivono su Console).
			new Star("S", 1, 1, Vector3D.Zero, 1).DisplayInfo();
			new Planet("P", 1, 1, Vector3D.Zero).DisplayInfo();
			new Moon("M", 1, 1, Vector3D.Zero).DisplayInfo();
		}
	}
}
