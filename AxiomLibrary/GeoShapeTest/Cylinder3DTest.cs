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

        Assert.ThrowsException<InvalidOperationException>(() => cylinder.GetAABBox());
    }
}
