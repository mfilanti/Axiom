using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;

namespace Axiom.GeoShape.Curves
{
	/// <summary>
	/// "Canonical spline" di terzo grado in 3D.
	/// <remark>La caratteristica principale è che il risultato PASSA per i punti di controllo e la curvatura viene 
	/// controllata da un parametro chiamato tensione. </remark>
	/// </summary>
	public class Spline3D : Curve3D
	{
		/// <summary>
		/// Indica con quanti punti interpolare la spline su ogni segmento. 
		/// Default = 4
		/// </summary>
		public static int InterpolationPointsPerSegment = 4;

		#region Properties
		/// <summary>
		/// Punti di controllo. 
		/// N.B. Occorrono almeno 3 punti di controllo. 
		/// </summary>
		public IEnumerable<Point3D> Points
		{
			get => _points;
			set
			{
				_points = new(value);
				ResetInterpolation();
			}
		}
		private List<Point3D> _points;

		/// <summary>
		/// Tensione
		/// </summary>
		public double Tension
		{
			get => _tension;
			set
			{
				_tension = value;
				ResetInterpolation();
			}
		}
		private double _tension;

		/// <summary>
		/// Indica se la spline è chiusa o aperta
		/// </summary>
		public bool Closed
		{
			get => _closed;
			set
			{
				_closed = value;
				ResetInterpolation();
			}
		}
		private bool _closed;

		/// <summary>
		/// Nel caso di spline aperta, la tangente sul punto iniziale e finale della Cardinal spline 
		/// viene calcolata in modo diverso dai punti intermedi, proprio perchè non esistono i punti 
		/// precedenti e successivi. 
		/// Con questo campo permetto di impostare un punto in più solo per la determinazione della 
		/// tangente iniziale. 
		/// Di default vale Point3D.NullPoint e in tal caso non viene utilizzato.
		/// </summary>
		public Point3D FirstExtraPoint
		{
			get => _firstExtraPoint;
			set
			{
				_firstExtraPoint = new(value);
				ResetInterpolation();
			}
		}
		private Point3D _firstExtraPoint;

		/// <summary>
		/// Nel caso di spline aperta, la tangente sul punto iniziale e finale della Cardinal spline 
		/// viene calcolata in modo diverso dai punti intermedi, proprio perchè non esistono i punti 
		/// precedenti e successivi. 
		/// Con questo campo permetto di impostare un punto in più solo per la determinazione della 
		/// tangente finale. 
		/// Di default vale Point3D.NullPoint e in tal caso non viene utilizzato.
		/// </summary>
		public Point3D LastExtraPoint
		{
			get => _lastExtraPoint;
			set
			{
				_lastExtraPoint = new(value);
				ResetInterpolation();
			}
		}
		private Point3D _lastExtraPoint;

		/// <summary>
		/// Punto di start. 
		/// </summary>
		public override Point3D StartPoint => _points[0];

		/// <summary>
		/// Punto di end. 
		/// </summary>
		public override Point3D EndPoint => Closed ? _points[0] : _points[_points.Count - 1];

		/// <summary>
		/// Tangente di start. 
		/// </summary>
		public override Vector3D StartTangent => InterpolationTangents[0];

		/// <summary>
		/// Tangente di end. 

		/// </summary>
		public override Vector3D EndTangent => InterpolationTangents[InterpolationTangents.Count - 1];

		/// <summary>
		/// Lunghezza del segmento. 

		/// </summary>
		public override double Length => Interpolation.Length;

		/// <summary>
		/// Punto medio. 

		/// </summary>
		public Point3D MiddlePoint => Evaluate(0.5);

		/// <summary>
		/// Vengono restituiti i punti di interpolazione. 
		/// L'interpolazione viene calcolata una volta e memorizzata. 
		/// Viene ricalcolata se cambiano i dati della spline o Spline3D.InterpolationPointsPerSegment. 

		/// </summary>
		[XmlIgnore]
		public Figure3D Interpolation
		{
			get
			{
				CheckInterpolation();
				return _interpolation;
			}
		}

		/// <summary>
		/// Viene restituita l'interpolazione con la tolleranza indicata in Spline3D.InterpolationTolerance. 
		/// L'interpolazione viene calcolata una volta e memorizzata. 
		/// Viene ricalcolata se cambiano i dati della spline o la tolleranza. 

