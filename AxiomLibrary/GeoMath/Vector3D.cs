using System;

namespace Axiom.GeoMath
{
	/// <summary>
	/// Rappresenta un vettore 3D.
	/// </summary>
	public class Vector3D
	{
		/// <summary>
		/// Componente X del vettore.
		/// </summary>
		public double X { get; }
		/// <summary>
		/// Componente Y del vettore.
		/// </summary>
		public double Y { get; }
		/// <summary>
		/// Componente Z del vettore.
		/// </summary>
		public double Z { get; }

		/// <summary>
		/// Crea un nuovo vettore 3D.
		/// </summary>
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Trasforma il vettore in un Point3D.
		/// </summary>
		/// <returns></returns>
		public Point3D ToPoint3D() => new Point3D(X, Y, Z);

		/// <summary>
		/// Restituisce la norma (lunghezza) del vettore.
		/// </summary>
		public double Norm() => Math.Sqrt(X * X + Y * Y + Z * Z);

		/// <summary>
		/// Restituisce il vettore normalizzato (versore).
		/// </summary>
		public Vector3D Normalize()
		{
			double norm = Norm();
			if (norm == 0) throw new InvalidOperationException("Cannot normalize a zero vector.");
			return new Vector3D(X / norm, Y / norm, Z / norm);
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
	}
}
