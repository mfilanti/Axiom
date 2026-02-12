using Axiom.GeoMath;

namespace Axiom.GeoMathTest;

[TestClass]
public class MathUtilsTest
{
    [TestMethod]
    public void TestDegreeRadConversion()
    {
        Assert.AreEqual(Math.PI, MathUtils.DegreeToRad(180), 1e-12);
        Assert.AreEqual(180, MathUtils.RadToDegree(Math.PI), 1e-12);
        Assert.IsTrue((MathUtils.RadToDeg * MathUtils.DegToRad).IsEquals(1));
    }

    [TestMethod]
    public void TestSwap()
    {
        var first = 1;
        var second = 2;
        MathUtils.Swap(ref first, ref second);
        Assert.AreEqual(2, first);
        Assert.AreEqual(1, second);
    }
}
