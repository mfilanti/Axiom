using Axiom.Cosmos.Model.Starships;
using Axiom.GeoMath;
using Axiom.Physics;
using System;

namespace Axiom.Cosmos.Model.Tests
{
	[TestClass]
	public class StarshipTests
	{
		[TestMethod]
		public void Constructor_SetsDefaults()
		{
			var ship = new Starship("X-Wing", 5000);

			Assert.IsFalse(string.IsNullOrEmpty(ship.Id), "Id assegnato (GUID)");
			Assert.AreEqual("X-Wing", ship.Name);
			Assert.AreEqual(5000, ship.Mass);
			Assert.AreEqual(5000, ship.Weight, "Weight = Mass (da PhysicsBody)");
			Assert.IsNotNull(ship.Dynamics, "Dynamics inizializzata");
			Assert.IsInstanceOfType<VelocityVerletMotion>(ship.Motion, "Motion = Velocity Verlet dal costruttore");
			Assert.AreEqual(10e12, ship.MaxThrust);
			Assert.IsTrue(ship.ThrustDirection.IsEquals(new Vector3D(0, 0, 1)));
			Assert.AreEqual(0, ship.CurrentThrottle);
		}

		[TestMethod]
		public void GetThrustForce_ScalesWithThrottle()
		{
			var ship = new Starship("X-Wing", 5000) { MaxThrust = 150000 };

			ship.CurrentThrottle = 0.0;
			Assert.IsTrue(ship.GetThrustForce().IsEquals(Vector3D.Zero), "throttle 0 => spinta nulla");

			ship.CurrentThrottle = 0.5;
			Assert.AreEqual(150000 * 0.5, ship.GetThrustForce().Length, 1e-6, "spinta = MaxThrust · throttle");
		}

		[TestMethod]
		public void FlightController_Damping_StopsRotation()
		{
			var ship = new Starship("X-Wing", 5000);
			var controller = new ShipFlightController(ship) { AngularDamping = 10.0 };

			// 1. Input di imbardata
			controller.HandleInput(0, 1.0, 0, 0, 0.1);

			// 2. Rilascio: il damping deve fermare la rotazione
			for (int i = 0; i < 100; i++)
				controller.HandleInput(0, 0, 0, 0, 0.01);

			ship.GetRotation(out _, out double yawStopped, out _);
			controller.HandleInput(0, 0, 0, 0, 0.01);
			ship.GetRotation(out _, out double yawFinal, out _);

			Assert.AreEqual(yawStopped, yawFinal, 0.0001, "il damping non ha fermato la rotazione");
		}

		[TestMethod]
		public void FlightController_Yaw_ChangesOrientation()
		{
			var ship = new Starship("X-Wing", 5000);
			var controller = new ShipFlightController(ship);
			ship.GetRotation(out _, out double yaw0, out _);

			controller.HandleInput(0, 1.0, 0, 0, 0.1);

			ship.GetRotation(out _, out double yaw1, out _);
			Assert.AreNotEqual(yaw0, yaw1, "l'input di imbardata deve cambiare l'orientamento");
		}
	}
}
