using Axiom.GeoMath;

namespace Axiom.GeoMathTest;

[TestClass]
public class MathExtensionsTest
{
    [TestMethod]
    public void TestIsEquals()
    {
        Assert.IsTrue(1.0.IsEquals(1.0 + 1e-6));
        Assert.IsFalse(1.0.IsEquals(1.1, 1e-6));
    }

    [TestMethod]
    public void TestIsEqualsOrGreaterAndLesser()
    {
        Assert.IsTrue(1.0.IsEqualsOrGreater(1.0));
        Assert.IsTrue(1.0.IsEqualsOrLesser(1.0));
        Assert.IsTrue(2.0.IsEqualsOrGreater(1.0));
        Assert.IsTrue(0.5.IsEqualsOrLesser(1.0));
    }

    [TestMethod]
    public void TestAngleRanges()
    {
        Assert.AreEqual(Math.PI * 1.5, (-Math.PI / 2).AngleToRange02PI(), 1e-12);
        Assert.AreEqual(350, (-10.0).AngleToRange0360(), 1e-12);
    }

    [TestMethod]
    public void TestInternalAngle()
    {
        Assert.AreEqual(20, 10.0.InternalAngle(350.0), 1e-12);
        Assert.AreEqual(180, 0.0.InternalAngle(180.0), 1e-12);
    }

    [TestMethod]
    public void TestRoundToString()
    {
        Assert.AreEqual("1.23", 1.2345.RoundToString(2));
        Assert.AreEqual("2", 1.6.RoundToString(-1));
    }

    [TestMethod]
    public void TestPointNullExtensions()
    {
        Point3D point = null!;
        Assert.IsTrue(point.IsNull());
        point = new Point3D(1, 2, 3);
        Assert.IsTrue(point.IsNotNull());
    }
}
