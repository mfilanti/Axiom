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
}
