using Axiom.GeoMath;
using Axiom.Physics;

namespace Axiom.Physics.Tests
{
	[TestClass]
	public class IntegratorTests
	{
		[TestMethod]
		public void DynamicsState_Defaults_AreZero()
		{
			var s = new DynamicsState();
			Assert.IsTrue(s.Velocity.IsEquals(Vector3D.Zero));
			Assert.IsTrue(s.Acceleration.IsEquals(Vector3D.Zero));
		}

		[TestMethod]
		public void PhysicalConstants_G_HasExpectedValue()
		{
			Assert.AreEqual(6.67430e-11, PhysicalConstants.G, 0.0);
		}

		[TestMethod]
		public void EulerIntegrator_UpdatesVelocityThenPosition()
		{
			// v += a·dt ; x += v·dt (velocità aggiornata PRIMA della posizione)
			var body = new TestBody(1, Vector3D.Zero);
			var state = new DynamicsState { Acceleration = new Vector3D(0, 0, 2) };
			IMotionModel euler = new EulerIntegrator();

			euler.Integrate(body, state, 0.5);

			Assert.IsTrue(state.Velocity.IsEquals(new Vector3D(0, 0, 1)), "v = a·dt");
			Assert.IsTrue(body.Translation.IsEquals(new Vector3D(0, 0, 0.5)), "x = v·dt con la v già aggiornata");
		}

		[TestMethod]
		public void VelocityVerlet_IntegrateThenCompleteStep()
		{
			// Integrate: x += v·dt + ½a·dt² ; v += ½a·dt   |   CompleteStep: v += ½a·dt
			var body = new TestBody(1, Vector3D.Zero);
			var state = new DynamicsState
			{
				Velocity = new Vector3D(1, 0, 0),
				Acceleration = new Vector3D(0, 2, 0)
			};
			var verlet = new VelocityVerletMotion();

			verlet.Integrate(body, state, 0.5);

			// x = (1,0,0)·0.5 + (0,2,0)·0.5·0.25 = (0.5, 0.25, 0)
			Assert.IsTrue(body.Translation.IsEquals(new Vector3D(0.5, 0.25, 0)), "posizione dopo il primo half-step");
			// v = (1,0,0) + (0,2,0)·0.25 = (1, 0.5, 0)
			Assert.IsTrue(state.Velocity.IsEquals(new Vector3D(1, 0.5, 0)), "mezza velocità");

			verlet.CompleteStep(state, 0.5);
			// v += (0,2,0)·0.25 = (1, 1, 0)
			Assert.IsTrue(state.Velocity.IsEquals(new Vector3D(1, 1, 0)), "velocità completata (secondo half-step)");
		}
	}
}
