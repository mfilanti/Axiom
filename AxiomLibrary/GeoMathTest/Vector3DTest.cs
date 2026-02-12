using Axiom.GeoMath;

namespace Axiom.GeoMathTest;

[TestClass]
public class Vector3DTest
{
    [TestMethod]
    public void TestCtor()
    {
        var v = new Vector3D(1.0, 2.0, 3.0);
        Assert.AreEqual(1.0, v.X);
        Assert.AreEqual(2.0, v.Y);
        Assert.AreEqual(3.0, v.Z);
        Assert.AreEqual(v.Norm, v.Length);
        Assert.AreEqual(14, v.LengthSquared);
    }

    [TestMethod]
    public void TestIndexer()
    {
        var v = new Vector3D(1, 2, 3);
        Assert.AreEqual(1, v[0]);
        Assert.AreEqual(2, v[1]);
        Assert.AreEqual(3, v[2]);
        v[1] = 5;
        Assert.AreEqual(5, v.Y);
    }

    [TestMethod]
    public void TestEqualsAndNormalize()
    {
        var v1 = new Vector3D(1, 2, 3);
        var v2 = new Vector3D(1.000000001, 2.000000001, 3.000000001);
        Assert.IsTrue(v1.IsEquals(v2));
        Assert.IsTrue(v1.IsEquals(v2, 0.001));

        var normalized = v1.Normalize();
        Assert.IsTrue(normalized.Length.IsEquals(1));

        var mutable = new Vector3D(3, 0, 0);
        var length = mutable.SetNormalize();
        Assert.AreEqual(3, length);
        Assert.IsTrue(mutable.IsEquals(Vector3D.UnitX));

        var zero = new Vector3D(0, 0, 0);
        var zeroLength = zero.SetNormalize();
        Assert.AreEqual(0, zeroLength);
        Assert.IsTrue(zero.IsEquals(Vector3D.Zero));
    }

    [TestMethod]
    public void TestNormalizeThrowsOnZero()
    {
        Assert.ThrowsException<InvalidOperationException>(() => Vector3D.Zero.Normalize());
    }

    [TestMethod]
    public void TestSetNegateAndNegate()
    {
        var v = new Vector3D(1, -2, 3);
        v.SetNegate();
        Assert.IsTrue(v.IsEquals(new Vector3D(-1, 2, -3)));
        Assert.IsTrue(v.Negate().IsEquals(new Vector3D(1, -2, 3)));
    }

    [TestMethod]
    public void TestDotCrossAndPerpendicular()
    {
        var v1 = new Vector3D(1, 0, 0);
        var v2 = new Vector3D(0, 1, 0);
        Assert.AreEqual(0, v1.Dot(v2));
        Assert.IsTrue(v1.Cross(v2).IsEquals(Vector3D.UnitZ));
        Assert.IsTrue(Vector3D.UnitX.Perpendicular().IsEquals(Vector3D.UnitY));
        Assert.IsTrue(Vector3D.NegativeUnitX.Perpendicular().IsEquals(Vector3D.NegativeUnitY));
        var perpendicular = new Vector3D(1, 1, 0).Perpendicular();
        Assert.IsTrue(perpendicular.Length.IsEquals(1));
        Assert.IsTrue(perpendicular.Dot(new Vector3D(1, 1, 0)).IsEquals(0));
    }

    [TestMethod]
    public void TestParallelAndAngles()
    {
        Assert.IsTrue(new Vector3D(2, 0, 0).IsParallel(Vector3D.UnitX));
        Assert.IsFalse(new Vector3D(1, 0, 0).IsParallel(Vector3D.UnitY));
        Assert.IsTrue(new Vector3D(1, 0, 0).ApproxEqualsInnerAngle(new Vector3D(2, 0, 0)));
        Assert.AreEqual(Math.PI / 2, Vector3D.UnitX.Angle(Vector3D.UnitY), 1e-10);
        Assert.AreEqual(0, Vector3D.UnitZ.Angle(), 1e-10);
        Assert.AreEqual(Math.PI / 2, Vector3D.UnitY.Angle(Vector3D.UnitX, Vector3D.UnitZ), 1e-10);
    }

    [TestMethod]
    public void TestRotateAndSlerp()
    {
        var rotated = Vector3D.UnitX.Rotate(Vector3D.UnitZ, Math.PI / 2);
        Assert.IsTrue(rotated.IsEquals(Vector3D.UnitY, 1e-10));

        var slerpSame = Vector3D.UnitX.Slerp(Vector3D.UnitX, 0.5, Vector3D.UnitZ);
        Assert.IsTrue(slerpSame.IsEquals(Vector3D.UnitX));

        var slerpQuarter = Vector3D.UnitX.Slerp(Vector3D.UnitY, 0.5, Vector3D.UnitZ);
        Assert.IsTrue(slerpQuarter.IsParallel(new Vector3D(1, 1, 0)));

        Assert.ThrowsException<InvalidOperationException>(() =>
            Vector3D.UnitX.Slerp(Vector3D.NegativeUnitX, 0.5, Vector3D.UnitZ));
    }

    [TestMethod]
    public void TestOperatorsAndConversions()
    {
        Vector3D fromPoint = new Point3D(1, 2, 3);
        Assert.IsTrue(fromPoint.IsEquals(new Vector3D(1, 2, 3)));

        var v = new Vector3D(1, 2, 3);
        Assert.IsTrue((v + new Vector3D(1, 1, 1)).IsEquals(new Vector3D(2, 3, 4)));
        Assert.IsTrue((v - new Vector3D(1, 1, 1)).IsEquals(new Vector3D(0, 1, 2)));
        Assert.IsTrue((-v).IsEquals(new Vector3D(-1, -2, -3)));
        Assert.IsTrue((v * 2).IsEquals(new Vector3D(2, 4, 6)));
        Assert.IsTrue((2 * v).IsEquals(new Vector3D(2, 4, 6)));
        Assert.IsTrue((v / 2).IsEquals(new Vector3D(0.5, 1, 1.5)));

        Assert.AreEqual("(1, 2, 3)", v.ToString());
    }
}
