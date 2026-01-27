using Axiom.Cosmos.Models;
using Axiom.Cosmos.Starships;
using Axiom.Cosmos.Utils;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Simulation
{
	public class CosmosPhysicsEngine
	{
		public void UpdateGalaxy(Galaxy galaxy, double dt) => galaxy.UpdatePhysics(dt);

		public void ApplyShipPhysics(Starship ship, CosmosOctreeNode gravityField, double dt)
		{
			// Gravità + Motore
			Vector3D gravityAcc = (gravityField == null) ? Vector3D.Zero : gravityField.GetAcceleration(ship, Galaxy.G);
			Vector3D engineAcc = ship.GetThrustForce() / ship.Mass;
			ship.Dynamics.Acceleration = gravityAcc + engineAcc;

			// Damping Lineare
			if (ship.CurrentThrottle < 0.01)
				ship.Dynamics.Velocity *= Math.Max(0, 1.0 - (0.5 * dt));

			ship.Motion?.Integrate(ship, ship.Dynamics, dt);
		}
	}
}
