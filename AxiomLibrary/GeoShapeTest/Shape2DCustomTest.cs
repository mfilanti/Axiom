using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Shapes;

namespace GeoShapeTest;

[TestClass]
public class Shape2DCustomTest
{
    [TestMethod]
    public void TestCanEmptyAndClone()
    {
        var openFigure = new Figure3D(new Point3D(0, 0), new Point3D(1, 0));
        var shape = new Shape2DCustom(openFigure);
        Assert.IsFalse(shape.CanEmpty);
        Assert.IsTrue(shape.Validate());

        shape.InverseVersus = true;
        var inverse = shape.GetFigure();
        Assert.IsTrue(inverse.StartPoint.IsEquals(openFigure.EndPoint));

        var clone = shape.CloneShape();
        Assert.IsInstanceOfType(clone, typeof(Shape2DCustom));
        Assert.AreEqual(shape.GetFigure().Count, clone.GetFigure().Count);
    }

    [TestMethod]
    public void TestClosedFigureCanEmpty()
    {
        var figure = new Figure3D();
        figure.AddPolygon(new Point3D(0, 0), new Point3D(1, 0), new Point3D(1, 1), new Point3D(0, 1), new Point3D(0, 0));
        var shape = new Shape2DCustom(figure);
        Assert.IsTrue(shape.CanEmpty);
    }
}
