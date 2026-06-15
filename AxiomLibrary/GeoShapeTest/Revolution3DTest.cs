using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using Axiom.GeoShape.Shapes;

namespace GeoShapeTest;

[TestClass]
public class Revolution3DTest
{
    [TestMethod]
    public void TestCloneAndAabbox()
    {
        var figure = new Figure3D(new Point3D(1, 0), new Point3D(1, 2));
        var shape = new Shape2DCustom(figure);
        var revolution = new Revolution3D(shape);

        var clone = revolution.Clone() as Revolution3D;
        Assert.IsNotNull(clone);

        var box = revolution.GetAABBox();
        Assert.IsTrue(box.MaxPoint.X > box.MinPoint.X);
    }
}
