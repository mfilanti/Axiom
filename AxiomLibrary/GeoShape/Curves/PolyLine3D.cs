using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Curves
{
	/// <summary>
	/// Polilinea 3D
	/// </summary>
	public class PolyLine3D : Curve3D
	{
		#region fields
		/// <summary>
		/// Punti. 
		/// </summary>
		private List<Point3D> _points;

		#endregion
		#region Properties
		/// <summary>
		/// Indica se la polyline è chiusa o aperta
		/// </summary>
		public bool Closed { get; private set; }
		/// <summary>
		/// Punto di start. 
		/// Sola lettura.
		/// </summary>
		public override Point3D StartPoint => _points[0];

		/// <summary>
		/// Punto di end. 
		/// Sola lettura.
		/// </summary>
		public override Point3D EndPoint => Closed ? _points[0] : _points[_points.Count - 1];

		/// <summary>
		/// Tangente di start. 
		/// Sola lettura.
		/// </summary>
		public override Vector3D StartTangent
		{
			get
			{
				Vector3D result = _points[1] - _points[0];
				result.SetNormalize();
				return result;
			}
		}

		/// <summary>
		/// Tangente di end. 
		/// Sola lettura.
		/// </summary>
		public override Vector3D EndTangent
		{
			get
			{
				Vector3D result = _points[_points.Count - 1] - _points[_points.Count - 2];
				result.SetNormalize();
				return result;
			}

		}

		/// <summary>
		/// Lunghezza del segmento. 
		/// Sola lettura.
		/// </summary>
		public override double Length => ToFigure().Length;

		/// <summary>
		/// Punto medio. 
		/// Sola lettura.
		/// </summary>
		public Point3D MiddlePoint => Evaluate(0.5);
		#endregion

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public PolyLine3D()
		{
			_points = new List<Point3D>();
			Closed = false;
		}

		/// <summary>
		/// Costruttore. 
		/// </summary>
		/// <param name="points">Punti (almeno 2)</param>
		public PolyLine3D(params double[] points)
		{
			_points = new List<Point3D>();
			for (int i = 0; i < points.Length - 2; i += 3)
				_points.Add(new Point3D(points[i], points[i + 1], points[i + 2]));
			Closed = false;
		}

		/// <summary>
		/// Costruttore. 
		/// </summary>
		/// <param name="points">Punti (almeno 2)</param>
		public PolyLine3D(params Point3D[] points)
		{
			_points = new List<Point3D>(points);
			Closed = false;
		}

		/// <summary>
		/// Costruttore. 
		/// </summary>
		/// <param name="points">Punti (almeno 2)</param>
		public PolyLine3D(IEnumerable<Point3D> points)
		{
			_points = new List<Point3D>(points);
			Closed = false;
		}
		#endregion

		#region PUBLIC PROPERTIES

		#endregion PUBLIC PROPERTIES

		#region PUBLIC METHODS
		/// <summary>
		/// Clona la curva
		/// </summary>
		/// <returns></returns>
		public override Curve3D Clone()
		{
			PolyLine3D result = new PolyLine3D(_points);
			result.Closed = Closed;
			return result;
		}

		/// <summary>
		/// Confronta le due spline considerando la tolleranza
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve)
		{
			return IsEquals(curve, MathUtils.FineTolerance);
		}

		/// <summary>
		/// Confronta le due linee considerando la tolleranza
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve, double tolerance)
		{
			bool result = false;
			if (curve is PolyLine3D)
			{
				PolyLine3D poly = curve as PolyLine3D;
				if (Closed == poly.Closed && _points.Count == poly._points.Count)
				{
					result = true;
					for (int i = 0; i < _points.Count; i++)
					{
						if (_points[i].IsEquals(poly._points[i]) == false)
						{
							result = false;
							break;
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Evaluate percentuale. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la percentuale da percorrere a partire dal punto di start. 
		/// Restituisce inoltre la tangente al punto valutato.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e 1 il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <param name="offset">Tangente</param>
		/// <returns></returns>
		public override Point3D Evaluate(double offset, out Vector3D tangent)
		{
			Figure3D figure = ToFigure();
			Point3D result = figure.Evaluate(offset, out tangent);
			return result;
		}

		/// <summary>
		/// Evaluate assoluto. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la lunghezza da percorrere a partire dal punto di start. 
		/// Restituisce inoltre la tangente al punto valutato.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e Length il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <param name="tangent">Tangente</param>
		/// <returns></returns>
		public override Point3D EvaluateAbs(double offset, out Vector3D tangent)
		{
			Figure3D figure = ToFigure();
			Point3D result = figure.EvaluateAbs(offset, out tangent);
			return result;
		}

		/// <summary>
		/// Trasla la curva della quantità indicata
		/// </summary>
		/// <param name="traslation">Vettore che indica la traslazione</param>
		public override void Move(Vector3D traslation)
		{
			for (int i = 0; i < _points.Count; i++)
				_points[i] = _points[i] + traslation;
		}

		/// <summary>
		/// Ruota e trasla la curva. 
		/// La componente Z viene ignorata.
		/// </summary>
		/// <param name="matrix"></param>
		public override void ApplyRT(RTMatrix matrix)
		{
			for (int i = 0; i < _points.Count; i++)
				_points[i] = matrix.Multiply(_points[i]);
		}

		/// <summary>
		/// Restituisce la curva scalata
		/// </summary>
		/// <param name="factor"></param>
		public override Curve3D Scale(double factor)
		{
			PolyLine3D result = (PolyLine3D)Clone();
			for (int i = 0; i < result._points.Count; i++)
				result._points[i] = factor * result._points[i];

			return result;
		}

		/// <summary>
		/// Restituisce la curva invertita
		/// </summary>
		/// <returns></returns>
		public override Curve3D Inverse()
		{
			PolyLine3D result = (PolyLine3D)Clone();
			result._points.Reverse();

			return result;
		}

		/// <summary>
		/// Inverte la curva
		/// </summary>
		/// <returns></returns>
		public override void SetInverse() => _points.Reverse();

		/// <summary>
		/// Restituisce l'ABBox3D corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetABBox() => AABBox3D.FromPoints(_points);

		/// <summary>
		/// Restituisce una parte della curva. 
		/// Se uno dei 2 parametri è minore di zero o maggiore della lunghezza 
		/// viene restituito null.
		/// </summary>
		/// <param name="start">Inizio: distanza dal punto di start</param>
		/// <param name="end">Fine: distanza dal punto di start</param>
		/// <returns></returns>
		public override Curve3D Trim(double start, double end)
		{
			Figure3D figure = ToFigure();
			Figure3D trimFigure = figure.Trim(start, end);
			List<Point3D> points = new List<Point3D>();
			foreach (Curve3D curve in trimFigure)
				points.Add(curve.StartPoint);

			points.Add(trimFigure.EndPoint);
			PolyLine3D result = new PolyLine3D(points);
			return result;
		}

		/// <summary>
		/// Valuta se il punto passato appartiene alla curva. 
		/// Restituisce inoltre il valore percentuale (0 - 1) che rappresenta la distanza dal punto di start. 
		/// Se non appartiene alla curva restituisce un valore negativo (-1).
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="offset">In uscita: distanza percentuale dal punto di start</param>
		/// <returns></returns>
		public override bool IsOnCurve(Point3D point, double tolerance, out double offset)
		{
			bool result = false;
			Figure3D figure = ToFigure();
			result = figure.IsOnFigure(point, tolerance, out offset);
			return result;
		}

		/// <summary>
		/// Restituisce la Figure3D corrispondente
		/// </summary>
		/// <returns></returns>
		public Figure3D ToFigure()
		{
			Figure3D result = new Figure3D();
			result.AddPolygon(_points);
			if (Closed)
				result.Add(new Line3D(_points[_points.Count - 1], _points[0]));

			return result;
		}

		/// <summary>
		/// Cerca il punto opportuno percorrendo la polilinea dall'inizio fino alla distanza indicata. 
		/// Restituisce -1 se si va oltre la lunghezza massima (o se distance è minore di 0).
		/// </summary>
		/// <param name="distance"></param>
		/// <returns></returns>
		public int FindIndexByDistance(double distance)
		{
			Figure3D figure = ToFigure();
			double curveDistance;
			int result = figure.FindIndexByDistance(distance, out curveDistance);
			return result;
		}

		#endregion PUBLIC METHODS

	}
}
