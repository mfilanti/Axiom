using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class Helix3DTest
{
    [TestMethod]
    public void TestHelixBasics()
    {
        // Elica: raggio 1, profondità 2 (in Z), da 0 per un giro completo (2π), antioraria, nel piano XY.
        var helix = new Helix3D(Point3D.Zero, 1, 2, 0, 2 * Math.PI, true, RTMatrix.Identity);

        Assert.IsTrue(helix.Length > 0, "La lunghezza deve essere positiva.");
        Assert.IsTrue(helix.Pitch.IsEquals(2), $"Pitch atteso 2, ottenuto {helix.Pitch}");

        var start = helix.StartPoint;
        Assert.IsFalse(start.IsNan(), $"StartPoint NaN: {start}");
        // Il punto di start è sul cerchio di raggio 1 a Z=0.
        Assert.IsTrue((start.X * start.X + start.Y * start.Y).IsEquals(1, 1e-6), $"Start non sul cilindro: {start}");
        Assert.IsTrue(start.Z.IsEquals(0), $"Start Z atteso 0: {start.Z}");

        // Punto valutato a metà: deve stare sul cilindro di raggio 1 e a Z intermedia.
        var mid = helix.Evaluate(0.5, out var tangent);
        Assert.IsFalse(mid.IsNan());
        Assert.IsTrue((mid.X * mid.X + mid.Y * mid.Y).IsEquals(1, 1e-3), $"Mid non sul cilindro: {mid}");
        Assert.IsTrue(tangent.Length.IsEquals(1, 1e-6) || tangent.IsZero(), $"Tangente non normalizzata: {tangent}");
    }

    /// <summary>
    /// Robustezza: eliche degeneri (span nullo, profondità nulla, raggio nullo) non devono
    /// lanciare eccezioni né produrre NaN.
    /// </summary>
    [TestMethod]
    public void TestHelixDegenerate_NoCrashNoNaN()
    {
        // Span nullo (elica collassata in un punto).
        var spanZero = new Helix3D(Point3D.Zero, 1, 2, 0, 0, true, RTMatrix.Identity);
        Assert.AreEqual(0, spanZero.Pitch, 1e-12);
        var p1 = spanZero.EvaluateAngle(0, out var t1);
        Assert.IsFalse(p1.IsNan(), "EvaluateAngle (span 0) NaN.");

        // Profondità nulla (l'elica degenera in un arco di cerchio, niente avanzamento in Z).
        var depthZero = new Helix3D(Point3D.Zero, 1, 0, 0, Math.PI, true, RTMatrix.Identity);
        var p2 = depthZero.Evaluate(0.5, out _);
        Assert.IsFalse(p2.IsNan(), "Evaluate (depth 0) NaN.");

        // Raggio nullo (l'elica degenera su una retta lungo l'asse).
        var radiusZero = new Helix3D(Point3D.Zero, 0, 2, 0, Math.PI, true, RTMatrix.Identity);
        var p3 = radiusZero.Evaluate(0.5, out _);
        Assert.IsFalse(p3.IsNan(), "Evaluate (radius 0) NaN.");

        // Tutti i parametri degeneri insieme (raggio/profondità/span nulli) con centro valido.
        var allZero = new Helix3D(Point3D.Zero, 0, 0, 0, 0, true, RTMatrix.Identity);
        var p4 = allZero.EvaluateAngle(0, out _);
        Assert.IsFalse(p4.IsNan(), "EvaluateAngle (tutti i parametri nulli) NaN.");
    }
}
