using System;

namespace Axiom.GeoMath
{
	/// <summary>
	/// Punto 3D
	/// </summary>
	public class Point3D
	{
		#region Static
		/// <summary>
		/// Cambia segno a tutte le componenti
		/// </summary>
		/// <param name="left"></param>
		/// <returns></returns>
		public static Point3D Negate(Point3D left) => -left;

		/// <summary>
		/// Punto zero
		/// </summary>
		public static Point3D Zero => new Point3D(0, 0, 0);

		/// <summary>
		/// Punto nullo: Point3D(double.NaN, double.NaN, double.NaN)
		/// </summary>
		public static Point3D NullPoint => new Point3D(double.NaN, double.NaN, double.NaN);
		#endregion

		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Coordinata X
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Coordinata Y
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Coordinata Z
		/// </summary>
		public double Z { get; set; }

		/// <summary>
		/// Indica che il punto è assoluto.
		/// </summary>
		public bool IsAbsolute { get; set; } = true;

		#endregion

		#region Constructors
		/// <summary>
		/// Costrutore di copia.
		/// </summary>
		/// <param name="other">Copia</param>
		public Point3D(Point3D other) : this(other.X, other.Y, other.Z)
		{
			IsAbsolute = other.IsAbsolute;
		}

		/// <summary>
		/// Costrutore di default.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="z">Z</param>
		public Point3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		/// <summary>
		/// Costrutore di default.
		/// </summary>
		public Point3D() : this(double.NaN, double.NaN, double.NaN)
		{
		}

		/// <summary>
		/// Init
		/// </summary>
		public Point3D(double x, double y) : this(x, y, 0)
		{
			IsAbsolute = false;
		}
		#endregion

		#region Methods

		/// <summary>
		/// Verifica l'uguaglianza con una tolleranza
		/// </summary>
		/// <param name="point">Punto di verifica</param>
		/// <returns><c>true</c>se sono uguali</returns>
		public bool IsEquals(Point3D point)
		{
			return X.IsEquals(point.X) && Y.IsEquals(point.Y) && Z.IsEquals(point.Z);
		}

		/// <summary>
		/// Verifica l'uguaglianza con una tolleranza
		/// </summary>
		/// <param name="point">Punto di verifica</param>
		/// <returns><c>true</c>se sono uguali</returns>
		public bool IsEquals(Point3D point, double tolerance)
		{
			return X.IsEquals(point.X, tolerance) && Y.IsEquals(point.Y, tolerance) && Z.IsEquals(point.Z, tolerance);
		}

		/// <summary>
		/// Calcola la distanza da un punto.
		/// </summary>
		/// <param name="point">Punto di riferimento.</param>
		/// <returns>Distanza dal punto di riferimento.</returns>
		public double Distance(Point3D point)
		{
			return Math.Sqrt(Math.Pow(X - point.X, 2.0) + Math.Pow(Y - point.Y, 2.0) + Math.Pow(Z - point.Z, 2.0));
		}

		/// <summary>
		/// Indica se il punto è colineare con altri due punti in 2D.
		/// </summary>
		/// <param name="point2">Secondo punto</param>
		/// <param name="point3">Terzo punto</param>
		/// <returns><c>true</c> sono collineari</returns>
		public bool AreColinear2D(Point3D point2, Point3D point3)
		{
			// Calcolo il determinante equivalente al doppio dell'area del triangolo
			double area2 = X * (point2.Y - point3.Y) + point2.X * (point3.Y - Y) + point3.X * (Y - point2.Y);

			return Math.Abs(area2) < MathUtils.FineTolerance;
		}

		/// <summary>
		/// Indica se il punto è colineare con altri due punti in 2D.
		/// </summary>
		/// <param name="point2">Secondo punto</param>
		/// <param name="point3">Terzo punto</param>
		/// <returns><c>true</c> sono collineari</returns>
		public bool AreColinear3D(Point3D point2, Point3D point3)
		{
			double x1 = X;
			double y1 = Y;
			double z1 = Z;
			double x2 = point2.X;
			double y2 = point2.Y;
			double z2 = point2.Z;
			double x3 = point3.X;
			double y3 = point3.Y;
			double z3 = point3.Z;
			double abx = x2 - x1, aby = y2 - y1, abz = z2 - z1;
			double acx = x3 - x1, acy = y3 - y1, acz = z3 - z1;

			// Calcola il prodotto vettoriale AB x AC
			double crossX = aby * acz - abz * acy;
			double crossY = abz * acx - abx * acz;
			double crossZ = abx * acy - aby * acx;

			return Math.Abs(crossX) < MathUtils.FineTolerance &&
				   Math.Abs(crossY) < MathUtils.FineTolerance &&
				   Math.Abs(crossZ) < MathUtils.FineTolerance;
		}



		#endregion

		#region EqualityOverrides

		/// <summary>
		/// Esegue un controllo di uguaglianza con tolleranza.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;
			if (obj is null || obj.GetType() != typeof(Point3D))
				return false;
			var other = (Point3D)obj;
			return IsEquals(other);
		}

