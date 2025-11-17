using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Globalization;
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

	}
}
