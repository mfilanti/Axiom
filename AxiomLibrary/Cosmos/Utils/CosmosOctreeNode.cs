using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Utils
{
	public class CosmosOctreeNode
	{
		#region Fields
		/// <summary>
		/// OctreeNode interno generico per la gestione dei corpi celesti.
		/// </summary>
		private readonly OctreeNode<CelestialBody> _internalNode;
		/// <summary>
		/// Soglia di approssimazione per l'algoritmo Barnes-Hut.
		/// </summary>
		private const double Theta = 0.5;
		#endregion

		#region Properties

		/// <summary>
		/// Peso totale dei corpi celesti in questo nodo.
		/// </summary>
		public double TotalWeight => _internalNode.TotalWeight;
		/// <summary>
		/// Posizione del centro di massa (baricentro pesato) dei corpi celesti in questo nodo.
		/// </summary>
		public Vector3D WeightedCenter => _internalNode.WeightedCenter;
		/// <summary>
		/// Foglia (senza figli).
		/// </summary>
		public bool? IsLeaf => _internalNode.IsLeaf;
		#endregion

		#region Constructors
		public CosmosOctreeNode(AABBox3D boundary)
		{
			_internalNode = new OctreeNode<CelestialBody>(boundary);
		}
		#endregion

		#region Methods
		/// <summary>
		///  --- RADAR: Ricerca Spaziale ---
		/// Restituisce tutti i corpi celesti entro un raggio specificato.
		/// Utilissimo per sensori navicella o collisioni.
		/// </summary>
		public List<CelestialBody> QueryRange(Point3D center, double radius)
		{
			var results = new List<CelestialBody>();
			_internalNode.GetEntitiesInRange(center, radius, results);
			return results;
		}

        /// <summary>
        /// Inserisce un corpo celeste nell'Octree.
        /// </summary>
        /// <param name="body"></param>
        public void Insert(CelestialBody body) => _internalNode.Insert(body);

        /// <summary>
        /// Calcola l'accelerazione gravitazionale integrando la logica Barnes-Hut 
        /// sopra l'Octree generico.
        /// </summary>
        public Vector3D GetAcceleration(IPointWeighted target, double G) => CalculateRecursiveAcceleration(_internalNode, target, G);

        private Vector3D CalculateRecursiveAcceleration(OctreeNode<CelestialBody> node, IPointWeighted target, double G)
		{
			Vector3D acceleration = Vector3D.Zero;

			if (node.IsLeaf)
			{
				// Accesso alle entries del nodo generico tramite riflessione o rendendo _entries protected/internal
				// In alternativa, aggiungi un metodo GetEntries() a OctreeNode<T>
				foreach (var body in node.GetEntries())
				{
					if (body == target) continue;
					acceleration += ComputeNewtonForce(target.Position, body.Position, body.Mass, G);
				}
			}
			else
			{
				double distance = (target.Position - node.WeightedCenter).Length;
				double size = node.Boundary.LX; // Usiamo LX dal tuo AABBox3D

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
