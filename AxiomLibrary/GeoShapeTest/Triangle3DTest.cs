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

    /// <summary>
    /// Robustezza: un triangolo degenere (punti collineari, area nulla) non deve lanciare eccezione.
    /// Normal restituisce Zero, IsDegenerate è true, Contains restituisce false senza NaN.
    /// </summary>
    [TestMethod]
    public void TestDegenerateTriangle()
    {
        // Tre punti collineari lungo X.
        var degenerate = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(2, 0, 0));

        Assert.IsTrue(degenerate.IsDegenerate, "Il triangolo collineare deve risultare degenere.");
        Assert.IsTrue(degenerate.Normal.IsZero(), "La normale di un triangolo degenere deve essere nulla (niente eccezione).");
        Assert.IsTrue(degenerate.Area.IsEquals(0), "L'area di un triangolo degenere è nulla.");
        // Contains non deve lanciare né restituire true (nessuna area contenente).
        Assert.IsFalse(degenerate.Contains(new Point3D(0.5, 0, 0), false));

        // Un triangolo valido non è degenere.
        var valid = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
        Assert.IsFalse(valid.IsDegenerate);
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
