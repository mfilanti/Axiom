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
        // Vector3D ha ora semantica a VALORE: due piani con stessa normale/asseX/location sono uguali
        // (in precedenza Normal/XAxis erano confrontati per riferimento -> istanze distinte diverse).
        Assert.IsTrue(custom.Equals(new Plane3D(Vector3D.UnitZ, Vector3D.UnitX, new Point3D(0, 0, 1))));
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

    /// <summary>
    /// Robustezza: costruire un piano da una normale nulla (es. Plane3D.ZeroPlane) NON deve lanciare
    /// eccezione (in passato Vector3D.Zero.Normalize() lanciava). Il piano risultante è "nullo".
    /// </summary>
    [TestMethod]
    public void TestZeroPlaneAndNullNormalDoNotThrow()
    {
        // In precedenza la sola valutazione di questa proprietà statica lanciava un'eccezione.
        var zero = Plane3D.ZeroPlane;
        Assert.IsTrue(zero.IsNull, "ZeroPlane deve risultare 'nullo'.");
        Assert.IsTrue(zero.Normal.IsZero());

        var fromZeroNormal = new Plane3D(Vector3D.Zero, new Point3D(1, 2, 3));
        Assert.IsTrue(fromZeroNormal.IsNull);

        // Un piano valido non è nullo.
        var valid = new Plane3D(Vector3D.UnitZ, Point3D.Zero);
        Assert.IsFalse(valid.IsNull);
    }

    /// <summary>
    /// Robustezza: FromPoints non deve lanciare (partiva da ZeroPlane, che lanciava) e deve
    /// restituire un piano valido per punti complanari, ZeroPlane per punti collineari/insufficienti.
    /// </summary>
    [TestMethod]
    public void TestFromPoints()
    {
        // Quattro punti complanari nel piano XY -> piano valido con normale lungo Z.
        var points = new List<Point3D>
        {
            new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 1, 0), new Point3D(0, 1, 0)
        };
        var plane = Plane3D.FromPoints(points);
        Assert.IsFalse(plane.IsNull, "Punti complanari devono dare un piano valido.");
        Assert.IsTrue(plane.Normal.IsParallel(Vector3D.UnitZ), $"Normale attesa lungo Z: {plane.Normal}");

        // Punti collineari -> piano nullo (nessuna eccezione).
        var collinear = new List<Point3D>
        {
            new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(2, 0, 0)
        };
        Assert.IsTrue(Plane3D.FromPoints(collinear).IsNull);
    }
}
