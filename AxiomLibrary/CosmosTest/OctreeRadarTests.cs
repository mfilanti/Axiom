using Axiom.Cosmos.Models;
using Axiom.Cosmos.Utils;
using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CosmosTest
{
	[TestClass]
	public class OctreeRadarTests
	{
		[TestMethod]
		public void RadarQuery_ShouldReturnOnlyBodiesWithinRadius()
		{
			// 1. ARRANGE: Setup dell'ambiente
			// Creiamo un box di 200 metri centrato nell'origine
			var bounds = new AABBox3D(new Point3D(-100, -100, -100), new Point3D(100, 100, 100));
			var cosmosOctree = new CosmosOctreeNode(bounds);

			// Aggiungiamo corpi a distanze diverse
			var alpha = new Planet("Alpha", 1e10, 1, new Vector3D(10, 0, 0));   // Molto vicino (10m)
			var beta = new Planet("Beta", 1e10, 1, new Vector3D(40, 0, 0));    // Nel raggio (40m)
			var gamma = new Planet("Gamma", 1e10, 1, new Vector3D(90, 0, 0));  // Fuori raggio (90m)

			cosmosOctree.Insert(alpha);
			cosmosOctree.Insert(beta);
			cosmosOctree.Insert(gamma);

			// Centro del radar (origine) e raggio di 50 metri
			Point3D radarPosition = new Point3D(0, 0, 0);
			double radarRadius = 50.0;

			// 2. ACT: Esecuzione della query
			List<CelestialBody> detectedBodies = cosmosOctree.QueryRange(radarPosition, radarRadius);

			// 3. ASSERT: Verifica dei risultati
			Assert.HasCount(2, detectedBodies, "Il radar avrebbe dovuto rilevare esattamente 2 corpi.");
			Assert.IsTrue(detectedBodies.Any(b => b.Name == "Alpha"), "Alpha dovrebbe essere rilevato.");
			Assert.IsTrue(detectedBodies.Any(b => b.Name == "Beta"), "Beta dovrebbe essere rilevato.");
			Assert.IsFalse(detectedBodies.Any(b => b.Name == "Gamma"), "Gamma non dovrebbe essere rilevato (fuori raggio).");
		}

		[TestMethod]
		public void RadarQuery_WhenEmpty_ShouldReturnZero()
		{
			var bounds = new AABBox3D(new Point3D(-100, -100, -100), new Point3D(100, 100, 100));
			var cosmosOctree = new CosmosOctreeNode(bounds);

			var detected = cosmosOctree.QueryRange(new Point3D(0, 0, 0), 10);

			Assert.IsEmpty(detected);
		}
	}
}
