using Axiom.GeoMath;
using Axiom.GeoShape.Curves2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Curves
{
	/// <summary>
	/// Estensione per le curve
	/// </summary>
	public static class CurveExtensions
	{
		/*
		#region STATICS
		/// <summary>
		/// Indica se considerare i punti di intersezione anche sugli estremi delle curve
		/// </summary>
		public static bool ConsiderExtremesInIntersections = true;
		#endregion STATICS

		/// <summary>
		/// Intersezione tra 2 Curve. 
		/// Vengono restituiti i punti di intersezione. 
		/// Le eventuali sovrapposizioni vengono ignorate.
		/// </summary>
		/// <param name="curve">Curva con cui determinare le intersezioni</param>
		/// <param name="intersections">Lista di punti intersezione</param>
		/// <param name="considerColinearity">Indica se considerare linee colineari</param>
		/// <returns>Indica se c'è o meno intersezione</returns>
		public bool Intersection(Curve curve, out List<Point3D> intersections, bool considerColinearity = false)
		{
			bool result = false;
			intersections = new List<Point3D>();

			// Line intersection
			if ((this is Line) && (curve is Line))
				result = ((Line)this).Intersection((Line)curve, out intersections, considerColinearity);

			// Arc intersection
			else if ((this is Arc2D) && (curve is Arc2D))
				result = ((Arc2D)this).Intersection((Arc2D)curve, out intersections);
			else if ((this is Arc2D) && (curve is Line))
				result = ((Arc2D)this).Intersection((Line)curve, out intersections);
			else if ((this is Line) && (curve is Arc2D))
				result = ((Arc2D)curve).Intersection((Line)this, out intersections);

			// Ellipse intersection
			else if ((this is Ellipse2D) && (curve is Ellipse2D))
				result = ((Ellipse2D)this).Intersection((Ellipse2D)curve, out intersections);
			else if ((this is Ellipse2D) && (curve is Line))
				result = ((Ellipse2D)this).Intersection((Line)curve, out intersections);
			else if ((this is Line) && (curve is Ellipse2D))
				result = ((Ellipse2D)curve).Intersection((Line)this, out intersections);
			else if ((this is Ellipse2D) && (curve is Arc2D))
				result = ((Ellipse2D)this).Intersection((Arc2D)curve, out intersections);
			else if ((this is Arc2D) && (curve is Ellipse2D))
				result = ((Ellipse2D)curve).Intersection((Arc2D)this, out intersections);

			// Spline intersection
			else if ((this is Spline2D) && (curve is Spline2D) == false)
				result = ((Spline2D)this).Intersection(curve, out intersections);
			else if ((curve is Spline2D) && (this is Spline2D) == false)
				result = ((Spline2D)curve).Intersection(this, out intersections);
			else if ((curve is Spline2D) && (this is Spline2D))
				result = (this as Spline2D).Interpolation.Intersection((curve as Spline2D).Interpolation, out intersections, considerColinearity);


			// PolyLine intersection
			else if (this is PolyLine2D)
				result = (this as PolyLine2D).ToFigure().Intersection(new Figure2D() { curve }, out intersections, considerColinearity);
			else if (curve is PolyLine2D)
				result = (curve as PolyLine2D).ToFigure().Intersection(new Figure2D() { this }, out intersections, considerColinearity);

			// Generic
			// questa andrà quando ci saranno tutte le intersezioni tra le varie curve e le linee
			//else
			//    result = subdivisionIntersection(curve, out intersections);

			return result;
		}

		/// <summary>
		/// Cerca le intersezioni approssimando la curva con linee
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="intersections"></param>
		/// <returns></returns>
		protected bool SubdivisionIntersection(Curve intersectionCurve, out List<Point3D> intersections)
		{
			bool result = false;
			intersections = new List<Point3D>();

			int maxTreeDepth = 20;
			SDVTree sdvTree = new SDVTree(maxTreeDepth, intersectionCurve.Clone(), Clone(), subdivisionIntersectionsBaseCaseDelegate);

			bool allDone = false;
			while (allDone == false)
			{
				// suddivido il nodo secondo necessità
				sdvTree.ActualNodeSubdivide();

				// proseguo la discesa puntando al primo figlio
				bool isChild = sdvTree.ActualNodeDown();

				// se non son riuscito a scendere sono una foglia
				if (isChild == false)
				{
					// salgo lungo l'albero finchè non trovo un nodo con dei figli da valutare
					do
					{
						// salgo di un livello al padre
						bool isFather = sdvTree.ActualNodeUp();

						if (isFather == false)
						{
							// ero già alla root ... esco
							allDone = true;
							break;
						}
					} while (sdvTree.ActualNode.ChildsCount == 0);

					// i nodi figli residui saranno ancora da valutare, per cui scendo
					if (sdvTree.ActualNode.ChildsCount > 0)
					{
						// proseguo la discesa puntando al primo figlio
						sdvTree.ActualNodeDown();
					}
				}
			}

			// recupero eventuali intersezioni
			intersections.AddRange(sdvTree.Intersections);

			if (intersections.Count > 0)
				result = true;

			return result;
		}

		/// <summary>
		/// Metodo di valutazione per il caso base Line-Curve
		/// </summary>
		/// <param name="line"></param>
		/// <param name="curveToTest"></param>
		/// <param name="intersections"></param>
		/// <returns></returns>
		private bool subdivisionIntersectionsBaseCaseDelegate(Line line, Curve curveToTest, out List<Point3D> intersections)
		{
			bool result = false;
			intersections = new List<Point3D>();

			// Line intersection
			if ((curveToTest is Line))
			{
				Point3D intersection;
				result = ((Line)curveToTest).Intersection(line, out intersection);
				if (result)
					intersections.Add(intersection);
			}

			// Arc intersection
			else if (curveToTest is Arc2D)
				result = ((Arc2D)curveToTest).Intersection(line, out intersections);

			// Ellipse intersection
			else if (curveToTest is Ellipse2D)
				result = ((Ellipse2D)curveToTest).Intersection(line, out intersections);

			// Spline intersection
			else if (curveToTest is Spline2D)
				result = ((Spline2D)curveToTest).Intersection(line, out intersections);

			return result;
		}
		*/
	}
}
