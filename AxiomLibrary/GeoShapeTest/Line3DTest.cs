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
}
