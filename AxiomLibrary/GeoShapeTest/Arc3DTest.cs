using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class Arc3DTest
{
    [TestMethod]
    public void TestArcBasics()
    {
        var arc = new Arc3D(Point3D.Zero, 1, 0, Math.PI / 2, true, RTMatrix.Identity);
        Assert.IsTrue(arc.StartPoint.IsEquals(new Point3D(1, 0, 0)));
        Assert.IsTrue(arc.EndPoint.IsEquals(new Point3D(0, 1, 0)));
        Assert.IsTrue(arc.SpanAngle.IsEquals(Math.PI / 2));
        var mid = arc.MiddlePoint;
        Assert.IsTrue(mid.Distance(new Point3D(Math.Sqrt(0.5), Math.Sqrt(0.5), 0)).IsEquals(0, 1e-6));
    }

    /// <summary>
    /// Regressione BUG-09: Arc3D.IsOnCurve delegava sempre a un sotto-arco senza mai calcolare il
    /// risultato, causando ricorsione infinita e StackOverflow. Deve invece terminare e restituire
    /// un esito corretto (punto sull'arco / fuori dall'arco).
    /// </summary>
    [TestMethod]
    public void TestIsOnCurve()
    {
        // Quarto di cerchio nel piano XY, raggio 1, da 0 a 90°, in senso antiorario.
        var arc = new Arc3D(Point3D.Zero, 1, 0, Math.PI / 2, true, RTMatrix.Identity);

        // Punto a metà arco (45°): appartiene alla curva.
        var onArc = new Point3D(Math.Sqrt(0.5), Math.Sqrt(0.5), 0);
        Assert.IsTrue(arc.IsOnCurve(onArc, 1e-6, out double offset));
        Assert.IsTrue(offset.IsEquals(0.5, 1e-3), $"Offset inatteso: {offset}");

        // Punto sul cerchio ma fuori dall'intervallo angolare dell'arco (270°): non appartiene.
        var offArcAngle = new Point3D(0, -1, 0);
        Assert.IsFalse(arc.IsOnCurve(offArcAngle, 1e-6, out _));

        // Punto sul raggio ma non sul cerchio: non appartiene.
        var offArcRadius = new Point3D(0.5, 0, 0);
        Assert.IsFalse(arc.IsOnCurve(offArcRadius, 1e-6, out _));

        // Punto fuori dal piano dell'arco: non appartiene.
        var offPlane = new Point3D(Math.Sqrt(0.5), Math.Sqrt(0.5), 1);
        Assert.IsFalse(arc.IsOnCurve(offPlane, 1e-6, out _));
    }
}
