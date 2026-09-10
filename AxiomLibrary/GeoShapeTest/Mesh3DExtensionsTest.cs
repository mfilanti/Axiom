using System.IO;
using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class Mesh3DExtensionsTest
{
    [TestMethod]
    public void TestStlRoundtrip()
    {
        var mesh = new Mesh3D(new List<Triangle3D>
        {
            new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0))
        });

        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".stl");
        try
        {
            Mesh3DExtensions.ToSTLFile(mesh, fileName);
            var loaded = Mesh3DExtensions.FromSTLFile(fileName);
            Assert.AreEqual(1, loaded.Triangles.Count);
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
