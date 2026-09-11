using Axiom.GeoShape;

namespace Axiom.Physics
{
	public sealed class EulerIntegrator : IMotionModel
	{
		public void Integrate(Node3D node, DynamicsState state, double dt)
		{
			state.Velocity += state.Acceleration * dt;
			node.Translation += state.Velocity * dt;
		}
	}
}
