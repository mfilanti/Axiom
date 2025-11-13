using System;

namespace Axiom.GeoMath
{
	/// <summary>
	/// Rappresenta un vettore 3D.
	/// </summary>
	public class Vector3D
	{
		#region STATIC
		/// <summary>
		/// Vettore unitario lungo l'asse X.
		/// </summary>
		public static Vector3D UnitX => new Vector3D(1, 0, 0);

		/// <summary>
		/// Vettore unitario lungo l'asse Y.
		/// </summary>
		public static Vector3D UnitY => new Vector3D(0, 1, 0);

		/// <summary>
		/// Vettore unitario lungo l'asse Z.
		/// </summary>
		public static Vector3D UnitZ => new Vector3D(0, 0, 1);

		/// <summary>
		/// Vettore unitario negativo lungo l'asse Z.
		/// </summary>
		public static Vector3D NegativeUnitX => new Vector3D(-1, 0, 0);

		/// <summary>
		/// Vettore unitario negativo lungo l'asse Z.
		/// </summary>
		public static Vector3D NegativeUnitY => new Vector3D(0, -1, 0);

		/// <summary>
		/// Vettore unitario negativo lungo l'asse Z.
		/// </summary>
		public static Vector3D NegativeUnitZ => new Vector3D(0, 0, -1);

		#endregion

		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Componente X del vettore.
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// Componente Y del vettore.
		/// </summary>
		public double Y { get; set; }
		/// <summary>
		/// Componente Z del vettore.
		/// </summary>
		public double Z { get; set; }

		/// <summary>
		/// Restituisce la norma (lunghezza) del vettore.
		/// </summary>
		public double Norm => Math.Sqrt(X * X + Y * Y + Z * Z);
		#endregion

		#region Constructors
		/// <summary>
		/// Crea un nuovo vettore 3D.
		/// </summary>
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Ugauale a un altro vettore, con tolleranza.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsEquals(Vector3D other)
		{
			return MathUtils.IsEqual(X, other.X) &&
				   MathUtils.IsEqual(Y, other.Y) &&
				   MathUtils.IsEqual(Z, other.Z);
		}

		/// <summary>
		/// Restituisce il vettore normalizzato (versore).
		/// </summary>
		public Vector3D Normalize()
		{
			double norm = Norm;
			if (norm == 0) throw new InvalidOperationException("Cannot normalize a zero vector.");
			return new Vector3D(X / norm, Y / norm, Z / norm);
		}

		/// <summary>
		/// NOrmalizza il vettore corrente.
		/// </summary>
		public void SetNormalized()
		{
			double length = Norm;
			if (length > double.Epsilon)
			{
				double inverseLength = 1.0 / length;

				X *= inverseLength;
				Y *= inverseLength;
				Z *= inverseLength;
			}
		}

		/// <summary>
		/// Cambia segno a tutte le componenti
		/// </summary>
		/// <returns></returns>
		public void SetNegate()
		{
			X = -X;
			Y = -Y;
			Z = -Z;
		}

		/// <summary>
		/// Prodotto scalare tra questo vettore e un altro.
		/// </summary>
		public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

		/// <summary>
		/// Prodotto vettoriale tra questo vettore e un altro.
		/// </summary>
		public Vector3D Cross(Vector3D other)
		{
			return new Vector3D(
				Y * other.Z - Z * other.Y,
				Z * other.X - X * other.Z,
				X * other.Y - Y * other.X
			);
		}

		/// <summary>
		/// Restituisce una stringa rappresentativa del vettore.
		/// </summary>
		public override string ToString() => $"({X}, {Y}, {Z})";

		/// <summary>
		/// Restituisce un vettore perpendicolare (normalizzato). 
		/// Tende a restituire il vettore lungo X. 
		/// Regola: [(this x UnitX) x this] con eccezioni nel caso this sia UnitX o NegativeUnitX.
		/// </summary>
		/// <returns></returns>
		public Vector3D Perpendicular()
		{
			Vector3D result;

			if (this.IsEquals(Vector3D.NegativeUnitX))
				result = Vector3D.NegativeUnitY;
			else if (this.IsEquals(Vector3D.UnitX))
				result = Vector3D.UnitY;
			else
				result = (this.Cross(Vector3D.UnitX)).Cross(this).Normalize();

			return result;
		}

		/// <summary>
		/// Indica se i due vettori sono paralleli, controlla che la proiezione sia circa 1. 
		/// Con tolleranza pari a MathUtils.Tolerance.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public bool IsParallel(Vector3D vector)
		{
			return IsParallel(vector, MathUtils.FineTolerance);
		}

		/// <summary>
		/// Indica se i due vettori sono paralleli, controlla che la proiezione sia circa 1. 
		/// Con tolleranza indicata.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public bool IsParallel(Vector3D vector, double tolerance)
		{
			Vector3D v1 = Normalize();
			Vector3D v2 = vector.Normalize();
			bool result = MathUtils.IsEquals(Math.Abs(v1.Dot(v2)), 1, tolerance);
			return result;
		}
		#endregion
		#region Operators

		/// <summary>
		/// Implicit conversion from Point3D to Vector3D.
		/// </summary>
		/// <param name="point3D"></param>
		public static implicit operator Vector3D(Point3D point3D) => new Vector3D(point3D.X, point3D.Y, point3D.Z);

		/// <summary>
		/// Somma tra due vettori.
		/// </summary>
		public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		/// <summary>
		/// Differenza tra due vettori.
		/// </summary>
		public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		/// <summary>
		/// Moltiplicazione per scalare.
		/// </summary>
		public static Vector3D operator *(Vector3D v, double scalar) => new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
		/// <summary>
		/// Moltiplicazione per scalare (commutativa).
		/// </summary>
		public static Vector3D operator *(double scalar, Vector3D v) => v * scalar;
		/// <summary>
		/// Divisione per scalare.
		/// </summary>
		public static Vector3D operator /(Vector3D v, double scalar) => new Vector3D(v.X / scalar, v.Y / scalar, v.Z / scalar);
		#endregion
	}
}