		/// </summary>
		[XmlIgnore]
		public List<Point3D> InterpolationPoints
		{
			get
			{
				CheckInterpolation();
				return _interpolationPoints;
			}
		}

		/// <summary>
		/// Vengono restituite le tangenti di interpolazione. 
		/// L'interpolazione viene calcolata una volta e memorizzata. 
		/// Viene ricalcolata se cambiano i dati della spline o Spline3D.InterpolationPointsPerSegment. 

		/// </summary>
		[XmlIgnore]
		public List<Vector3D> InterpolationTangents
		{
			get
			{
				CheckInterpolation();
				return _interpolationTangents;
			}
		}

		#endregion

		#region Fields
		private int _lastInterpolationPointsPerSegment;
		private List<Point3D> _interpolationPoints;
		private List<Vector3D> _interpolationTangents;
		private Figure3D _interpolation;
		private Dictionary<int, int> _pointsInterpolationLink;
		#endregion PRIVATE FIELDS

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Spline3D()
		{
			_points = new List<Point3D>();
			Tension = 0.5;
			Closed = false;
			FirstExtraPoint = Point3D.NullPoint;
			LastExtraPoint = Point3D.NullPoint;
			_interpolation = null;
			_interpolationPoints = null;
			_interpolationTangents = null;
			_pointsInterpolationLink = null;
		}

		/// <summary>
		/// Costruttore. 
		/// N.B. Occorrono almeno 3 punti di controllo. 
		/// </summary>
		/// <param name="points"></param>
		public Spline3D(params double[] points)
		{
			_points = new List<Point3D>();
			for (int i = 0; i < points.Length - 1; i += 3)
				_points.Add(new Point3D(points[i], points[i + 1], points[i + 2]));

			Tension = 0.5;
			Closed = false;
			FirstExtraPoint = Point3D.NullPoint;
			LastExtraPoint = Point3D.NullPoint;
			_interpolation = null;
			_interpolationPoints = null;
			_interpolationTangents = null;
			_pointsInterpolationLink = null;
		}

		/// <summary>
		/// Costruttore. 
		/// N.B. Occorrono almeno 3 punti di controllo. 
		/// </summary>
		/// <param name="points"></param>
		public Spline3D(params Point3D[] points)
		{
			_points = new List<Point3D>(points);
			Tension = 0.5;
			Closed = false;
			FirstExtraPoint = Point3D.NullPoint;
			LastExtraPoint = Point3D.NullPoint;
			_interpolation = null;
			_interpolationPoints = null;
			_interpolationTangents = null;
			_pointsInterpolationLink = null;
		}

		/// <summary>
		/// Costruttore. 
		/// N.B. Occorrono almeno 3 punti di controllo. 
		/// </summary>
		/// <param name="points"></param>
		public Spline3D(IEnumerable<Point3D> points)
		{
			_points = new List<Point3D>(points);
			Tension = 0.5;
			Closed = false;
			FirstExtraPoint = Point3D.NullPoint;
			LastExtraPoint = Point3D.NullPoint;
			_interpolation = null;
			_interpolationPoints = null;
			_interpolationTangents = null;
			_pointsInterpolationLink = null;
		}
		#endregion CONSTRUCTORS

