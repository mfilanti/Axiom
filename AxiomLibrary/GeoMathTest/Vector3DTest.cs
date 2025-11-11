namespace Axiom.GeoMathTest;

[TestClass]
public class Vector3DTest
{
    [TestMethod]
    public void TestCtor()
    {
        var v = new Axiom.GeoMath.Vector3D(1.0, 2.0, 3.0);
        Assert.AreEqual(1.0, v.X);
        Assert.AreEqual(2.0, v.Y);
        Assert.AreEqual(3.0, v.Z);
	}
}
