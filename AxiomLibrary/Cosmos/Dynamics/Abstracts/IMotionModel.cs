using Axiom.GeoShape;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Dynamics
{
	public interface IMotionModel
	{
		void Integrate(Node3D node, DynamicsState state, double deltaTime);
	}
}