		#region Methods
		/// <summary>
		/// Clona la curva
		/// </summary>
		/// <returns></returns>
		public override Curve3D Clone()
		{
			Spline3D result = new Spline3D(_points)
			{
				Tension = Tension,
				Closed = Closed,
				FirstExtraPoint = FirstExtraPoint,
				LastExtraPoint = LastExtraPoint
			};
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
			if (curve is Spline3D)
			{
				Spline3D spline = curve as Spline3D;
				if (Tension == spline.Tension && Closed == spline.Closed && _points.Count == spline._points.Count)
				{
					result = true;
					for (int i = 0; i < _points.Count; i++)
					{
						if (_points[i].IsEquals(spline._points[i]) == false)
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
		/// <param name="tangent">Tangente</param>
		/// <returns></returns>
		public override Point3D Evaluate(double offset, out Vector3D tangent) => EvaluateAbs(offset * Length, out tangent);

		/// <summary>
		/// Evaluate assoluto. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la lunghezza da percorrere a partire dal punto di start. 
		/// Restituisce inoltre la tangente al punto valutato.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e Length il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <param name="tangent">Tangente</param>
		/// <returns></returns>
		public override Point3D EvaluateAbs(double offset, out Vector3D tangent) => EvaluateAbs(offset, out tangent, out _);

		/// <summary>
		/// Evaluate assoluto. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la lunghezza da percorrere a partire dal punto di start. 
		/// Restituisce inoltre la tangente al punto valutato. 
		/// Il parametro in uscita pointIndexOffset indica: con la parte intera l'indice del segmento iniziale 
		/// corrispondente, con il decimale in che percentuale siamo nel segmento.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e Length il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <param name="tangent">Tangente</param>
		/// <param name="pointIndexOffset">Indica a quale porzione dei segmenti iniziale corrisponde</param>
		/// <returns></returns>
		public Point3D EvaluateAbs(double offset, out Vector3D tangent, out double pointIndexOffset)
		{
			Point3D result = Interpolation.EvaluateAbs(offset, out tangent);
			double curveDistance;
			int index = Interpolation.FindIndexByDistance(offset, out curveDistance);
			if (index != -1)
			{
				double t = curveDistance / Interpolation[index].Length;
				Vector3D tn = InterpolationTangents[index];
				Vector3D tn1 = InterpolationTangents[index + 1];
				tangent = tn.Slerp(tn1, t, Vector3D.UnitZ);
				pointIndexOffset = 0;
				for (int i = 1; i < _points.Count; i++)
				{
					if (_pointsInterpolationLink[i] > index)
					{
						pointIndexOffset = i - 1;
						break;
					}
				}
				pointIndexOffset += (double)(index + t - _pointsInterpolationLink[(int)pointIndexOffset]) / (double)(_pointsInterpolationLink[(int)pointIndexOffset + 1] - _pointsInterpolationLink[(int)pointIndexOffset]);
			}
			else
			{
				if (offset < 0)
					pointIndexOffset = 0;
				else
					pointIndexOffset = _points.Count - 1;
			}
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

			ResetInterpolation();
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

			if (FirstExtraPoint.IsNull() == false)
				FirstExtraPoint = matrix.Multiply(FirstExtraPoint);

			if (LastExtraPoint.IsNull() == false)
				LastExtraPoint = matrix.Multiply(LastExtraPoint);

			ResetInterpolation();
		}

		/// <summary>
		/// Restituisce la curva scalata
		/// </summary>
		/// <param name="factor"></param>
		public override Curve3D Scale(double factor)
		{
			Spline3D result = (Spline3D)Clone();
			for (int i = 0; i < result._points.Count; i++)
				result._points[i] = factor * result._points[i];

			if (FirstExtraPoint.IsNull() == false)
				FirstExtraPoint = factor * FirstExtraPoint;

			if (LastExtraPoint.IsNull() == false)
				LastExtraPoint = factor * LastExtraPoint;

			ResetInterpolation();
			return result;
		}

		/// <summary>
		/// Restituisce la curva invertita
		/// </summary>
		/// <returns></returns>
		public override Curve3D Inverse()
		{
			Spline3D result = (Spline3D)Clone();
			result.SetInverse();

			return result;
		}

		/// <summary>
		/// Inverte la curva
		/// </summary>
		/// <returns></returns>
		public override void SetInverse()
		{
			_points.Reverse();
			Point3D tmp = FirstExtraPoint;
			FirstExtraPoint = LastExtraPoint;
			LastExtraPoint = tmp;
			ResetInterpolation();
		}

		/// <summary>
		/// Restituisce l'ABBox3D corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetABBox()
		{
			AABBox3D result = Interpolation.GetABBox();
			return result;
		}

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
			Spline3D result = null;
			if (start >= 0 || end <= Length)
			{
				// Teoricamente non è possibile trimmare una spline, quindi facciamo una approssimazione
				// Aumento però il numero di punti della spline risultante prendendo i punti di interpolazione di this
				Vector3D startTangent, endTangent;
				double startIndex, endIndex;
				Point3D startPoint, endPoint, middlePoint;
				startPoint = EvaluateAbs(start, out startTangent, out startIndex);
				endPoint = EvaluateAbs(end, out endTangent, out endIndex);
				middlePoint = EvaluateAbs(start + (end - start) / 2);

				result = new Spline3D();
				Figure3D approx = Interpolation.Clone().Trim(start, end);
				foreach (Curve3D curve in approx)
					result._points.Add(curve.StartPoint);

				result._points.Add(approx.EndPoint);
				if (result._points.Count == 2)
					result._points.Insert(1, middlePoint);

				result.AutomaticSetFirstLastTangents(startTangent, endTangent);
				result.ResetInterpolation();

				#region Tentativo di mantenere il numero di punti
				//result = (Spline3D)Clone();
				//Vector3D startTangent, endTangent;
				//double startIndex, endIndex;
				//Point3D startPoint, endPoint, middlePoint;
				//int index;
				//startPoint = result.EvaluateAbs(start, out startTangent, out startIndex);
				//endPoint = result.EvaluateAbs(end, out endTangent, out endIndex);
				//middlePoint = result.EvaluateAbs(start + (end - start) / 2);

				//index = (int)Math.Ceiling(endIndex);
				//result._points.RemoveRange(index, result._points.Count - index);
				//result._points.Insert(result._points.Count, endPoint);

				//if (startIndex > 0)
				//{
				//    index = (int)Math.Floor(startIndex);
				//    result._points.RemoveRange(0, index + 1);
				//    result._points.Insert(0, startPoint);
				//}
				//if (result._points.Count == 2)
				//    result._points.Insert(1, middlePoint);

				//result.AutomaticSetFirstLastTangents(startTangent, endTangent);
				//result.ResetInterpolation();
				#endregion Tentativo di mantenere il numero di punti
			}
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
			offset = 0;
			result = Interpolation.IsOnFigure(point, tolerance, out offset);
			return result;
		}

		/// <summary>
		/// Imposta in modo automatico FirstExtraPoint e LastExtraPoint in maniera tale da ottenere 
		/// le tangenti iniziali e finali indicate. 
		/// Vale solo per spline aperte.
		/// </summary>
		/// <param name="startTangent"></param>
		/// <param name="endTangent"></param>
		public void AutomaticSetFirstLastTangents(Vector3D startTangent, Vector3D endTangent)
		{
			FirstExtraPoint = _points[1] - (_points[2] - _points[0]).Length * startTangent.Normalize();
			LastExtraPoint = _points[_points.Count - 2] + (_points[_points.Count - 1] - _points[_points.Count - 3]).Length * endTangent.Normalize();
		}

		/// <summary>
		/// Forza il ricalcolo della interpolazione
		/// </summary>
		public void ResetInterpolation()
		{
			_interpolationPoints = null;
			_interpolation = null;
		}

		/// <summary>
		/// Restituisce la tangente relativa al punto indicato. 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Vector3D TangentAt(int i)
		{
			Vector3D result = InterpolationTangents[_pointsInterpolationLink[i]];
			return result;
		}

		#endregion 

		#region Private methods

		private void CheckInterpolation()
		{
			if (_interpolation == null || _lastInterpolationPointsPerSegment != Spline3D.InterpolationPointsPerSegment)
			{
				_lastInterpolationPointsPerSegment = Spline3D.InterpolationPointsPerSegment;
				_interpolationPoints = GetInterpolation(_lastInterpolationPointsPerSegment, out _interpolationTangents, out _pointsInterpolationLink);
				_interpolation = new Figure3D(_interpolationPoints);
			}
		}

		/// <summary>
		/// Restituisce la figura formata da una serie di punti che interpolano la spline. 
		/// Con numero di punti per segmento specificati.
		/// </summary>
		/// <param name="pointsPerSegment"></param>
		/// <param name="tangents"></param>
		/// <param name="pointsInterpolationLink"></param>
		/// <returns></returns>
		private List<Point3D> GetInterpolation(int pointsPerSegment, out List<Vector3D> tangents, out Dictionary<int, int> pointsInterpolationLink)
		{
			List<Point3D> result = new List<Point3D>();
			tangents = new List<Vector3D>();
			pointsInterpolationLink = new Dictionary<int, int>();
			pointsInterpolationLink.Add(0, 0);
			List<Vector3D> segmentTangents;
			for (int i = 0; i < _points.Count; i++)
			{
				if (i > 0)
					pointsInterpolationLink.Add(i, result.Count - 1);

				if (i == 0)
				{
					// Primo segmento: da 0 a 1
					Point3D previousPoint = Closed ? _points[_points.Count - 1] : _points[0];
					if (!Closed && FirstExtraPoint.IsNull() == false) previousPoint = FirstExtraPoint;
					result.AddRange(CardinalSegment(pointsPerSegment, previousPoint, _points[0], _points[1], _points[2], Tension, true, out segmentTangents));
					tangents.AddRange(segmentTangents);
				}
				else if (i == _points.Count - 2)
				{
					// Ultimo segmento: da cnt-1 a cnt
					Point3D nextPoint = Closed ? _points[0] : _points[i + 1];
					if (!Closed && LastExtraPoint.IsNull() == false) nextPoint = LastExtraPoint;
					result.AddRange(CardinalSegment(pointsPerSegment, _points[i - 1], _points[i], _points[i + 1], nextPoint, Tension, false, out segmentTangents));
					tangents.AddRange(segmentTangents);
				}
				else if (i == _points.Count - 1)
				{
					// Solo se chiuso, segmento in più: da cnt a 0
					// N.B. Non accorpare questa if alla else if precedente, è diverso!
					if (Closed)
					{
						result.AddRange(CardinalSegment(pointsPerSegment, _points[i - 1], _points[i], _points[0], _points[1], Tension, false, out segmentTangents));
						tangents.AddRange(segmentTangents);
						result.Add(result[0]);
						tangents.Add(tangents[0]);
					}
				}
				else
				{
					// Segmento intermedio (standard): da i a i+1
					result.AddRange(CardinalSegment(pointsPerSegment, _points[i - 1], _points[i], _points[i + 1], _points[i + 2], Tension, false, out segmentTangents));
					tangents.AddRange(segmentTangents);
				}
			}
			return result;
		}

		private List<Point3D> CardinalSegment(int pointsCount, Point3D p0, Point3D p1, Point3D p2, Point3D p3, double tension, bool addFirstPoint, out List<Vector3D> tangents)
		{
			List<Point3D> result = new List<Point3D>();
			tangents = new List<Vector3D>();
			double sx1 = tension * (p2.X - p0.X);
			double sy1 = tension * (p2.Y - p0.Y);
			double sz1 = tension * (p2.Z - p0.Z);
			double sx2 = tension * (p3.X - p1.X);
			double sy2 = tension * (p3.Y - p1.Y);
			double sz2 = tension * (p3.Z - p1.Z);
			double ax = sx1 + sx2 + 2 * p1.X - 2 * p2.X;
			double ay = sy1 + sy2 + 2 * p1.Y - 2 * p2.Y;
			double az = sz1 + sz2 + 2 * p1.Z - 2 * p2.Z;
			double bx = -2 * sx1 - sx2 - 3 * p1.X + 3 * p2.X;
			double by = -2 * sy1 - sy2 - 3 * p1.Y + 3 * p2.Y;
			double bz = -2 * sz1 - sz2 - 3 * p1.Z + 3 * p2.Z;
			double cx = sx1;
			double cy = sy1;
			double cz = sz1;
			double dx = p1.X;
			double dy = p1.Y;
			double dz = p1.Z;

			int start = addFirstPoint ? 0 : 1;
			for (int i = start; i < pointsCount; i++)
			{
				double t = (double)i / (pointsCount - 1);
				double x = ax * t * t * t + bx * t * t + cx * t + dx;
				double y = ay * t * t * t + by * t * t + cy * t + dy;
				double z = az * t * t * t + bz * t * t + cz * t + dz;
				double tx = 3 * ax * t * t + 2 * bx * t + cx;
				double ty = 3 * ay * t * t + 2 * by * t + cy;
				double tz = 3 * az * t * t + 2 * bz * t + cz;
				result.Add(new Point3D(x, y, z));
				tangents.Add(new Vector3D(tx, ty, tz).Normalize());
			}
			return result;
		}
		#endregion 
	}
}
