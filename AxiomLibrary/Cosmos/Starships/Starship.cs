using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using System;

namespace Axiom.Cosmos.Starships
{
	public class Starship : PhysicsBody
	{
		#region Properties
		/// <summary>
		/// Potenza dei motori (Newton)
		/// </summary>
		public double MaxThrust { get; set; } = 10e12;

		/// <summary>
		/// Direzione attuale della spinta (vettore normalizzato)
		/// </summary>
		public Vector3D ThrustDirection { get; set; } = new Vector3D(0, 0, 1);

		/// <summary>
		/// Proprietà specifiche della nave (Star Wars style)
		/// </summary>
		public double CurrentThrottle { get; set; } // 0.0 a 1.0

		#endregion

		#region Constructors

		public Starship(string name, double mass) : base()
		{
			Id = Guid.NewGuid().ToString();
			Name = name;
			Mass = mass;
			Dynamics = new DynamicsState();
			Motion = new VelocityVerletMotion(); // O un modello più "arcade"
		}
		#endregion

		#region Methods

		/// <summary>
		/// Calcola la forza di spinta attuale dei motori
		/// </summary>
		public Vector3D GetThrustForce()
		{
			// Direzione Forward estratta dalla matrice di rotazione del Node3D
			Vector3D forward = WorldMatrix.ZVector;
			return forward * (MaxThrust * CurrentThrottle);
		}

		#endregion

	}
}
