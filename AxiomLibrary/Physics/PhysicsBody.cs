using Axiom.GeoMath;
using Axiom.GeoShape;

namespace Axiom.Physics
{
	/// <summary>
	/// Base comune a tutti i corpi soggetti alla dinamica del motore (astratta, indipendente dal
	/// dominio). È un nodo dello scene-graph (<see cref="Node3D"/>) dotato di stato fisico e, in quanto
	/// <see cref="IPointWeighted"/>, inseribile in un octree e utilizzabile dal campo gravitazionale.
	/// </summary>
	public abstract class PhysicsBody : Node3D, IPointWeighted
	{
		/// <summary>
		/// Nome del corpo.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Massa del corpo in kg (usata come peso nell'Octree e in F = m·a).
		/// </summary>
		public double Mass { get; set; }

		/// <summary>
		/// Stato cinematico (velocità, accelerazione).
		/// </summary>
		public DynamicsState Dynamics { get; set; }

		/// <summary>
		/// Modello di integrazione del moto (strategia).
		/// </summary>
		public IMotionModel Motion { get; set; }

		/// <summary>
		/// Posizione assoluta (traslazione della matrice di mondo).
		/// </summary>
		public Vector3D Position => WorldMatrix.Translation;

		/// <summary>
		/// Vettore Z della matrice di mondo (direzione avanti).
		/// </summary>
		public Vector3D ZVector => WorldMatrix.ZVector;

		/// <summary>
		/// Vettore Y della matrice di mondo (direzione su).
		/// </summary>
		public Vector3D YVector => WorldMatrix.YVector;

		/// <summary>
		/// Vettore X della matrice di mondo (direzione destra).
		/// </summary>
		public Vector3D XVector => WorldMatrix.XVector;

		/// <summary>
		/// Peso del corpo per l'Octree/baricentro: in fisica coincide con la massa.
		/// </summary>
		public double Weight => Mass;
	}
}
