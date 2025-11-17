using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;

namespace Axiom.GeoShape.Curves
{
	/// <summary>
	/// Classe segmento lineare tridimensionale
	/// </summary>
	[Serializable]
	public class Line3D : Curve3D
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Punto di start
		/// </summary>
		public override Point3D StartPoint => PStart;

		/// <summary>
		/// Punto di end
		/// </summary>
		public override Point3D EndPoint => PEnd;

		/// <summary>
		/// Tangente di start (sola lettura)
		/// </summary>
		public override Vector3D StartTangent => (PEnd - PStart).Normalize();

		/// <summary>
		/// Tangente di end (sola lettura)
		/// </summary>
		public override Vector3D EndTangent => (PEnd - PStart).Normalize();

		/// <summary>
		/// Lunghezza del segmento
		/// </summary>
		public override double Length => PStart.Distance(PEnd);

		/// <summary>
		/// Punto medio (sola lettura)
		/// </summary>
		public Point3D MiddlePoint => PStart + (0.5 * (PEnd - PStart));

		/// <summary>
		/// Punto di inizio
		/// </summary>
		public Point3D PStart { get; set; }

		/// <summary>
		/// Punto di fine
		/// </summary>
		public Point3D PEnd { get; set; }

		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Line3D()
		{
			PStart = new Point3D();
			PEnd = new Point3D();
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="z1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="z2"></param>
		public Line3D(double x1, double y1, double z1, double x2, double y2, double z2)
		{
			PStart = new Point3D(x1, y1, z1);
			PEnd = new Point3D(x2, y2, z2);
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public Line3D(Point3D start, Point3D end)
		{
			PStart = new(start);
			PEnd = new(end);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Specchia la curva sull'asse X solo in 2D
		/// </summary>
		/// <returns></returns>
		public override Curve3D MirrorX()
		{
			Line3D result = ToLine2D();
			result.PStart.Y *= -1;
			result.PEnd.Y *= -1;
			return result;
		}

		/// <summary>
		/// Specchia la curva sull'asse Y solo in 2D
		/// </summary>
		/// <returns></returns>
		public override Curve3D MirrorY()
		{
			Line3D result = ToLine2D();
			result.PStart.X *= -1;
			result.PEnd.X *= -1;
			return result;
		}

		/// <summary>
		/// Distanza punto - linea sul piano XY
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public override double Dist(Point3D point)
		{
			double result = Double.MaxValue;
			bool isInside;

			Point3D projected = Projection(point, out isInside);

			if (isInside)
			{
				result = point.Distance(projected);
			}
			else
			{
				double distStart = point.Distance(this.StartPoint);
				double distEnd = point.Distance(this.EndPoint);
				result = distStart <= distEnd ? distStart : distEnd;
			}

			return result;
		}

		/// <summary>
		/// Clona la curva
		/// </summary>
		/// <returns></returns>
		public override Curve3D Clone()
		{
			return new Line3D(PStart, PEnd);
		}

		/// <summary>
		/// Confronta le due linee considerando la tolleranza
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve)
		{
			bool result = false;
			if (curve is Line3D line)
			{
				result = PStart.IsEquals(line.PStart) && PEnd.IsEquals(line.PEnd);
			}

			return result;
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
			if (curve is Line3D line)
			{
				result = PStart.IsEquals(line.PStart, tolerance) && PEnd.IsEquals(line.PEnd, tolerance);
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
			Point3D result = new Point3D
			{
				X = PStart.X + offset * (PEnd.X - PStart.X),
				Y = PStart.Y + offset * (PEnd.Y - PStart.Y),
				Z = PStart.Z + offset * (PEnd.Z - PStart.Z)
			};
			tangent = PEnd - PStart;
			tangent.SetNormalize();

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
			double relOffset = offset / Length;
			return Evaluate(relOffset, out tangent);
		}

		/// <summary>
		/// Trasla la curva della quantità indicata
		/// </summary>
		/// <param name="traslation">Vettore che indica la traslazione</param>
		public override void Move(Vector3D traslation)
		{
			PStart = PStart + traslation;
			PEnd = PEnd + traslation;
		}

		/// <summary>
		/// Ruota e trasla la curva
		/// </summary>
		/// <param name="matrix"></param>
		public override void ApplyRT(RTMatrix matrix)
		{
			PStart = matrix.Multiply(PStart);
			PEnd = matrix.Multiply(PEnd);
		}

		/// <summary>
		/// Restituisce la curva scalata
		/// </summary>
		/// <param name="factor"></param>
		public override Curve3D Scale(double factor) => new Line3D(factor * PStart, factor * PEnd);

		/// <summary>
		/// Restituisce la curva invertita
		/// </summary>
		/// <returns></returns>
		public override Curve3D Inverse() => new Line3D(PEnd, PStart);

		/// <summary>
		/// Inverte la curva
		/// </summary>
		/// <returns></returns>
		public override void SetInverse()
		{
			var ps = PStart;
			var pe = PEnd;
			MathUtils.Swap<Point3D>(ref ps, ref pe);
			PStart = ps;
			PEnd = pe;
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
			Line3D result = null;
			double length = Length;
			if (end.IsEquals(length))
				end = length;

			if (start >= 0 && end <= length)
			{
				result = Clone() as Line3D;
				if (start > 0)
					result.PStart = EvaluateAbs(start);
				if (end < length)
					result.PEnd = EvaluateAbs(end);
			}
			return result;
		}

		/// <summary>
		/// Proiezione del punto sulla linea (può stare o meno sul segmento)
		/// </summary>
		/// <param name="point">Punto da proiettare</param>
		/// <param name="isInside">Indica se il punto proiettato appartiene al segmento</param>
		/// <param name="offset">Indica la distanza percentuale dal punto di start</param>
		/// <returns></returns>
		public Point3D Projection(Point3D point, out bool isInside, out double offset)
		{
			Point3D result;
			Vector3D vecLine = PEnd - PStart;
			Vector3D vecPoint = point - PStart;
			double l = vecLine.SetNormalize();

			double proj = vecPoint.Dot(vecLine);
			offset = proj / l;

			if (offset >= 0 && offset <= 1)
				isInside = true;
			else
				isInside = false;

			result = PStart + proj * vecLine;

			return result;
		}

		/// <summary>
		/// Proiezione del punto sulla linea (può stare o meno sul segmento)
		/// </summary>
		/// <param name="point">Punto da proiettare</param>
		/// <param name="isInside">Indica se il punto proiettato appartiene al segmento</param>
		/// <returns></returns>
		public Point3D Projection(Point3D point, out bool isInside) => Projection(point, out isInside, out _);

		/// <summary>
		/// Proiezione del punto sulla linea (può stare o meno sul segmento)
		/// </summary>
		/// <param name="point">Punto da proiettare</param>
		/// <returns></returns>
		public Point3D Projection(Point3D point) => Projection(point, out _);

		/// <summary>
		/// Distanza punto - linea
		/// </summary>
		/// <param name="point">Punto con cui calcolare la distanza</param>
		/// <returns></returns>
		public double DistancePerp(Point3D point)
		{
			Point3D projection = Projection(point);
			return point.Distance(projection);
		}

		/// <summary>
		/// Indica se la proiezione del punto dato è interno al segmento.
		/// </summary>
		/// <param name="point">Punto da verificare</param>
		/// <returns></returns>
		public bool ProjectionIsInside(Point3D point)
		{
			bool result;
			Projection(point, out result);

			return result;
		}

		/// <summary>
		/// Indica se il punto dato è interno al segmento.
		/// </summary>
		/// <param name="point">Punto da verificare</param>
		/// <returns></returns>
		public bool IsInside(Point3D point)
		{
			bool result = false;
			bool projInside;
			Point3D proj = Projection(point, out projInside);
			result = (proj.IsEquals(point) && projInside);
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
			offset = 0;

			// distanza dalla linea
			double distance = DistancePerp(point);

			// se è sulla stessa retta
			if (distance.IsEquals(0, tolerance))
			{
				// controlla se sta tra start ed end
				double startDistance = (point - PStart).Dot((PEnd - PStart).Normalize());

				if (startDistance.IsEquals(0, tolerance)) startDistance = 0;
				if (startDistance.IsEquals(Length, tolerance)) startDistance = Length;

				offset = startDistance / Length;
				if (offset >= 0 && offset <= 1)
					result = true;
			}
			return result;
		}

		/// <summary>
		/// Intersezione tra 2 Line. 
		/// Viene restituito true l'intersezione è interna a entrambi i segmenti. 
		/// </summary>
		/// <param name="line">Linea con cui determinare le intersezioni</param>
		/// <param name="intersection">Intersezione</param>
		/// <returns>Indica se c'è o meno intersezione</returns>
		public bool Intersection(Line3D line, out Point3D intersection) => Intersection(line, out _, out _, out intersection);

		/// <summary>
		/// Intersezione tra 2 Line. 
		/// Viene restituito true se uA e uB sono compresi tra 0 e 1. 
		/// </summary>
		/// <param name="line">Linea con cui determinare le intersezioni</param>
		/// <param name="uA"></param>
		/// <param name="uB"></param>
		/// <returns>Indica se c'è o meno intersezione</returns>
		public bool Intersection(Line3D line, out double uA, out double uB) => Intersection(line, out uA, out uB, out _);

		/// <summary>
		/// Intersezione tra 2 Line. 
		/// Viene restituito true se uA e uB sono compresi tra 0 e 1. 
		/// http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
		/// </summary>
		/// <param name="line">Linea con cui determinare le intersezioni</param>
		/// <param name="uA"></param>
		/// <param name="uB"></param>
		/// <param name="intersection">Intersezione</param>
		/// <returns>Indica se c'è o meno intersezione</returns>
		public bool Intersection(Line3D line, out double uA, out double uB, out Point3D intersection)
		{
			bool result = false;
			uA = double.PositiveInfinity;
			uB = double.PositiveInfinity;
			intersection = Point3D.NullPoint;

			Vector3D v13 = PStart - line.PStart;
			Vector3D v43 = line.PEnd - line.PStart;
			Vector3D v21 = PEnd - PStart;

			double d1343 = v13.X * v43.X + v13.Y * v43.Y + v13.Z * v43.Z;
			double d4321 = v43.X * v21.X + v43.Y * v21.Y + v43.Z * v21.Z;
			double d1321 = v13.X * v21.X + v13.Y * v21.Y + v13.Z * v21.Z;
			double d4343 = v43.X * v43.X + v43.Y * v43.Y + v43.Z * v43.Z;
			double d2121 = v21.X * v21.X + v21.Y * v21.Y + v21.Z * v21.Z;

			double denom = d2121 * d4343 - d4321 * d4321;
			if (denom.IsEquals(0) == false)
			{
				double numer = d1343 * d4321 - d1321 * d4343;
				uA = numer / denom;
				uB = (d1343 + d4321 * uA) / d4343;
				Point3D evaluateA = Evaluate(uA);
				Point3D evaluateB = line.Evaluate(uB);
				if (evaluateA.IsEquals(evaluateB))
				{
					intersection = evaluateA;
					result = ((uA >= 0 && uA <= 1) && (uB >= 0 && uB <= 1));
					// TODO: valutare l'opzione per considerare o meno gli estremi
					//if (Curve2D.ConsiderExtremesInIntersections == false)
					//	result = ((uA > 0 && uA < 1) && (uB > 0 && uB < 1));
				}
			}

			return result;
		}


		/// <summary>
		/// Converte in Line2D ignorando la componente Z
		/// </summary>
		/// <returns></returns>
		public Line3D ToLine2D()
		{
			Line3D result;
			Point3D pStart2 = new Point3D(PStart.X, PStart.Y);
			Point3D pEnd2 = new Point3D(PEnd.X, PEnd.Y);
			result = new Line3D(pStart2, pEnd2);

			return result;
		}

		/// <summary>
		/// Nell'approssimare le linee in spline viene utilizzato un algoritmo opportuno. 
		/// Il parametro P1 è il segmento base (massimo 0,5mm) e il parametro P2 è un moltiplicatore da 1 a 10. 
		/// Verranno presi i punti in questo modo fino ad arrivare alla metà del segmento (P1=0.1 e P2=10): 
		/// 0;0.1;1;10... 
		/// Fino a che non si arriva alla metà del segmento. 
		/// Se il parametro P3 è a true allora verrà creata l'altra metà in maniera speculare, altrimenti verrà 
		/// aggiunto solo il punto finale.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="p3"></param>
		/// <returns></returns>
		public List<double> ApproxLineToSplineOffsets(double p1, double p2, bool p3)
		{
			List<double> result = new List<double>();
			result.Add(0);
			double lenght = Length;
			double locP1 = p1 > 0 ? p1 : 0.1;
			double locP2 = p2 > 0 ? p2 : 5;
			for (double offset = locP1; offset < lenght / 2;)
			{
				result.Add(offset);
				offset *= locP2;
			}
			if (p3)
			{
				List<double> mirror = new List<double>();
				for (int i = result.Count - 1; i >= 0; i--)
					mirror.Add(lenght - result[i]);

				if (mirror[0].IsEquals(result[result.Count - 1]))
					mirror.RemoveAt(0);

				result.AddRange(mirror);
			}
			else
			{
				result.Add(lenght);
			}
			if (result.Count == 2)
				result.Insert(1, lenght / 2);

			return result;
		}
		#endregion PUBLIC METHODS

	}
}
