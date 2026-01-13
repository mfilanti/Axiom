using Axiom.GeoShape;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Dynamics
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
