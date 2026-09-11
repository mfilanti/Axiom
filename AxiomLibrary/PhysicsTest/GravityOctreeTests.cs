using Axiom.GeoMath;
using Axiom.Physics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axiom.Physics.Tests
{
	[TestClass]
	public class GravityOctreeTests
	{
		private static GravityOctree<TestBody> NewOctree(double size = 100)
			=> new GravityOctree<TestBody>(new AABBox3D(new Point3D(0, 0, 0), new Point3D(size, size, size)));

		[TestMethod]
		public void TotalWeightAndCenterOfMass_AreWeightedAverage()
		{
			var octree = NewOctree();
			octree.Insert(new TestBody(100, new Vector3D(0, 0, 0)));
			octree.Insert(new TestBody(300, new Vector3D(40, 0, 0)));

			// (100·0 + 300·40) / 400 = 30
			Assert.AreEqual(400, octree.TotalWeight, "massa totale");
			Assert.AreEqual(30, octree.WeightedCenter.X, 1e-3, "baricentro pesato X");
			Assert.AreEqual(0, octree.WeightedCenter.Y, 1e-9);
		}

		[TestMethod]
		public void Subdivide_Occurs_WhenMaxEntriesExceeded()
		{
			var octree = NewOctree();
			var bodies = new[]
			{
				new TestBody(10, new Vector3D(10, 10, 10)),
				new TestBody(10, new Vector3D(90, 10, 10)),
				new TestBody(10, new Vector3D(10, 90, 10)),
				new TestBody(10, new Vector3D(10, 10, 90)),
				new TestBody(10, new Vector3D(90, 90, 90)), // il quinto forza la suddivisione (MaxEntries = 4)
			};
			foreach (var b in bodies) octree.Insert(b);

			Assert.IsFalse(octree.IsLeaf, "il nodo radice dovrebbe essersi suddiviso");
			Assert.AreEqual(50, octree.TotalWeight, "massa totale invariata dopo la suddivisione");
		}

		[TestMethod]
		public void QueryRange_ReturnsOnlyBodiesWithinRadius()
		{
			var octree = new GravityOctree<TestBody>(new AABBox3D(new Point3D(-100, -100, -100), new Point3D(100, 100, 100)));
			var near = new TestBody(1e10, new Vector3D(10, 0, 0), "near");
			var edge = new TestBody(1e10, new Vector3D(40, 0, 0), "edge");
			var far = new TestBody(1e10, new Vector3D(90, 0, 0), "far");
			octree.Insert(near);
			octree.Insert(edge);
			octree.Insert(far);

			List<TestBody> hits = octree.QueryRange(new Point3D(0, 0, 0), 50);

			Assert.HasCount(2, hits);
			Assert.IsTrue(hits.Any(b => b.Name == "near"));
			Assert.IsTrue(hits.Any(b => b.Name == "edge"));
			Assert.IsFalse(hits.Any(b => b.Name == "far"));
		}

		[TestMethod]
		public void QueryRange_WhenEmpty_ReturnsEmpty()
		{
			Assert.IsEmpty(NewOctree().QueryRange(new Point3D(0, 0, 0), 10));
		}

		[TestMethod]
		public void BarnesHut_Acceleration_IsCloseToDirectGravity()
		{
			double g = 6.674e-11;
			var octree = new GravityOctree<TestBody>(
				new AABBox3D(new Point3D(-1e12, -1e12, -1e12), new Point3D(1e12, 1e12, 1e12)));

			// Ammasso compatto di 3 corpi vicino all'origine (si comporta come un punto di massa).
			octree.Insert(new TestBody(1e24, new Vector3D(0, 0, 0)));
			octree.Insert(new TestBody(1e24, new Vector3D(1000, 0, 0)));
			octree.Insert(new TestBody(1e24, new Vector3D(0, 1000, 0)));

			var probe = new TestBody(1000, new Vector3D(1e11, 0, 0), "probe");

			Vector3D octreeAcc = octree.GetAcceleration(probe, g);

			// Riferimento diretto: massa totale concentrata nell'origine.
			double directX = -(g * 3e24) / Math.Pow(1e11, 2); // attrazione verso -X
			Assert.AreEqual(directX, octreeAcc.X, Math.Abs(directX * 0.01), "Barnes-Hut fuori tolleranza (1%)");
		}
	}
}
