using Axiom.GeoMath;
using Axiom.GeoShape.Elements;

namespace GeoShapeTest;

[TestClass]
public class Triangle3DTest
{
    [TestMethod]
    public void TestConstructorsAndProperties()
    {
        var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
        Assert.IsTrue(triangle.Normal.IsEquals(Vector3D.UnitZ));
        Assert.IsTrue(triangle.Center.IsEquals(new Point3D(1.0 / 3.0, 1.0 / 3.0, 0)));
        Assert.IsTrue(triangle.Area.IsEquals(0.5));

        var defaultTriangle = new Triangle3D();
        Assert.IsTrue(defaultTriangle.P1.IsEquals(Point3D.Zero));
    }

    [TestMethod]
    public void TestEqualsAndIsEquals()
    {
        var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
        Assert.IsTrue(triangle.Equals(new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0))));
        Assert.IsFalse(triangle.Equals(new object()));

        var comparison = new Triangle3D(new Point3D(1, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
        Assert.IsFalse(triangle.IsEquals(comparison));
        Assert.IsFalse(triangle.IsEquals(comparison, 0.001));
    }

    [TestMethod]
    public void TestApplyRtAndAabbox()
    {
        var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
        var matrix = RTMatrix.FromTraslation(new Vector3D(1, 2, 3));
        triangle.ApplyRT(matrix);
        Assert.IsTrue(triangle.P1.IsEquals(new Point3D(1, 2, 3)));

        var box = triangle.GetAABBox();
        Assert.IsTrue(box.MinPoint.IsEquals(new Point3D(1, 2, 3)));
    }

    [TestMethod]
    public void TestSubdivideNoIntersection()
    {
        var triangle = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
        var plane = new Plane3D(Vector3D.UnitZ, new Point3D(0, 0, 10));
        var result = triangle.Subdivide(plane);
        Assert.AreEqual(1, result.Count);
    }
}
