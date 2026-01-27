using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using Axiom.GeoShape;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Axiom.Cosmos.Starships
{
	public class Starship : Node3D, IPointWeighted
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Potenza dei motori (Newton)
		/// </summary>
		public double MaxThrust { get; set; } =10e12;

		/// <summary>
		/// Direzione attuale della spinta (vettore normalizzato)
		/// </summary>
		public Vector3D ThrustDirection { get; set; } = new Vector3D(0, 0, 1);

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
		public Vector3D Position => WorldMatrix.Translation;

		/// <summary>
		/// Vettore Z della matrice di mondo (direzione avanti)
		/// </summary>
		public Vector3D ZVector => WorldMatrix.ZVector;

		/// <summary>
		/// Vettore Y della matrice di mondo (direzione su)
		/// </summary>
		public Vector3D YVector => WorldMatrix.YVector;

		/// <summary>
		/// Vettore Y della matrice di mondo (direzione su)
		/// </summary>
		public Vector3D XVector => WorldMatrix.XVector;
		/// <summary>
		/// Peso della nave (uguale alla massa in assenza di gravità)
		/// </summary>
		public double Weight => Mass;

		#endregion

		#region Constructors

		public Starship(string name, double mass) : base()
		{
			Id = Guid.NewGuid().ToString();
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
			// Direzione Forward estratta dalla matrice di rotazione del Node3D
			Vector3D forward = WorldMatrix.ZVector;
			return forward * (MaxThrust * CurrentThrottle);
		}

		
		#endregion

	}
}
