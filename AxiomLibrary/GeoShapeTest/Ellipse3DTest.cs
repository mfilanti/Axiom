using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class Ellipse3DTest
{
    [TestMethod]
    public void TestEllipseBasics()
    {
        // Ellisse nel piano XY: semiasse A=2 (X), B=1 (Y), arco da 0 a 90°, antiorario.
        var ellipse = new Ellipse3D(Point3D.Zero, 2, 1, 0, 0, Math.PI / 2, true, RTMatrix.Identity);

        // Il punto di start deve appartenere all'ellisse: x²/A² + y²/B² = 1.
        var start = ellipse.StartPoint;
        Assert.IsFalse(start.IsNan(), $"StartPoint NaN: ({start.X},{start.Y},{start.Z})");
        Assert.IsTrue(((start.X * start.X) / 4 + start.Y * start.Y).IsEquals(1, 1e-3), $"Start non sull'ellisse: {start}");

        Assert.IsTrue(ellipse.SpanAngle.IsEquals(Math.PI / 2), $"SpanAngle inatteso: {ellipse.SpanAngle}");
        Assert.IsTrue(ellipse.Length > 0, "La lunghezza deve essere positiva.");

        // Anche il punto medio deve appartenere all'ellisse.
        var mid = ellipse.Evaluate(0.5);
        Assert.IsFalse(mid.IsNan(), $"Mid NaN: {mid}");
        Assert.IsTrue(((mid.X * mid.X) / 4 + mid.Y * mid.Y).IsEquals(1, 1e-2), $"Mid non sull'ellisse: {mid}");

        // EvaluateAngle deve restituire un punto valido e una tangente unitaria (o nulla nei degeneri).
        var p = ellipse.EvaluateAngle(0.3, out var tangent);
        Assert.IsFalse(p.IsNan(), $"EvaluateAngle NaN: {p}");
        Assert.IsTrue(tangent.Length.IsEquals(1, 1e-6) || tangent.IsZero(), $"Tangente non normalizzata: {tangent}");
    }

    /// <summary>
    /// Robustezza: un'ellisse degenere (semiassi nulli) NON deve lanciare eccezioni, produrre NaN,
    /// né andare in ciclo infinito (le conversioni offset↔angolo usano un limite superiore).
    /// </summary>
    [TestMethod]
    public void TestEllipseDegenerate_NoCrashNoNaNNoHang()
    {
        var degenerate = new Ellipse3D(Point3D.Zero, 0, 0, 0, 0, Math.PI, true, RTMatrix.Identity);

        var p = degenerate.EvaluateAngle(0.5, out var tangent);
        Assert.IsFalse(p.IsNan(), "EvaluateAngle su ellisse degenere ha prodotto NaN.");
        Assert.IsTrue(tangent.IsZero(), "La tangente di un'ellisse degenere deve essere nulla.");

        // Devono terminare (guardie anti-ciclo-infinito) e non lanciare.
        double length = degenerate.Length;
        Assert.IsFalse(double.IsNaN(length), "Length degenere NaN.");

        // IsOnCurve non deve lanciare (Normalize su vettore nullo evitato).
        bool onCurve = degenerate.IsOnCurve(Point3D.Zero, 1e-6, out _);
        Assert.IsFalse(onCurve);
    }
}
