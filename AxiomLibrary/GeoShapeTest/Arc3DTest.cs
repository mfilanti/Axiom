using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;

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

    /// <summary>
    /// Robustezza: costruttori e Set3Points con input degeneri (punti coincidenti/collineari,
    /// tangente allineata) non devono lanciare eccezioni.
    /// </summary>
    [TestMethod]
    public void TestDegenerateConstructionDoesNotThrow()
    {
        // Set3Points su 3 punti allineati -> false (in passato lanciava da Normalize).
        var arc = new Arc3D();
        bool ok = arc.Set3Points(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(2, 0, 0));
        Assert.IsFalse(ok, "Tre punti allineati non definiscono un arco.");

        // Costruttore (start, end, tangent) con start == end -> arco degenere (raggio 0), niente crash.
        var degenerate = new Arc3D(new Point3D(1, 1, 0), new Point3D(1, 1, 0), Vector3D.UnitX);
        Assert.AreEqual(0, degenerate.Radius, 1e-9);

        // Costruttore (start, end, tangent) con tangente allineata alla corda -> arco degenere.
        var straight = new Arc3D(new Point3D(0, 0, 0), new Point3D(2, 0, 0), Vector3D.UnitX);
        Assert.AreEqual(0, straight.Radius, 1e-9);

        // Costruttore (start, end, center) con punti collineari (cross nullo) -> nessuna eccezione.
        var collinear = new Arc3D(new Point3D(1, 0, 0), new Point3D(2, 0, 0), new Point3D(0, 0, 0), true);
        Assert.IsFalse(double.IsNaN(collinear.Radius));
    }

    /// <summary>
    /// Regressione: ApplyRT metteva la traslazione anche in RMatrix, che deve contenere solo la rotazione,
    /// quindi i punti dell'arco risultavano traslati due volte.
    /// </summary>
    [TestMethod]
    public void TestApplyRTTranslatesOnce()
    {
        var arc = new Arc3D(Point3D.Zero, 10, 0, Math.PI, true, RTMatrix.Identity);
        arc.ApplyRT(RTMatrix.FromTraslation(new Vector3D(1000, 2000, 0)));
        Assert.IsTrue(arc.Center.IsEquals(new Point3D(1000, 2000, 0)));
        Assert.IsTrue(arc.StartPoint.IsEquals(new Point3D(1010, 2000, 0), 1e-9), $"Start {arc.StartPoint}");
        Assert.IsTrue(arc.MiddlePoint.IsEquals(new Point3D(1000, 2010, 0), 1e-9), $"Middle {arc.MiddlePoint}");

        AABBox3D box = arc.GetABBox();
        Assert.IsTrue(box.MinPoint.IsEquals(new Point3D(990, 2000, 0), 1e-9), $"Min {box.MinPoint}");
        Assert.IsTrue(box.MaxPoint.IsEquals(new Point3D(1010, 2010, 0), 1e-9), $"Max {box.MaxPoint}");
    }

    /// <summary>
    /// Regressione: GetABBox usava le direzioni degli assi proiettate sul piano dell'arco senza
    /// normalizzarle, perdendo i punti estremi quando il piano non è parallelo agli assi.
    /// </summary>
    [TestMethod]
    public void TestGetABBoxOfTiltedArc()
    {
        double a = 0.5;
        var arc = new Arc3D(Point3D.Zero, 10, 0, Math.PI, true, RTMatrix.Identity);
        arc.ApplyRT(RTMatrix.FromEulerAnglesXYZ(a, 0, 0));
        AABBox3D box = arc.GetABBox();
        Assert.AreEqual(20, box.LX, 1e-9);
        Assert.AreEqual(10 * Math.Cos(a), box.LY, 1e-9);
        Assert.AreEqual(10 * Math.Sin(a), box.LZ, 1e-9);
    }
}
