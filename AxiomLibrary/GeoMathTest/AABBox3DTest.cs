using Axiom.GeoMath;

namespace Axiom.GeoMathTest;

[TestClass]
public class AABBox3DTest
{
    [TestMethod]
    public void TestProperties()
    {
        var box = new AABBox3D(new Point3D(0, 0, 0), new Point3D(2, 4, 6));
        Assert.AreEqual(2, box.LX);
        Assert.AreEqual(4, box.LY);
        Assert.AreEqual(6, box.LZ);
        Assert.AreEqual(6, box.MaxSide);
        Assert.AreEqual(2, box.MinSide);
        Assert.IsTrue(box.Center.IsEquals(new Point3D(1, 2, 3)));
        Assert.AreEqual(88, box.Area);
        Assert.AreEqual(48, box.Volume);
        Assert.AreEqual(8, box.Points.Count);
        Assert.IsTrue(box.Points[0].IsEquals(box.MinPoint));
        Assert.IsTrue(box.Points[4].IsEquals(box.MaxPoint));
    }

    [TestMethod]
    public void TestMoveUnionAndEnlarge()
    {
        var box = new AABBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        box.Move(new Vector3D(1, 2, 3));
        Assert.IsTrue(box.MinPoint.IsEquals(new Point3D(1, 2, 3)));
        Assert.IsTrue(box.MaxPoint.IsEquals(new Point3D(2, 3, 4)));

        var unionBox = new AABBox3D(new Point3D(-1, -1, -1), new Point3D(0.5, 0.5, 0.5));
        box.Union(unionBox);
        Assert.IsTrue(box.MinPoint.IsEquals(new Point3D(-1, -1, -1)));

        box.EnlargeByPoint(new Point3D(3, 0, 0));
        Assert.AreEqual(3, box.MaxPoint.X);

        box.Enlarge(1, 2, 3);
        Assert.IsTrue(box.MinPoint.IsEquals(new Point3D(-2, -3, -4)));

        box = new AABBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        box.Enlarge(2);
        Assert.IsTrue(box.MinPoint.IsEquals(new Point3D(-1, -1, -1)));
        Assert.IsTrue(box.MaxPoint.IsEquals(new Point3D(3, 3, 3)));
    }

    [TestMethod]
    public void TestIntersectAndContains()
    {
        var boxA = new AABBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        var boxB = new AABBox3D(new Point3D(1, 1, 1), new Point3D(3, 3, 3));
        Assert.IsTrue(boxA.Intersect(boxB));
        Assert.IsTrue(boxA.Intersect(boxB, out var intersection));
        Assert.IsNotNull(intersection);
        Assert.IsTrue(intersection.MinPoint.IsEquals(new Point3D(1, 1, 1)));
        Assert.IsTrue(intersection.MaxPoint.IsEquals(new Point3D(2, 2, 2)));

        var boxC = new AABBox3D(new Point3D(3, 3, 3), new Point3D(4, 4, 4));
        Assert.IsFalse(boxA.Intersect(boxC));

        Assert.IsTrue(boxA.Contains(new Point3D(1, 1, 1)));
        Assert.IsFalse(boxA.Contains(new Point3D(5, 5, 5)));
        Assert.IsTrue(boxB.Contains(new AABBox3D(new Point3D(1.5, 1.5, 1.5), new Point3D(2, 2, 2))));
    }

    [TestMethod]
    public void TestSphereIntersection()
    {
        var box = new AABBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        Assert.IsTrue(box.IntersectsSphere(new Point3D(1, 1, 1), 0.5));
        Assert.IsFalse(box.IntersectsSphere(new Point3D(5, 5, 5), 1));
    }

    [TestMethod]
    public void TestApproxEqualsCloneAndNull()
    {
        var box = new AABBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var clone = box.Clone();
        Assert.IsTrue(box.ApproxEquals(clone));
        Assert.IsTrue(box.ApproxEquals(clone, 0.001));
        Assert.IsFalse(box.ApproxEquals(new AABBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2)), 0.0001));

        var nullBox = AABBox3D.NullAABBox;
        Assert.IsTrue(nullBox.IsNullAABBox());
        Assert.IsFalse(box.IsNullAABBox());

        var fromPoints = AABBox3D.FromPoints(new[]
        {
            new Point3D(0, 0, 0),
            new Point3D(2, 2, 2),
            new Point3D(-1, 1, 3)
        });
        Assert.IsTrue(fromPoints.MinPoint.IsEquals(new Point3D(-1, 0, 0)));
        Assert.IsTrue(fromPoints.MaxPoint.IsEquals(new Point3D(2, 2, 3)));
    }
}
