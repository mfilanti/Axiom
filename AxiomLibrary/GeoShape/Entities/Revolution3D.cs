using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Shapes;
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
	/// Entità di rivoluzione 3D.
	/// Rappresenta in genere un solido di rivoulzione nello spazio.
	/// E' definito con:
	/// - una Figure2D che rappresenta la sezione di rivoluzione (nel quadrante YZ)
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity3D)
	/// </summary>
	public class Revolution3D : Entity3D
	{
		#region Properties
		private Shape2D _shape;
		/// <summary>
		/// Profilo. 
		/// Rappresenta la sezione nel piano YZ. 
		/// Deve essere una figura continua. 
		/// </summary>
		public Shape2D Shape
		{
			get => _shape;
			set
			{
				if (_shape is not null)
				{
					foreach (var item in _shape.Parameters)
					{
						_parameters.Remove(item.Name);
					}
				}
				_shape = value;
				if (_shape == null)
				{
					foreach (var item in _shape.Parameters)
					{
						_parameters.Add(item.Name, item);
					}
				}
			}
		}

		#endregion

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Revolution3D()
			: base()
		{
			Shape = new Shape2DCustom();
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="section"></param>
		public Revolution3D(Shape2D section)
			: base()
		{
			Shape = section.Clone();
		}

		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Clona la rivoluzione
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			Revolution3D result = new Revolution3D(Shape.Clone());
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente. 
		/// N.B. Calcolo approssimato per eccesso: per Shape che hanno solo linee è esatto, ma 
		/// se ci sono archi o altro, considera il box della curva in 2D come ingombro iniziale.
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			Figure3D figure = new Figure3D();
			foreach (Curve3D curve in Shape.GetFigure())
			{
				List<Point3D> points = new List<Point3D>();
				if (curve is Line3D line)
				{
					points.Add(new(line.PStart.X, 0, line.PStart.Y));
					points.Add(new(line.PEnd.X, 0, line.PEnd.Y));
				}
				else
				{
					AABBox3D box = curve.GetABBox();
					points.Add(new(box.MaxPoint.X, 0, box.MaxPoint.Y));
					points.Add(new(box.MaxPoint.X, 0, box.MinPoint.Y));
					points.Add(new(box.MinPoint.X, 0, box.MinPoint.Y));
					points.Add(new(box.MinPoint.X, 0, box.MaxPoint.Y));
				}
				foreach (Point3D point in points)
				{
					double radius = Math.Abs(point.X);
					if (radius.IsEquals(0))
						radius = 0.1;

					figure.Add(new Arc3D(new Point3D(0, 0, point.Z), radius, 0, Math.PI, true, RTMatrix.Identity));
					figure.Add(new Arc3D(new Point3D(0, 0, point.Z), radius, Math.PI, 2 * Math.PI, true, RTMatrix.Identity));
				}
			}
			RTMatrix matrix = ParentRTMatrix.Multiply(RTMatrix);
			figure.ApplyRT(matrix);
			AABBox3D result = figure.GetABBox();
			return result;
		}
		#endregion
	}
}
