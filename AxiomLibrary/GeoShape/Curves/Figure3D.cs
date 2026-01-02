using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axiom.GeoShape.Curves
{
	/// <summary>
	/// Classe che rappresenta un insieme di Curve. 
	/// </summary>
	public class Figure3D : List<Curve3D>, ICloneable
	{
		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Figure3D()
			: base()
		{ }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="curves"></param>
		public Figure3D(params Curve3D[] curves)
			: base()
		{
			AddRange(curves);
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="curves"></param>
		public Figure3D(IEnumerable<Curve3D> curves)
			: base()
		{
			AddRange(curves);
		}

		/// <summary>
		/// Costruttore. 
		/// A partire da una serie di segmenti consecutivi formanti un poligono (aperto o chiuso). 
		/// I parametri rappresentano i vertici del poligono. 
		/// Il numero di parametri deve essere almeno 2, cioè almeno 2 punti.
		/// </summary>
		/// <param name="points"></param>
		public Figure3D(params Point3D[] points)
			: base()
		{
			AddPolygon(points);
		}

		/// <summary>
		/// Costruttore. 
		/// A partire da una serie di segmenti consecutivi formanti un poligono (aperto o chiuso). 
		/// I parametri rappresentano i vertici del poligono. 
		/// Il numero di parametri deve essere pari altrimenti viene creata una figura vuota. 
		/// Il numero di parametri deve essere almeno 6, cioè almeno 2 punti.
		/// </summary>
		/// <param name="points"></param>
		public Figure3D(params double[] points)
			: base()
		{
			AddPolygon(points);
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="points"></param>
		public Figure3D(List<Point3D> points)
			: base()
		{
			AddPolygon(points);
		}
		#endregion CONSTRUCTORS

		#region Properties
		/// <summary>
		/// Punto di start
		/// </summary>
		public Point3D StartPoint
		{
			get
			{
				Point3D result = Point3D.NullPoint;
				if (Count > 0)
					result = this[0].StartPoint;

				return result;
			}
		}

		/// <summary>
		/// Punto di end
		/// </summary>
		public Point3D EndPoint
		{
			get
			{
				Point3D result = Point3D.NullPoint;
				if (Count > 0)
					result = this[Count - 1].EndPoint;

				return result;
			}
		}

		/// <summary>
		/// Tangente di start
		/// </summary>
		public Vector3D StartTangent
		{
			get
			{
				Vector3D result = Vector3D.NullVector;
				if (Count > 0)
					result = this[0].StartTangent;

				return result;
			}
		}

		/// <summary>
		/// Tangente di end
		/// </summary>
		public Vector3D EndTangent
		{
			get
			{
				Vector3D result = Vector3D.NullVector;
				if (Count > 0)
					result = this[Count - 1].EndTangent;

				return result;
			}
		}

		/// <summary>
		/// Somma di tutte le lunghezza delle curve
		/// </summary>
		public double Length => (double)this.Sum(p => p.Length);
		#endregion

		#region Methods
		/// <summary>
		/// Clona la Figure3D
		/// </summary>
		/// <returns></returns>
		public Figure3D Clone()
		{
			Figure3D cloneFigure = new Figure3D();

			foreach (Curve3D curve in this)
				cloneFigure.Add(curve.Clone());

			return cloneFigure;
		}

		/// <summary>
		/// Aggiunge una serie di segmenti consecutivi formanti un poligono (aperto o chiuso).
		/// I parametri rappresentano i vertici del poligono.
		/// Il numero di parametri deve essere multiplo di 3 altrimenti restituisce false.
		/// Il numero di parametri deve essere almeno 6, cioè almeno 2 punti
		/// </summary>
		/// <param name="points">Terne di double che rappresentano i vertici del poligono</param>
		/// <returns></returns>
		public bool AddPolygon(params double[] points)
		{
			bool result = false;
			if (((points.Length % 3) == 0) && (points.Length >= 6))
			{
				result = true;
				for (int i = 0; i <= points.Length - 6; i = i + 3)
					Add(new Line3D(points[i], points[i + 1], points[i + 2], points[i + 3], points[i + 4], points[i + 5]));
			}

			return result;
		}

		/// <summary>
		/// Aggiunge una serie di segmenti consecutivi formanti un poligono (aperto o chiuso).
		/// I parametri rappresentano i vertici del poligono.
		/// Il numero di parametri deve essere almeno 2, cioè almeno 2 punti
		/// </summary>
		/// <param name="points">Vertici del poligono</param>
		public void AddPolygon(params Point3D[] points)
		{
			if (points.Length >= 2)
			{
				for (int i = 0; i < points.Length - 1; i++)
					Add(new Line3D(points[i], points[i + 1]));
			}
		}

		/// <summary>
		/// Aggiunge una serie di segmenti consecutivi formanti un poligono (aperto o chiuso).
		/// I parametri rappresentano i vertici del poligono.
		/// Il numero di parametri deve essere almeno 2, cioè almeno 2 punti
		/// </summary>
		/// <param name="points">Vertici del poligono</param>
		public void AddPolygon(List<Point3D> points)
		{
			if (points.Count >= 2)
			{
				for (int i = 0; i < points.Count - 1; i++)
					Add(new Line3D(points[i], points[i + 1]));
			}
		}

		/// <summary>
		/// Crea un poligono a partire dalla figura. 
		/// Da per scontato che si tratta di un loop chiuso.
		/// </summary>
		/// <returns></returns>
		public Polygon3D ToPolygon()
		{
			List<Point3D> points = new List<Point3D>();
			foreach (Curve3D curve in this)
				points.Add(curve.StartPoint);

			return new Polygon3D(points);
		}

		/// <summary>
		/// Accoda una figura
		/// </summary>
		/// <param name="figure"></param>
		public void AddFigure(Figure3D figure)
		{
			foreach (Curve3D curve in figure)
				Add(curve);
		}

		/// <summary>
		/// Restituisce una lista di Figure3D che rappresentano gli insiemi di curve consecutive.
		/// Ciascun loop può essere aperto oppure chiuso.
		/// </summary>
		/// <returns></returns>
		public List<Figure3D> Loops() => Loops(MathUtils.FineTolerance);

		/// <summary>
		/// Restituisce una lista di Figure3D che rappresentano gli insiemi di curve consecutive.
		/// Ciascun loop può essere aperto oppure chiuso.
		/// </summary>
		/// <returns></returns>
		public List<Figure3D> Loops(double tolerance)
		{
			List<Figure3D> result = new List<Figure3D>();
			if (Count > 0)
			{
				Figure3D loop = new Figure3D();
				Point3D previousPoint = this[0].StartPoint;
				foreach (Curve3D curve in this)
				{
					Point3D firstPoint = curve.StartPoint;
					if (previousPoint.IsEquals(firstPoint, tolerance) == true)
					{
						loop.Add(curve);
					}
					else
					{
						result.Add(loop);
						loop = new Figure3D();
						loop.Add(curve);
					}
					previousPoint = curve.EndPoint;
				}
				result.Add(loop);
			}
			return result;
		}

		/// <summary>
		/// Restituisce una lista di Figure3D che sono state divise in caso di 
		/// discontinuità e in caso di variazione brusca di tangenza. 
		/// Il limite (in radianti) può essere specificato, di default è impostato a 10°.
		/// </summary>
		/// <returns></returns>
		public List<Figure3D> SubdivideDiscontinuity(double tolerance = 0.01, double radTangentLimit = 10 * MathUtils.DegToRad)
		{
			List<Figure3D> result = new List<Figure3D>();
			if (Count > 0)
			{
				Figure3D subdivision = new Figure3D();
				Point3D previousPoint = this[0].StartPoint;
				Vector3D previousTangent = this[0].StartTangent;
				foreach (Curve3D curve in this)
				{
					Point3D firstPoint = curve.StartPoint;
					if (previousPoint.IsEquals(firstPoint, tolerance) &&
						previousTangent.ApproxEqualsInnerAngle(curve.StartTangent, radTangentLimit))
					{
						subdivision.Add(curve);
					}
					else
					{
						result.Add(subdivision);
						subdivision = new Figure3D();
						subdivision.Add(curve);
					}
					previousPoint = curve.EndPoint;
					previousTangent = curve.EndTangent;
				}
				result.Add(subdivision);
			}
			return result;
		}

		/// <summary>
		/// Indica se l'inizio della prima curva coincide con la fine dell'ultima
		/// </summary>
		/// <returns></returns>
		public bool IsClosed()
		{
			bool result = true;
			if (Count > 0)
				result = this[0].StartPoint.IsEquals(this[Count - 1].EndPoint);

			return result;
		}

		/// <summary>
		/// Indica se le curve sono tutte consecutive.
		/// Può essere sia aperto che chiuso.
		/// </summary>
		/// <returns></returns>
		public bool IsLoop()
		{
			return IsLoop(MathUtils.FineTolerance);
		}

		/// <summary>
		/// Indica se le curve sono tutte consecutive.
		/// Può essere sia aperto che chiuso.
		/// </summary>
		/// <returns></returns>
		public bool IsLoop(double tolerance)
		{
			bool result = false;
			if (Count > 0)
			{
				result = true;
				Point3D previousPoint = this[0].StartPoint;
				foreach (Curve3D curve in this)
				{
					Point3D firstPoint = curve.StartPoint;
					if (previousPoint.IsEquals(firstPoint, tolerance) == false)
					{
						result = false;
						break;
					}
					previousPoint = curve.EndPoint;
				}
			}
			return result;
		}

		/// <summary>
		/// Indica se le curve sono tutte consecutive e chiuse tra loro.
		/// </summary>
		/// <returns></returns>
		public bool IsClosedLoop() => IsClosedLoop(MathUtils.FineTolerance);

		/// <summary>
		/// Indica se le curve sono tutte consecutive e chiuse tra loro.
		/// </summary>
		/// <returns></returns>
		public bool IsClosedLoop(double tolerance)
		{
			bool result = IsLoop();
			if (result == true)
			{
				Point3D startPoint = this[0].StartPoint;
				Point3D endPoint = this[Count - 1].EndPoint;
				result = startPoint.IsEquals(endPoint, tolerance);
			}
			return result;
		}

		/// <summary>
		/// Effettua una traslazione di tutte le curve della quantità indicata
		/// </summary>
		/// <param name="traslation">Vettore che indica la traslazione</param>
		public void Move(Vector3D traslation)
		{
			foreach (Curve3D curve in this)
				curve.Move(traslation);
		}

		/// <summary>
		/// Inverte il verso di percorrenza della Figure2D
		/// Cioè inverte l'ordine e inverte ciascuna Curve2D
		/// </summary>
		/// <returns></returns>
		public Figure3D Inverse()
		{
			Figure3D result = new Figure3D();
			for (int i = Count - 1; i >= 0; i--)
			{
				Curve3D curve = this[i];
				result.Add(curve.Inverse());
			}

			return result;
		}

		/// <summary>
		/// Inverte il verso di percorrenza della Figure2D
		/// Cioè inverte l'ordine e inverte ciascuna Curve2D
		/// </summary>
		/// <returns></returns>
		public void SetInverse()
		{
			for (int i = 0; i < Count; i++)
				this[i].SetInverse();

			Reverse();
		}

		/// <summary>
		/// Restituisce l'ABBox3D corrispondente
		/// </summary>
		/// <returns></returns>
		public AABBox3D GetABBox()
		{
			AABBox3D result = AABBox3D.NullAABBox;
			foreach (Curve3D curve in this)
				result.Union(curve.GetABBox());

			return result;
		}

		/// <summary>
		/// Ruota e trasla la curva
		/// </summary>
		/// <param name="matrix"></param>
		public void ApplyRT(RTMatrix matrix)
		{
			foreach (Curve3D curve in this)
				curve.ApplyRT(matrix);
		}

		/// <summary>
		/// Restituisce una figura scalata della quantità indicata
		/// </summary>
		/// <param name="factor"></param>
		/// <returns></returns>
		public Figure3D Scale(double factor)
		{
			Figure3D result = new Figure3D();
			foreach (Curve3D curve in this)
				result.Add(curve.Scale(factor));

			return result;
		}

		/// <summary>
		/// Cancella le curve nulle della figura
		/// </summary>
		public void DeleteNulls() => DeleteNulls(MathUtils.FineTolerance);

		/// <summary>
		/// Cancella le curve nulle della figura 
		/// con tolleranza specificata
		/// </summary>
		/// <param name="tolerance"></param>
		public void DeleteNulls(double tolerance)
		{
			Figure3D figureClone = Clone();

			for (int i = 0; i < figureClone.Count; i++)
			{
				Curve3D actualCurve = figureClone[i];
				if (MathExtensions.IsEquals(actualCurve.Length, 0, tolerance))
				{
					figureClone.RemoveAt(i);
					i--;
				}
			}
			Clear();
			AddFigure(figureClone);
		}

		/// <summary>
		/// Cancella le curve duplicate della figura
		/// </summary>
		public void DeleteDuplicates() => DeleteDuplicates(MathUtils.FineTolerance);

		/// <summary>
		/// Cancella le curve duplicate della figura
		/// con tolleranza specificata
		/// </summary>
		/// <param name="tolerance"></param>
		public void DeleteDuplicates(double tolerance)
		{
			Figure3D figureClone = Clone();

			for (int i = 0; i < figureClone.Count; i++)
			{
				Curve3D actualCurve = figureClone[i];
				for (int j = i + 1; j < figureClone.Count; j++)
				{
					Curve3D curve = figureClone[j];
					if (actualCurve.IsEquals(curve, tolerance) == true)
					{
						figureClone.RemoveAt(j);
						j--;
					}
					else if (actualCurve.IsEquals(curve.Inverse(), tolerance) == true)
					{
						figureClone.RemoveAt(j);
						j--;
					}
				}
			}
			Clear();
			AddFigure(figureClone);
		}

		/// <summary>
		/// Aggiusta le curve consecutive che sono "vicine"
		/// </summary>
		public void AdjusteCurves() => AdjusteCurves(MathUtils.FineTolerance);

		/// <summary>
		/// Aggiusta le curve consecutive che sono "vicine" 
		/// con tolleranza specificata
		/// </summary>
		public void AdjusteCurves(double tolerance)
		{
			Curve3D curve, nextCurve;
			if (Count > 1)
			{
				for (int i = 0; i < Count - 1; i++)
				{
					curve = this[i];
					nextCurve = this[i + 1];
					if ((curve.EndPoint != nextCurve.StartPoint) && curve.EndPoint.IsEquals(nextCurve.StartPoint, tolerance) == true)
						Adjuste(ref curve, ref nextCurve);
				}
				curve = this[Count - 1];
				nextCurve = this[0];
				if ((curve.EndPoint != nextCurve.StartPoint) && curve.EndPoint.IsEquals(nextCurve.StartPoint, tolerance) == true)
					Adjuste(ref curve, ref nextCurve);
			}
		}

		private void Adjuste(ref Curve3D curve, ref Curve3D nextCurve)
		{
			// Se una delle due è una linea cambio quella
			if (curve is Line3D)
			{
				(curve as Line3D).PEnd = nextCurve.StartPoint;
			}
			else if (nextCurve is Line3D)
			{
				(nextCurve as Line3D).PStart = curve.EndPoint;
			}
			else
			{
				if (curve is Arc3D arc)
				{
					arc.Set3Points(arc.StartPoint, arc.MiddlePoint, nextCurve.StartPoint);
				}
				else if (nextCurve is Arc3D nextArc)
				{
					nextArc.Set3Points(curve.EndPoint, nextArc.MiddlePoint, nextArc.EndPoint);
				}
				else if ((curve is Ellipse3D) || (curve is Helix3D))
				{
					//throw new Exception("Not exist adjust for this class: " + curve.GetType().ToString());
				}
			}
		}

		/// <summary>
		/// Evaluate percentuale.
		/// Restituisce un punto che appartiene alla Figure3D oppure al suo prolungamento 
		/// in base al parametro che indica la percentuale da percorrere a partire dal
		/// punto di start.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e 1 il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <returns></returns>
		public Point3D Evaluate(double offset) => Evaluate(offset, out _);

		/// <summary>
		/// Evaluate percentuale.
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la percentuale da percorrere a partire dal
		/// punto di start.
		/// Restituisce inoltre la tangente al punto valutato.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e 1 il punto apparterrà alla Figure3D altrimenti ai prolungamenti</param>
		/// <param name="offset">Tangente</param>
		/// <returns></returns>
		public Point3D Evaluate(double offset, out Vector3D tangent) => EvaluateAbs(offset * Length, out tangent);

		/// <summary>
		/// Evaluate assoluto.
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la lunghezza da percorrere a partire dal
		/// punto di start.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e Length il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <returns></returns>
		public Point3D EvaluateAbs(double offset) => EvaluateAbs(offset, out _);

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
		public Point3D EvaluateAbs(double offset, out Vector3D tangent)
		{
			Point3D result = new Point3D();
			tangent = new Vector3D(0, 0, 0);
			double cntLength = 0;
			Curve3D curveToEval = null;
			if (Count > 0)
			{
				foreach (Curve3D curve in this)
				{
					if (cntLength + curve.Length > offset)
					{
						offset -= cntLength;
						curveToEval = curve;
						break;
					}
					cntLength += curve.Length;
				}
				if (curveToEval == null)
				{
					offset += this[Count - 1].Length - cntLength;
					curveToEval = this[Count - 1];
				}
				result = curveToEval.EvaluateAbs(offset, out tangent);
			}

			return result;
		}

		/// <summary>
		/// Cerca la curva opportuna percorrendo la figura dall'inizio fino alla distanza indicata. 
		/// Restituisce -1 se si va oltre la lunghezza massima (o se distance è minore di 0)
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="curveDistance">La frazione della curva individuata</param>
		/// <returns></returns>
		public int FindIndexByDistance(double distance, out double curveDistance)
		{
			int result = -1;
			curveDistance = 0;
			if (distance.IsEquals(0))
			{
				result = 0;
				curveDistance = distance;
			}
			else if (distance.IsEquals(Length))
			{
				result = Count - 1;
				curveDistance = this[Count - 1].Length;
			}
			else if ((distance > 0) && (distance < Length))
			{
				double cntLength = 0;

				for (int i = 0; i < Count; i++)
				{
					Curve3D curve = this[i];
					if (cntLength + curve.Length > distance)
					{
						curveDistance = distance - cntLength;
						result = i;
						break;
					}
					cntLength += curve.Length;
				}
			}
			return result;
		}

		/// <summary>
		/// Estrae una Figure3D sottoinsieme di  
		/// Le curve in uscita sono tutte copie di quelle originali. 
		/// Potrebbero essere anche essere tagliate (Trim). 
		/// Se uno dei 2 parametri è minore di zero o maggiore della lunghezza totale viene restituito null
		/// </summary>
		/// <param name="start">Inizio: distanza dal punto di start</param>
		/// <param name="end">Fine: distanza dal punto di start</param>
		/// <returns></returns>
		public Figure3D Trim(double start, double end)
		{
			Figure3D result = null;
			double cntStartLength = 0;
			double cntEndLength = 0;
			bool startFound = false;
			int startIndex = -1;
			double startDistance = 0;
			int endIndex = -1;
			double endDistance = 0;

			double length = Length;
			if (end.IsEquals(length))
				end = length;

			if ((start >= 0) && (start <= length) && (end >= 0) && (end <= length))
			{
				result = new Figure3D();
				for (int i = 0; i < Count; i++)
				{
					Curve3D curve = this[i];
					// START
					if (startFound == false)
					{
						if (cntStartLength + curve.Length >= start)
						{
							startFound = true;
							startDistance = start - cntStartLength;
							startIndex = i;
						}
						else
						{
							cntStartLength += curve.Length;
						}
					}
					// END
					if (cntEndLength + curve.Length >= end)
					{
						endDistance = end - cntEndLength;
						endIndex = i;
						break;
					}
					else
					{
						cntEndLength += curve.Length;
					}
				}
				if (startIndex != -1 && endIndex != -1)
				{
					if (startIndex == endIndex)
					{
						result.Add(this[startIndex].Trim(startDistance, endDistance));
					}
					else
					{
						result.Add(this[startIndex].Trim(startDistance, this[startIndex].Length));

						for (int i = startIndex + 1; i < endIndex; i++)
							result.Add(this[i].Clone());

						result.Add(this[endIndex].Trim(0, endDistance));
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Valuta se il punto passato appartiene alla figura.
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <returns></returns>
		public bool IsOnFigure(Point3D point) => IsOnFigure(point, out _);

		/// <summary>
		/// Valuta se il punto passato appartiene alla figura. 
		/// Restituisce inoltre il valore percentuale (0 - 1) che rappresenta la distanza dal punto di start. 
		/// Se non appartiene alla curva restituisce un valore negativo (-1).
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="offset">In uscita: distanza percentuale dal punto di start</param>
		/// <returns></returns>
		public bool IsOnFigure(Point3D point, out double offset) => IsOnFigure(point, MathUtils.FineTolerance, out offset);

		/// <summary>
		/// Valuta se il punto passato appartiene alla figura. 
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="tolerance">Tolleranza</param>
		/// <returns></returns>
		public bool IsOnFigure(Point3D point, double tolerance) => IsOnFigure(point, tolerance, out _);

		/// <summary>
		/// Valuta se il punto passato appartiene alla figura. 
		/// Restituisce inoltre il valore percentuale (0 - 1) che rappresenta la distanza dal punto di start. 
		/// Se non appartiene alla curva restituisce un valore negativo (-1).
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="tolerance">Tolleranza</param>
		/// <param name="offset">In uscita: distanza percentuale dal punto di start</param>
		/// <returns></returns>
		public bool IsOnFigure(Point3D point, double tolerance, out double offset)
		{
			bool result = false;
			offset = 0;

			double cntAbsOffset = 0;
			double localOffset;

			for (int i = 0; i < Count; i++)
			{
				Curve3D curve = this[i];
				if (curve.IsOnCurve(point, tolerance, out localOffset) == true)
				{
					result = true;
					cntAbsOffset += localOffset * curve.Length;
					break;
				}
				else
				{
					cntAbsOffset += curve.Length;
				}

			}
			if (result == true)
			{
				offset = cntAbsOffset / Length;
			}

			return result;
		}

		/// <summary>
		/// Approssima una Figure3D in una serie di linee (passo lineare). 
		/// adaptToEnd: indica se adattare il passo per passi uguali e finire alla punto di end. 
		/// maxStep: indica se adattare lo step considerando quello passato come massimo (true) 
		/// o come minimo (false)
		/// </summary>
		/// <returns></returns>
		public Figure3D ApproxFigure(bool lineApprox, bool arcApprox, bool ellipseApprox,
									 double lineStep, double arcStep, double ellipseStep,
									 bool adaptToEnd, bool maxStep)
		{
			Figure3D result = new Figure3D();
			for (int i = 0; i < Count; i++)
			{
				Curve3D curve3 = this[i];
				if (curve3 is Line3D)
				{
					if (lineApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						double length = curve3.Length;

						Point3D startPoint = curve3.StartPoint;
						Point3D endPoint;
						double step = lineStep;
						if (adaptToEnd)
						{
							if (length % step > 0.1)
							{
								int count = (int)(length / step);
								if (maxStep) count++;
								step = length / count;
							}
							// Accorcio appositamente la fine del ciclo
							length = length - (step / 2);
						}

						for (double offSet = step; offSet < length; offSet += step)
						{
							endPoint = curve3.EvaluateAbs(offSet);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						// Aggiungo l'ultimo punto se non ci sono già
						endPoint = curve3.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));
					}
				}
				else if (curve3 is Arc3D arc)
				{
					if (arcApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						double length = arc.Length;
						Point3D startPoint = arc.StartPoint;
						Point3D endPoint;
						double step = arcStep;
						if (adaptToEnd)
						{
							if (length % step > 0.1)
							{
								int count = (int)(length / step);
								if (maxStep) count++;
								step = length / count;
							}
							// Accorcio appositamente la fine del ciclo
							length = length - (step / 2);
						}

						for (double offset = step; offset < length; offset += step)
						{
							endPoint = arc.EvaluateAbs(offset);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						// Aggiungo l'ultimo punto se non ci sono già
						endPoint = curve3.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));
					}
				}
				else if (curve3 is Helix3D)
				{
					Helix3D helix = curve3 as Helix3D;
					if (arcApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						double length = helix.Length;
						Point3D startPoint = helix.StartPoint;
						Point3D endPoint;
						double step = arcStep;
						if (adaptToEnd)
						{
							if (length % step > 0.1)
							{
								int count = (int)(length / step);
								if (maxStep) count++;
								step = length / count;
							}
							// Accorcio appositamente la fine del ciclo
							length = length - (step / 2);
						}

						for (double offset = step; offset < length; offset += step)
						{
							endPoint = helix.EvaluateAbs(offset);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						// Aggiungo l'ultimo punto se non ci sono già
						endPoint = curve3.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));
					}
				}
				else if (curve3 is Ellipse3D ellipse)
				{
					if (ellipseApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						// Valuto con l'angolo perchè è l'unico evaluate corretto per l'ellisse
						// Gli altri evaluate fanno sempre delle approssimazioni 
						// (non c'è legame dimostrato tra lunghezza e un angolo qualsiasi)
						double angStep = ellipseStep / ellipse.A;
						double spanAngle = ellipse.SpanAngle;
						Point3D startPoint = ellipse.EvaluateAngle(0);
						Point3D endPoint;
						double step = angStep;
						if (adaptToEnd)
						{
							if (spanAngle % step > 0.1)
							{
								int count = (int)(spanAngle / step);
								if (maxStep) count++;
								step = spanAngle / count;
							}
							// Accorcio appositamente la fine del ciclo
							spanAngle = spanAngle - (step / 2);
						}

						for (double angOffset = step; angOffset < spanAngle; angOffset += step)
						{
							endPoint = ellipse.EvaluateAngle(angOffset);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						// Aggiungo l'ultimo punto se non ci sono già
						endPoint = curve3.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));
					}
				}
				else
				{
					throw new Exception("Not exist case for this class: " + curve3.GetType().ToString());
				}
			}
			return result;
		}

		/// <summary>
		/// Approssima una Figure3D in una serie di linee (passo angolare). 
		/// adaptToEnd: indica se adattare il passo per passi uguali e finire alla punto di end. 
		/// maxStep: indica se adattare lo step considerando quello passato come massimo (true) 
		/// o come minimo (false)  
		/// </summary>
		/// <returns></returns>
		public Figure3D ApproxFigureAngle(bool arcApprox, bool ellipseApprox,
										  double arcDegAngleStep, double ellipseDegAngleStep,
										  bool adaptToEnd, bool maxStep)
		{
			Figure3D result = new Figure3D();
			for (int i = 0; i < Count; i++)
			{
				Curve3D curve3 = this[i];
				if (curve3 is Arc3D)
				{
					Arc3D arc = curve3 as Arc3D;
					if (arcApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						double spanAngle = arc.SpanAngle;
						double stepRadAngle = arcDegAngleStep / 180 * Math.PI;
						Vector3D tangent;
						Point3D startPoint = arc.StartPoint;
						Point3D endPoint;
						double step = stepRadAngle;
						if (adaptToEnd)
						{
							if (spanAngle % step > 0.1)
							{
								int count = (int)(spanAngle / step);
								if (maxStep) count++;
								step = spanAngle / count;
							}
							// Accorcio appositamente la fine del ciclo
							spanAngle = spanAngle - (step / 2);
						}

						for (double angOffset = step; angOffset < spanAngle; angOffset += step)
						{
							endPoint = arc.EvaluateAngle(angOffset, out tangent);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						if (!adaptToEnd)
						{
							// Aggiungo l'ultimo punto se non ci sono già
							endPoint = curve3.EndPoint;
							if (startPoint.IsEquals(endPoint) == false)
								result.Add(new Line3D(startPoint, endPoint));
						}
					}
				}
				else if (curve3 is Ellipse3D)
				{
					Ellipse3D ellipse = curve3 as Ellipse3D;
					if (ellipseApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						double spanAngle = ellipse.SpanAngle;
						double stepRadAngle = ellipseDegAngleStep / 180 * Math.PI;
						Point3D startPoint = ellipse.StartPoint;
						Point3D endPoint;
						double step = stepRadAngle;
						if (adaptToEnd)
						{
							if (spanAngle % step > 0.1)
							{
								int count = (int)(spanAngle / step);
								if (maxStep) count++;
								step = spanAngle / count;
							}
							// Accorcio appositamente la fine del ciclo
							spanAngle = spanAngle - (step / 2);
						}
						for (double angOffset = step; angOffset < spanAngle; angOffset += step)
						{
							endPoint = ellipse.EvaluateAngle(angOffset);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						if (!adaptToEnd)
						{
							// Aggiungo l'ultimo punto se non ci sono già
							endPoint = curve3.EndPoint;
							if (startPoint.IsEquals(endPoint) == false)
								result.Add(new Line3D(startPoint, endPoint));
						}
					}
				}
				else
				{
					result.Add(curve3.Clone());
				}
			}
			return result;
		}

		/// <summary>
		/// Approssima una Figure3D in una serie di linee (approccio smart).
		/// Essenzialmente effettua un'approssimazione per angolo, con dei limiti sulla lunghezza massima e minima
		/// del singolo segmento.
		/// In questo modo gli archi piccoli non avranno tante linee inutili e gli archi grandi avranno un numero di
		/// linee superiore. Inoltre modifica il passo per finire esattamente con l'EndPoint. 
		/// </summary>
		/// <returns></returns>
		public Figure3D ApproxFigureAngleSmart(bool arcApprox, bool ellipseApprox, double arcDegAngleStep, double arcMinStep, double arcMaxStep,
											   double ellipseDegAngleStep, double ellipseMinStep, double ellipseMaxStep)
		{
			Figure3D result = new Figure3D();
			for (int i = 0; i < Count; i++)
			{
				Curve3D curve3 = this[i];
				if (curve3 is Arc3D)
				{
					Arc3D arc = curve3 as Arc3D;
					if (arcApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						double spanAngle = arc.SpanAngle;
						double stepRadAngle = arcDegAngleStep / 180 * Math.PI;
						double step = stepRadAngle * arc.Radius;
						if (step < arcMinStep)
						{
							stepRadAngle = arcMinStep / arc.Radius;
							stepRadAngle = spanAngle / (Math.Floor(spanAngle / stepRadAngle));
						}
						else if (step > arcMaxStep)
						{
							stepRadAngle = arcMaxStep / arc.Radius;
							stepRadAngle = spanAngle / (Math.Ceiling(spanAngle / stepRadAngle));
						}
						else
						{
							stepRadAngle = spanAngle / (Math.Round(spanAngle / stepRadAngle, 0, MidpointRounding.ToEven));
						}

						Vector3D tangent;
						Point3D startPoint = arc.StartPoint;
						Point3D endPoint;
						for (double angOffset = stepRadAngle; angOffset < spanAngle - stepRadAngle / 2; angOffset += stepRadAngle)
						{
							endPoint = arc.EvaluateAngle(angOffset, out tangent);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						// Aggiungo sempre l'ultimo punto
						endPoint = arc.EndPoint;
						result.Add(new Line3D(startPoint, endPoint));

					}
				}
				else if (curve3 is Ellipse3D)
				{
					Ellipse3D ellipse = curve3 as Ellipse3D;
					if (ellipseApprox == false)
					{
						result.Add(curve3.Clone());
					}
					else
					{
						double spanAngle = ellipse.SpanAngle;
						double stepRadAngle = ellipseDegAngleStep / 180 * Math.PI;
						double step = stepRadAngle * ellipse.A;
						if (step < ellipseMinStep)
						{
							stepRadAngle = ellipseMinStep / ellipse.A;
							stepRadAngle = spanAngle / (Math.Floor(spanAngle / stepRadAngle));
						}
						else if (step > ellipseMaxStep)
						{
							stepRadAngle = ellipseMaxStep / ellipse.A;
							stepRadAngle = spanAngle / (Math.Ceiling(spanAngle / stepRadAngle));
						}
						else
						{
							stepRadAngle = spanAngle / (Math.Round(spanAngle / stepRadAngle, 0, MidpointRounding.ToEven));
						}

						Point3D startPoint = ellipse.StartPoint;
						Point3D endPoint;
						for (double angOffset = stepRadAngle; angOffset < spanAngle - stepRadAngle / 2; angOffset += stepRadAngle)
						{
							endPoint = ellipse.EvaluateAngle(angOffset);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						// Aggiungo sempre l'ultimo punto
						endPoint = ellipse.EndPoint;
						result.Add(new Line3D(startPoint, endPoint));
					}
				}
				else
				{
					result.Add(curve3.Clone());
				}
			}
			return result;
		}

		/// <summary>
		/// Approssima una Figure3D in una serie di linee (distanza cordale). 
		/// Effettua un'approssimazione tramite un angolo determinato in modo da non 
		/// discostarsi dalla figura iniziale oltre l'errore massimo indicato. 
		/// Inoltre modifica il passo per finire esattamente con l'EndPoint (rimanendo sempre sotto l'errore).
		/// </summary>
		/// <returns></returns>
		public Figure3D ApproxFigureMaxChordalDeviation(bool arcApprox, bool ellipseApprox, double arcMaxDeviation, double ellipseMaxDeviation)
		{
			List<Vector3D> tangents;
			return ApproxFigureMaxChordalDeviation(arcApprox, ellipseApprox, arcMaxDeviation, ellipseMaxDeviation, out tangents);
		}

		/// <summary>
		/// Approssima una Figure3D in una serie di linee (distanza cordale). 
		/// Effettua un'approssimazione tramite un angolo determinato in modo da non 
		/// discostarsi dalla figura iniziale oltre l'errore massimo indicato. 
		/// Inoltre modifica il passo per finire esattamente con l'EndPoint (rimanendo sempre sotto l'errore). 
		/// Nella lista tangents vengono indicate le tangenti della figura originale per ciascun punto di start e di end 
		/// di ogni Curve in uscita (tipicamente tutte linee);
		/// </summary>
		/// <returns></returns>
		public Figure3D ApproxFigureMaxChordalDeviation(bool arcApprox, bool ellipseApprox, double arcMaxDeviation, double ellipseMaxDeviation, out List<Vector3D> tangents)
		{
			Figure3D result = new Figure3D();
			tangents = new List<Vector3D>();
			for (int i = 0; i < Count; i++)
			{
				Curve3D curve3 = this[i];
				if (curve3 is Arc3D arc)
				{
					#region ARC
					if (arcApprox == false)
					{
						result.Add(curve3.Clone());
						Vector3D tangent;
						curve3.Evaluate(0, out tangent);
						tangents.Add(tangent);
						curve3.Evaluate(1, out tangent);
						tangents.Add(tangent);
					}
					else
					{
						double spanAngle = arc.SpanAngle;
						double stepRadAngle = spanAngle;
						if (arcMaxDeviation < arc.Radius)
						{
							stepRadAngle = 2 * Math.Acos((arc.Radius - arcMaxDeviation) / arc.Radius);
							stepRadAngle = spanAngle / ((int)(spanAngle / stepRadAngle) + 1);
						}

						Vector3D oldTangent, tangent;
						Point3D startPoint = arc.Evaluate(0, out oldTangent);
						Point3D endPoint;
						for (double angOffset = stepRadAngle; angOffset < spanAngle - stepRadAngle / 2; angOffset += stepRadAngle)
						{
							endPoint = arc.EvaluateAngle(angOffset, out tangent);
							result.Add(new Line3D(startPoint, endPoint));
							tangents.Add(oldTangent);
							tangents.Add(tangent);
							startPoint = endPoint;
							oldTangent = tangent;
						}
						// Aggiungo sempre l'ultimo punto
						endPoint = arc.Evaluate(1, out tangent);
						result.Add(new Line3D(startPoint, endPoint));
						tangents.Add(oldTangent);
						tangents.Add(tangent);
					}
					#endregion ARC
				}
				else if (curve3 is Ellipse3D ellipse)
				{
					#region ELLIPSE
					if (ellipseApprox == false)
					{
						result.Add(curve3.Clone());
						Vector3D tangent;
						curve3.Evaluate(0, out tangent);
						tangents.Add(tangent);
						curve3.Evaluate(1, out tangent);
						tangents.Add(tangent);
					}
					else
					{
						double spanAngle = ellipse.SpanAngle;
						double stepRadAngle = spanAngle;
						if (ellipseMaxDeviation < ellipse.A)
						{
							stepRadAngle = 2 * Math.Acos((ellipse.A - ellipseMaxDeviation) / ellipse.A);
							stepRadAngle = spanAngle / ((int)(spanAngle / stepRadAngle) + 1);
						}

						Vector3D oldTangent, tangent;
						Point3D startPoint = ellipse.Evaluate(0, out oldTangent);
						Point3D endPoint;
						for (double angOffset = stepRadAngle; angOffset < spanAngle - stepRadAngle / 2; angOffset += stepRadAngle)
						{
							endPoint = ellipse.EvaluateAngle(angOffset, out tangent);
							result.Add(new Line3D(startPoint, endPoint));
							tangents.Add(oldTangent);
							tangents.Add(tangent);
							startPoint = endPoint;
							oldTangent = tangent;
						}
						// Aggiungo sempre l'ultimo punto
						endPoint = ellipse.Evaluate(1, out tangent);
						result.Add(new Line3D(startPoint, endPoint));
						tangents.Add(oldTangent);
						tangents.Add(tangent);
					}
					#endregion ELLIPSE
				}
				else if (curve3 is Helix3D helix)
				{
					#region HELIX
					if (arcApprox == false)
					{
						result.Add(curve3.Clone());
						Vector3D tangent;
						curve3.Evaluate(0, out tangent);
						tangents.Add(tangent);
						curve3.Evaluate(1, out tangent);
						tangents.Add(tangent);
					}
					else
					{
						double spanAngle = helix.SpanAngle;
						double stepRadAngle = spanAngle;
						if (arcMaxDeviation < helix.Radius)
						{
							stepRadAngle = 2 * Math.Acos((helix.Radius - arcMaxDeviation) / helix.Radius);
							stepRadAngle = spanAngle / ((int)(spanAngle / stepRadAngle) + 1);
						}

						Vector3D oldTangent, tangent;
						Point3D startPoint = helix.Evaluate(0, out oldTangent);
						Point3D endPoint;
						for (double angOffset = stepRadAngle; angOffset < spanAngle - stepRadAngle / 2; angOffset += stepRadAngle)
						{
							endPoint = helix.EvaluateAngle(angOffset, out tangent);
							result.Add(new Line3D(startPoint, endPoint));
							tangents.Add(oldTangent);
							tangents.Add(tangent);
							startPoint = endPoint;
							oldTangent = tangent;
						}
						// Aggiungo sempre l'ultimo punto
						endPoint = helix.Evaluate(1, out tangent);
						result.Add(new Line3D(startPoint, endPoint));
						tangents.Add(oldTangent);
						tangents.Add(tangent);
					}
					#endregion HELIX
				}
				else if (curve3 is Spline3D spline)
				{
					#region SPLINE
					result.AddPolygon(spline.InterpolationPoints);
					List<Vector3D> tan = spline.InterpolationTangents;
					tangents.Add(tan[0]);
					for (int j = 1; j < spline.InterpolationTangents.Count - 1; j++)
					{
						tangents.Add(tan[j]);
						tangents.Add(tan[j]);
					}
					tangents.Add(tan[tan.Count - 1]);
					#endregion SPLINE
				}
				else if (curve3 is PolyLine3D poly)
				{
					Figure3D polyFigure = poly.ToFigure();
					List<Vector3D> intTangents;
					polyFigure.ApproxFigureMaxChordalDeviation(arcApprox, ellipseApprox, arcMaxDeviation, ellipseMaxDeviation, out intTangents);
					result.AddFigure(polyFigure);
					tangents.AddRange(intTangents);
				}
				else
				{
					result.Add(curve3.Clone());
					Vector3D tang;
					curve3.Evaluate(0, out tang);
					tangents.Add(tang);
					curve3.Evaluate(1, out tang);
					tangents.Add(tang);
				}
			}
			return result;
		}

		/// <summary>
		/// Trova tutte le autointersezioni e genera una Figure2D equivalente
		/// spezzando tutte le linee nel punto di intersezione
		/// </summary>
		/// <param name="offsets">Dizionario che ha come chiave la linea,
		/// e come valore una lista ordinata di distanze da StartPoint (espresse in percentuale 0-1)</param>
		/// <returns></returns>
		public Figure3D SubdivideCrossCurve(out Dictionary<Line3D, List<double>> offsets)
		{
			Figure3D result = new Figure3D();
			offsets = new Dictionary<Line3D, List<double>>();
			for (int i = 0; i < this.Count - 1; i++)
			{
				for (int j = i + 1; j < this.Count; j++)
				{
					Line3D line1 = (Line3D)this[i];
					Line3D line2 = (Line3D)this[j];
					if (!offsets.ContainsKey(line1))
					{
						offsets.Add(line1, new List<double>());
						offsets[line1].Add(0);
						offsets[line1].Add(1);
					}
					if (!offsets.ContainsKey(line2))
					{
						offsets.Add(line2, new List<double>());
						offsets[line2].Add(0);
						offsets[line2].Add(1);
					}

					double u1, u2;
					if (line1.Intersection(line2, out u1, out u2))
					{
						offsets[line1].Add(u1);
						offsets[line2].Add(u2);
					}
				}
			}

			foreach (KeyValuePair<Line3D, List<double>> pair in offsets)
			{
				Line3D line = pair.Key;
				List<double> offsetsDouble = pair.Value;
				offsetsDouble.Sort();
				double length = line.Length;
				double precOffsetLength = 0;
				for (int i = 1; i < offsetsDouble.Count; i++)
				{
					double offsetLength = offsetsDouble[i] * length;

					if ((offsetLength - precOffsetLength).IsEquals(0, MathUtils.FineTolerance))
					{
						offsetsDouble.RemoveAt(i);
						i--;
					}
					else
					{
						Line3D trim = (Line3D)line.Trim(precOffsetLength, offsetLength);
						result.Add(trim);
						precOffsetLength = offsetLength;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Divide tutte le curve sovrapposte con altre nel punto di sovrapposizione (per ora solo linee e archi). 
		/// Modifica la figura originale.
		/// </summary>
		public void SubdivideCrossCurve()
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				Curve3D curve1 = this[i];
				List<Point3D> allCurveIntersectionPoints = new List<Point3D>();

				for (int j = Count - 1; j >= 0; j--)
				{
					Curve3D curve2 = this[j];
					if (curve1 != curve2)
					{
						List<Point3D> intersectionPoints = new List<Point3D>();
						curve1.Intersection(curve2, out intersectionPoints);
						allCurveIntersectionPoints.AddRange(intersectionPoints);
					}
				}
				if (allCurveIntersectionPoints.Count > 0)
				{
					Remove(curve1);
					List<Curve3D> subdivisions = Subdivide(curve1, allCurveIntersectionPoints);
					AddRange(subdivisions);
				}
			}
		}

		private List<Curve3D> Subdivide(Curve3D curve, List<Point3D> intersectionPoint)
		{
			List<Curve3D> result = new List<Curve3D>();
			if (curve is Line3D)
				result = (List<Curve3D>)Subdivide((Line3D)curve, intersectionPoint);

			if (curve is Arc3D)
				result = (List<Curve3D>)Subdivide((Arc3D)curve, intersectionPoint);

			return result;
		}

		private List<Curve3D> Subdivide(Line3D line, List<Point3D> intersectionPoints)
		{
			List<Curve3D> result = new List<Curve3D>();
			// inserisco il punto di start e di end per poi ricostruire le sezioni
			intersectionPoints.Add(line.StartPoint);
			intersectionPoints.Add(line.EndPoint);
			// ordina i punti di intersezione partendo dallo start point della linea
			intersectionPoints.Sort(delegate (Point3D p1, Point3D p2)
			{
				double p1Distance = p1.Distance(line.StartPoint);
				double p2Distance = p2.Distance(line.StartPoint);
				return p1Distance.CompareTo(p2Distance);
			});

			Point3D prev = line.StartPoint;
			// escludo il primo (lo start)
			for (int k = 1; k < intersectionPoints.Count; k++)
			{
				Point3D currentIntersection = intersectionPoints[k];
				result.Add(new Line3D(prev, currentIntersection));
				prev = currentIntersection;
			}
			return result;
		}

		private List<Curve3D> Subdivide(Arc3D arc, List<Point3D> intersectionPoints)
		{
			List<Curve3D> result = new List<Curve3D>();
			// ordina i punti di intersezione partendo dallo start point dell'arco
			intersectionPoints.Sort(delegate (Point3D p1, Point3D p2)
			{
				double p1AngleDistance, p2AngleDistance;
				arc.IsOnCurve(p1, out p1AngleDistance);
				arc.IsOnCurve(p2, out p2AngleDistance);
				return p1AngleDistance.CompareTo(p2AngleDistance);
			});

			double startAngle = arc.StartAngle;
			double endAngle = arc.EndAngle;
			double prev = startAngle;

			for (int k = 0; k < intersectionPoints.Count; k++)
			{
				Point3D currentIntersection = intersectionPoints[k];
				double currentAngle;
				arc.IsOnCurve(currentIntersection, out currentAngle);
				currentAngle *= arc.SpanAngle;

				if (arc.CounterClockWise)
					currentAngle += startAngle;
				else
					currentAngle -= startAngle;

				// escludo le intersezioni agli estremi
				if (!currentAngle.IsEquals( prev) && !currentAngle.IsEquals(endAngle))
				{
					result.Add(new Arc3D(arc.Center, arc.Radius, prev, currentAngle, arc.CounterClockWise, arc.RMatrix));
					prev = currentAngle;
				}
			}
			result.Add(new Arc3D(arc.Center, arc.Radius, prev, endAngle, arc.CounterClockWise, arc.RMatrix));

			return result;
		}

		/// <summary>
		/// Riduce il numero di linee e archi che sono consecutivi
		/// e che appartengono alla stessa curva
		/// </summary>
		/// <param name="reduceLine"></param>
		/// <param name="reduceArc"></param>
		public void Reduce(bool reduceLine, bool reduceArc) => Reduce(reduceLine, reduceArc, MathUtils.FineTolerance);

		/// <summary>
		/// Riduce il numero di linee e archi che sono consecutivi
		/// e che appartengono alla stessa curva
		/// </summary>
		/// <param name="reduceLine"></param>
		/// <param name="reduceArc"></param>
		/// <param name="tolerance"></param>
		public void Reduce(bool reduceLine, bool reduceArc, double tolerance)
		{
			Figure3D figureClone = Clone();

			for (int i = 0; i < figureClone.Count; i++)
			{
				Curve3D actualCurve = figureClone[i];
				int next = (i + 1) % figureClone.Count;
				Curve3D nextCurve = figureClone[next];
				if (reduceLine == true)
				{
					if ((actualCurve is Line3D) && (nextCurve is Line3D))
					{
						Line3D line1 = actualCurve as Line3D;
						Line3D line2 = nextCurve as Line3D;
						Vector3D dir1, dir2;
						line1.Evaluate(0, out dir1);
						line2.Evaluate(0, out dir2);
						if ((line1.PEnd.IsEquals(line2.PStart, tolerance)) &&
							(MathExtensions.IsEquals(line1.DistancePerp(line2.PEnd), 0, tolerance)) &&
							(dir1.IsEquals(dir2)))
						{
							line1.PEnd = line2.PEnd;
							figureClone.RemoveAt(next);
							i--;
						}
					}
				}
				if (reduceArc == true)
				{
					if ((actualCurve is Arc3D) && (nextCurve is Arc3D))
					{
						Arc3D arc1 = actualCurve as Arc3D;
						Arc3D arc2 = nextCurve as Arc3D;
						if ((arc1.EndPoint.IsEquals(arc2.StartPoint, tolerance)) && (arc1.Center.IsEquals(arc2.Center, tolerance)) && (arc1.CounterClockWise == arc2.CounterClockWise))
						{
							// Escludo 2 archi che formano un cerchio
							if (arc1.StartPoint.IsEquals(arc2.EndPoint, tolerance) == false)
							{
								arc1.EndAngle = arc2.EndAngle;
								figureClone.RemoveAt(next);
								i--;
							}
						}
					}
				}
			}
			Clear();
			AddFigure(figureClone);
		}

		#region ONLY 3D
		
		/// <summary>
		/// Restituisce un Figure3D che rappresenta un estruso nella direzione indicata
		/// </summary>
		/// <param name="figure"></param>
		/// <param name="extrusionVector"></param>
		/// <returns></returns>
		public static Figure3D ExtrudeFigure2D(Figure3D figure, Vector3D extrusionVector)
		{
			Figure3D result = figure.Clone();
			Figure3D secondFigure = result.Clone();
			secondFigure.Move(extrusionVector);
			for (int i = 0; i < secondFigure.Count; i++)
			{
				Curve3D curve = secondFigure[i];
				Point3D[] points;
				if (curve is Line3D)
					points = new Point3D[] { curve.StartPoint, curve.EndPoint };
				else
					points = new Point3D[] { curve.StartPoint, curve.Evaluate(0.5), curve.EndPoint };

				foreach (Point3D point in points)
					result.Add(new Line3D(point + (-1 * extrusionVector), point));
			}
			if (secondFigure.IsClosedLoop())
				result.RemoveAt(result.Count - 1);

			result.AddFigure(secondFigure);

			return result;
		}

		/// <summary>
		/// Restituisce una figura contentente il verso di tutte le curve 
		/// rappresentate da una freccia di dimensione indicata. 
		/// Se addStartEndReferences è a true vengono anche inseriti dei riferimenti sul punto di start e di end. 
		/// Il parametro normalReference indica il piano di riferimento
		/// </summary>
		/// <returns></returns>
		public Figure3D GetFigureVersus(double arrowLength, bool addStartEndReferences, Vector3D normalReference)
		{
			Figure3D result = new Figure3D();

			for (int i = 0; i < Count; i++)
			{
				Curve3D curve = this[i];
				double length = arrowLength;
				if (curve.Length > 0)
				{
					if (length > curve.Length / 2)
						length = curve.Length / 2;
				}
				if (addStartEndReferences && (i == 0 || i == Count - 1))
					length = 2 * arrowLength;

				Vector3D tangent, perpendicularL, perpendicularR;
				Point3D point = curve.EvaluateAbs(length * 1.5, out tangent);
				if (addStartEndReferences && i == 0)
					point = curve.EvaluateAbs(length, out tangent);
				else if (addStartEndReferences && i == Count - 1)
					point = curve.Evaluate(1, out tangent);

				perpendicularL = tangent.Cross(normalReference);
				if (perpendicularL.Length.IsEquals( 0) == false)
					perpendicularL.SetNormalize();
				else
					perpendicularL = tangent.Cross(normalReference.Perpendicular()).Normalize();

				tangent = -1 * tangent;
				perpendicularR = -1 * perpendicularL;
				Point3D pointL = point + length * tangent + length / 2 * perpendicularL;
				Point3D pointR = point + length * tangent + length / 2 * perpendicularR;
				Line3D line = new Line3D(pointL, point);
				result.Add(line);
				line = new Line3D(point, pointR);
				result.Add(line);
			}
			if (addStartEndReferences == true)
			{
				// Cerchio sul punto di start e di end
				Point3D start = this[0].StartPoint;
				RTMatrix matrix = new RTMatrix(normalReference.Perpendicular().Cross(normalReference), normalReference.Perpendicular(), normalReference, Vector3D.Zero);
				Arc3D arc = new Arc3D(start, arrowLength / 2, 0, Math.PI, true, matrix);
				result.Add(arc);
				arc = new Arc3D(start, arrowLength / 2, Math.PI, 2 * Math.PI, true, matrix);
				result.Add(arc);
				Point3D end = this[Count - 1].EndPoint;
				arc = new Arc3D(end, arrowLength / 2, 0, Math.PI, true, matrix);
				result.Add(arc);
				arc = new Arc3D(end, arrowLength / 2, Math.PI, 2 * Math.PI, true, matrix);
				result.Add(arc);
			}

			return result;
		}

		/// <summary>
		/// Indica se la figura si trova tutta su un piano. 
		/// </summary>
		/// <returns></returns>
		public bool IsOnPlane() => IsOnPlane(out _);

		/// <summary>
		/// Indica se la figura si trova tutta su un piano. 
		/// Viene restituito il piano.
		/// </summary>
		/// <param name="planeMatrix"></param>
		/// <returns></returns>
		public bool IsOnPlane(out Plane3D plane)
		{
			bool result = true;
			plane = Plane3D.ZeroPlane;
			List<Point3D> points = new List<Point3D>();
			foreach (Curve3D curve in this)
			{
				points.Add(curve.StartPoint);
				if ((curve is Line3D) == false)
					points.Add(curve.Evaluate(0.5));

				points.Add(curve.EndPoint);
			}

			plane = Plane3D.FromPoints(points);
			if (plane == Plane3D.ZeroPlane)
				result = false;

			return result;
		}

		/// <summary>
		/// Ordina le curve della figura rendendole consecutive partendo dalla curva
		/// più vicina al punto referencePoint. 
		/// Eventualmente inverte anche le singole curve per renderle consecutive.
		/// Inverte anche il senso del loop se necessario. 
		/// </summary>
		public void AutomaticSort(Point3D referencePoint) => AutomaticSort(referencePoint, MathUtils.FineTolerance);

		/// <summary>
		/// Ordina le curve della figura rendendole consecutive partendo dalla curva 
		/// più vicina al punto referencePoint. 
		/// Eventualmente inverte anche le singole curve per renderle consecutive. 
		/// Inverte anche il senso del loop se necessario. 
		/// Con tolleranza specificata.
		/// </summary>
		public void AutomaticSort(Point3D referencePoint, double tolerance) => AutomaticSort(referencePoint, true, tolerance);

		/// <summary>
		/// Ordina le curve della figura rendendole consecutive partendo dalla curva 
		/// più vicina al punto referencePoint. 
		/// Eventualmente inverte anche le singole curve per renderle consecutive. 
		/// Il parametro reverseLoopIfNeeded indica se è possibile invertire il loop (la prima curva in realtà).
		/// Con tolleranza specificata.
		/// </summary>
		public void AutomaticSort(Point3D referencePoint, bool reverseLoopIfNeeded, double tolerance)
		{
			if (Count > 0)
			{
				Figure3D cloneFigure = Clone();
				int minIndex = -1;
				double minDistance = double.MaxValue;
				Curve3D firstCurve = null;
				for (int i = 0; i < Count; i++)
				{
					Curve3D curve = this[i];
					double distance = curve.StartPoint.Distance(referencePoint);
					if (distance < minDistance)
					{
						minIndex = i;
						minDistance = distance;
						firstCurve = curve;
					}

					if (reverseLoopIfNeeded)
					{
						distance = curve.EndPoint.Distance(referencePoint);
						if (distance < minDistance)
						{
							minIndex = i;
							minDistance = distance;
							firstCurve = curve.Inverse();
						}
					}
				}
				cloneFigure.RemoveAt(minIndex);
				cloneFigure.Insert(0, firstCurve);
				cloneFigure.AutomaticSort(tolerance);

				Clear();
				AddFigure(cloneFigure);
			}
		}

		/// <summary>
		/// Ordina le curve della figura rendendole consecutive. 
		/// Eventualmente inverte anche le singole curve per renderle consecutive.
		/// N.B. Il loop non viene invertito: la prima curva è il riferimento. 
		/// </summary>
		public void AutomaticSort() => AutomaticSort(MathUtils.FineTolerance);

		/// <summary>
		/// Ordina le curve della figura rendendole consecutive. 
		/// Eventualmente inverte anche le singole curve per renderle consecutive. 
		/// N.B. Il loop non viene invertito: la prima curva è il riferimento. 
		/// Con tolleranza specificata.
		/// </summary>
		public void AutomaticSort(double tolerance)
		{
			bool found;
			double minDistance;
			if (Count > 0)
			{
				Figure3D result = new Figure3D();
				Figure3D figureClone = Clone();

				Curve3D actualCurve = figureClone[0];
				Curve3D nearestCurve = null;
				int actualIndex = 0;
				figureClone.RemoveAt(actualIndex);
				result.Add(actualCurve);

				while (figureClone.Count > 0)
				{
					minDistance = double.MaxValue;
					found = false;
					for (int i = 0; i < figureClone.Count; i++)
					{
						Curve3D curve = figureClone[i];
						if (actualCurve.EndPoint.IsEquals(curve.StartPoint, tolerance) == true)
						{
							actualCurve = curve;
							actualIndex = i;
							found = true;
							break;
						}
						else if (actualCurve.EndPoint.IsEquals(curve.EndPoint, tolerance) == true)
						{
							actualCurve = curve.Inverse();
							actualIndex = i;
							found = true;
							break;
						}
						else
						{
							double dist = actualCurve.EndPoint.Distance(curve.StartPoint);
							if (dist < minDistance)
							{
								nearestCurve = curve;
								actualIndex = i;
								minDistance = dist;
							}
						}
					}
					if (found == false)
						actualCurve = nearestCurve;

					result.Add(actualCurve);
					figureClone.RemoveAt(actualIndex);
				}

				// Fino qui ho tanti loop ordinati ma se c'erano loop aperti
				// c'è la possibilità che siano divisi in più parti
				List<Figure3D> loops = result.Loops();
				if (loops.Count > 1)
				{
					List<Figure3D> openLoops = new List<Figure3D>();
					List<Figure3D> closedLoops = new List<Figure3D>();
					foreach (Figure3D loop in loops)
					{
						if (loop.IsClosed() == true)
							closedLoops.Add(loop);
						else
							openLoops.Add(loop);
					}
					if (openLoops.Count > 0)
					{
						int cnt = 0;
						while (cnt != openLoops.Count)
						{
							cnt = openLoops.Count;
							openLoops = JoinOpenLoops(openLoops, tolerance);
						}
						List<Figure3D> allLoops = new List<Figure3D>();
						allLoops.AddRange(closedLoops);
						allLoops.AddRange(openLoops);
						allLoops.Insert(0, new Figure3D(this[0].StartPoint, this[0].StartPoint));
						allLoops = Figure3DExtensions.LoopsSort(allLoops, false, false, tolerance);
						allLoops.RemoveAt(0);
						result = new Figure3D();
						foreach (Figure3D loop in allLoops)
							result.AddFigure(loop);
					}
				}
				Clear();
				AddFigure(result);
			}
		}

		private List<Figure3D> JoinOpenLoops(List<Figure3D> openLoops, double tolerance)
		{
			List<Figure3D> result = new List<Figure3D>();
			List<Figure3D> openLoopsClone = new List<Figure3D>();
			foreach (Figure3D openLoop in openLoops)
				openLoopsClone.Add(openLoop.Clone());

			bool found;
			if (openLoops.Count > 0)
			{
				while (openLoopsClone.Count > 0)
				{
					Figure3D actualLoop = openLoopsClone[0];
					int indexToRemove = 0;
					openLoopsClone.RemoveAt(indexToRemove);

					found = false;
					for (int i = 0; i < openLoopsClone.Count; i++)
					{
						Figure3D loop = openLoopsClone[i];
						if (actualLoop.EndPoint.IsEquals(loop.StartPoint, tolerance) == true)
						{
							actualLoop.AddFigure(loop);
							indexToRemove = i;
							found = true;
							break;
						}
						else if (actualLoop.EndPoint.IsEquals(loop.EndPoint, tolerance) == true)
						{
							actualLoop.AddFigure(loop.Inverse());
							indexToRemove = i;
							found = true;
							break;
						}
						else if (actualLoop.StartPoint.IsEquals(loop.StartPoint, tolerance) == true)
						{
							actualLoop = actualLoop.Inverse();
							actualLoop.AddFigure(loop);
							indexToRemove = i;
							found = true;
							break;
						}
						else if (actualLoop.StartPoint.IsEquals(loop.EndPoint, tolerance) == true)
						{
							actualLoop = actualLoop.Inverse();
							actualLoop.AddFigure(loop.Inverse());
							indexToRemove = i;
							found = true;
							break;
						}
					}
					if (found == true)
						openLoopsClone.RemoveAt(indexToRemove);

					result.Add(actualLoop);
				}
			}

			return result;
		}


		#endregion ONLY3D

		#endregion 

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
