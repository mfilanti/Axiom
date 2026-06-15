using Axiom.GeoMath;
using Axiom.GeoShape.Curves;

namespace GeoShapeTest;

[TestClass]
public class Spline3DTest
{
    [TestMethod]
    public void TestSplineBasics()
    {
        var spline = new Spline3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 1, 0));
        Assert.IsTrue(spline.StartPoint.IsEquals(new Point3D(0, 0, 0)));
        Assert.IsTrue(spline.EndPoint.IsEquals(new Point3D(1, 1, 0)));
        Assert.IsTrue(spline.Length > 0);

        var point = spline.Evaluate(0.5, out var tangent);
        Assert.IsTrue(tangent.Length > 0);
        Assert.IsTrue(spline.InterpolationPoints.Count > 0);
        Assert.IsTrue(spline.InterpolationTangents.Count > 0);
    }
}
