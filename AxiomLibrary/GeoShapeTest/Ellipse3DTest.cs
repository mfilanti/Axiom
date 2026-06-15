using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class Ellipse3DTest
{
    //[TestMethod]
    //public void TestEllipseBasics()
    //{
    //    var ellipse = new Ellipse3D(Point3D.Zero, 2, 1, 0, 0, Math.PI / 2, true, RTMatrix.Identity);
    //    var start = ellipse.StartPoint;
    //    var startEq = (start.X * start.X) / 4 + (start.Y * start.Y);
    //    Assert.IsTrue(startEq.IsEquals(1, 1e-1));
    //    Assert.IsTrue(ellipse.EndPoint.Y > 0);
    //    Assert.IsTrue(ellipse.SpanAngle.IsEquals(Math.PI / 2));
    //    var mid = ellipse.Evaluate(0.5);
    //    var midEq = (mid.X * mid.X) / 4 + (mid.Y * mid.Y);
    //    Assert.IsTrue(midEq.IsEquals(1, 1e-1));
    //}
}
