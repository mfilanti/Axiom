using Axiom.GeoMath;
using Axiom.GeoShape.Elements;

namespace GeoShapeTest;

[TestClass]
public class Ray3DTest
{
    [TestMethod]
    public void TestConstructorsAndEquality()
    {
        var ray = new Ray3D();
        Assert.IsTrue(ray.Location.IsEquals(Point3D.Zero));
        Assert.IsTrue(ray.Direction.IsEquals(Vector3D.UnitZ));

        var other = new Ray3D(new Point3D(1, 2, 3), new Vector3D(0, 1, 0));
        var copy = new Ray3D(other.Location, other.Direction);
        Assert.IsTrue(other.IsEquals(copy));
        // Vector3D ha ora semantica a VALORE (readonly struct): due Ray3D con stessa Location e
        // Direction sono uguali (in precedenza Equals confrontava Direction per riferimento, quindi
        // due istanze distinte risultavano diverse — comportamento poco corretto).
        Assert.IsTrue(other.Equals(copy));
        Assert.IsFalse(other.Equals(new object()));

        Assert.IsTrue(Ray3D.XRay.Direction.IsEquals(Vector3D.UnitX));
        Assert.IsTrue(Ray3D.YRay.Direction.IsEquals(Vector3D.UnitY));
        Assert.IsTrue(Ray3D.ZRay.Direction.IsEquals(Vector3D.UnitZ));
    }

    [TestMethod]
    public void TestNegateOperatorsAndApply()
    {
        var ray = new Ray3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        ray.SetNegate();
        Assert.IsTrue(ray.Direction.IsEquals(Vector3D.NegativeUnitX));

        var negated = -ray;
        Assert.IsTrue(negated.Direction.IsEquals(Vector3D.UnitX));

        Assert.IsTrue(ray != negated);
        // Semantica a valore: un ray è uguale a un nuovo ray con stessa Location/Direction.
        Assert.IsTrue(ray == new Ray3D(ray.Location, ray.Direction));

        var matrix = RTMatrix.FromTraslation(new Vector3D(1, 0, 0));
        ray.ApplyRT(matrix);
        Assert.IsTrue(ray.Location.IsEquals(new Point3D(1, 0, 0)));
    }
}
