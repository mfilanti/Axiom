using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class Mesh3DTest
{
    [TestMethod]
    public void TestNormalsAndClone()
    {
        var triangles = new List<Triangle3D>
        {
            new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0))
        };
        var mesh = new Mesh3D(triangles);
        mesh.SetNormals();
        Assert.AreEqual(1, mesh.VertexNormals.Count);

        var clone = mesh.CloneWithNormals();
        Assert.AreEqual(mesh.Triangles.Count, clone.Triangles.Count);
        Assert.IsNotNull(clone.VertexNormals);
    }

    [TestMethod]
    public void TestEdgesAndOutline()
    {
        var mesh = new Mesh3D(new List<Triangle3D>
        {
            new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0))
        });

        var edges = mesh.GetEdges();
        Assert.AreEqual(3, edges.Count);

        var ok = mesh.CheckCorrectness(edges, out var edges1, out var edges2, out var edges3);
        Assert.IsFalse(ok);
        Assert.AreEqual(3, edges1);
        Assert.AreEqual(0, edges2);
        Assert.AreEqual(0, edges3);

        mesh.AutomaticSetOutline(0, edges);
        Assert.IsNotNull(mesh.Outline);
        Assert.IsTrue(mesh.Outline.Count > 0);
    }
}
