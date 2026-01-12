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
	/// Classe arco di ellisse tridimensionale
	/// </summary>
	public class Ellipse3D : Curve3D
	{
		#region Properties
		/// <summary>
		/// Centro
		/// </summary>
		public Point3D Center { get; set; }

		/// <summary>
		/// Raggio maggiore
		/// </summary>
		public double A { get; set; }

		/// <summary>
		/// Raggio minore
		/// </summary>
		public double B { get; set; }

		/// <summary>
		/// Indica il verso di percorrenza dall'angolo 
		/// di inizio a quello di fine (di default è true)
		/// </summary>
		public bool CounterClockWise { get; set; }

		/// <summary>
		/// Angolo di rotazione in radianti
		/// Indica la rotazione del raggio maggiore
		/// </summary>
		public double RotationA { get; set; }

		/// <summary>
		/// Angolo di inizio in radianti.
		/// Valori validi da 0 a 2*PI (nel set accetto tutto e riporto io nel range valido).
		/// (N.B. Esiste il parametro CounterClockWise che indica il verso dell'ellisse)
		/// </summary>
		public double StartAngle
        {
            get => _startAngle;
            set
            {
                if (value.IsEquals(2 * Math.PI) == true)
                {
                    _startAngle = 2 * Math.PI;
                }
                else
                {
                    _startAngle = value % (2 * Math.PI);
                    if (_startAngle < 0)
                        _startAngle += 2 * Math.PI;
                }
            }
        }
        private double _startAngle;

		/// <summary>
		/// Angolo di fine in radianti.
		/// Valori validi da 0 a 2*PI (nel set accetto tutto e riporto io nel range valido).
		/// (N.B. Esiste il parametro CounterClockWise che indica il verso dell'ellisse)
		/// </summary>
		public double EndAngle
		{
			get { return _endAngle; }
			set
			{
				if (value.IsEquals(2 * Math.PI) == true)
				{
					_endAngle = 2 * Math.PI;
				}
				else
				{
					_endAngle = value % (2 * Math.PI);
					if (_endAngle < 0)
						_endAngle += 2 * Math.PI;
				}
			}
		}
		private double _endAngle;

		/// <summary>
		/// Matrice di rotazione (la traslazione è in Center)
		/// </summary>
		public RTMatrix RMatrix { get; set; }

		/// <summary>
		/// Punto di start
		/// </summary>
		public override Point3D StartPoint => EvaluateAngle(0);

		/// <summary>
		/// Punto di end
		/// </summary>
		public override Point3D EndPoint => EvaluateAngle(SpanAngle);

		/// <summary>
		/// Tangente di start (sola lettura)
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
		/// Tangente di end (sola lettura)
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
		/// Lunghezza dell'ellisse
		/// N.B. Funzione approssimata tramite sommatoria. 
		/// Dell'ellisse completa ci sono diverse approx: 
		/// Es: pi (a + b) [ 1 + 3 h / (10 + (4 - 3 h)1/2 ) ]
		/// </summary>
		public override double Length
		{
			get
			{
				double l = 0;
				double spanAngle = SpanAngle;
				double step = 0.01; // un centesimo di grado
				Vector3D tangent;
				Point3D point1 = EvaluateAngle(0, out tangent);
				Point3D point2;
				for (double offsetAng = 0; offsetAng < spanAngle;)
				{
					offsetAng += step;
					if (offsetAng > spanAngle)
						offsetAng = spanAngle;
					point2 = EvaluateAngle(offsetAng, out tangent);
					l += (point2 - point1).Length;
					point1 = point2;
				}
				return l;
			}
		}

		/// <summary>
		/// L'angolo del punto medio (sola lettura)
		/// </summary>
		public double MidAngle
		{
			get
			{
				double result;
				double spanAngle = SpanAngle;
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
		/// Angolo interno all'ellisse in radianti
		/// </summary>
		public double SpanAngle
		{
			get
			{
				double result = 0;
				if (EndAngle != StartAngle)
				{
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
				}
				return result;
			}
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Ellipse3D()
		{
			Center = new Point3D();
			A = 0;
			B = 0;
			RotationA = 0;
			StartAngle = 0;
			EndAngle = 0;
			CounterClockWise = true;
			RMatrix = RTMatrix.Identity;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="center"></param>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <param name="startAngle"></param>
		/// <param name="endAngle"></param>
		/// <param name="counterClockWise"></param>
		public Ellipse3D(Point3D center, double a, double b, double rotationA, double startAngle, double endAngle, bool counterClockWise, RTMatrix rMatrix)
		{
			Center = center;
			A = a;
			B = b;
			RotationA = rotationA;
			StartAngle = startAngle;
			EndAngle = endAngle;
			CounterClockWise = counterClockWise;
			RMatrix = rMatrix;
		}
		#endregion

		#region PUBLIC PROPERTIES

		#endregion PUBLIC PROPERTIES

		#region PUBLIC METHODS
		/// <summary>
		/// Clona la curva
		/// </summary>
		/// <returns></returns>
		public override Curve3D Clone() => new Ellipse3D(Center, A, B, RotationA, StartAngle, EndAngle, CounterClockWise, RMatrix);

		/// <summary>
		/// Confronta le due ellissi
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve)
		{
			bool result = false;
			if (curve is Ellipse3D ellipse)
			{
				result =
					Center.IsEquals(ellipse.Center) &&
					A.IsEquals(ellipse.A) &&
					B.IsEquals(ellipse.B) &&
					RotationA.IsEquals(ellipse.RotationA) &&
					StartAngle.IsEquals(ellipse.StartAngle) &&
					EndAngle.IsEquals(ellipse.EndAngle) &&
					CounterClockWise == ellipse.CounterClockWise &&
					RMatrix.IsEquals(ellipse.RMatrix);
			}

			return result;
		}

		/// <summary>
		/// Confronta le due ellissi considerando la tolleranza
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public override bool IsEquals(Curve3D curve, double tolerance)
		{
			bool result = false;
			if (curve is Ellipse3D ellipse)
			{
				result =
					Center.IsEquals(ellipse.Center, tolerance) &&
					A.IsEquals(ellipse.A, tolerance) &&
					B.IsEquals(ellipse.B, tolerance) &&
					RotationA.IsEquals(ellipse.RotationA, tolerance) &&
					StartAngle.IsEquals(ellipse.StartAngle, tolerance) &&
					EndAngle.IsEquals(ellipse.EndAngle, tolerance) &&
					CounterClockWise == ellipse.CounterClockWise &&
					RMatrix.IsEquals(ellipse.RMatrix);
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
			double absOffset = offset * Length;
			return EvaluateAbs(absOffset, out tangent);
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
			double radOffsetAngle = AbsOffsetToOffsetAngle(offset);
			return EvaluateAngle(radOffsetAngle, out tangent);
		}

		private double AbsOffsetToOffsetAngle(double offset)
		{
			// N.B. Funzione approssimata
			double l = 0;
			double step = 0.001; // un millesimo di grado
			Vector3D tangent;
			Point3D point1 = EvaluateAngle(0, out tangent);
			Point3D point2;
			double offsetAng = 0;
			while (l < offset)
			{
				offsetAng += step;
				point2 = EvaluateAngle(offsetAng, out tangent);
				l += point2.Distance(point1);
				point1 = point2;
			}
			return offsetAng;
		}

		private double OffsetAngleToAbsOffset(double offsetAng)
		{
			double offset = 0;
			double step = 0.001; // un millesimo di grado
			Vector3D tangent;
			Point3D point1 = EvaluateAngle(0, out tangent);
			Point3D point2;
			double actualOffsetAng = 0;
			while (actualOffsetAng < offsetAng)
			{
				actualOffsetAng += step;
				point2 = EvaluateAngle(actualOffsetAng, out tangent);
				offset += point2.Distance(point1);
				point1 = point2;
			}
			return offset;
		}

		/// <summary>
		/// Dato un angolo in radianti che identifica un punto sull'ellisse, restituisce l'angolo calcolato sulla circonferenza di raggio A
		/// che identifica la proiezione del punto su di questa (con vettore (0,1)).
		/// </summary>
		/// <param name="ellipseRadAngle"></param>
		/// <returns></returns>
		public double EllipseAngleToCircularAngle(double ellipseRadAngle)
		{
			Point3D point = new Point3D(0, 0);

			// Riportiamo l'angolo tra 0 e 360 per comodità
			ellipseRadAngle = ellipseRadAngle.AngleToRange02PI();

			double a2 = A * A;
			double b2 = B * B;
			double tan2 = Math.Tan(ellipseRadAngle) * Math.Tan(ellipseRadAngle);
			point.X = A * B / (Math.Sqrt(b2 + a2 * tan2));
			point.Y = A * B * Math.Tan(ellipseRadAngle) / (Math.Sqrt(b2 + a2 * tan2));
			// La formula sopra vale solo per il primo e quarto quadrante, altrimenti va invertito
			if (ellipseRadAngle > Math.PI / 2 && ellipseRadAngle < 1.5 * Math.PI)
				point = -point;

			double x = ((Vector3D)point).Dot(Vector3D.UnitX) / A;
			double y = ((Vector3D)point).Dot(Vector3D.UnitY) / B;
			double teta = Math.Atan2(y, x);

			return teta;
		}

		/// <summary>
		/// Dato un angolo in radianti, identifica un punto sull'ellisse tramite la proiezione (con vettore (0,1)) del punto ottenuto 
		/// da questo angolo sulla circonferenza di raggio A. Restituisce l'angolo corrispondente al punto sull'ellisse.
		/// </summary>
		/// <param name="circleRadAngle"></param>
		/// <returns></returns>
		public double CircularAngleToEllipseAngle(double circleRadAngle)
		{
			double angle;
			double x = A * Math.Cos(circleRadAngle);
			double y = B * Math.Sin(circleRadAngle);
			Point3D point = new Point3D(x, y);
			Vector3D vect = (Vector3D)point;
			angle = vect.Angle(Vector3D.UnitX, Vector3D.UnitZ);

			return angle;
		}

		/// <summary>
		/// Evaluate angolare.
		/// Restituisce un punto che appartiene all'ellisse oppure al suo prolungamento 
		/// in base al parametro che indica l'angolo da percorrere a partire dal
		/// punto di start verso il punto di end (quindi con il verso dell'ellisse). 
		/// </summary>
		/// <param name="radOffsetAngle">Angolo da sommare a quello iniziale (in radianti)</param>
		/// <returns></returns>
		public Point3D EvaluateAngle(double radOffsetAngle, out Vector3D tangent)
		{
			Point3D result;

			Point3D point = new Point3D();
			Vector3D tangent2 = Vector3D.Zero;
			double angle;
			if (CounterClockWise == true)
				angle = StartAngle + radOffsetAngle;
			else
				angle = StartAngle - radOffsetAngle;

			// Riportiamo l'angolo tra 0 e 360 per comodità
			angle = angle.AngleToRange02PI();

			// PI e 1.5*PI sono casi limite per cui la tan va all'infinito per cui vanno gestiti a parte
			if (angle == Math.PI / 2 || angle == 1.5 * Math.PI)
			{
				point.X = A * Math.Cos(angle);
				point.Y = B * Math.Sin(angle);
			}
			else
			{
				double a2 = A * A;
				double b2 = B * B;
				double tan2 = Math.Tan(angle) * Math.Tan(angle);
				point.X = A * B / (Math.Sqrt(b2 + a2 * tan2));
				point.Y = A * B * Math.Tan(angle) / (Math.Sqrt(b2 + a2 * tan2));
				// La formula sopra vale solo per il primo e quarto quadrante, altrimenti va invertito
				if (angle > Math.PI / 2 && angle < 1.5 * Math.PI)
					point = -point;
			}

			double tanAngle = EllipseAngleToCircularAngle(angle);

			tangent2.X = -Math.Sin(tanAngle);
			tangent2.Y = B / A * Math.Cos(tanAngle);
			if (CounterClockWise == false)
				tangent2.SetNegate();

			tangent2.SetNormalize();

			point = (Point3D)((Vector3D)point).Rotate(Vector3D.UnitZ, RotationA);
			result = RMatrix.Multiply((Point3D)point);
			result = result + (Vector3D)Center;
			tangent = RMatrix.Multiply((Vector3D)(tangent2.Rotate(Vector3D.UnitZ, RotationA)));

			return result;
		}

		/// <summary>
		/// Evaluate angolare.
		/// Restituisce un punto che appartiene all'ellisse oppure al suo prolungamento 
		/// in base al parametro che indica l'angolo da percorrere a partire dal
		/// punto di start verso il punto di end (quindi con il verso dell'ellisse). 
		/// </summary>
		/// <param name="radOffsetAngle">Angolo da sommare a quello iniziale (in radianti)</param>
		/// <returns></returns>
		public Point3D EvaluateAngle(double radOffsetAngle) => EvaluateAngle(radOffsetAngle, out _);

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
		/// La componente Z viene ignorata
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
		public override Curve3D Scale(double factor) => new Ellipse3D(factor * Center, factor * A, factor * B, RotationA, StartAngle, EndAngle, CounterClockWise, RMatrix);

		/// <summary>
		/// Restituisce la curva invertita
		/// </summary>
		/// <returns></returns>
		public override Curve3D Inverse() => new Ellipse3D(Center, A, B, RotationA, EndAngle, StartAngle, !CounterClockWise, RMatrix);

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
		/// Restituisce l'ABBox3D corrispondente. 
		/// N.B. Funzione approssimata
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetABBox()
		{
			List<Point3D> points = new List<Point3D>();
			double angStep = Math.PI / 1800; // un decimo di grado
											 // Aggiungo il primo punto
			points.Add(StartPoint);
			double spanAngle = SpanAngle;
			for (double angOffset = angStep; angOffset < spanAngle; angOffset += angStep)
				points.Add(EvaluateAngle(angOffset));

			// Aggiungo l'ultimo punto
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
			Ellipse3D result = null;
			double length = Length;
			if (end.IsEquals( length)) end = length;

			if (start >= 0 && end <= length)
			{
				result = Clone() as Ellipse3D;
				int sign = CounterClockWise ? 1 : -1;
				if (start > 0)
					result.StartAngle = StartAngle + sign * AbsOffsetToOffsetAngle(start);
				if (end < length)
					result.EndAngle = StartAngle + sign * AbsOffsetToOffsetAngle(end);
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

			Vector3D direction = ((Vector3D)(point - Center)).Normalize();
			Vector3D directionStart = ((Vector3D)(StartPoint - Center)).Normalize();
			Vector3D refZ = (CounterClockWise ? Vector3D.UnitZ : Vector3D.NegativeUnitZ);
			double offsetAngle = direction.Angle(directionStart, refZ);

			// Riporto l'angolo tra 0 e 2*PI
			offsetAngle = offsetAngle.AngleToRange02PI();

			if (offsetAngle <= SpanAngle)
			{
				if (EvaluateAngle(offsetAngle).IsEquals(point, tolerance) == true)
				{
					result = true;
					offset = OffsetAngleToAbsOffset(offsetAngle) / Length;
				}
			}

			return result;
		}

		/// <summary>
		/// Restituisce un Ellipse3D partendo da un Ellipse2D e Z
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Ellipse3D FromEllipse2D(Ellipse3D ellipse2, double z)
		{
			Point3D center = (Point3D)ellipse2.Center;
			center.Z = z;
			return new Ellipse3D(center, ellipse2.A, ellipse2.B, ellipse2.RotationA, ellipse2.StartAngle, ellipse2.EndAngle, ellipse2.CounterClockWise, RTMatrix.Identity);
		}

		/// <summary>
		/// Converte in Ellipse2D ignorando la componente Z. 
		/// Se l'ellisse è ruotata nello spazio restituisce null.
		/// </summary>
		/// <returns></returns>
		public Ellipse3D ToEllipse2D()
		{
			Ellipse3D result;
			Point3D pCenter2 = new Point3D(Center.X, Center.Y);
			double xAngle, yAngle, zAngle;
			RMatrix.ToEulerAnglesXYZ(true, out xAngle, out yAngle, out zAngle);
			if (xAngle == 0 && yAngle == 0)
				result = new Ellipse3D(pCenter2, A, B, RotationA, StartAngle + zAngle, EndAngle + zAngle, CounterClockWise, RTMatrix.Identity);
			else
				result = null;

			return result;
		}
		#endregion PUBLIC METHODS
	}
}
