using System.Reflection;
using Axiom.GeoMath;
using Axiom.GeoShape;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class Node3DTest
{
    private static void SetParametersFormula(Node3D node, IEnumerable<Parameter> parameters)
    {
        var property = typeof(Node3D).GetProperty("ParametersFormula", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        property!.SetValue(node, parameters);
    }

    [TestMethod]
    public void TestAddNodeEntityAndPaths()
    {
        var root = new Node3D("root") { Path = "" };
        var child = new Node3D("child");
        root.AddNode(child);

        Assert.AreEqual("/root", child.Path);
        Assert.AreEqual("/root/child", child.PathId);

        var sphere = new Sphere3D(1) { Id = "sphere" };
        root.AddEntity(sphere);
        Assert.AreEqual("/root", sphere.Path);
        Assert.AreEqual("/root/sphere", sphere.PathId);
    }

    [TestMethod]
    public void TestHierarchyQueries()
    {
        var root = new Node3D("root") { Path = "" };
        var child = new Node3D("child");
        var grandChild = new Node3D("grand");
        root.AddNode(child);
        child.AddNode(grandChild);

        var sphere = new Sphere3D(2) { Id = "sphere" };
        child.AddEntity(sphere);

        Assert.AreSame(child, root.GetNodeByPathId("/root/child"));
        Assert.AreSame(grandChild, root.GetNodeByPathId("/root/child/grand"));
        Assert.AreSame(sphere, root.GetEntityByPathId("/root/child/sphere"));
        Assert.AreSame(child, root.GetParentNodeByPathId("/root/child/sphere"));

        Assert.AreEqual(2, root.GetSubNodes().Count);
        Assert.AreEqual(1, root.GetSubEntities().Count);
    }

    [TestMethod]
    public void TestRotationAndClone()
    {
        var node = new Node3D("n") { Path = "" };
        node.SetRotation(Math.PI / 2, 0, 0);
        node.GetRotation(out var x, out var y, out var z);
        Assert.IsTrue(x.IsEquals(Math.PI / 2));
        Assert.IsTrue(y.IsEquals(0));
        Assert.IsTrue(z.IsEquals(0));

        var cloned = node.Clone();
        Assert.AreEqual(node.Id, cloned.Id);
        Assert.IsTrue(cloned.WorldMatrix.IsEquals(node.WorldMatrix));
    }

    [TestMethod]
    public void TestUpdateWithEvaluator()
    {
        var previous = Delegates.DelegateEvaluator;
        Delegates.DelegateEvaluator = (Dictionary<string, Variable> variables, string expression, out string error) =>
        {
            error = "";
            if (string.IsNullOrEmpty(expression))
                return 0;
            if (variables != null && variables.TryGetValue(expression, out var variable) && variable != null)
                return variable.Value;
            return double.TryParse(expression, out var value) ? value : double.NaN;
        };

        try
        {
            var node = new Node3D("node") { Path = "" };
            SetParametersFormula(node, new List<Parameter> { new Parameter("p", false, "5", 0) });
            node.Variables.Add("v", new Variable { Name = "v", Value = 7 });
            node.XFormula = "1";
            node.YFormula = "2";
            node.ZFormula = "3";
            node.RotXFormula = "90";
            node.RotYFormula = "0";
            node.RotZFormula = "0";

            var child = new Node3D("child");
            SetParametersFormula(child, new List<Parameter>());
            node.AddNode(child);

            var sphere = new Sphere3D(1) { Id = "sphere" };
            sphere.XFormula = "4";
            sphere.YFormula = "5";
            sphere.ZFormula = "6";
            sphere.RotXFormula = "0";
            sphere.RotYFormula = "0";
            sphere.RotZFormula = "0";
            sphere.RadiusFormula = "8";
            node.AddEntity(sphere);

            var result = node.Update(new Dictionary<string, Variable>(), out var errorDescription);
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, errorDescription);
            Assert.AreEqual(1, node.X);
            Assert.AreEqual(2, node.Y);
            Assert.AreEqual(3, node.Z);
            Assert.AreEqual(8, sphere.Radius);

            Assert.AreEqual(5, node.ParametersFormula.First().Value);
        }
        finally
        {
            Delegates.DelegateEvaluator = previous;
        }
    }
}
