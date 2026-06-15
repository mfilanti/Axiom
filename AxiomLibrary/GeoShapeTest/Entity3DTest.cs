using Axiom.GeoMath;
using Axiom.GeoShape;
using Axiom.GeoShape.Entities;

namespace GeoShapeTest;

[TestClass]
public class Entity3DTest
{
    [TestMethod]
    public void TestParametersFormulaSetterThrows()
    {
        var sphere = new Sphere3D(1);
        Assert.ThrowsException<Exception>(() => sphere.ParametersFormula = new List<Parameter>());
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
            var sphere = new Sphere3D(1)
            {
                Id = "s",
                Path = "/root",
                XFormula = "1",
                YFormula = "2",
                ZFormula = "3",
                RotXFormula = "90",
                RotYFormula = "0",
                RotZFormula = "0",
                RadiusFormula = "4"
            };

            var result = sphere.Update(new Dictionary<string, Variable>(), out var error);
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(1, sphere.X);
            Assert.AreEqual(2, sphere.Y);
            Assert.AreEqual(3, sphere.Z);
            Assert.AreEqual(4, sphere.Radius);

            Delegates.DelegateEvaluator = (Dictionary<string, Variable> variables, string expression, out string errorDescription) =>
            {
                errorDescription = "err";
                return double.NaN;
            };

            sphere.XFormula = "bad";
            var failed = sphere.Update(new Dictionary<string, Variable>(), out var errorDescription);
            Assert.IsFalse(failed);
            Assert.IsTrue(errorDescription.Contains("Entity"));
        }
        finally
        {
            Delegates.DelegateEvaluator = previous;
        }
    }
}
