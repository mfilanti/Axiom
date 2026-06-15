using Axiom.GeoShape;

namespace GeoShapeTest;

[TestClass]
public class VariableTest
{
    [TestMethod]
    public void TestCloneCopiesValues()
    {
        var variable = new Variable
        {
            Name = "v",
            Value = 1.5,
            Formula = "x+1",
            ApplyUomFactor = true,
            ReadOnly = true,
            Description = "desc"
        };

        var clone = variable.Clone();
        Assert.AreNotSame(variable, clone);
        Assert.AreEqual(variable.Name, clone.Name);
        Assert.AreEqual(variable.Value, clone.Value);
        Assert.AreEqual(variable.Formula, clone.Formula);
        Assert.AreEqual(variable.ApplyUomFactor, clone.ApplyUomFactor);
        Assert.AreEqual(variable.ReadOnly, clone.ReadOnly);
        Assert.AreEqual(variable.Description, clone.Description);
    }
}
