using Axiom.GeoMath;
using Axiom.Physics;
using System.Collections.Generic;

namespace Axiom.Physics.Tests
{
	[TestClass]
	public class NewtonianGravityTests
	{
		private const double G = PhysicalConstants.G;

		[TestMethod]
		public void DefaultCtor_ComputeForce_IsZero_AndDoesNotThrow()
		{
			var field = new NewtonianGravity(); // nessun corpo memorizzato
			var a = new TestBody(1e24, Vector3D.Zero);

			Vector3D force = field.ComputeForce(a);

			Assert.IsTrue(force.IsEquals(Vector3D.Zero));
		}

		[TestMethod]
		public void ComputeForce_TwoBodies_PointsTowardOther_WithNewtonMagnitude()
		{
			double mA = 5e24, mB = 3e24, d = 10;
			var a = new TestBody(mA, Vector3D.Zero);
			var b = new TestBody(mB, new Vector3D(d, 0, 0));
			var field = new NewtonianGravity(new List<PhysicsBody> { a, b });

			Vector3D force = field.ComputeForce(a);

			double expected = G * mA * mB / (d * d);
			Assert.AreEqual(expected, force.Length, expected * 1e-9, "modulo della forza di Newton");
			Assert.IsGreaterThan(0, force.X, "la forza su A punta verso B (+X)");
			Assert.AreEqual(0, force.Y, 1e-6);
			Assert.AreEqual(0, force.Z, 1e-6);
		}

		[TestMethod]
		public void ComputeAcceleration_ScalesWithSourceMassOverDistanceSquared()
		{
			double mB = 3e24, d = 10;
			var target = new TestBody(1e10, Vector3D.Zero);
			var source = new TestBody(mB, new Vector3D(d, 0, 0));
			var field = new NewtonianGravity();

			Vector3D acc = field.ComputeAcceleration(target, new List<PhysicsBody> { target, source });

			double expected = G * mB / (d * d); // indipendente dalla massa del target
			Assert.AreEqual(expected, acc.Length, expected * 1e-9);
			Assert.IsGreaterThan(0, acc.X, "accelerazione verso la sorgente (+X)");
		}

		[TestMethod]
		public void ComputeAcceleration_IgnoresSelf()
		{
			var target = new TestBody(1e24, new Vector3D(5, 0, 0));
			// L'unico "altro" corpo è il target stesso: nessuna accelerazione, nessuna divisione per zero.
			Vector3D acc = new NewtonianGravity().ComputeAcceleration(target, new List<PhysicsBody> { target });
			Assert.IsTrue(acc.IsEquals(Vector3D.Zero));
		}

		[TestMethod]
		public void CoincidentBodies_AreSkipped_NoSingularity()
		{
			// Due corpi nella stessa posizione: la guardia dist² < 1e-6 evita NaN/infiniti.
			var a = new TestBody(1e24, Vector3D.Zero);
			var b = new TestBody(1e24, Vector3D.Zero);
			var field = new NewtonianGravity(new List<PhysicsBody> { a, b });

			Vector3D force = field.ComputeForce(a);
			Vector3D acc = field.ComputeAcceleration(a, new List<PhysicsBody> { a, b });

			Assert.IsTrue(force.IsEquals(Vector3D.Zero));
			Assert.IsTrue(acc.IsEquals(Vector3D.Zero));
			Assert.IsFalse(double.IsNaN(force.X), "nessun NaN");
		}
	}
}
