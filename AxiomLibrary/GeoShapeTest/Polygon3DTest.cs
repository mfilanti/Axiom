using Axiom.GeoMath;
using Axiom.GeoShape.Elements;

namespace GeoShapeTest;

[TestClass]
public class Polygon3DTest
{
    private static Polygon3D UnitSquareCcw() => new Polygon3D(new[]
    {
        new Point3D(0, 0, 0),
        new Point3D(1, 0, 0),
        new Point3D(1, 1, 0),
        new Point3D(0, 1, 0),
    });

    [TestMethod]
    public void TestAreaAndOrientation()
    {
        var square = UnitSquareCcw();
        Assert.IsTrue(square.Area().IsEquals(1), $"Area attesa 1, ottenuta {square.Area()}");
        Assert.IsTrue(square.IsCounterClockWise(), "Il quadrato dovrebbe essere antiorario.");

        square.Inverse();
        Assert.IsFalse(square.IsCounterClockWise(), "Dopo Inverse dovrebbe essere orario.");
        Assert.IsTrue(square.Area().IsEquals(-1), $"Area dopo Inverse attesa -1, ottenuta {square.Area()}");
    }

    [TestMethod]
    public void TestBarycenterVerticesAndClone()
    {
        var square = UnitSquareCcw();

        Assert.IsTrue(square.GetBarycenter().IsEquals(new Point3D(0.5, 0.5, 0)), $"Baricentro inatteso: {square.GetBarycenter()}");
        Assert.AreEqual(4, square.Vertices.Count);
        Assert.AreEqual(1, square.VertexIndex(new Point3D(1, 0, 0)));

        // NextPoint / PreviousPoint (ciclici)
        Assert.IsTrue(square.NextPoint(0).IsEquals(new Point3D(1, 0, 0)));
        Assert.IsTrue(square.PreviousPoint(0).IsEquals(new Point3D(0, 1, 0)));

        var clone = square.Clone();
        Assert.AreEqual(square.Vertices.Count, clone.Vertices.Count);
        Assert.IsTrue(clone.Vertices[2].IsEquals(new Point3D(1, 1, 0)));
    }

    [TestMethod]
    public void TestToFigure()
    {
        var figure = UnitSquareCcw().ToFigure();
        Assert.IsNotNull(figure);
        Assert.IsTrue(figure.Count > 0, "La figura del poligono deve contenere curve.");
        Assert.IsTrue(figure.IsClosed(), "La figura di un poligono deve essere chiusa.");
    }
}
