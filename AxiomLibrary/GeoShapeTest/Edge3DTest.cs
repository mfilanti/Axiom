using Axiom.GeoMath;
using Axiom.GeoShape.Elements;

namespace GeoShapeTest;

[TestClass]
public class Edge3DTest
{
    [TestMethod]
    public void TestConstructionAndClone()
    {
        var v1 = new Point3D(0, 0, 0);
        var v2 = new Point3D(1, 0, 0);
        var edge = new Edge3D(v1, v2, new List<int> { 3, 7 });

        Assert.IsTrue(edge.Vertex1.IsEquals(v1));
        Assert.IsTrue(edge.Vertex2.IsEquals(v2));
        CollectionAssert.AreEqual(new List<int> { 3, 7 }, edge.TriangleIndexes);

        var clone = edge.Clone();
        Assert.IsTrue(clone.Vertex1.IsEquals(v1));
        Assert.IsTrue(clone.Vertex2.IsEquals(v2));
        CollectionAssert.AreEqual(edge.TriangleIndexes, clone.TriangleIndexes);
        // La lista degli indici è copiata (non condivisa).
        clone.TriangleIndexes.Add(99);
        Assert.AreEqual(2, edge.TriangleIndexes.Count, "La clonazione deve copiare la lista, non condividerla.");
    }

    [TestMethod]
    public void TestEdgeKeyIsOrderIndependent()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(2, 4, 6);

        // Lo spigolo è non orientato: la chiave (basata sul punto medio) non dipende dall'ordine dei vertici.
        string keyAb = Edge3D.GetEdgeKey(a, b, 3);
        string keyBa = Edge3D.GetEdgeKey(b, a, 3);
        Assert.AreEqual(keyAb, keyBa, "La chiave dello spigolo deve essere indipendente dall'ordine dei vertici.");

        // Spigoli con punti medi diversi devono avere chiavi diverse.
        string keyOther = Edge3D.GetEdgeKey(a, new Point3D(10, 10, 10), 3);
        Assert.AreNotEqual(keyAb, keyOther);
    }
}
