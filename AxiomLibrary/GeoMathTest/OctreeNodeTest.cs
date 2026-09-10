using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using System.Linq;

namespace Axiom.GeoMathTest;

[TestClass]
public class OctreeNodeTest
{
    private sealed class WeightedPoint : IPointWeighted
    {
        public WeightedPoint(Vector3D position, double weight)
        {
            Position = position;
            Weight = weight;
        }

        public Vector3D Position { get; }
        public double Weight { get; }
        public Vector3D ZVector => Vector3D.UnitZ;
        public Vector3D YVector => Vector3D.UnitY;
        public Vector3D XVector => Vector3D.UnitX;
    }

    [TestMethod]
    public void TestInsertAndWeightedCenter()
    {
        var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var node = new OctreeNode<WeightedPoint>(boundary);

        var p1 = new WeightedPoint(new Vector3D(1, 1, 1), 2);
        var p2 = new WeightedPoint(new Vector3D(3, 1, 1), 1);
        node.Insert(p1);
        node.Insert(p2);

        Assert.IsTrue(node.IsLeaf);
        Assert.AreEqual(3, node.TotalWeight);

        var expectedCenter = (p1.Position * p1.Weight + p2.Position * p2.Weight) / node.TotalWeight;
        Assert.IsTrue(node.WeightedCenter.IsEquals(expectedCenter));
    }

    [TestMethod]
    public void TestSubdivideAndChildren()
    {
        var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(8, 8, 8));
        var node = new OctreeNode<WeightedPoint>(boundary);

        for (var i = 0; i < 5; i++)
        {
            node.Insert(new WeightedPoint(new Vector3D(i + 1, i + 1, i + 1), 1));
        }

        Assert.IsFalse(node.IsLeaf);
        Assert.IsNotNull(node.GetChildren());
        Assert.AreEqual(8, node.GetChildren().Length);
        Assert.AreEqual(0, node.GetEntries().Count());
    }

    [TestMethod]
    public void TestInsertOutsideBoundary()
    {
        var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var node = new OctreeNode<WeightedPoint>(boundary);
        node.Insert(new WeightedPoint(new Vector3D(5, 5, 5), 1));
        Assert.AreEqual(0, node.TotalWeight);
    }

    [TestMethod]
    public void TestGetEntitiesInRange()
    {
        var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var node = new OctreeNode<WeightedPoint>(boundary);
        var near = new WeightedPoint(new Vector3D(1, 1, 1), 1);
        var far = new WeightedPoint(new Vector3D(9, 9, 9), 1);

        node.Insert(near);
        node.Insert(far);

        var results = new List<WeightedPoint>();
        node.GetEntitiesInRange(new Point3D(0, 0, 0), 3, results);

        Assert.AreEqual(1, results.Count);
        Assert.AreSame(near, results[0]);
    }

    [TestMethod]
    public void TestGetEntitiesInRangeFromChildrenAndNoIntersection()
    {
        var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(8, 8, 8));
        var node = new OctreeNode<WeightedPoint>(boundary);
        for (var i = 0; i < 6; i++)
        {
            node.Insert(new WeightedPoint(new Vector3D(i + 1, i + 1, i + 1), 1));
        }

        var results = new List<WeightedPoint>();
        node.GetEntitiesInRange(new Point3D(2, 2, 2), 5, results);
        Assert.IsTrue(results.Count > 0);

        results.Clear();
        node.GetEntitiesInRange(new Point3D(100, 100, 100), 1, results);
        Assert.AreEqual(0, results.Count);
    }

    /// <summary>
    /// Regressione BUG-02: più di MaxEntries voci con la STESSA posizione non devono provocare
    /// ricorsione infinita (StackOverflow). Devono essere accumulate e restare interrogabili.
    /// </summary>
    [TestMethod]
    public void TestCoincidentPoints_DoNotCauseInfiniteRecursion()
    {
        var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var node = new OctreeNode<WeightedPoint>(boundary);

        const int count = 10; // > MaxEntries (4), tutte coincidenti
        var inserted = new List<WeightedPoint>();
        for (var i = 0; i < count; i++)
        {
            var p = new WeightedPoint(new Vector3D(5, 5, 5), 1);
            inserted.Add(p);
            node.Insert(p); // NON deve mandare in StackOverflow
        }

        Assert.AreEqual(count, node.TotalWeight, "Il peso totale non corrisponde al numero di voci inserite.");

        var results = new List<WeightedPoint>();
        node.GetEntitiesInRange(new Point3D(5, 5, 5), 1, results);

        Assert.AreEqual(count, results.Count, "Non tutte le voci coincidenti sono state trovate.");
        Assert.AreEqual(count, results.Distinct().Count(), "Alcune voci risultano duplicate.");
    }

    /// <summary>
    /// Regressione BUG-03: le voci che giacciono esattamente sui piani di suddivisione non devono
    /// essere inserite in più ottanti (doppio conteggio del peso / duplicati nelle query).
    /// </summary>
    [TestMethod]
    public void TestPointsOnSplitPlane_AreNotDuplicated()
    {
        var boundary = new AABBox3D(new Point3D(0, 0, 0), new Point3D(8, 8, 8)); // mid = (4,4,4)
        var node = new OctreeNode<WeightedPoint>(boundary);

        var pts = new List<WeightedPoint>
        {
            new WeightedPoint(new Vector3D(4, 1, 1), 1), // sul piano x = 4
            new WeightedPoint(new Vector3D(4, 7, 7), 1), // sul piano x = 4
            new WeightedPoint(new Vector3D(1, 4, 1), 1), // sul piano y = 4
            new WeightedPoint(new Vector3D(1, 1, 1), 1),
            new WeightedPoint(new Vector3D(7, 7, 7), 1), // il quinto forza la suddivisione
        };
        foreach (var p in pts) node.Insert(p);

        Assert.IsFalse(node.IsLeaf, "Il nodo dovrebbe essersi suddiviso.");
        Assert.AreEqual(pts.Count, node.TotalWeight, "Peso totale errato: possibile doppio conteggio sui piani di split.");

        var results = new List<WeightedPoint>();
        node.GetEntitiesInRange(new Point3D(4, 4, 4), 20, results); // il raggio copre l'intero box

        Assert.AreEqual(pts.Count, results.Count, "Alcune voci risultano duplicate nella query di range.");
        Assert.AreEqual(pts.Count, results.Distinct().Count(), "Presenza di duplicati tra i risultati.");
    }
}
