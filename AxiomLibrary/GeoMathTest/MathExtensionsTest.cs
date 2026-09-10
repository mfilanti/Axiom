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

    /// <summary>
    /// Regressione BUG-10: il "punto nullo" nella libreria è il sentinella Point3D.NullPoint
    /// (coordinate NaN), non un riferimento null. IsNull/IsNotNull devono riconoscerlo.
    /// In precedenza IsNull controllava solo la nullità del riferimento, quindi trattava il
    /// sentinella NaN come "non nullo", causando propagazione di NaN (es. Spline3D).
    /// </summary>
    [TestMethod]
    public void TestNullPointSentinelIsRecognizedAsNull()
    {
        Point3D nullPoint = Point3D.NullPoint; // (NaN, NaN, NaN)
        Assert.IsTrue(nullPoint.IsNull(), "Il sentinella NullPoint deve risultare 'nullo'.");
        Assert.IsFalse(nullPoint.IsNotNull(), "Il sentinella NullPoint non deve risultare 'non nullo'.");

        Point3D realPoint = new Point3D(1, 2, 3);
        Assert.IsFalse(realPoint.IsNull(), "Un punto valido non deve risultare 'nullo'.");
        Assert.IsTrue(realPoint.IsNotNull(), "Un punto valido deve risultare 'non nullo'.");
    }
}
