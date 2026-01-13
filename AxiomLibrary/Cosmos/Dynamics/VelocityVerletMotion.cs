using Axiom.GeoShape;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Dynamics
{
	public sealed class VelocityVerletMotion : IMotionModel
	{
		public void Integrate(Node3D node, DynamicsState state, double dt)
		{
			// posizione
			node.Translation +=
				state.Velocity * dt +
				state.Acceleration * (0.5 * dt * dt);

			// velocità (half step)
			state.Velocity += state.Acceleration * (0.5 * dt);
		}

		/// <summary>
		/// Chiamato dopo il ricalcolo delle accelerazioni
		/// </summary>
		public void CompleteStep(DynamicsState state, double dt)
		{
			state.Velocity += state.Acceleration * (0.5 * dt);
		}
	}
}
