using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Axiom.Cosmos.Dynamics
{
	public sealed class NewtonianGravity : IGravityField
	{
		private readonly IReadOnlyList<CelestialBody> _bodies;
		private const double G = 6.67430e-11;

		public Vector3D ComputeForce(CelestialBody body)
		{
			Vector3D force = Vector3D.Zero;

			foreach (var other in _bodies)
			{
				if (other == body) continue;

				var r = other.Translation - body.Translation;
				var distance = r.Length;

				force += r.Normalize() * (G * body.Mass * other.Mass / (distance * distance));
			}

			return force;
		}

		public Vector3D ComputeAcceleration(
		CelestialBody target,
		IReadOnlyCollection<CelestialBody> allBodies)
		{
			Vector3D acceleration = Vector3D.Zero;

			Vector3D targetPos = target.WorldMatrix.Translation;

			foreach (var other in allBodies)
			{
				if (other == target)
					continue;

				Vector3D otherPos = other.WorldMatrix.Translation;
				Vector3D direction = otherPos - targetPos;

				double distanceSquared = direction.LengthSquared;

				if (distanceSquared < 1e-6)
					continue;

				double force = G * other.Mass / distanceSquared;

				acceleration += direction.Normalize() * force;
			}

			return acceleration;
		}
	}
}
