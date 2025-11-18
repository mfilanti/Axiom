using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Elements
{
	/// <summary>
	/// Classe che rappresenta un raggio (semiretta) nello spazio
	/// </summary>
	public class Ray3D
	{
		#region Properties
		/// <summary>
		/// Origine del raggio
		/// </summary>
		public Point3D Location { get; set; }

		/// <summary>
		/// Direzione del raggio
		/// </summary>
		public Vector3D Direction { get; set; }
		#endregion

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore
		/// </summary>
		public Ray3D()
		{
			Location = Point3D.Zero;
			Direction = Vector3D.UnitZ;
		}
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="location"></param>
		/// <param name="direction"></param>
		public Ray3D(Point3D location, Vector3D direction)
		{
			Location = new(location);
			Direction = new(direction);
		}
		#endregion CONSTRUCTORS
		#region Overrides

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override bool Equals(object? obj)
		{
			if (obj is Ray3D ray)
			{
				return Direction == ray.Direction  && Location == ray.Location;
			}
			return false;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
		#region PUBLIC METHODS
		/// <summary>
		/// Confronta i due raggi considerando la tolleranza uguale a MathUtils.Tolerance
		/// </summary>
		/// <param name="ray"></param>
		/// <returns></returns>
		public bool IsEquals(Ray3D ray) => Location.IsEquals(ray.Location) && Direction.IsEquals(ray.Direction);

		/// <summary>
		/// Confronta i due raggi considerando la tolleranza indicata
		/// </summary>
		/// <param name="ray"></param>
		/// <returns></returns>
		public bool IsEquals(Ray3D ray, double tolerance) => Location.IsEquals(ray.Location, tolerance) && Direction.IsEquals(ray.Direction, tolerance);

		/// <summary>
		/// Restituisce un raggio con segno cambiato alla direzione
		/// </summary>
		/// <returns></returns>
		public Ray3D Negate()
		{
			Ray3D result = this;
			result.SetNegate();
			return result;
		}

		/// <summary>
		/// Cambia segno alla direzione
		/// </summary>
		/// <returns></returns>
		public void SetNegate()
		{
			Direction = -Direction;
		}

		/// <summary>
		/// Ruota e trasla il raggio
		/// </summary>
		/// <param name="matrix"></param>
		public void ApplyRT(RTMatrix matrix)
		{
			Location = matrix * Location;
			Direction = matrix * Direction;
		}
		#endregion PUBLIC METHODS

		#region OPERATORS
		/// <summary>
		/// Uguaglianza precisa
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(Ray3D left, Ray3D right) => left.Location == right.Location && left.Direction == right.Direction;

		/// <summary>
		/// Disuguaglianza precisa
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(Ray3D left, Ray3D right) => left.Location != right.Location || left.Direction != right.Direction;

		/// <summary>
		/// Cambia segno alla direzione
		/// </summary>
		/// <param name="left"></param>
		/// <returns></returns>
		public static Ray3D operator -(Ray3D left) => new Ray3D(left.Location, -left.Direction);
		#endregion OPERATORS

		#region STATICS
		/// <summary>
		/// Raggio lungo l'asse X (centrato sull'origine)
		/// </summary>
		public static Ray3D XRay => new Ray3D(Point3D.Zero, Vector3D.UnitX);

		/// <summary>
		/// Raggio lungo l'asse Y (centrato sull'origine)
		/// </summary>
		public static Ray3D YRay => new Ray3D(Point3D.Zero, Vector3D.UnitY);

		/// <summary>
		/// Raggio lungo l'asse Z (centrato sull'origine)
		/// </summary>
		public static Ray3D ZRay => new Ray3D(Point3D.Zero, Vector3D.UnitZ);
		#endregion STATICS
	}
}
