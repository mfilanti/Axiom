using Axiom.GeoMath;
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
	/// Elica 3D
	/// </summary>
	public class Helix3D : Curve3D
	{
		#region Properties
		/// <summary>
		/// Centro
		/// </summary>
		public Point3D Center { get; set; }

		/// <summary>
		/// Raggio
		/// </summary>
		public double Radius { get; set; }

		/// <summary>
		/// Angolo di inizio (in radianti).
		/// Valori validi da 0 a 2*PI (nel set accetto tutto e riporto io nel range valido). 
		/// (N.B. Esiste il parametro CounterClockWise che indica il verso dell'arco)
		/// </summary>
		public double StartAngle
		{
			get { return _startAngle; }
			set
			{
				_startAngle = value % (2 * Math.PI);
				if (_startAngle < 0)
					_startAngle += 2 * Math.PI;
			}
		}
		private double _startAngle;

		/// <summary>
		/// Angolo totale effettuato dall'elica (in radianti)
		/// </summary>
		public double SpanAngle { get; set; }

		/// <summary>
		/// Profondità totale dell'elica.
		/// Se positiva va verso "Z-" altrimenti verso "Z+"
		/// </summary>
		public double Depth { get; set; }

		/// <summary>
		/// Indica il verso di percorrenza dall'angolo 
		/// di inizio a quello di fine (di default è true)
		/// </summary>
		public bool CounterClockWise { get; set; }

		/// <summary>
		/// Matrice di rotazione (la traslazione è in Center)
		/// </summary>
		public RTMatrix RMatrix { get; set; }

		/// <summary>
		/// Punto di start. 
		
		/// </summary>
		public override Point3D StartPoint => Evaluate(0);

		/// <summary>
		/// Punto di end. 
		
		/// </summary>
		public override Point3D EndPoint => Evaluate(1);

		/// <summary>
		/// Tangente di start. 
		
		/// </summary>
		public override Vector3D StartTangent
		{
			get
			{
				Evaluate(0, out Vector3D result);
				return result;
			}
		}

		/// <summary>
		/// Tangente di end. 
		
		/// </summary>
		public override Vector3D EndTangent
		{
			get
			{
				Evaluate(1, out Vector3D result);
				return result;
			}
		}

		/// <summary>
		/// Lunghezza dell'elica. 
		/// </summary>
		public override double Length
		{
			get
			{
				double result = SpanAngle * Radius;
				result = Math.Sqrt(result * result + Depth * Depth);
				return result;
			}
		}

		/// <summary>
		/// Passo dell'elica. 
		/// </summary>
		public double Pitch => Depth / SpanAngle * 2 * Math.PI;
		#endregion

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Helix3D()
		{
			Center = new Point3D();
			Radius = 0;
			Depth = 0;
			StartAngle = 0;
			SpanAngle = 0;
			CounterClockWise = true;
			RMatrix = RTMatrix.Identity;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="depth"></param>
		/// <param name="startAngle"></param>
		/// <param name="spanAngle"></param>
		/// <param name="counterClockwise"></param>
		/// <param name="rMatrix"></param>
		public Helix3D(Point3D center, double radius, double depth, double startRadAngle, double spanRadAngle, bool counterClockwise, RTMatrix rMatrix)
		{
			Center = new(center);
			Radius = radius;
			Depth = depth;
			StartAngle = startRadAngle;
			SpanAngle = spanRadAngle;
			CounterClockWise = counterClockwise;
			RMatrix = rMatrix;
		}
		#endregion CONSTRUCTORS


		#region Methods
		/// <summary>
		/// Clona la curva
		/// </summary>
		/// <returns></returns>
		public override Curve3D Clone() => new Helix3D(Center, Radius, Depth, StartAngle, SpanAngle, CounterClockWise, RMatrix);

		/// <summary>
		/// Confronta le due eliche
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve)
		{
			bool result = false;
			if (curve is Helix3D helix3D)
			{
				if (Center.IsEquals(helix3D.Center) &&
					Radius.IsEquals(helix3D.Radius) &&
					Depth.IsEquals(helix3D.Depth) &&
					StartAngle.IsEquals(helix3D.StartAngle) &&
					SpanAngle.IsEquals(helix3D.SpanAngle) &&
					CounterClockWise == helix3D.CounterClockWise &&
					RMatrix.IsEquals(helix3D.RMatrix))
					result = true;
			}

			return result;
		}

		/// <summary>
		/// Confronta le due eliche considerando la tolleranza
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve, double tolerance)
		{
			bool result = false;
			if (curve is Helix3D helix3D)
			{
				if (Center.IsEquals(helix3D.Center, tolerance) &&
					Radius.IsEquals( helix3D.Radius, tolerance) &&
					Depth.IsEquals(helix3D.Depth, tolerance) &&
					StartAngle.IsEquals(helix3D.StartAngle, tolerance) &&
					SpanAngle.IsEquals(helix3D.SpanAngle, tolerance) &&
					CounterClockWise == helix3D.CounterClockWise &&
					RMatrix.IsEquals(helix3D.RMatrix))
					result = true;
			}

			return result;
		}

		private double FromAbsOffsetToRadOffset(double absOffset)
		{
			double l = Length;
			double l2 = SpanAngle * Radius;
			double result = 0;
			if (l > 0)
				result = l2 * absOffset / l;

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
			double absOffset = offset * Length;
			double radOffsetAngle = FromAbsOffsetToRadOffset(absOffset) / Radius;
			return EvaluateAngle(radOffsetAngle, out tangent);
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
			double radOffsetAngle = FromAbsOffsetToRadOffset(offset) / Radius;
			return EvaluateAngle(radOffsetAngle, out tangent);
		}

		/// <summary>
		/// Evaluate angolare. 
		/// Restituisce un punto che appartiene all'elica oppure al suo prolungamento 
		/// in base al parametro che indica l'angolo da percorrere a partire dal 
		/// punto di start verso il punto di end (quindi con il verso dell'elica). 
		/// </summary>
		/// <param name="radOffsetAngle">Angolo da sommare a quello iniziale (in radianti)</param>
		/// <returns></returns>
		public Point3D EvaluateAngle(double offsetRadAngle, out Vector3D tangent)
		{
			Point3D result = new Point3D();
			tangent = Vector3D.Zero;
			double offsetZ = -Depth * offsetRadAngle / SpanAngle;
			double tangentZ = (new Vector3D(Length, 0, -Depth)).Normalize().Z;

			if (CounterClockWise == true)
			{
				result.X = Radius * Math.Cos(StartAngle + offsetRadAngle);
				result.Y = Radius * Math.Sin(StartAngle + offsetRadAngle);
				result.Z = offsetZ;
				tangent.X = -Math.Sin(StartAngle + offsetRadAngle);
				tangent.Y = Math.Cos(StartAngle + offsetRadAngle);
				tangent.Z = tangentZ;
				tangent.SetNormalize();
			}
			else
			{
				result.X = Radius * Math.Cos(StartAngle - offsetRadAngle);
				result.Y = Radius * Math.Sin(StartAngle - offsetRadAngle);
				result.Z = offsetZ;
				tangent.X = Math.Sin(StartAngle - offsetRadAngle);
				tangent.Y = -Math.Cos(StartAngle - offsetRadAngle);
				tangent.Z = tangentZ;
				tangent.SetNormalize();
			}
			result = RMatrix * result;
			result = result + (Vector3D)Center;
			tangent = RMatrix * tangent;

			return result;
		}

		/// <summary>
		/// Trasla la curva della quantità indicata
		/// </summary>
		/// <param name="traslation">Vettore che indica la traslazione</param>
		public override void Move(Vector3D traslation) => Center = Center + traslation;

		/// <summary>
		/// Ruota e trasla la curva
		/// </summary>
		/// <param name="matrix"></param>
		public override void ApplyRT(RTMatrix matrix)
		{
			Center = matrix * Center;
			RMatrix = matrix * RMatrix;
		}

		/// <summary>
		/// Restituisce la curva scalata
		/// </summary>
		/// <param name="factor"></param>
		public override Curve3D Scale(double factor) => new Helix3D(factor * Center, factor * Radius, factor * Depth, StartAngle, SpanAngle, CounterClockWise, RMatrix);

		/// <summary>
		/// Restituisce la curva invertita
		/// </summary>
		/// <returns></returns>
		public override Curve3D Inverse()
		{
			Point3D newCenter = Center + RMatrix * new Vector3D(0, 0, -Depth);
			return new Helix3D(newCenter, Radius, -Depth, GetEndAngle(true), SpanAngle, !CounterClockWise, RMatrix);
		}

		/// <summary>
		/// Inverte la curva
		/// </summary>
		/// <returns></returns>
		public override void SetInverse()
		{
			Depth = -1 * Depth;
			StartAngle = GetEndAngle(true);
			CounterClockWise = !CounterClockWise;
		}

		/// <summary>
		/// Restituisce l'ABBox3D corrispondente. 
		/// Approssimato: calcolato quello di un ipotetico cilindro descritto dall'elica.
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetABBox()
		{
			Arc3D circle1 = new Arc3D(Center, Radius, 0, 2 * Math.PI - 0.001, true, RMatrix);
			Arc3D circle2 = new Arc3D(Center + Depth * Vector3D.NegativeUnitZ, Radius, 0, 2 * Math.PI - 0.001, true, RMatrix);
			AABBox3D result = circle1.GetABBox();
			result.Union(circle2.GetABBox());
			return result;
		}

		/// <summary>
		/// Restituisce una parte della curva. 
		/// Se uno dei 2 parametri è minore di zero o maggiore della lunghezza
		/// viene restituito null
		/// </summary>
		/// <param name="start">Inizio: distanza dal punto di start</param>
		/// <param name="end">Fine: distanza dal punto di start</param>
		/// <returns></returns>
		public override Curve3D Trim(double start, double end)
		{
			Helix3D result = null;
			double length = Length;
			if (start >= 0 && end <= length)
			{
				result = Clone() as Helix3D;
				if (start > 0)
				{
					double alpha = result.SpanAngle / result.Depth;
					double deltaAngle = start / Radius;
					result.StartAngle += deltaAngle;
					result.Depth -= deltaAngle / alpha;
					result.Center = result.Center - deltaAngle / alpha * result.RMatrix.GetVector(2);
				}
				if (end < length)
				{
					double alpha = result.SpanAngle / result.Depth;
					double deltaAngle = end / Radius;
					result.Depth -= deltaAngle / alpha;
				}
			}
			return result;
		}

		/// <summary>
		/// Valuta se il punto passato appartiene alla curva.
		/// Restituisce inoltre il valore percentuale (0 - 1) che rappresenta la distanza dal punto di start.
		/// Se non appartiene alla curva restituisce un valore negativo (-1).
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="tolerance">Tolleranza</param>
		/// <param name="offset">In uscita: distanza percentuale dal punto di start</param>
		/// <returns></returns>
		public override bool IsOnCurve(Point3D point, double tolerance, out double offset)
		{
			bool result = false;
			offset = -1;
			Line3D line = new Line3D(Center, Center - Depth * RMatrix.GetVector(2));
			bool isInside;
			Point3D proj = line.Projection(point, out isInside);
			if (proj.IsEquals(point, tolerance) && isInside)
			{
				double alpha = SpanAngle / Depth;
				double offsetRadAngle = proj.Distance(Center) / alpha;
				Vector3D tangent;
				Point3D eval = EvaluateAngle(offsetRadAngle, out tangent);
				if (eval.IsEquals(point, tolerance))
				{
					result = true;
					offset = offsetRadAngle * Radius;
				}
			}

			return result;
		}

		/// <summary>
		/// Restituisce un Helix3D partendo da un Helix2D e Z
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Helix3D FromHelix2D(Helix3D helix2, double z)
		{
			Point3D center = (Point3D)helix2.Center;
			center.Z = z;
			return new Helix3D(center, helix2.Radius, helix2.Depth, helix2.StartAngle, helix2.SpanAngle, helix2.CounterClockWise, RTMatrix.Identity);
		}

		/// <summary>
		/// Converte in Helix2D ignorando la componente Z
		/// </summary>
		/// <returns></returns>
		public Helix3D ToHelix2D()
		{
			Helix3D result;
			Point3D pCenter2 = new Point3D(Center.X, Center.Y);
			double xAngle, yAngle, zAngle;
			RMatrix.ToEulerAnglesXYZ(true, out xAngle, out yAngle, out zAngle);

			result = new Helix3D(pCenter2, Radius, Depth, StartAngle + zAngle, SpanAngle, CounterClockWise, RTMatrix.Identity);

			return result;
		}

		/// <summary>
		/// Angolo di fine in radianti.
		/// Viene calcolato a partire da StartAngle sommando SpanAngle
		/// </summary>
		/// <param name="standardRange">Se a true viene restituito un valore tra 0 e 2*PI</param>
		/// <returns></returns>
		public double GetEndAngle(bool standardRange)
		{
			double result = _startAngle + SpanAngle;
			if (standardRange == true)
			{
				result = result % (2 * Math.PI);
				if (result < 0)
					result += 2 * Math.PI;
			}

			return result;
		}

		#endregion 

	}
}
