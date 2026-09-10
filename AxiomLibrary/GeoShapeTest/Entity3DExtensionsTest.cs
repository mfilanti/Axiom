using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;
using Axiom.GeoShape.Shapes;

namespace GeoShapeTest;

[TestClass]
public class Entity3DExtensionsTest
{
    [TestMethod]
    public void TestFromEntity3DAndCylinder()
    {
        var mesh = new Mesh3D(new List<Triangle3D>
        {
            new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0))
        });
        var fromMesh = mesh.FromEntity3D(0.1);
        Assert.IsNotNull(fromMesh);
        Assert.AreEqual(mesh.Triangles.Count, fromMesh.Triangles.Count);

        var cylinder = new Cylinder3D(1, 2);
        Assert.IsNull(cylinder.FromCylinder3D(2));
        var cylinderMesh = cylinder.FromCylinder3D(4);
        Assert.IsNotNull(cylinderMesh);
    }

    [TestMethod]
    public void TestFromSphereAndObbox()
    {
        var sphere = new Sphere3D(1);
        var sphereMesh = Entity3DExtensions.FromSphere3D(sphere, 4, 2);
        Assert.IsNotNull(sphereMesh);

        var box = new OBBox3D(2, 2, 2);
        var boxMesh = Entity3DExtensions.FromOBBox3D(box);
        Assert.IsNotNull(boxMesh);
    }

    [TestMethod]
    public void TestFromTorusAndRevolution()
    {
        var torus = new Torus3D(1, 3);
        var torusMesh = Entity3DExtensions.FromTorus3D(torus, 4, 4);
        Assert.IsNotNull(torusMesh);

        var figure = new Figure3D(new Point3D(1, 0), new Point3D(1, 2));
        var shape = new Shape2DCustom(figure);
        var revolution = new Revolution3D(shape);
        var revMesh = Entity3DExtensions.FromRevolution3D(revolution, 0.1, 4);
        Assert.IsNotNull(revMesh);
    }
}
