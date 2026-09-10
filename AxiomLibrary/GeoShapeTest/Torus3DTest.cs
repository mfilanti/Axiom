using Axiom.GeoMath;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class Torus3DTest
{
    [TestMethod]
    public void TestCloneAndAabbox()
    {
        var torus = new Torus3D(10, 30);
        var clone = torus.Clone() as Torus3D;
        Assert.IsNotNull(clone);
        Assert.AreEqual(10, clone.InnerRadius);
        Assert.AreEqual(30, clone.OuterRadius);

        var box = torus.GetAABBox();
        Assert.IsTrue(box.MaxPoint.Z > box.MinPoint.Z);
    }
}
