using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class Line3DTest
{
    [TestMethod]
    public void TestConstructorsAndProperties()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(2, 0, 0));
        Assert.IsTrue(line.StartPoint.IsEquals(new Point3D(0, 0, 0)));
        Assert.IsTrue(line.EndPoint.IsEquals(new Point3D(2, 0, 0)));
        Assert.AreEqual(2, line.Length);
        Assert.IsTrue(line.MiddlePoint.IsEquals(new Point3D(1, 0, 0)));
    }

    [TestMethod]
    public void TestMirrorAndDistance()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(2, 0, 0));
        var mirroredX = line.MirrorX();
        Assert.IsTrue(((Line3D)mirroredX).PStart.IsEquals(new Point3D(0, 0, 0)));
        var mirroredY = line.MirrorY();
        Assert.IsTrue(((Line3D)mirroredY).PEnd.IsEquals(new Point3D(-2, 0, 0)));

        var insideDist = line.Dist(new Point3D(1, 1, 0));
        Assert.IsTrue(insideDist.IsEquals(1));

        var outsideDist = line.Dist(new Point3D(3, 0, 0));
        Assert.IsTrue(outsideDist.IsEquals(1));
    }

    [TestMethod]
    public void TestCloneAndEquals()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 1, 0));
        var clone = line.Clone();
        Assert.IsTrue(clone.IsEquals(line));
        Assert.IsTrue(clone.IsEquals(line, 0.001));
    }

    /// <summary>
    /// Robustezza: un segmento di lunghezza nulla (start == end) non deve lanciare né produrre NaN
    /// su tangenti, valutazione, proiezione e IsOnCurve.
    /// </summary>
    [TestMethod]
    public void TestZeroLengthSegment_NoCrashNoNaN()
    {
        var p = new Point3D(3, 4, 5);
        var line = new Line3D(p, new Point3D(p));

        Assert.AreEqual(0, line.Length);
        // Tangenti: vettore nullo, non NaN, nessuna eccezione.
        Assert.IsTrue(line.StartTangent.IsZero());
        Assert.IsTrue(line.EndTangent.IsZero());

        // EvaluateAbs: qualunque offset -> il punto di start, tangente nulla.
        var eval = line.EvaluateAbs(1.0, out var tangent);
        Assert.IsTrue(eval.IsEquals(p), $"EvaluateAbs inatteso: {eval}");
        Assert.IsTrue(tangent.IsZero());

        // Projection: si degrada al punto di start, offset 0 (niente NaN da 0/0).
        var proj = line.Projection(new Point3D(10, 10, 10), out bool isInside, out double offset);
        Assert.IsTrue(proj.IsEquals(p));
        Assert.AreEqual(0, offset);
        Assert.IsFalse(isInside);

        // IsOnCurve: vero solo se il punto coincide con lo start.
        Assert.IsTrue(line.IsOnCurve(p, 1e-6, out _));
        Assert.IsFalse(line.IsOnCurve(new Point3D(0, 0, 0), 1e-6, out _));
    }
}
