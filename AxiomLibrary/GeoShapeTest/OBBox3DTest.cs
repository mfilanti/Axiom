using Axiom.GeoMath;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class OBBox3DTest
{
    [TestMethod]
    public void TestAabboxAndPlanes()
    {
        var box = new OBBox3D(2, 4, 6);
        var aabb = box.GetAABBox();
        Assert.IsTrue(aabb.MinPoint.IsEquals(new Point3D(-1, -2, -3)));
        Assert.IsTrue(aabb.MaxPoint.IsEquals(new Point3D(1, 2, 3)));

        box.GetPlane(BoxFace.Top, out var plane);
        Assert.IsTrue(plane.Normal.IsEquals(Vector3D.UnitZ));

        box.GetPlane(BoxFace.Bottom, out plane, out var borders);
        Assert.IsTrue(borders.Count > 0);
    }
}
