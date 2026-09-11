using Axiom.GeoMath;
using System.Collections.Generic;

namespace Axiom.Physics
{
	/// <summary>
	/// Octree gravitazionale (approssimazione Barnes-Hut) generico su un qualunque
	/// <see cref="IPointWeighted"/>: incapsula l'<see cref="OctreeNode{T}"/> di GeoMath e vi aggiunge
	/// il calcolo dell'accelerazione con soglia θ e la ricerca spaziale ("radar").
	/// </summary>
	/// <typeparam name="T">Tipo del corpo (posizione + peso).</typeparam>
	public class GravityOctree<T> where T : class, IPointWeighted
	{
		#region Fields
		/// <summary>
		/// OctreeNode interno generico per la gestione dei corpi.
		/// </summary>
		private readonly OctreeNode<T> _internalNode;
		/// <summary>
		/// Soglia di approssimazione per l'algoritmo Barnes-Hut.
		/// </summary>
		private const double Theta = 0.5;
		#endregion

		#region Properties
		/// <summary>
		/// Peso totale (massa) dei corpi in questo nodo.
		/// </summary>
		public double TotalWeight => _internalNode.TotalWeight;
		/// <summary>
		/// Posizione del centro di massa (baricentro pesato) dei corpi in questo nodo.
		/// </summary>
		public Vector3D WeightedCenter => _internalNode.WeightedCenter;
		/// <summary>
		/// Foglia (senza figli).
		/// </summary>
		public bool? IsLeaf => _internalNode.IsLeaf;
		#endregion

		#region Constructors
		public GravityOctree(AABBox3D boundary)
		{
			_internalNode = new OctreeNode<T>(boundary);
		}
		#endregion

		#region Methods
		/// <summary>
		///  --- RADAR: Ricerca Spaziale ---
		/// Restituisce tutti i corpi entro un raggio specificato (utile per sensori/collisioni).
		/// </summary>
		public List<T> QueryRange(Point3D center, double radius)
		{
			var results = new List<T>();
			_internalNode.GetEntitiesInRange(center, radius, results);
			return results;
		}

		/// <summary>
		/// Inserisce un corpo nell'Octree.
		/// </summary>
		public void Insert(T body) => _internalNode.Insert(body);

		/// <summary>
		/// Calcola l'accelerazione gravitazionale integrando la logica Barnes-Hut sull'Octree.
		/// </summary>
		public Vector3D GetAcceleration(IPointWeighted target, double G) => CalculateRecursiveAcceleration(_internalNode, target, G);

		private Vector3D CalculateRecursiveAcceleration(OctreeNode<T> node, IPointWeighted target, double G)
		{
			Vector3D acceleration = Vector3D.Zero;

			if (node.IsLeaf)
			{
				foreach (var body in node.GetEntries())
				{
					if (body == target) continue;
					acceleration += ComputeNewtonForce(target.Position, body.Position, body.Weight, G);
				}
			}
			else
			{
				double distance = (target.Position - node.WeightedCenter).Length;
				double size = node.Boundary.LX;

				if (size / distance < Theta)
				{
					// Nodo lontano: usiamo il baricentro pesato (Centro di Massa)
					acceleration += ComputeNewtonForce(target.Position, node.WeightedCenter, node.TotalWeight, G);
				}
				else
				{
					// Nodo vicino: scendi nei figli
					foreach (var child in node.GetChildren())
					{
						acceleration += CalculateRecursiveAcceleration(child, target, G);
					}
				}
			}

			return acceleration;
		}

		private Vector3D ComputeNewtonForce(Vector3D targetPos, Vector3D sourcePos, double mass, double G)
		{
			Vector3D direction = sourcePos - targetPos;
			double distSq = direction.LengthSquared;
			if (distSq < 1e-6) return Vector3D.Zero;

			return direction.Normalize() * (G * mass / distSq);
		}

		#endregion
	}
}
