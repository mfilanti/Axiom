namespace Axiom.GeoMathTest
{
	[TestClass]
	public sealed class Point3DTest
	{
		[TestMethod]
		public void TestCtor()
		{
			var p = new Axiom.GeoMath.Point3D(1.0, 2.0, 3.0);
			Assert.AreEqual(1.0, p.X);
			Assert.AreEqual(2.0, p.Y);
			Assert.AreEqual(3.0, p.Z);
			Assert.IsTrue(p.IsAbsolute);
		}
	}
}