		/// <summary>
		/// HashCode per il punto 3D.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// Usa una tolleranza per evitare problemi di arrotondamento
			int hashX = Math.Round(X / MathUtils.FineTolerance).GetHashCode();
			int hashY = Math.Round(Y / MathUtils.FineTolerance).GetHashCode();
			int hashZ = Math.Round(Z / MathUtils.FineTolerance).GetHashCode();
			return HashCode.Combine(hashX, hashY, hashZ);
		}

		/// <summary>
		/// Indica che il punto è NaN.
		/// </summary>
		/// <returns></returns>
		public bool IsNan()
		{
			return double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z);
		}

		/// <summary>
		/// Trasforma il punto in un vettore.
		/// </summary>
		/// <returns></returns>
		public Vector3D ToVector() => new Vector3D(X, Y, Z);

		/// <summary>
		/// Distanza tra punti al quadrato
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public double DistanceSqr(Point3D point)
		{
			return Math.Pow(this.X - point.X, 2) + Math.Pow(this.Y - point.Y, 2) + Math.Pow(this.Z - point.Z, 2);
		}
		#endregion

		#region Operators
		/// <summary>
		/// Implicit conversion from Vector3D to Point3D .
		/// </summary>
		/// <param name="vector3D">Vettore</param>
		public static implicit operator Point3D(Vector3D vector3D) => new Point3D(vector3D.X, vector3D.Y, vector3D.Z);


		/// <summary>
		/// Operatore ==
		/// </summary>
		/// <param name="obj1">Primo oggetto</param>
		/// <param name="obj2">Secondo oggetto</param>
		/// <returns>True se gli oggetti sono uguali altrimenti false.</returns>
		public static bool operator ==(Point3D obj1, Point3D obj2)
		{
			if (obj1 as object == null)
			{
				if (obj2 as object == null)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return obj1.Equals(obj2);
			}
		}

		/// <summary>
		/// Operatore !=
		/// </summary>
		/// <param name="obj1">Primo oggetto</param>
		/// <param name="obj2">Secondo oggetto</param>
		/// <returns>True se gli oggetti sono diversi altrimenti false.</returns>
		public static bool operator !=(Point3D obj1, Point3D obj2)
		{
			return !(obj1 == obj2);
		}

		/// <summary>
		/// Operatore +
		/// </summary>
		/// <param name="a">Primo oggetto</param>
		/// <param name="b">Secondo oggetto</param>
		/// <returns>Somma dei due oggetti.</returns>
		public static Point3D operator +(Point3D a, Point3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

		/// <summary>
		/// Operatore +
		/// </summary>
		/// <param name="a">Primo oggetto</param>
		/// <param name="b">Secondo oggetto</param>
		/// <returns>Somma dei due oggetti.</returns>
		public static Point3D operator +(Point3D a, Vector3D b) => new Point3D (a.X + b.X, a.Y + b.Y, a.Z + b.Z);

		/// <summary>
		/// Operatore -
		/// </summary>
		/// <param name="a">Primo oggetto</param>
		/// <param name="b">Secondo oggetto</param>
		/// <returns>Differenza dei due oggetti.</returns>
		public static Vector3D operator -(Point3D a, Point3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

		/// <summary>
		/// Operatore -
		/// </summary>
		/// <param name="a">Primo oggetto</param>
		/// <param name="b">Secondo oggetto</param>
		/// <returns>Differenza dei due oggetti.</returns>
		public static Point3D operator -(Point3D a, Vector3D b) => new Point3D( a.X - b.X, a.Y - b.Y, a.Z - b.Z);

		/// <summary>
		/// Cambia segno a tutte le componenti
		/// </summary>
		/// <param name="left"></param>
		/// <returns></returns>
		public static Point3D operator -(Point3D left) => new Point3D(-left.X, -left.Y, -left.Z);

		/// <summary>
		/// Operatore *
		/// </summary>
		/// <param name="a">Oggetto</param>
		/// <param name="scalar">Scalare</param>
		/// <returns>Moltiplicazione dell'oggetto per lo scalare.</returns>
		public static Point3D operator *(Point3D a, double scalar) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
		/// <summary>
		/// Operatore *
		/// </summary>
		/// <param name="a">Oggetto</param>
		/// <param name="scalar">Scalare</param>
		/// <returns>Moltiplicazione dell'oggetto per lo scalare.</returns>
		public static Point3D operator *(double scalar, Point3D a) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);

		/// <summary>
		/// Operatore /
		/// </summary>
		/// <param name="a">Oggetto</param>
		/// <param name="scalar">Scalare</param>
		/// <returns>Divisione dell'oggetto per lo scalare.</returns>
		public static Point3D operator /(Point3D a, double scalar) => new Point3D(a.X / scalar, a.Y / scalar, a.Z / scalar);

		/// <summary>
		/// Confronto strettamente maggiore
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator >(Point3D left, Point3D right) => left.X > right.X && left.Y > right.Y && left.Z > right.Z;

		/// <summary>
		/// Confronto strettamente minore
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <(Point3D left, Point3D right) => left.X < right.X && left.Y < right.Y && left.Z < right.Z;
		#endregion
	}
}
