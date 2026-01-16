using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Elements
{
	/// <summary>
	/// estensione del poligono
	/// </summary>
	public static class Polygon3DExtensions
	{
		/// <summary>
		/// Trasforma un punto di un quadrilatero in uno appartenente all'altro
		/// </summary>
		/// <param name="initialQuad"></param>
		/// <param name="initialPoint"></param>
		/// <param name="finalQuad"></param>
		/// <param name="finalPoint"></param>
		public static void Bilinear(this Polygon3D initialQuad, Point3D initialPoint, Polygon3D finalQuad, out Point3D finalPoint)
		{
			double x = initialPoint.X;
			double y = initialPoint.Y;
			Point3D a = initialQuad.Vertices[0];
			Point3D b = initialQuad.Vertices[1];
			Point3D c = initialQuad.Vertices[2];
			Point3D d = initialQuad.Vertices[3];
			// Risolvendo l'equazione mi vengono queste formule
			double k1 = x - a.X;
			double k2 = b.X - a.X;
			double k3 = d.X - a.X;
			double k4 = a.X - b.X + c.X - d.X;
			double k5 = y - a.Y;
			double k6 = b.Y - a.Y;
			double k7 = d.Y - a.Y;
			double k8 = a.Y - b.Y + c.Y - d.Y;
			double kA = k3 * k8 - k4 * k7;
			double kB = k3 * k6 - k1 * k8 - k2 * k7 + k5 * k4;
			double kC = k5 * k2 - k1 * k6;
			double v, u, v1, u1, v2, u2;
			if (kA.IsEquals(0))
			{
				v1 = -kC / kB;
				v2 = v1;
				u1 = (k1 - v1 * k3) / (k2 + v1 * k4);
				u2 = u1;
			}
			else
			{
				v1 = (-kB + Math.Sqrt(kB * kB - 4 * kA * kC)) / (2 * kA);
				v2 = (-kB - Math.Sqrt(kB * kB - 4 * kA * kC)) / (2 * kA);
				u1 = (k1 - v1 * k3) / (k2 + v1 * k4);
				u2 = (k1 - v2 * k3) / (k2 + v2 * k4);
			}

			u = 0;
			v = 0;
			if (v1.IsEqualsOrGreater(v1, 0) && v1.IsEqualsOrLesser(1) &&
				u1.IsEqualsOrGreater(0) && u1.IsEqualsOrLesser(1))
			{
				u = u1;
				v = v1;
			}
			else if (v2.IsEqualsOrGreater(0) && v2.IsEqualsOrLesser(1) &&
					 u2.IsEqualsOrGreater(0) && u2.IsEqualsOrLesser(1))
			{
				u = u2;
				v = v2;
			}

			Point3D e = finalQuad.Vertices[0];
			Point3D f = finalQuad.Vertices[1];
			Point3D g = finalQuad.Vertices[2];
			Point3D h = finalQuad.Vertices[3];
			finalPoint = new Point3D();
			finalPoint.X = (1 - u) * (1 - v) * e.X + u * (1 - v) * f.X + u * v * g.X + (1 - u) * v * h.X;
			finalPoint.Y = (1 - u) * (1 - v) * e.Y + u * (1 - v) * f.Y + u * v * g.Y + (1 - u) * v * h.Y;
		}

		/// <summary>
		/// Calcola la convex hull
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Polygon3D CalculateConvexHull(IEnumerable<Point3D> points)
		{
			// Ordino
			List<Point3D> ordered = points.OrderBy(x => x.X).ThenBy(x => x.Y).ToList();
			// Rimuovo i punti doppi
			for (int i = ordered.Count - 1; i > 0; i--)
			{
				if (ordered[i].IsEquals(ordered[i - 1]))
					ordered.RemoveAt(i);
			}

			List<Point3D> chPointsTop = new List<Point3D>();
			chPointsTop.Add(ordered[0]);
			chPointsTop.Add(ordered[1]);
			for (int i = 2; i < ordered.Count; i++)
			{
				Point3D p = ordered[i];
				Vector3D pV = (chPointsTop[chPointsTop.Count - 1] - chPointsTop[chPointsTop.Count - 2]).Normalize();
				Vector3D v = (p - chPointsTop[chPointsTop.Count - 1]).Normalize();
				double angle = v.Angle(pV);
				if (angle >= 0)
				{
					// Rimozione
					chPointsTop.RemoveAt(chPointsTop.Count - 1);
					for (int j = chPointsTop.Count - 1; j > 0; j--)
					{
						pV = chPointsTop[j] - chPointsTop[j - 1];
						v = p - chPointsTop[j];
						angle = v.Angle(pV);
						if (angle >= 0)
							chPointsTop.RemoveAt(j);
					}
				}
				chPointsTop.Add(p);
			}

			ordered.Reverse();
			List<Point3D> chPointsBottom = new() { ordered[0], ordered[1] };
			for (int i = 2; i < ordered.Count; i++)
			{
				Point3D p = ordered[i];
				Vector3D pV = (chPointsBottom[chPointsBottom.Count - 1] - chPointsBottom[chPointsBottom.Count - 2]).Normalize();
				Vector3D v = (p - chPointsBottom[chPointsBottom.Count - 1]).Normalize();
				double angle = v.Angle(pV);
				if (angle <= 0)
				{
					// Rimozione
					chPointsBottom.RemoveAt(chPointsBottom.Count - 1);
					for (int j = chPointsBottom.Count - 1; j > 0; j--)
					{
						pV = chPointsBottom[j] - chPointsBottom[j - 1];
						v = p - chPointsBottom[j];
						angle = v.Angle(pV);
						if (angle <= 0)
							chPointsBottom.RemoveAt(j);
					}
				}
				chPointsBottom.Add(p);
			}

			chPointsBottom.RemoveAt(0);
			chPointsBottom.RemoveAt(chPointsBottom.Count - 1);
			List<Point3D> allPoints = new List<Point3D>(chPointsTop);
			allPoints.AddRange(chPointsBottom);

			return new(allPoints);
		}
	}
}
