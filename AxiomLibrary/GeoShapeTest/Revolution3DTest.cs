using Axiom.GeoMath;
using Axiom.GeoShape;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using Axiom.GeoShape.Shapes;

namespace GeoShapeTest;

[TestClass]
public class Revolution3DTest
{
    [TestMethod]
    public void TestCloneAndAabbox()
    {
        var figure = new Figure3D(new Point3D(1, 0), new Point3D(1, 2));
        var shape = new Shape2DCustom(figure);
        var revolution = new Revolution3D(shape);

        var clone = revolution.Clone() as Revolution3D;
        Assert.IsNotNull(clone);

        var box = revolution.GetAABBox();
        Assert.IsTrue(box.MaxPoint.X > box.MinPoint.X);
    }

    // Regressione: il setter di Revolution3D.Shape controllava "_shape == null" invece di "is not null",
    // quindi non registrava mai i parametri del nuovo profilo e con Shape = null lanciava NullReferenceException.

    /// <summary>Profilo rettangolare con un parametro "R" (raggio esterno)</summary>
    private sealed class ParametricShape : Shape2D
    {
        public ParametricShape(double radius) => _parameters.Add("R", new Parameter("R", true, "", radius));
        public override bool CanEmpty => false;
        public override Figure3D GetFigure()
        {
            double r = _parameters["R"].Value;
            return new Figure3D(new Point3D(0, 0, 0), new Point3D(r, 0, 0), new Point3D(r, 10, 0), new Point3D(0, 10, 0));
        }
        public override bool Validate() => true;
        public override Shape2D CloneShape() => new ParametricShape(_parameters["R"].Value);
    }

    [TestMethod]
    public void TestShapeRegistersProfileParameters()
    {
        var revolution = new Revolution3D(new ParametricShape(25));
        Assert.IsTrue(revolution.ParametersFormula.Any(p => p.Name == "R" && p.Value == 25));
    }

    [TestMethod]
    public void TestReplacingShapeSwapsParameters()
    {
        var revolution = new Revolution3D(new ParametricShape(25));
        revolution.Shape = new ParametricShape(40);
        Assert.AreEqual(40, revolution.ParametersFormula.Single(p => p.Name == "R").Value);
    }

    [TestMethod]
    public void TestShapeSetToNullDoesNotThrow()
    {
        var revolution = new Revolution3D(new ParametricShape(25));
        revolution.Shape = null!;
        Assert.IsFalse(revolution.ParametersFormula.Any(p => p.Name == "R"));
    }
}
