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

    /// <summary>
    /// Regressione: FromTorus3D(torus, maxError) passava slices e sides scambiati, quindi un toro
    /// con tubo sottile diventava un poligono di pochi lati lungo la circonferenza principale.
    /// </summary>
    [TestMethod]
    public void TestThinTorusFollowsMajorCircle()
    {
        double major = 500, tube = 0.8, maxError = 0.2;
        Mesh3D mesh = Entity3DExtensions.FromTorus3D(new Torus3D(tube, major), maxError);
        // Ogni vertice sta sulla superficie del toro e la corda tra vertici vicini sulla
        // circonferenza principale rispetta l'errore: nessun punto a metà corda si allontana dal tubo
        foreach (Triangle3D t in mesh.Triangles)
        {
            foreach (Point3D p in new[] { t.P1, t.P2, t.P3 })
            {
                double ring = Math.Sqrt(p.X * p.X + p.Y * p.Y) - major;
                Assert.AreEqual(tube, Math.Sqrt(ring * ring + p.Z * p.Z), 1e-6);
            }
            Point3D c = new Point3D((t.P1.X + t.P2.X + t.P3.X) / 3, (t.P1.Y + t.P2.Y + t.P3.Y) / 3, (t.P1.Z + t.P2.Z + t.P3.Z) / 3);
            double cr = Math.Sqrt(c.X * c.X + c.Y * c.Y) - major;
            Assert.IsTrue(Math.Abs(Math.Sqrt(cr * cr + c.Z * c.Z) - tube) < 2 * maxError, $"Triangolo troppo grande in {c}");
        }
    }

    /// <summary>
    /// Regressione: l'ingombro di una Revolution3D ruotata e traslata era errato (conseguenza di Arc3D.ApplyRT).
    /// </summary>
    [TestMethod]
    public void TestRevolutionAABBoxWithMatrix()
    {
        var figure = new Figure3D(new Point3D(25, -0.5, 0), new Point3D(32, -0.5, 0), new Point3D(32, 0.5, 0), new Point3D(25, 0.5, 0), new Point3D(25, -0.5, 0));
        var revolution = new Revolution3D(new Shape2DCustom(figure))
        {
            RTMatrix = RTMatrix.FromTraslation(new Vector3D(1000, 2000, 0)) * RTMatrix.FromEulerAnglesXYZ(Math.PI / 2, 0, 0),
        };
        AABBox3D box = revolution.GetAABBox();
        Assert.IsTrue(box.Center.IsEquals(new Point3D(1000, 2000, 0), 1e-6), $"Centro {box.Center}");
        Assert.AreEqual(64, box.LX, 1e-6);
        Assert.AreEqual(1, box.LY, 1e-6);
        Assert.AreEqual(64, box.LZ, 1e-6);
    }
}
