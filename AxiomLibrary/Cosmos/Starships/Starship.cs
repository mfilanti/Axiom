using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Dynamics.Abstracts;
using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using Axiom.GeoShape;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Axiom.Cosmos.Starships
{
	public class Starship : Node3D, IDynamicEntity
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Potenza dei motori (Newton)
		/// </summary>
		public double MaxThrust { get; set; } = 100000;

		/// <summary>
		/// Direzione attuale della spinta (vettore normalizzato)
		/// </summary>
		public Vector3D ThrustDirection { get; set; } = new Vector3D(0, 0, 1);

		/// <summary>
		/// Indica se i motori sono accesi (0.0 a 1.0)
		/// </summary>
		public double Throttle { get; set; } = 0;

		/// <summary>
		/// Nome della nave
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Massa della nave (kg)
		/// </summary>
		public double Mass { get; set; } // Necessaria per F = m * a

		/// <summary>
		/// Dinamica della nave
		/// </summary>
		public DynamicsState Dynamics { get; set; } = new DynamicsState();

		/// <summary>
		/// Modello di movimento della nave
		/// </summary>
		public IMotionModel Motion { get; set; }

		/// <summary>
		/// Proprietà specifiche della nave (Star Wars style)
		/// </summary>
		public double CurrentThrottle { get; set; } // 0.0 a 1.0

		/// <summary>
		/// Posizione assoluta
		/// </summary>
		public Vector3D AbsolutePosition => WorldMatrix.Translation;

		#endregion

		#region Constructors

		public Starship(string name, double mass) : base()
		{
			Name = name;
			Mass = mass;
			Motion = new VelocityVerletMotion(); // O un modello più "arcade"
		}
		#endregion

		#region Methods

		/// <summary>
		/// Calcola la forza di spinta attuale dei motori
		/// </summary>
		public Vector3D GetThrustForce()
		{
			// La spinta segue la rotazione della nave (matrice RT)
			// Possiamo estrarre la direzione "Forward" dalla WorldMatrix del Node3D
			Vector3D forward = WorldMatrix.ZVector;// new Vector3D(WorldMatrix.M13, WorldMatrix.M23, WorldMatrix.M33);
			return forward * (MaxThrust * Throttle);
		}


		public Vector3D GetEngineForce()
		{
			// Direzione Forward estratta dalla matrice di rotazione del Node3D
			Vector3D forward = WorldMatrix.ZVector;
			return forward * (MaxThrust * CurrentThrottle);
		}
		#endregion

	}
}
