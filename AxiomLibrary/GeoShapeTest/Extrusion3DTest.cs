using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using Axiom.GeoShape.Shapes;

namespace GeoShapeTest;

[TestClass]
public class Extrusion3DTest
{
    [TestMethod]
    public void TestCloneAndAabbox()
    {
        var figure = new Figure3D();
        figure.AddPolygon(new Point3D(0, 0), new Point3D(1, 0), new Point3D(1, 1), new Point3D(0, 1), new Point3D(0, 0));
        var shape = new Shape2DCustom(figure);
        var extrusion = new Extrusion3D(shape, Vector3D.UnitZ, 5, null, null, "");

        var clone = extrusion.Clone() as Extrusion3D;
        Assert.IsNotNull(clone);
        Assert.AreEqual(5, clone.Length);

        var box = extrusion.GetAABBox();
        Assert.IsTrue(box.MaxPoint.Z >= 5);
    }
}
