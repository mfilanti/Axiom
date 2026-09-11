using Axiom.Cosmos.Model;
using Axiom.Cosmos.Model.Starships;
using Axiom.GeoMath;
using Axiom.Physics;
using System;

namespace Axiom.Cosmos.Simulation
{
	/// <summary>
	/// Motore di fisica della simulazione: punto unico in cui vengono aggiornati i corpi celesti
	/// (delegando l'integrazione gravitazionale a <see cref="Galaxy.UpdatePhysics"/>) e le navi.
	/// </summary>
	public class CosmosPhysicsEngine
	{
		/// <summary>
		/// Avanza la fisica della galassia e restituisce l'Octree coerente con le posizioni aggiornate,
		/// riutilizzabile come campo gravitazionale per le navi.
		/// </summary>
		public GravityOctree<PhysicsBody> UpdateGalaxy(Galaxy galaxy, double dt) => galaxy.UpdatePhysics(dt);

		/// <summary>
		/// Applica gravità, spinta dei motori e smorzamento lineare alla nave, poi ne integra il moto.
		/// </summary>
		/// <returns>Il vettore di accelerazione gravitazionale applicato (per HUD/telemetria).</returns>
		public Vector3D ApplyShipPhysics(Starship ship, GravityOctree<PhysicsBody> gravityField, double dt)
		{
			// Gravità + Motore
			Vector3D gravityAcc = (gravityField == null) ? Vector3D.Zero : gravityField.GetAcceleration(ship, PhysicalConstants.G);
			Vector3D engineAcc = ship.GetThrustForce() / ship.Mass;
			ship.Dynamics.Acceleration = gravityAcc + engineAcc;

			// Smorzamento lineare
			ApplyLinearDamping(ship, dt);

			ship.Motion?.Integrate(ship, ship.Dynamics, dt);

			return gravityAcc;
		}

		/// <summary>
		/// Smorzamento lineare della velocità quando la nave non sta accelerando:
		/// velocità *= 0.5^dt (si dimezza ogni secondo).
		/// </summary>
		private static void ApplyLinearDamping(Starship ship, double dt)
		{
			const double dampingFactor = 0.50;
			if (ship.CurrentThrottle < 0.1)
				ship.Dynamics.Velocity *= Math.Pow(dampingFactor, dt);
		}
	}
}
