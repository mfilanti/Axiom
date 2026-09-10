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
			var original = new Point3D(1.5, 2.5, 3.5, false);
			var copy = new Point3D(original);
			Assert.AreEqual(original.X, copy.X);
			Assert.AreEqual(original.Y, copy.Y);
			Assert.AreEqual(original.Z, copy.Z);
			Assert.AreEqual(original.IsAbsolute, copy.IsAbsolute);
		}

		[TestMethod]
		public void TestDefaultCtorAnd2DCtor()
		{
			// Point3D è uno struct: new Point3D() è il valore di default (0,0,0), NON il sentinella NaN
			// (che è Point3D.NullPoint). Questo differisce dalla precedente implementazione a classe.
			var defaultPoint = new Point3D();
			Assert.AreEqual(0, defaultPoint.X);
			Assert.AreEqual(0, defaultPoint.Y);
			Assert.AreEqual(0, defaultPoint.Z);
			Assert.IsFalse(defaultPoint.IsNan());
			Assert.IsTrue(Point3D.NullPoint.IsNan());

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
			Assert.IsTrue(p1 == p2);
			Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());

			// Point3D è uno struct: non può essere null. Il "nullo" concettuale è il sentinella NaN.
			Point3D? nullable = null;
			Assert.IsTrue(nullable == null);
			Assert.IsTrue(Point3D.NullPoint.IsNull());
		}

		/// <summary>
		/// Robustezza: contratto Equals/GetHashCode. Due punti "uguali" secondo Equals (tollerante)
		/// devono avere lo STESSO hash, anche se non identici bit-a-bit e a cavallo di una cella di
		/// arrotondamento. In precedenza l'hash arrotondava alla griglia della tolleranza e poteva
		/// differire per punti uguali, violando il contratto.
		/// </summary>
		[TestMethod]
		public void TestEqualsHashCodeContract_TolerantEqualPoints()
		{
			// A cavallo della cella di arrotondamento (0.0000049 e 0.0000051 arrotondavano a celle diverse)
			var a = new Point3D(0.0000049, 1, 2);
			var b = new Point3D(0.0000051, 1, 2);
			Assert.IsTrue(a.Equals(b), "I due punti devono risultare uguali (tolleranza).");
			Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), "Punti uguali DEVONO avere lo stesso hash.");

			// Anche punti chiaramente diversi rispettano il contratto (hash non deve contraddire Equals).
			var c = new Point3D(100, 200, 300);
			if (a.Equals(c))
				Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
		}

		[TestMethod]
		public void TestEqualsWithNullAndDifferentType()
		{
			var point = new Point3D(1, 2, 3);
			Assert.IsTrue(point.Equals(point));
			Assert.IsFalse(point.Equals((object?)null));
			Assert.IsFalse(point.Equals("not a point"));
		}

		[TestMethod]
		public void TestIsNanAndToString()
		{
			var p = new Point3D(1, 2, 3);
			Assert.IsFalse(p.IsNan());
			Assert.IsTrue(Point3D.NullPoint.IsNan());
		}

		/// <summary>
		/// Immutabilità: i metodi With restituiscono nuove istanze, l'originale non cambia.
		/// </summary>
		[TestMethod]
		public void TestImmutabilityWithMethods()
		{
			var original = new Point3D(1, 2, 3);

			var wx = original.WithX(10);
			var wy = original.WithY(20);
			var wz = original.WithZ(30);
			var w = original.With(7, 8, 9);

			// L'originale è immutato.
			Assert.IsTrue(original.IsEquals(new Point3D(1, 2, 3)), "L'originale non deve cambiare.");

			Assert.IsTrue(wx.IsEquals(new Point3D(10, 2, 3)));
			Assert.IsTrue(wy.IsEquals(new Point3D(1, 20, 3)));
			Assert.IsTrue(wz.IsEquals(new Point3D(1, 2, 30)));
			Assert.IsTrue(w.IsEquals(new Point3D(7, 8, 9)));

			// I metodi With preservano IsAbsolute.
			var rel = new Point3D(1, 2, 3, false);
			Assert.IsFalse(rel.WithX(9).IsAbsolute);
		}

		/// <summary>
		/// Semantica a valore: assegnando un Point3D si copia (nessun aliasing/condivisione di stato).
		/// </summary>
		[TestMethod]
		public void TestValueSemantics()
		{
			var a = new Point3D(1, 2, 3);
			var b = a;            // copia per valore
			b = b.WithX(99);      // modifica solo la copia

			Assert.AreEqual(1, a.X, "L'originale non deve essere influenzato dalla copia.");
			Assert.AreEqual(99, b.X);

			// In una lista, sostituire un elemento non tocca la variabile originale.
			var list = new List<Point3D> { a };
			list[0] = list[0].WithY(50);
			Assert.AreEqual(2, a.Y);
			Assert.AreEqual(50, list[0].Y);
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
