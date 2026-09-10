using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class PolyLine3DTest
{
    [TestMethod]
    public void TestPolylineBasics()
    {
        var poly = new PolyLine3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 1, 0));
        Assert.IsFalse(poly.Closed);
        Assert.IsTrue(poly.StartPoint.IsEquals(new Point3D(0, 0, 0)));
        Assert.IsTrue(poly.EndPoint.IsEquals(new Point3D(1, 1, 0)));
        Assert.IsTrue(poly.Length > 0);

        var clone = poly.Clone();
        Assert.IsTrue(clone.IsEquals(poly));

        var point = poly.Evaluate(0.5, out var tangent);
        Assert.IsTrue(tangent.Length > 0);
    }
}
