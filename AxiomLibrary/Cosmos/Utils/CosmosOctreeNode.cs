using Axiom.Cosmos.Dynamics.Abstracts;
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
		/// Capacità prima di dividersi
		/// </summary>
		private const int MaxBodies = 4;
		private const double Theta = 0.5; // Soglia di precisione Barnes-Hut (0.5 è lo standard)

		/// <summary>
		/// Contenuto del nodo
		/// </summary>
		private List<CelestialBody> _bodies = new List<CelestialBody>();

		private CosmosOctreeNode[] _children;



		#endregion

		#region Properties
		public bool IsLeaf => _children == null;

		/// <summary>
		/// Il volume di spazio occupato da questo nodo
		/// </summary>
		public AABBox3D Boundary { get; private set; }

		/// <summary>
		///  Dati fisici aggregati (per Barnes-Hut)
		/// </summary>
		public double TotalMass { get; private set; }

		/// <summary>
		/// Centro di massa aggregato
		/// </summary>
		public Vector3D CenterOfMass { get; private set; }
		#endregion

		#region Constructors
		public CosmosOctreeNode(AABBox3D boundary)
		{
			Boundary = boundary;
			CenterOfMass = Vector3D.Zero;
			TotalMass = 0;
		}
		#endregion

		#region Methods
		// Inserisce un corpo e aggiorna i dati fisici del nodo
		public void Insert(CelestialBody body)
		{
			// 1. Controllo se il corpo è fisicamente dentro il box
			// (Assumiamo che body.Position sia un Vector3D o Point3D)
			if (!Contains(Boundary, body.AbsolutePosition)) return;

			// 2. Aggiornamento Centro di Massa e Massa Totale del nodo
			UpdateMassProperties(body);

			// 3. Se è una foglia e ha spazio, aggiungi qui
			if (IsLeaf && _bodies.Count < MaxBodies)
			{
				_bodies.Add(body);
				return;
			}

			// 4. Se è pieno, suddividi (se non è già stato fatto)
			if (IsLeaf) Subdivide();

			// 5. Inserisci ricorsivamente nei figli
			foreach (var child in _children)
			{
				child.Insert(body);
			}
		}

		private void UpdateMassProperties(CelestialBody body)
		{
			double newTotalMass = TotalMass + body.Mass;
			// Calcolo del nuovo centro di massa: (m1*r1 + m2*r2) / (m1+m2)
			CenterOfMass = (CenterOfMass * TotalMass + body.WorldMatrix.Translation * body.Mass) / newTotalMass;
			TotalMass = newTotalMass;
		}

		private void Subdivide()
		{
			_children = new CosmosOctreeNode[8];
			Point3D min = Boundary.MinPoint;
			Point3D max = Boundary.MaxPoint;
			double midX = (min.X + max.X) / 2.0;
			double midY = (min.Y + max.Y) / 2.0;
			double midZ = (min.Z + max.Z) / 2.0;

			// Creazione degli 8 ottanti usando il tuo AABBox3D
			_children[0] = new CosmosOctreeNode(new AABBox3D(new Point3D(min.X, min.Y, min.Z), new Point3D(midX, midY, midZ)));
			_children[1] = new CosmosOctreeNode(new AABBox3D(new Point3D(midX, min.Y, min.Z), new Point3D(max.X, midY, midZ)));
			_children[2] = new CosmosOctreeNode(new AABBox3D(new Point3D(min.X, midY, min.Z), new Point3D(midX, max.Y, midZ)));
			_children[3] = new CosmosOctreeNode(new AABBox3D(new Point3D(midX, midY, min.Z), new Point3D(max.X, max.Y, midZ)));
			_children[4] = new CosmosOctreeNode(new AABBox3D(new Point3D(min.X, min.Y, midZ), new Point3D(midX, midY, max.Z)));
			_children[5] = new CosmosOctreeNode(new AABBox3D(new Point3D(midX, min.Y, midZ), new Point3D(max.X, midY, max.Z)));
			_children[6] = new CosmosOctreeNode(new AABBox3D(new Point3D(min.X, midY, midZ), new Point3D(midX, max.Y, max.Z)));
			_children[7] = new CosmosOctreeNode(new AABBox3D(new Point3D(midX, midY, midZ), new Point3D(max.X, max.Y, max.Z)));

			// Sposta i corpi correnti nei nuovi figli
			foreach (var b in _bodies)
			{
				foreach (var child in _children) child.Insert(b);
			}
			_bodies.Clear();
		}

		private bool Contains(AABBox3D box, Point3D p)
		{
			return p.X >= box.MinPoint.X && p.X <= box.MaxPoint.X &&
				   p.Y >= box.MinPoint.Y && p.Y <= box.MaxPoint.Y &&
				   p.Z >= box.MinPoint.Z && p.Z <= box.MaxPoint.Z;
		}

		/// <summary>
		/// Calcola l'accelerazione gravitazionale esercitata su un corpo navigando l'albero.
		/// </summary>
		public Vector3D GetAcceleration(IDynamicEntity target, double G)
		{
			Vector3D acceleration = Vector3D.Zero;

			// Se è un corpo singolo (foglia), calcola Newton direttamente
			if (IsLeaf)
			{
				foreach (var body in _bodies)
				{
					if (body == target) continue;
					acceleration += ComputeNewtonForce(target, body.WorldMatrix.Translation, body.Mass, G);
				}
			}
			else
			{
				// Algoritmo Barnes-Hut: Verifica se il nodo è abbastanza lontano da essere trattato come punto unico
				double distance = (target.AbsolutePosition - CenterOfMass).Length;
				double size = Boundary.MaxPoint.X - Boundary.MinPoint.X;

				if (size / distance < Theta)
				{
					// Nodo lontano: calcola la forza usando il centro di massa totale
					acceleration += ComputeNewtonForce(target, CenterOfMass, TotalMass, G);
				}
				else
				{
					// Nodo vicino: scendi ricorsivamente nei figli
					foreach (var child in _children)
					{
						acceleration += child.GetAcceleration(target, G);
					}
				}
			}

			return acceleration;
		}

		private Vector3D ComputeNewtonForce(IDynamicEntity target, Vector3D sourcePos, double sourceMass, double G)
		{
			Vector3D direction = sourcePos - target.AbsolutePosition;
			double distanceSq = direction.LengthSquared;
			if (distanceSq < 1e-6) return Vector3D.Zero; // Evita divisioni per zero

			double forceMagnitude = (G * sourceMass) / distanceSq;
			return direction.Normalize() * forceMagnitude;
		}
		#endregion
	}
}
