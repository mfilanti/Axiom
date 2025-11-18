using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Axiom.GeoShape.Entities
{
	/// <summary>
	/// Sfera 3D.
	/// Rappresenta una sfera nello spazio.
	/// E' definito con:
	/// - il raggio
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity3D)
	/// Se la matrice è un'identità allora lo zero è al centro della sfera
	/// </summary>
	public class Sphere3D : Entity3D
	{
		private const string RADIUS = "radius";
		#region PUBLIC FIELDS
		/// <summary>
		/// Raggio
		/// </summary>
		public double Radius { get => _parameters[RADIUS].Value; set => _parameters[RADIUS].Value = value; }

		/// <summary>
		/// Formula raggio
		/// </summary>
		public string RadiusFormula { get => _parameters[RADIUS].Formula; set => _parameters[RADIUS].Formula = value; }
		#endregion PUBLIC METHODS

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Sphere3D()
			: this(0, "")
		{

		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="radius"></param>
		public Sphere3D(double radius)
			: this(radius,"")
		{
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="radiusFormula"></param>
		public Sphere3D(double radius, string radiusFormula)
			: base()
		{
			_parameters.Add(RADIUS, new Parameter(RADIUS, true, radiusFormula, radius));
		}
		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Clona la sfera
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			Sphere3D result = new Sphere3D(this.Radius, this.RadiusFormula);
			this.CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			Point3D minPoint = new Point3D(-this.Radius, -this.Radius, -this.Radius);
			Point3D maxPoint = new Point3D(this.Radius, this.Radius, this.Radius);
			RTMatrix matrix = ParentRTMatrix.Multiply(this.RTMatrix);
			minPoint += matrix.Translation;
			maxPoint += matrix.Translation;
			AABBox3D result = new AABBox3D(minPoint, maxPoint);

			return result;
		}

		//Calculate the intersection of a ray and a sphere
		//The line segment is defined from p1 to p2
		//The sphere is of radius r and centered at sc
		//There are potentially two points of intersection given by
		//p = p1 + mu1 (p2 - p1)
		//p = p1 + mu2 (p2 - p1)
		//Return FALSE if the ray doesn't intersect the sphere.
		// http://local.wasp.uwa.edu.au/~pbourke/geometry/sphereline/
		public bool Intersect(Line3D line, out double mu1, out double mu2)
		{
			bool result;
			Vector3D sc = this.RTMatrix.Translation;
			double r = this.Radius;
			Vector3D p1 = (Vector3D)line.StartPoint;
			Vector3D p2 = (Vector3D)line.EndPoint;

			double a, b, c;
			double bb4ac;
			Vector3D dp;

			dp = p2 - p1;
			a = dp.X * dp.X + dp.Y * dp.Y + dp.Z * dp.Z;
			b = 2 * (dp.X * (p1.X - sc.X) + dp.Y * (p1.Y - sc.Y) + dp.Z * (p1.Z - sc.Z));
			c = sc.X * sc.X + sc.Y * sc.Y + sc.Z * sc.Z;
			c += p1.X * p1.X + p1.Y * p1.Y + p1.Z * p1.Z;
			c -= 2 * (sc.X * p1.X + sc.Y * p1.Y + sc.Z * p1.Z);
			c -= r * r;
			bb4ac = b * b - 4 * a * c;

			if (a.IsEquals(0) || bb4ac < 0)
			{
				mu1 = 0;
				mu2 = 0;
				result = false;
			}
			else
			{
				mu1 = (-b + Math.Sqrt(bb4ac)) / (2 * a);
				mu2 = (-b - Math.Sqrt(bb4ac)) / (2 * a);
				result = true;
			}
			return result;
		}
		#endregion PUBLIC METHODS

	}
}
