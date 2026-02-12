using Axiom.GeoMath;

namespace Axiom.GeoMathTest
{
	[TestClass]
	public sealed class Point3DTest
	{
		[TestMethod]
		public void TestCtor()
		{
			var p = new Point3D(1.0, 2.0, 3.0);
			Assert.AreEqual(1.0, p.X);
			Assert.AreEqual(2.0, p.Y);
			Assert.AreEqual(3.0, p.Z);
			Assert.IsTrue(p.IsAbsolute);
		}

		[TestMethod]
		public void TestCopyCtor()
		{
			var original = new Point3D(1.5, 2.5, 3.5) { IsAbsolute = false };
			var copy = new Point3D(original);
			Assert.AreEqual(original.X, copy.X);
			Assert.AreEqual(original.Y, copy.Y);
			Assert.AreEqual(original.Z, copy.Z);
			Assert.AreEqual(original.IsAbsolute, copy.IsAbsolute);
		}

		[TestMethod]
		public void TestDefaultCtorAnd2DCtor()
		{
			var defaultPoint = new Point3D();
			Assert.IsTrue(defaultPoint.IsNan());

			var point2d = new Point3D(3, 4);
			Assert.AreEqual(0, point2d.Z);
			Assert.IsFalse(point2d.IsAbsolute);
		}

		[TestMethod]
		public void TestStatics()
		{
			Assert.IsTrue(Point3D.Zero.IsEquals(new Point3D(0, 0, 0)));
			Assert.IsTrue(Point3D.NullPoint.IsNan());
			var negated = Point3D.Negate(new Point3D(1, -2, 3));
			Assert.IsTrue(negated.IsEquals(new Point3D(-1, 2, -3)));
		}

		[TestMethod]
		public void TestEqualsAndDistance()
		{
			var p1 = new Point3D(1, 2, 3);
			var p2 = new Point3D(1.000000001, 2.000000001, 3.000000001);
			Assert.IsTrue(p1.IsEquals(p2));
			Assert.IsTrue(p1.IsEquals(p2, 0.001));
			Assert.AreEqual(0, p1.Distance(p1));
			Assert.AreEqual(1, new Point3D(1, 0, 0).Distance(new Point3D(2, 0, 0)));
			Assert.AreEqual(1, new Point3D(1, 0, 0).DistanceSqr(new Point3D(2, 0, 0)));
		}

		[TestMethod]
		public void TestColinear()
		{
			var p1 = new Point3D(0, 0, 0);
			var p2 = new Point3D(1, 1, 0);
			var p3 = new Point3D(2, 2, 0);
			Assert.IsTrue(p1.AreColinear2D(p2, p3));
			Assert.IsTrue(p1.AreColinear3D(p2, p3));

			var p4 = new Point3D(0, 1, 0);
			Assert.IsFalse(p1.AreColinear2D(p2, p4));
			Assert.IsFalse(p1.AreColinear3D(p2, p4));
		}

		[TestMethod]
		public void TestEqualityOverrides()
		{
			var p1 = new Point3D(1, 2, 3);
			var p2 = new Point3D(1, 2, 3);
			Assert.IsTrue(p1.Equals(p2));
			Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
			Assert.IsTrue(p1 != null);
			Point3D? nullPoint = null;
			Assert.IsTrue(nullPoint == null);
		}

		[TestMethod]
		public void TestEqualsWithNullAndDifferentType()
		{
			var point = new Point3D(1, 2, 3);
			Assert.IsTrue(point.Equals(point));
			Assert.IsFalse(point.Equals(null));
			Assert.IsFalse(point.Equals("not a point"));
		}

		[TestMethod]
		public void TestIsNanAndToString()
		{
			var p = new Point3D(1, 2, 3);
			Assert.IsFalse(p.IsNan());
			Assert.IsTrue(Point3D.NullPoint.IsNan());
		}

		[TestMethod]
		public void TestConversionsAndOperators()
		{
			var point = new Point3D(1, 2, 3);
			var vector = point.ToVector();
			Assert.AreEqual(1, vector.X);
			Assert.AreEqual(2, vector.Y);
			Assert.AreEqual(3, vector.Z);

			Point3D fromVector = new Vector3D(4, 5, 6);
			Assert.IsTrue(fromVector.IsEquals(new Point3D(4, 5, 6)));

			Assert.IsTrue((point + new Point3D(1, 1, 1)).IsEquals(new Point3D(2, 3, 4)));
			Assert.IsTrue((point + new Vector3D(1, 1, 1)).IsEquals(new Point3D(2, 3, 4)));
			Assert.IsTrue((point - new Vector3D(1, 1, 1)).IsEquals(new Point3D(0, 1, 2)));
			Assert.IsTrue((point - new Point3D(1, 1, 1)).IsEquals(new Vector3D(0, 1, 2)));
			Assert.IsTrue((-point).IsEquals(new Point3D(-1, -2, -3)));
			Assert.IsTrue((point * 2).IsEquals(new Point3D(2, 4, 6)));
			Assert.IsTrue((2 * point).IsEquals(new Point3D(2, 4, 6)));
			Assert.IsTrue((point / 2).IsEquals(new Point3D(0.5, 1, 1.5)));
			Assert.IsTrue(new Point3D(2, 2, 2) > new Point3D(1, 1, 1));
			Assert.IsTrue(new Point3D(0, 0, 0) < new Point3D(1, 1, 1));
			Assert.IsFalse(new Point3D(1, 0, 0) > new Point3D(0, 1, 0));
			Assert.IsFalse(new Point3D(1, 0, 0) < new Point3D(0, 1, 0));
			Assert.IsTrue(new Point3D(1, 2, 3) == new Point3D(1, 2, 3));
			Assert.IsTrue(new Point3D(1, 2, 3) != new Point3D(3, 2, 1));

			Point3D? leftNull = null;
			var right = new Point3D(1, 2, 3);
			Assert.IsFalse(leftNull == right);
			Assert.IsTrue(leftNull != right);
		}
	}
}
