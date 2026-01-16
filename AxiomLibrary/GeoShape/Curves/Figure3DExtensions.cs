using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Curves
{
	/// <summary>
	/// Estensioni per le figure 3D
	/// </summary>
	public static class Figure3DExtensions
	{
		/// <summary>
		/// Permette di salvare la figura in formato dxf. 
		/// Salva solo linee e archi, quindi è necessario controllare la figura ed eventualmente, 
		/// semplificare curve più complesse.
		/// N.B. L'export è relativamente semplice, l'import è impraticabile per noi
		/// </summary>
		/// <param name="fileName"></param>
		public static void SimpleExportToDxf(this Figure3D figure3D, string fileName)
		{
			List<string> lines = SimpleExportToDxf(figure3D);
			File.WriteAllLines(fileName, lines.ToArray());
		}

		/// <summary>
		/// Permette di salvare la figura in formato dxf. 
		/// Salva solo linee e archi, quindi è necessario controllare la figura ed eventualmente, 
		/// semplificare curve più complesse.
		/// N.B. L'export è relativamente semplice, l'import è impraticabile per noi
		/// </summary>
		public static List<string> SimpleExportToDxf(Figure3D figure3D)
		{
			List<string> result = new List<string>();
			// Header fisso
			result.Add("0");
			result.Add("SECTION");
			result.Add("2");
			result.Add("ENTITIES");
			result.Add("0");
			foreach (Curve3D curve in figure3D)
			{
				if (curve is Line3D)
				{
					Line3D line = curve as Line3D;
					result.Add("LINE");
					result.Add("8");
					result.Add("0");
					result.Add("10");
					result.Add(line.PStart.X.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("20");
					result.Add(line.PStart.Y.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("30");
					result.Add(line.PStart.Z.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("11");
					result.Add(line.PEnd.X.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("21");
					result.Add(line.PEnd.Y.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("31");
					result.Add(line.PEnd.Z.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("0");
				}
				else if (curve is Arc3D)
				{
					Arc3D arc = curve as Arc3D;
					result.Add("ARC");
					result.Add("8");
					result.Add("0");
					result.Add("10");
					result.Add(arc.Center.X.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("20");
					result.Add(arc.Center.Y.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("30");
					result.Add(arc.Center.Z.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("40");
					result.Add(arc.Radius.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("50");
					double startDegAngle = MathUtils.RadToDeg * (arc.CounterClockWise ? arc.StartAngle : arc.EndAngle);
					result.Add(startDegAngle.ToString("###########0.############", CultureInfo.InvariantCulture));
					double endDegAngle = MathUtils.RadToDeg * (arc.CounterClockWise ? arc.EndAngle : arc.StartAngle);
					result.Add("51");
					result.Add(endDegAngle.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("0");
				}
				else if (curve is Ellipse3D)
				{
					Ellipse3D ellipse = curve as Ellipse3D;
					result.Add("ELLIPSE");
					result.Add("8");
					result.Add("0");
					result.Add("10");
					result.Add(ellipse.Center.X.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("20");
					result.Add(ellipse.Center.Y.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("30");
					result.Add(ellipse.Center.Z.ToString("###########0.############", CultureInfo.InvariantCulture));

					Vector3D pRef = (ellipse.A * Vector3D.UnitX).Rotate(Vector3D.UnitZ, ellipse.RotationA);
					result.Add("11");
					result.Add(pRef.X.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("21");
					result.Add(pRef.Y.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("31");
					result.Add(pRef.Z.ToString("###########0.############", CultureInfo.InvariantCulture));

					result.Add("40");
					result.Add((ellipse.B / ellipse.A).ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("41");
					double startRadAngle = (ellipse.CounterClockWise ? ellipse.StartAngle : ellipse.EndAngle);
					startRadAngle = ellipse.EllipseAngleToCircularAngle(startRadAngle);
					result.Add(startRadAngle.ToString("###########0.############", CultureInfo.InvariantCulture));
					double endRadAngle = (ellipse.CounterClockWise ? ellipse.EndAngle : ellipse.StartAngle);
					endRadAngle = ellipse.EllipseAngleToCircularAngle(endRadAngle);
					result.Add("42");
					result.Add(endRadAngle.ToString("###########0.############", CultureInfo.InvariantCulture));
					result.Add("0");
				}
				else if (curve is PolyLine3D || curve is Spline3D)
				{
					Figure3D localFigure = curve is PolyLine3D ? (curve as PolyLine3D).ToFigure() : (curve as Spline3D).Interpolation;
					foreach (Curve3D localCurve in localFigure)
					{
						Line3D line = localCurve as Line3D;
						result.Add("LINE");
						result.Add("8");
						result.Add("0");
						result.Add("10");
						result.Add(line.PStart.X.ToString("###########0.############", CultureInfo.InvariantCulture));
						result.Add("20");
						result.Add(line.PStart.Y.ToString("###########0.############", CultureInfo.InvariantCulture));
						result.Add("30");
						result.Add("0");
						result.Add("11");
						result.Add(line.PEnd.X.ToString("###########0.############", CultureInfo.InvariantCulture));
						result.Add("21");
						result.Add(line.PEnd.Y.ToString("###########0.############", CultureInfo.InvariantCulture));
						result.Add("31");
						result.Add("0");
						result.Add("0");
					}
				}
			}

			// Footer fisso
			result.Add("ENDSEC");
			result.Add("0");
			result.Add("EOF");

			return result;
		}

		/// <summary>
		/// Ordina i loop rendendoli consecutivi. 
		/// Eventualmente inverte anche i singoli loop per renderli consecutivi. 
		/// Con tolleranza specificata. 
		/// N.B. Il primo loop non viene modificato, perchè preso come riferimento.
		/// </summary>
		public static List<Figure3D> LoopsSort(List<Figure3D> loops, double tolerance) => LoopsSort(loops, false, true, tolerance);

		/// <summary>
		/// Ordina i loop rendendoli consecutivi. 
		/// Eventualmente inverte anche i singoli loop per renderli consecutivi (da parametro reverseLoopsIfNeeded). 
		/// In parametro sortSingleLoops indica se ciascun loop può essere riordinato. 
		/// Con tolleranza specificata.
		/// N.B. Il primo loop non viene modificato, perchè preso come riferimento.
		/// </summary>
		public static List<Figure3D> LoopsSort(List<Figure3D> loops, bool sortSingleLoops, bool reverseLoopsIfNeeded, double tolerance)
		{
			List<Figure3D> result = new List<Figure3D>();
			List<Figure3D> loopsClone = new List<Figure3D>();
			foreach (Figure3D loop in loops)
				loopsClone.Add(loop.Clone());

			bool found;
			double minDistance;
			if (loops.Count > 0)
			{
				Figure3D actualLoop = loopsClone[0];
				Figure3D nearestLoop = null;
				int actualIndex = 0;
				loopsClone.RemoveAt(actualIndex);
				result.Add(actualLoop);

				while (loopsClone.Count > 0)
				{
					minDistance = double.MaxValue;
					found = false;
					for (int i = 0; i < loopsClone.Count; i++)
					{
						Figure3D loop = loopsClone[i];
						if (sortSingleLoops)
							loop.AutomaticSort(actualLoop.EndPoint, reverseLoopsIfNeeded, tolerance);

						if (actualLoop.EndPoint.IsEquals(loop.StartPoint, tolerance) == true)
						{
							actualLoop = loop;
							actualIndex = i;
							found = true;
							break;
						}
						else if (reverseLoopsIfNeeded && actualLoop.EndPoint.IsEquals(loop.EndPoint, tolerance) == true)
						{
							actualLoop = loop.Inverse();
							actualIndex = i;
							found = true;
							break;
						}
						else
						{
							double dist = actualLoop.EndPoint.Distance(loop.StartPoint);
							if (loop.Count == 0)
								dist = 10000000;

							if (dist < minDistance)
							{
								nearestLoop = loop;
								actualIndex = i;
								minDistance = dist;
							}
						}
					}
					if (found == false)
						actualLoop = nearestLoop;

					if (actualLoop != null)
					{
						result.Add(actualLoop);
					}
					loopsClone.RemoveAt(actualIndex);
				}
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
		public static Figure3D ApproxFigure(this Figure3D figure, bool lineApprox, bool arcApprox, bool ellipseApprox,
									 double lineStep, double arcStep, double ellipseStep,
									 bool adaptToEnd, bool maxStep)
		{
			Figure3D result = new Figure3D();
			for (int i = 0; i < figure.Count; i++)
			{
				Curve3D curve2 = figure[i];
				if (curve2 is Line3D)
				{
					if (lineApprox == false)
					{
						result.Add(curve2.Clone());
					}
					else
					{
						double length = curve2.Length;

						Point3D startPoint = curve2.StartPoint;
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
							endPoint = curve2.EvaluateAbs(offSet);
							result.Add(new Line3D(startPoint, endPoint));
							startPoint = endPoint;
						}
						// Aggiungo l'ultimo punto se non ci sono già
						endPoint = curve2.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));
					}
				}
				else if (curve2 is Arc3D arc)
				{
					if (arcApprox == false)
					{
						result.Add(curve2.Clone());
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
						endPoint = curve2.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));

					}
				}
				else if (curve2 is Helix3D helix)
				{
					if (arcApprox == false)
					{
						result.Add(curve2.Clone());
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
						endPoint = curve2.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));

					}
				}
				else if (curve2 is Ellipse3D ellipse)
				{
					if (ellipseApprox == false)
					{
						result.Add(curve2.Clone());
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
						endPoint = curve2.EndPoint;
						if (startPoint.IsEquals(endPoint) == false)
							result.Add(new Line3D(startPoint, endPoint));
					}
				}
				else if (curve2 is Spline3D spline)
				{
					List<Point3D> points = new List<Point3D>(spline.Points);
					if (spline.Closed)
						points.Add(points[0]);

					result.AddFigure(new Figure3D(points));
				}
				else
				{
					throw new Exception("Not exist case for this class: " + curve2.GetType().ToString());
				}
			}
			return result;
		}
	}
}
