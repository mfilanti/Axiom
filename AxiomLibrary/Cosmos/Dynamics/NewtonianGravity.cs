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
		private const double G = PhysicalConstants.G;

		/// <summary>
		/// Costruttore di default: nessun corpo di riferimento memorizzato.
		/// Con questo costruttore usare <see cref="ComputeAcceleration"/>, che riceve i corpi come parametro.
		/// </summary>
		public NewtonianGravity()
		{
			_bodies = Array.Empty<CelestialBody>();
		}

		/// <summary>
		/// Costruttore che memorizza l'insieme dei corpi usato da <see cref="ComputeForce"/>.
		/// </summary>
		/// <param name="bodies">Corpi che generano il campo gravitazionale.</param>
		public NewtonianGravity(IReadOnlyList<CelestialBody> bodies)
		{
			_bodies = bodies ?? Array.Empty<CelestialBody>();
		}

		public Vector3D ComputeForce(CelestialBody body)
		{
			Vector3D force = Vector3D.Zero;

			foreach (var other in _bodies)
			{
				if (other == body) continue;

				var r = other.WorldMatrix.Translation - body.WorldMatrix.Translation;
				var distanceSquared = r.LengthSquared;

				// Evita la singolarità (e la Normalize su vettore nullo) per corpi coincidenti.
				if (distanceSquared < 1e-6) continue;

				force += r.Normalize() * (G * body.Mass * other.Mass / distanceSquared);
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
