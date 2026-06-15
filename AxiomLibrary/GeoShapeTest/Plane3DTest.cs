using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;

namespace GeoShapeTest;

[TestClass]
public class Plane3DTest
{
    [TestMethod]
    public void TestResetXAxisAndEquals()
    {
        var plane = new Plane3D(Vector3D.UnitX, Point3D.Zero);
        Assert.IsTrue(plane.XAxis.IsEquals(Vector3D.UnitY));

        var negativePlane = new Plane3D(Vector3D.NegativeUnitX, Point3D.Zero);
        Assert.IsTrue(negativePlane.XAxis.IsEquals(Vector3D.NegativeUnitY));

        var custom = new Plane3D(Vector3D.UnitZ, Vector3D.UnitX, new Point3D(0, 0, 1));
        Assert.IsTrue(custom.Equals(custom));
        Assert.IsFalse(custom.Equals(new Plane3D(Vector3D.UnitZ, Vector3D.UnitX, new Point3D(0, 0, 1))));
        Assert.IsTrue(custom.Normal.IsEquals(Vector3D.UnitZ));
        Assert.IsTrue(custom.XAxis.IsEquals(Vector3D.UnitX));
        Assert.IsTrue(custom.Location.IsEquals(new Point3D(0, 0, 1)));
        Assert.IsFalse(custom.Equals(new object()));
    }

    [TestMethod]
    public void TestIntersectLineAndRay()
    {
        var plane = new Plane3D(Vector3D.UnitZ, new Point3D(0, 0, 0));
        var line = new Line3D(new Point3D(0, 0, -1), new Point3D(0, 0, 1));
        var hasIntersection = plane.IntersectLine(line, out var insideLine, out var intersection);
        Assert.IsTrue(hasIntersection);
        Assert.IsTrue(insideLine);
        Assert.IsTrue(intersection.IsEquals(Point3D.Zero));

        var parallelLine = new Line3D(new Point3D(0, 0, 1), new Point3D(1, 0, 1));
        Assert.IsFalse(plane.IntersectLine(parallelLine, out _, out _));

        var ray = new Ray3D(new Point3D(0, 0, -1), Vector3D.UnitZ);
        Assert.IsTrue(plane.IntersectRay(ray, out var rayIntersection));
        Assert.IsTrue(rayIntersection.IsEquals(Point3D.Zero));

        var awayRay = new Ray3D(new Point3D(0, 0, 1), Vector3D.UnitZ);
        Assert.IsFalse(plane.IntersectRay(awayRay, out _));
    }

    [TestMethod]
    public void TestIntersectTriangle()
    {
        var plane = new Plane3D(Vector3D.UnitZ, new Point3D(0, 0, 0));
        var triangle = new Triangle3D(new Point3D(0, 0, -1), new Point3D(1, 0, 1), new Point3D(0, 1, 1));
        var intersects = plane.IntersectTriangle(triangle, out var intersection);
        Assert.IsTrue(intersects);
        Assert.IsNotNull(intersection);

        var parallelPlane = new Plane3D(Vector3D.UnitZ, new Point3D(0, 0, 10));
        Assert.IsFalse(parallelPlane.IntersectTriangle(triangle, out _));
    }
}
