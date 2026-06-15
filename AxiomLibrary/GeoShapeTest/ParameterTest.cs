using Axiom.GeoShape;

namespace GeoShapeTest;

[TestClass]
public class ParameterTest
{
    [TestMethod]
    public void TestConstructorAndClone()
    {
        var parameter = new Parameter("p", true, "2.5", 3.0);
        Assert.AreEqual("p", parameter.Name);
        Assert.IsTrue(parameter.ApplyLinearUom);
        Assert.AreEqual("2.5", parameter.Formula);
        Assert.AreEqual(3.0, parameter.Value);

        var clone = parameter.Clone();
        Assert.AreNotSame(parameter, clone);
        Assert.AreEqual(parameter.Name, clone.Name);
        Assert.AreEqual(parameter.ApplyLinearUom, clone.ApplyLinearUom);
        Assert.AreEqual(parameter.Formula, clone.Formula);
        Assert.AreEqual(parameter.Value, clone.Value);
    }
}
