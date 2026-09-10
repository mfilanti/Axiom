using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class Figure3DTest
{
    [TestMethod]
    public void TestConstructorsAndPolygon()
    {
        var figure = new Figure3D(new Point3D(0, 0), new Point3D(1, 0), new Point3D(1, 1));
        Assert.AreEqual(2, figure.Count);
        Assert.IsTrue(figure.StartPoint.IsEquals(new Point3D(0, 0)));
        Assert.IsTrue(figure.EndPoint.IsEquals(new Point3D(1, 1)));

        var invalid = new Figure3D();
        Assert.IsFalse(invalid.AddPolygon(0, 0, 0, 1));
        Assert.IsTrue(invalid.AddPolygon(0, 0, 0, 1, 0, 0));
    }

    [TestMethod]
    public void TestLoopAndInverse()
    {
        var figure = new Figure3D();
        figure.AddPolygon(new Point3D(0, 0), new Point3D(1, 0), new Point3D(1, 1), new Point3D(0, 1), new Point3D(0, 0));
        Assert.IsTrue(figure.IsClosed());
        Assert.IsTrue(figure.IsLoop());
        Assert.IsTrue(figure.IsClosedLoop());
        Assert.AreEqual(1, figure.Loops().Count);

        var inverse = figure.Inverse();
        Assert.IsTrue(inverse.StartPoint.IsEquals(figure.EndPoint));

        figure.SetInverse();
        Assert.IsTrue(figure.StartPoint.IsEquals(inverse.StartPoint));
    }

    [TestMethod]
    public void TestMoveAndDelete()
    {
        var figure = new Figure3D();
        figure.Add(new Line3D(new Point3D(0, 0), new Point3D(1, 0)));
        figure.Add(new Line3D(new Point3D(1, 0), new Point3D(1, 0)));
        figure.DeleteNulls();
        Assert.AreEqual(1, figure.Count);

        figure.Add(new Line3D(new Point3D(0, 0), new Point3D(1, 0)));
        figure.DeleteDuplicates();
        Assert.AreEqual(1, figure.Count);

        figure.Move(new Vector3D(1, 0, 0));
        Assert.IsTrue(figure.StartPoint.IsEquals(new Point3D(1, 0)));
    }
}
