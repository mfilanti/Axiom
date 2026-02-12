using Axiom.GeoMath;

namespace Axiom.GeoMathTest;

[TestClass]
public class RTMatrixTest
{
    private static void AssertMatrixClose(RTMatrix left, RTMatrix right, double tolerance)
    {
        Assert.IsTrue(left.XVector.IsEquals(right.XVector, tolerance));
        Assert.IsTrue(left.YVector.IsEquals(right.YVector, tolerance));
        Assert.IsTrue(left.ZVector.IsEquals(right.ZVector, tolerance));
        Assert.IsTrue(left.Translation.IsEquals(right.Translation, tolerance));
    }

    [TestMethod]
    public void TestStatics()
    {
        Assert.IsTrue(RTMatrix.Identity.IsEquals(RTMatrix.FromEulerAnglesXYZ(0, 0, 0)));
        Assert.AreEqual(0, RTMatrix.Zero.Determinant);

        var fromZx = RTMatrix.FromEulerAnglesZX(0, 0);
        Assert.IsTrue(fromZx.IsEquals(RTMatrix.Identity));

        var x = new Vector3D(1, 0, 0);
        var y = new Vector3D(0, 1, 0);
        var z = new Vector3D(0, 0, 1);
        var t = new Vector3D(1, 2, 3);
        var fromVectors = RTMatrix.FromVectors(x, y, z, t);
        Assert.IsTrue(fromVectors.Translation.IsEquals(t));

        var fromVectorsNoTranslation = RTMatrix.FromVectors(x, y, z);
        Assert.IsTrue(fromVectorsNoTranslation.Translation.IsEquals(Vector3D.Zero));

        var fromTranslation = RTMatrix.FromTraslation(t);
        Assert.IsTrue(fromTranslation.Translation.IsEquals(t));

        var fromNormal = RTMatrix.FromNormal(Vector3D.UnitZ, t);
        Assert.IsTrue(fromNormal.ZVector.IsEquals(Vector3D.UnitZ));
        Assert.IsTrue(fromNormal.Translation.IsEquals(t));

        var fromNormalNoTranslation = RTMatrix.FromNormal(Vector3D.UnitZ);
        Assert.IsTrue(fromNormalNoTranslation.Translation.IsEquals(Vector3D.Zero));
    }

    [TestMethod]
    public void TestPropertiesAndIndexers()
    {
        var matrix = RTMatrix.Identity;
        matrix[0, 3] = 5;
        Assert.AreEqual(5, matrix[3]);
        Assert.AreEqual(5, matrix.TraslationX);

        var values = new double[16];
        values[0] = 2;
        values[15] = 1;
        matrix.Values = values;
        Assert.AreEqual(2, matrix[0]);
        Assert.AreEqual(1, matrix[15]);

        var readValues = matrix.Values;
        Assert.AreEqual(16, readValues.Length);
        Assert.AreEqual(2, readValues[0]);

        matrix.Values = null!;

        matrix.Values = new double[4];
        Assert.AreEqual(2, matrix[0]);

        matrix.Scale = new Vector3D(2, 3, 4);
        Assert.IsTrue(matrix.Scale.IsEquals(new Vector3D(2, 3, 4)));

        matrix.Translation = new Vector3D(1, 2, 3);
        Assert.IsTrue(matrix.Translation.IsEquals(new Vector3D(1, 2, 3)));
        Assert.AreEqual(2, matrix.TraslationY);
        Assert.AreEqual(3, matrix.TraslationZ);

        for (var i = 0; i < 16; i++)
        {
            matrix[i] = i + 1;
            Assert.AreEqual(i + 1, matrix[i]);
        }

        Assert.AreEqual(0, matrix[99]);
        var before = matrix[0];
        matrix[99] = 123;
        Assert.AreEqual(before, matrix[0]);

        Assert.IsTrue(matrix.XVector.IsEquals(new Vector3D(1, 5, 9)));
        matrix.YVector = new Vector3D(0, 5, 0);
        Assert.IsTrue(matrix.YVector.IsEquals(new Vector3D(0, 5, 0)));
    }

    [TestMethod]
    public void TestSettersAndClone()
    {
        var matrix = RTMatrix.FromTraslation(new Vector3D(1, 2, 3));
        matrix.SetRotation(0, 0, 0);
        Assert.IsTrue(matrix.Translation.IsEquals(new Vector3D(1, 2, 3)));

        var axes = RTMatrix.Identity.Clone();
        axes.SetFromAxes(Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ);
        Assert.IsTrue(axes.IsEquals(RTMatrix.Identity));

        var clone = matrix.Clone();
        Assert.IsTrue(clone.IsEquals(matrix));

        Assert.ThrowsException<ArgumentNullException>(() => matrix.CloneTo(null!));
    }

    [TestMethod]
    public void TestEqualsHashCodeAndToString()
    {
        var matrix = RTMatrix.Identity;
        Assert.IsTrue(matrix.Equals(RTMatrix.Identity));
        Assert.IsFalse(matrix.Equals(new object()));
        _ = matrix.GetHashCode();
        Assert.IsTrue(matrix.ToString().Contains("|"));
    }

