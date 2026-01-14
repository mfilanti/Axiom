using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axiom.Cosmos.Models;
using Axiom.GeoShape.Elements;
using Axiom.GeoMath;
using System.Collections.Generic;
using Axiom.Cosmos.Utils;

namespace Axiom.Cosmos.Tests
{
	[TestClass]
	public class CosmosOctreeNodeTests
	{
		/// <summary>
		/// Verifica che il Centro di Massa sia calcolato correttamente 
		/// come media pesata delle masse inserite.
		/// </summary>
		[TestMethod]
		public void CenterOfMass_ShouldBeWeightedAverage()
		{
			// ARRANGE
			// Creiamo un box 100x100x100
			var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(100, 100, 100));
			var root = new CosmosOctreeNode(boundary);

			// Corpo 1: Massa 100 a (0,0,0)
			var p1 = new Planet("P1", 100, 1, new Vector3D(0, 0, 0));
			// Corpo 2: Massa 300 a (40,0,0)
			var p2 = new Planet("P2", 300, 1, new Vector3D(40, 0, 0));

			// ACT
			root.Insert(p1);
			root.Insert(p2);

			// CALCOLO ATTESO: (100*0 + 300*40) / (100 + 300) = 12000 / 400 = 30
			double expectedX = 30;

			// ASSERT
			Assert.AreEqual(400, root.TotalWeight, "La massa totale non corrisponde.");
			Assert.AreEqual(expectedX, root.WeightedCenter.X, 0.001, "Il Centro di Massa X è errato.");
			Assert.AreEqual(0, root.WeightedCenter.Y, "Il Centro di Massa Y dovrebbe essere 0.");
		}

		/// <summary>
		/// Verifica che l'Octree si suddivida correttamente quando supera MaxBodies.
		/// </summary>
		[TestMethod]
		public void Subdivide_ShouldOccur_WhenMaxBodiesExceeded()
		{
			// ARRANGE
			var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(100, 100, 100));
			var root = new CosmosOctreeNode(boundary);

			// Inseriamo 5 corpi (assumendo che MaxBodies sia 4)
			// Li mettiamo in angoli diversi per forzare la distribuzione nei figli
			var bodies = new List<CelestialBody>
			{
				new Planet("1", 10, 1, new Vector3D(10, 10, 10)),
				new Planet("2", 10, 1, new Vector3D(90, 10, 10)),
				new Planet("3", 10, 1, new Vector3D(10, 90, 10)),
				new Planet("4", 10, 1, new Vector3D(10, 10, 90)),
				new Planet("5", 10, 1, new Vector3D(90, 90, 90))
			};

			// ACT
			foreach (var b in bodies) root.Insert(b);

			// ASSERT
			Assert.IsFalse(root.IsLeaf, "Il nodo radice dovrebbe essersi suddiviso.");
			// Verifichiamo che la massa totale sia comunque corretta
			Assert.AreEqual(50, root.TotalWeight, "La massa totale dopo la suddivisione è errata.");
		}

		/// <summary>
		/// Verifica che la gravità calcolata tramite l'approssimazione Barnes-Hut (nodo lontano)
		/// sia simile a quella calcolata con Newton diretto.
		/// </summary>
		[TestMethod]
		public void BarnesHut_Approximation_ShouldBeCloseToDirectGravity()
		{
			// ARRANGE
			double G = 6.674e-11;
			// Box molto grande
			var boundary = new AABBox3D(new Point3D(-1e12, -1e12, -1e12), new Point3D(1e12, 1e12, 1e12));
			var root = new CosmosOctreeNode(boundary);

			// Creiamo un ammasso di 3 pianeti molto vicini tra loro (formano un "punto" di massa)
			var p1 = new Planet("P1", 1e24, 1, new Vector3D(0, 0, 0));
			var p2 = new Planet("P2", 1e24, 1, new Vector3D(1000, 0, 0));
			var p3 = new Planet("P3", 1e24, 1, new Vector3D(0, 1000, 0));

			root.Insert(p1);
			root.Insert(p2);
			root.Insert(p3);

			// Corpo di test molto lontano (100 milioni di km)
			var probe = new Planet("Probe", 1000, 1, new Vector3D(1e11, 0, 0));

			// ACT
			// 1. Calcolo tramite Octree (Barnes-Hut)
			Vector3D octreeAcc = root.GetAcceleration(probe, G);

			// 2. Calcolo Newtoniano diretto per confronto
			Vector3D directAcc = Vector3D.Zero;
			double distSq = Math.Pow(1e11, 2);
			double totalMass = 3e24;
			double force = (G * totalMass) / distSq;
			directAcc = new Vector3D(-force, 0, 0); // Attrazione verso l'origine

			// ASSERT
			// La differenza tra calcolo puntuale e approssimazione Octree deve essere minima (< 1%)
			double precision = 0.01;
			Assert.AreEqual(directAcc.X, octreeAcc.X, Math.Abs(directAcc.X * precision), "L'approssimazione Barnes-Hut è fuori tolleranza.");
		}
	}
}