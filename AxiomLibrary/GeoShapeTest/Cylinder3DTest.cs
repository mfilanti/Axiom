using Axiom.GeoMath;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class Cylinder3DTest
{
    [TestMethod]
    public void TestCloneAndAabbox()
    {
        var cylinder = new Cylinder3D(1, 2);
        var clone = cylinder.Clone() as Cylinder3D;
        Assert.IsNotNull(clone);
        Assert.AreEqual(1, clone.Radius);
        Assert.AreEqual(2, clone.Height);

        // Regressione BUG-09: GetAABBox() andava in StackOverflow (ricorsione infinita in
        // Arc3D.IsOnCurve). Ora deve restituire il bounding box corretto del cilindro
        // (raggio 1, altezza 2, con matrice identità): [-1,-1,0] .. [1,1,2].
        var aabb = cylinder.GetAABBox();
        Assert.IsTrue(aabb.MinPoint.IsEquals(new Point3D(-1, -1, 0), 1e-6), $"MinPoint inatteso: {aabb.MinPoint}");
        Assert.IsTrue(aabb.MaxPoint.IsEquals(new Point3D(1, 1, 2), 1e-6), $"MaxPoint inatteso: {aabb.MaxPoint}");
    }
}
