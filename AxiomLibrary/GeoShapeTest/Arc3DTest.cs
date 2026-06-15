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
}