    [TestMethod]
    public void TestAddSubtractNegate()
    {
        var matrix = RTMatrix.Identity;
        var sum = matrix.Add(RTMatrix.Identity);
        Assert.AreEqual(2, sum[0]);
        var diff = sum.Subtract(RTMatrix.Identity);
        Assert.IsTrue(diff.IsEquals(matrix));
        var neg = matrix.Negate();
        Assert.AreEqual(-1, neg[0]);
    }

    [TestMethod]
    public void TestMultiply()
    {
        var matrix = RTMatrix.FromTraslation(new Vector3D(1, 2, 3));
        var point = new Point3D(1, 1, 1);
        var transformedPoint = matrix.Multiply(point);
        Assert.IsTrue(transformedPoint.IsEquals(new Point3D(2, 3, 4)));

        var vector = new Vector3D(1, 1, 1);
        var transformedVector = matrix.Multiply(vector);
        Assert.IsTrue(transformedVector.IsEquals(vector));

        var scaled = matrix.Multiply(2);
        Assert.AreEqual(2, scaled[0]);

        var multiplied = RTMatrix.Identity * matrix;
        Assert.IsTrue(multiplied.IsEquals(matrix));

        var wrapper = RTMatrix.Identity.Multiply(RTMatrix.Identity);
        Assert.IsTrue(wrapper.IsEquals(RTMatrix.Identity));
    }

    [TestMethod]
    public void TestInverseAndTranspose()
    {
        var matrix = RTMatrix.Identity;
        Assert.IsTrue(matrix.Inverse().IsEquals(RTMatrix.Identity));

        Assert.ThrowsException<InvalidOperationException>(() => RTMatrix.Zero.Inverse());

        var rt = RTMatrix.FromEulerAnglesXYZ(0, 0, Math.PI / 2);
        rt.Translation = new Vector3D(1, 0, 0);
        var inverseRt = rt.InverseRT();
        var point = new Point3D(2, 1, 3);
        var restored = inverseRt * (rt * point);
        Assert.IsTrue(restored.IsEquals(point, 1e-10));

        var transposed = rt.Transpose();
        Assert.AreEqual(rt[1, 0], transposed[0, 1]);
    }

    [TestMethod]
    public void TestDeterminantAndTraslateAndIsNaN()
    {
        var matrix = RTMatrix.Identity;
        Assert.AreEqual(1, matrix.Determinant);
        Assert.AreEqual(1, matrix.DeterminantAffine);

        var translated = matrix.Traslate(1, 2, 3);
        Assert.IsTrue(translated.Translation.IsEquals(new Vector3D(1, 2, 3)));
        Assert.IsTrue(matrix.Translation.IsEquals(Vector3D.Zero));

        var nanValues = new double[16];
        nanValues[0] = double.NaN;
        nanValues[15] = 1;
        var nanMatrix = new RTMatrix(nanValues);
        Assert.IsTrue(nanMatrix.IsNaN());

        for (var i = 0; i < 16; i++)
        {
            var values = new double[16];
            values[i] = double.NaN;
            var local = new RTMatrix(values);
            Assert.IsTrue(local.IsNaN());
        }

        var clean = RTMatrix.Identity;
        Assert.IsFalse(clean.IsNaN());
    }

    [TestMethod]
    public void TestTransformAndAngles()
    {
        var matrix = RTMatrix.Identity;
        var transformed = matrix.Transform(new Point3D(0, 0, 0), Vector3D.UnitZ, 0);
        Assert.IsTrue(transformed.IsEquals(matrix));
        var rotated = matrix.Transform(new Point3D(0, 0, 0), Vector3D.UnitZ, Math.PI / 2);
        AssertMatrixClose(rotated, RTMatrix.FromEulerAnglesXYZ(0, 0, Math.PI / 2), 1e-10);

        var x = 0.2;
        var y = 0.1;
        var z = -0.3;
        var fromAngles = RTMatrix.FromEulerAnglesXYZ(x, y, z);
        fromAngles.ToEulerAnglesXYZ(true, out var outX, out var outY, out var outZ);
        var reconstructed = RTMatrix.FromEulerAnglesXYZ(outX, outY, outZ);
        AssertMatrixClose(reconstructed, fromAngles, 1e-2);

        var zxMatrix = RTMatrix.FromEulerAnglesZX(0.4, 0.2);
        zxMatrix.ToEulerAnglesZX(true, out var outZx, out var outXx);
        var reconstructedZx = RTMatrix.FromEulerAnglesZX(outZx, outXx);
        AssertMatrixClose(reconstructedZx, zxMatrix, 1e-2);
    }

    [TestMethod]
    public void TestVectorsAndEqualityOperators()
    {
        var matrix = RTMatrix.Identity;
        Assert.IsTrue(matrix.GetVector(0).IsEquals(Vector3D.UnitX));

        Assert.IsTrue(matrix == RTMatrix.Identity);
        Assert.IsTrue(matrix != RTMatrix.Zero);
    }
}
