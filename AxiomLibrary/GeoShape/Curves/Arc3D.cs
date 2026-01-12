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
	/// Classe che rappresenta un arco 3D
	/// </summary>
	public class Arc3D : Curve3D
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
		/// Angolo di fine (in radianti).
		/// Valori validi da 0 a 2*PI (nel set accetto tutto e riporto io nel range valido).
		/// (N.B. Esiste il parametro CounterClockWise che indica il verso dell'arco)
		/// </summary>
		public double EndAngle
		{
			get { return _endAngle; }
			set
			{
				_endAngle = value % (2 * Math.PI);
				if (_endAngle < 0)
					_endAngle += 2 * Math.PI;
			}
		}
		private double _endAngle;

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
		/// Lunghezza dell'arco
		/// </summary>
		public override double Length => Radius * SpanAngle;

		/// <summary>
		/// Punto medio dell'arco. 

		/// </summary>
		public Point3D MiddlePoint => Evaluate(0.5);

		/// <summary>
		/// L'angolo del punto medio (in radianti). 

		/// </summary>
		public double MidAngle
		{
			get
			{
				double result, spanAngle;
				spanAngle = SpanAngle;
				if (CounterClockWise == true)
					result = StartAngle + spanAngle / 2;
				else
					result = StartAngle - spanAngle / 2;
				// Riporto su 0 - 2PI
				result = result % (2 * Math.PI);
				if (result < 0)
					result += 2 * Math.PI;
				return result;
			}
		}

		/// <summary>
		/// Angolo interno all'arco (in radianti). 

		/// </summary>
		public double SpanAngle
		{
			get
			{
				double result;
				if (CounterClockWise == true)
				{
					if (EndAngle > StartAngle)
						result = EndAngle - StartAngle;
					else
						result = EndAngle + 2 * Math.PI - StartAngle;
				}
				else
				{
					if (EndAngle < StartAngle)
						result = StartAngle - EndAngle;
					else
						result = StartAngle + Math.PI * 2 - EndAngle;
				}
				return result;
			}
		}
		#endregion PUBLIC FIELDS

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Arc3D() : this(new Point3D(), 0, 0, 0, true, RTMatrix.Identity)
		{
		}

		/// <summary>
		/// Centro, raggio, angolo iniziale, angolo finale, verso di percorrenza, matrice
		/// </summary>
		/// <param name="center">Centro</param>
		/// <param name="radius">Raggio</param>
		/// <param name="startAngle">Angolo iniziale (rad)</param>
		/// <param name="endAngle">Angolo finale (rad)</param>
		/// <param name="counterClockWise">Verso di percorrenza</param>
		/// <param name="rMatrix">Matrice</param>
		public Arc3D(Point3D center, double radius, double startRadAngle, double endRadAngle, bool counterClockWise, RTMatrix rMatrix)
		{
			Center = center;
			Radius = radius;
			StartAngle = startRadAngle;
			EndAngle = endRadAngle;
			CounterClockWise = counterClockWise;
			RMatrix = rMatrix;
		}

		/// <summary>
		/// Centro, raggio, angolo iniziale, angolo finale, verso di percorrenza, matrice
		/// </summary>
		/// <param name="center">Centro</param>
		/// <param name="radius">Raggio</param>
		/// <param name="startAngle">Angolo iniziale (rad)</param>
		/// <param name="endAngle">Angolo finale (rad)</param>
		/// <param name="counterClockWise">Verso di percorrenza</param>
		/// <param name="rMatrix">Matrice</param>
		public Arc3D(Point3D center, double radius, double startRadAngle, double endRadAngle, bool counterClockWise):
			this(center,radius,startRadAngle,endRadAngle,counterClockWise,RTMatrix.Identity)
		{
			
		}

		/// <summary>
		/// Tre punti
		/// </summary>
		/// <param name="start">Punto iniziale</param>
		/// <param name="middle">Punto medio</param>
		/// <param name="end">Punto finale</param>
		public Arc3D(Point3D start, Point3D middle, Point3D end):this()
		{
			Set3Points(start, middle, end);
		}

		/// <summary>
		/// Punto iniziale, finale, centro
		/// </summary>
		/// <param name="start">Punto iniziale</param>
		/// <param name="end">PUnto finale</param>
		/// <param name="center">Centro</param>
		/// <param name="counterClockWise">Verso di percorrenza</param>
		public Arc3D(Point3D start, Point3D end, Point3D center, bool counterClockWise)
		{
			RMatrix = RTMatrix.FromNormal((start - center).Cross(end - center));
			Center = center;
			Radius = (start - center).Length;
			StartAngle = (start - center).Angle(RMatrix.GetVector(0), RMatrix.GetVector(2));
			EndAngle = (end - center).Angle(RMatrix.GetVector(0), RMatrix.GetVector(2));
			CounterClockWise = counterClockWise;
		}

		/// <summary>
		/// Inizio, Fine, tangente
		/// </summary>
		/// <param name="start">PUnto iniziale</param>
		/// <param name="end">Punto finale</param>
		/// <param name="startTangent">Tangente nel punto iniziale</param>
		public Arc3D(Point3D start, Point3D end, Vector3D startTangent)
		{
			Vector3D vES = end - start;
			Vector3D vESn = vES.Normalize();
			Vector3D vZ = vESn.Cross(startTangent).Normalize();
			Vector3D vESperp = vZ.Cross(vESn);

			double dX = Math.Abs(startTangent.Dot(vESn));
			double dY = Math.Abs(startTangent.Dot(vESperp));
			double d = vES.Length / 2 * dX / dY;
			Vector3D dirCenter;
			if (startTangent.Dot(vESn) >= 0)
			{
				// Arco di ampiezza minore o uguale a 180
				if (startTangent.Dot(vESperp) >= 0)
				{
					// Arco orario
					CounterClockWise = false;
					dirCenter = vESperp.Negate();
				}
				else
				{
					// Arco antiorario
					CounterClockWise = true;
					dirCenter = vESperp;
				}
			}
			else
			{
				// Arco di ampiezza maggiore a 180
				if (startTangent.Dot(vESperp) >= 0)
				{
					// Arco orario
					CounterClockWise = false;
					dirCenter = vESperp;
				}
				else
				{
					// Arco antiorario
					CounterClockWise = true;
					dirCenter = vESperp.Negate();
				}
			}

			Center = start + 0.5 * (end - start) + d * dirCenter;
			Radius = (start - Center).Length;
			Vector3D vX = (start - Center).Normalize();
			RMatrix = RTMatrix.FromVectors(vX, vZ.Cross(vX), vZ);

			StartAngle = 0;
			if (start.IsEquals(end) == true)
				EndAngle = StartAngle;
			else
				EndAngle = (end - Center).Angle(vX, vZ);
		}

		#endregion CONSTRUCTORS

		#region Public Methods
		/// <summary>
		/// Clona la curva
		/// </summary>
		/// <returns></returns>
		public override Curve3D Clone() => new Arc3D(Center, Radius, StartAngle, EndAngle, CounterClockWise, RMatrix);

		/// <summary>
		/// Confronta i due archi
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve)
		{
			bool result = false;
			if (curve is Arc3D arc)
			{
				result =
					Center.IsEquals(arc.Center) &&
					Radius.IsEquals(arc.Radius) &&
					StartAngle.IsEquals(arc.StartAngle) &&
					EndAngle.IsEquals(arc.EndAngle) &&
					CounterClockWise == arc.CounterClockWise &&
					RMatrix.IsEquals(arc.RMatrix);
			}

			return result;
		}

		/// <summary>
		/// Confronta i due archi considerando la tolleranza
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve, double tolerance)
		{
			bool result = false;
			if (curve is Arc3D arc)
			{
				result =
					Center.IsEquals(arc.Center, tolerance) &&
					Radius.IsEquals(arc.Radius, tolerance) &&
					StartAngle.IsEquals(arc.StartAngle, tolerance) &&
					EndAngle.IsEquals(arc.EndAngle, tolerance) &&
					CounterClockWise == arc.CounterClockWise &&
					RMatrix.IsEquals(arc.RMatrix);
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
		/// <param name="tangent">Tangente</param>
		/// <returns></returns>
		public override Point3D Evaluate(double offset, out Vector3D tangent)
		{
			double absOffset = offset * Length;
			double radOffsetAngle = absOffset / Radius;
			return EvaluateAngle(radOffsetAngle, out tangent);
		}

		/// <summary>
		/// Evaluate assoluto.
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la lunghezza da percorrere a partire dal
		/// punto di start.
		/// Restituisce inoltre la tangente al punto valutato.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e Length il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <param name="tangent">Tangente</param>
		/// <returns></returns>
		public override Point3D EvaluateAbs(double offset, out Vector3D tangent)
		{
			double radOffsetAngle = offset / Radius;
			return EvaluateAngle(radOffsetAngle, out tangent);
		}

		/// <summary>
		/// Evaluate angolare.
		/// Restituisce un punto che appartiene all'arco oppure al suo prolungamento 
		/// in base al parametro che indica l'angolo da percorrere a partire dal
		/// punto di start verso il punto di end (quindi con il verso dell'arco). 
		/// </summary>
		/// <param name="radOffsetAngle">Angolo da sommare a quello iniziale (in radianti)</param>
		/// <returns></returns>
		public Point3D EvaluateAngle(double offsetRadAngle, out Vector3D tangent)
		{
			Point3D result = new Point3D();
			tangent = Vector3D.Zero;
			if (CounterClockWise == true)
			{
				result.X = Radius * Math.Cos(StartAngle + offsetRadAngle);
				result.Y = Radius * Math.Sin(StartAngle + offsetRadAngle);
				result.Z = 0;
				tangent.X = -Math.Sin(StartAngle + offsetRadAngle);
				tangent.Y = Math.Cos(StartAngle + offsetRadAngle);
				tangent.Z = 0;
				tangent.SetNormalize();
			}
			else
			{
				result.X = Radius * Math.Cos(StartAngle - offsetRadAngle);
				result.Y = Radius * Math.Sin(StartAngle - offsetRadAngle);
				result.Z = 0;
				tangent.X = Math.Sin(StartAngle - offsetRadAngle);
				tangent.Y = -Math.Cos(StartAngle - offsetRadAngle);
				tangent.Z = 0;
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
		public override void Move(Vector3D traslation)
		{
			Center = Center + traslation;
		}

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
		public override Curve3D Scale(double factor) => new Arc3D(factor * Center, factor * Radius, StartAngle, EndAngle, CounterClockWise, RMatrix);


		/// <summary>
		/// Restituisce la curva invertita
		/// </summary>
		/// <returns></returns>
		public override Curve3D Inverse() => new Arc3D(Center, Radius, EndAngle, StartAngle, !CounterClockWise, RMatrix);

		/// <summary>
		/// Inverte la curva
		/// </summary>
		/// <returns></returns>
		public override void SetInverse()
		{
			double angle = StartAngle;
			StartAngle = EndAngle;
			EndAngle = angle;
			CounterClockWise = !CounterClockWise;
		}

		/// <summary>
		/// Restituisce l'ABBox3D corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetABBox()
		{
			List<Point3D> points = new List<Point3D>();
			points.Add(StartPoint);
			points.Add(EndPoint);
			Plane3D plane = new Plane3D(RMatrix.GetVector(2), Center);
			Vector3D projX = (plane.Project(plane.Location + Vector3D.UnitX) - plane.Location).Normalize();
			Vector3D projY = (plane.Project(plane.Location + Vector3D.UnitY) - plane.Location).Normalize();
			Vector3D projZ = (plane.Project(plane.Location + Vector3D.UnitZ) - plane.Location).Normalize();


			if (projX.IsEquals(Vector3D.Zero) == false)
			{
				Point3D pointA = Center + Radius * projX;
				if (IsOnCurve(pointA) == true) points.Add(pointA);
				Point3D pointB = Center - Radius * projX;
				if (IsOnCurve(pointB) == true) points.Add(pointB);
			}
			if (projY.IsEquals(Vector3D.Zero) == false)
			{
				Point3D pointC = Center + Radius * projY;
				if (IsOnCurve(pointC) == true) points.Add(pointC);
				Point3D pointD = Center - Radius * projY;
				if (IsOnCurve(pointD) == true) points.Add(pointD);
			}
			if (projZ.IsEquals(Vector3D.Zero) == false)
			{
				Point3D pointE = Center + Radius * projZ;
				if (IsOnCurve(pointE) == true) points.Add(pointE);
				Point3D pointF = Center - Radius * projZ;
				if (IsOnCurve(pointF) == true) points.Add(pointF);
			}
			AABBox3D result = AABBox3D.FromPoints(points);
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
			Arc3D result = null;
			double length = Length;
			if (end.IsEquals(length))
				end = length;

			if (start >= 0 && end <= length)
			{
				result = Clone() as Arc3D;
				int sign = CounterClockWise ? 1 : -1;
				if (start > 0)
					result.StartAngle = StartAngle + sign * start / Radius;
				if (end < length)
					result.EndAngle = StartAngle + sign * end / Radius;
			}
			return result;
		}

		/// <summary>
		/// Impone all'arco di passare per tre punti (viene restituito false solo se i tre punti sono allineati)
		/// </summary>
		/// <param name="pStart">Punto di inizio arco</param>
		/// <param name="pInt">Punto intermedio dell'arco (non necessariamente il punto medio)</param>
		/// <param name="pEnd">Punto di fine arco</param>
		/// <returns>Viene restituito false solo se i tre punti sono allineati</returns>
		public bool Set3Points(Point3D pStart, Point3D pInt, Point3D pEnd)
		{
			bool result = false;
			Vector3D v1 = pEnd - pInt;
			Vector3D v2 = pStart - pInt;
			Vector3D vZ = v1.Cross(v2).Normalize();
			// Se il vettore è zero allora i tre punti sono allineati
			if (vZ.IsEquals(Vector3D.Zero) == false)
			{
				Vector3D vY = vZ.Perpendicular();
				Vector3D vX = vY.Cross(vZ);
				// Riporto sul piano i 3 punti e chiamo il metodo corrispondente in 2D
				RTMatrix matrix = RTMatrix.Identity;
				matrix.SetFromAxes(vX, vY, vZ);
				RTMatrix inverse = matrix.Inverse();
				Point3D pStartXY = inverse * pStart;
				Point3D pIntXY = inverse * pInt;
				Point3D pEndXY = inverse * pEnd;

				Arc3D arc2 = new Arc3D();
				result = arc2.Set3Points(pStartXY, pIntXY, pEndXY);
				if (result == true)
				{
					Center = matrix * new Point3D(arc2.Center.X, arc2.Center.Y, pStartXY.Z);
					Radius = arc2.Radius;
					StartAngle = arc2.StartAngle;
					EndAngle = arc2.EndAngle;
					CounterClockWise = arc2.CounterClockWise;
					RMatrix = new(matrix);
				}
			}
			return result;
		}

		/// <summary>
		/// Trova i punti di intersezione tra un arco e una linea, possono essere uno 
		/// o al massimo due se linea e arco sono sullo stesso piano
		/// </summary>
		/// <param name="line">Linea con cui provare l'intersezione</param>
		/// <param name="intersections">Punti di intersezione trovati</param>
		/// <returns>Se sono intersecati o meno</returns>
		public bool Intersection(Line3D line, out List<Point3D> intersections)
		{
			bool result = false;
			intersections = new List<Point3D>();

			Vector3D arcNormal = RMatrix.GetVector(2);
			Plane3D arcPlane = new Plane3D(arcNormal, Center);

			bool insideLine;
			Point3D linePlaneIntersection;
			bool notParallel = arcPlane.IntersectLine(line, out insideLine, out linePlaneIntersection);

			// se l'intersezione appartiene al segmento
			if (notParallel)
			{
				if (insideLine)
				{
					if (IsOnCurve(linePlaneIntersection))
					{
						result = true;
						intersections.Add(linePlaneIntersection);
					}
				}
			}
			else
			{   // se la linea è parallela al piano dell'arco, mi affido all'intersezione 2d
				if (arcPlane.Distance(line.StartPoint).IsEquals(0))
				{
					// ottengo la matrice di rotazione del piano
					RTMatrix rtMatrix = arcPlane.GetRTMatrix();
					// ottengo la matrice per riportare l'arco sul piano XY
					RTMatrix inverseRtMatrix = rtMatrix.Inverse();

					Line3D lineClone = (Line3D)line.Clone();
					lineClone.ApplyRT(inverseRtMatrix);

					// creo un arco 2d orientato sul piano
					Arc3D arcClone = (Arc3D)Clone();
					arcClone.ApplyRT(inverseRtMatrix);
					Arc3D arc2d = arcClone.ToArc2D();

					List<Point3D> intersections2d;
					result = arc2d.Intersection(lineClone, out intersections2d);

					// riposiziono tutte le intersezioni in 3D
					foreach (Point3D currentIntersection2d in intersections2d)
					{
						result = true;
						Point3D currentIntersection3d = (Point3D)currentIntersection2d;
						currentIntersection3d = rtMatrix * currentIntersection3d;
						intersections.Add(currentIntersection3d);
					}
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
			RTMatrix inverseRtMatrix = ((RTMatrix)RMatrix).Inverse();

			// Passo tutto al 2d
			Point3D center2d = (inverseRtMatrix * Center);
			Point3D point2d = (inverseRtMatrix * point);
			Arc3D arc2d = new Arc3D(center2d, Radius, StartAngle, EndAngle, CounterClockWise, RTMatrix.Identity);

			return arc2d.IsOnCurve(point2d, tolerance, out offset);
		}

		/// <summary>
		/// Restituisce un Arc3D partendo da un Arc2D e Z
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Arc3D FromArc2D(Arc3D arc2, double z)
		{
			Point3D center = (Point3D)arc2.Center;
			center.Z = z;
			return new Arc3D(center, arc2.Radius, arc2.StartAngle, arc2.EndAngle, arc2.CounterClockWise, RTMatrix.Identity);
		}

		/// <summary>
		/// Converte in Arc2D ignorando la componente Z. 
		/// Se l'arco è ruotato nello spazio restituisce null.
		/// </summary>
		/// <returns></returns>
		public Arc3D ToArc2D()
		{
			Arc3D result;
			Point3D pCenter2 = new Point3D(Center.X, Center.Y);
			double xAngle, yAngle, zAngle;
			RMatrix.ToEulerAnglesXYZ(true, out xAngle, out yAngle, out zAngle);
			if (MathExtensions.IsEquals(xAngle % Math.PI, 0) && MathExtensions.IsEquals(yAngle % Math.PI, 0))
				result = new Arc3D(StartPoint, MiddlePoint, EndPoint);
			else
				result = null;

			return result;
		}
		#endregion PUBLIC METHODS

	}

}
