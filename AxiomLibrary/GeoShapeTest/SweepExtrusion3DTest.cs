using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using Axiom.GeoShape.Shapes;

namespace GeoShapeTest;

[TestClass]
public class SweepExtrusion3DTest
{
    [TestMethod]
    public void TestCloneAndAabbox()
    {
        var figure = new Figure3D();
        figure.AddPolygon(new Point3D(0, 0), new Point3D(1, 0), new Point3D(1, 1), new Point3D(0, 1), new Point3D(0, 0));
        var shape = new Shape2DCustom(figure);
        var path = new Figure3D(new Point3D(0, 0, 0), new Point3D(0, 0, 5));
        var sweep = new SweepExtrusion3D(shape, path, null, null);

        var clone = sweep.Clone() as SweepExtrusion3D;
        Assert.IsNotNull(clone);

        var box = sweep.GetAABBox();
        Assert.IsTrue(box.MaxPoint.Z >= 5);
    }
}
