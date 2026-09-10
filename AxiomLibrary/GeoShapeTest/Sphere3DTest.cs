using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class Sphere3DTest
{
    [TestMethod]
    public void TestCloneAndAabbox()
    {
        var sphere = new Sphere3D(2);
        sphere.RTMatrix = sphere.RTMatrix.WithTranslation(new Vector3D(1, 2, 3));
        var clone = sphere.Clone();
        Assert.IsInstanceOfType(clone, typeof(Sphere3D));

        var box = sphere.GetAABBox();
        Assert.IsTrue(box.MinPoint.IsEquals(new Point3D(-1, 0, 1)));
        Assert.IsTrue(box.MaxPoint.IsEquals(new Point3D(3, 4, 5)));
    }

    [TestMethod]
    public void TestIntersectLine()
    {
        var sphere = new Sphere3D(2);
        var line = new Line3D(new Point3D(-5, 0, 0), new Point3D(5, 0, 0));
        var intersects = sphere.Intersect(line, out var mu1, out var mu2);
        Assert.IsTrue(intersects);
        Assert.IsTrue(mu1 > mu2);

        var farLine = new Line3D(new Point3D(5, 0, 0), new Point3D(5, 1, 0));
        Assert.IsFalse(sphere.Intersect(farLine, out _, out _));
    }
}
