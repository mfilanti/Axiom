using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

namespace Axiom.GeoShape.Elements
{
	/// <summary>
	/// Estension per AABox3D per ottenere la figura 3D degli edge
	/// </summary>
	public static class AABox3DExtensions
	{
		/// <summary>
		/// Restituisce la figura che rappresenta gli edge del box
		/// </summary>
		public static Figure3D ToFigure(this AABBox3D box)
		{
			Figure3D result = new Figure3D();
			result.AddPolygon(box.MinPoint, box.XmaxYminZminPoint, box.XmaxYmaxZminPoint, box.XminYmaxZminPoint, box.MinPoint);
			result.AddPolygon(box.XminYminZmaxPoint, box.XmaxYminZmaxPoint, box.MaxPoint, box.XminYmaxZmaxPoint, box.XminYminZmaxPoint);
			result.AddPolygon(box.MinPoint, box.XminYminZmaxPoint);
			result.AddPolygon(box.XmaxYminZminPoint, box.XmaxYminZmaxPoint);
			result.AddPolygon(box.XmaxYmaxZminPoint, box.MaxPoint);
			result.AddPolygon(	box.XminYmaxZminPoint, box.XminYmaxZmaxPoint);
			return result;
		}

		/// <summary>
		/// Crea e restituisce un OBBox corrispondente
		/// </summary>
		/// <returns></returns>
		public static OBBox3D GetOBBOX(this AABBox3D box)
		{
			double lX = box.MaxPoint.X - box.MinPoint.X;
			double lY = box.MaxPoint.Y - box.MinPoint.Y;
			double lZ = box.MaxPoint.Z - box.MinPoint.Z;
			OBBox3D result = new OBBox3D(lX, lY, lZ);
			result.RTMatrix.Translation = (Vector3D)box.Center;
			return result;
		}


		/// <summary>
		/// Intersezione con una linea 3D. 
		/// p1 e p2 indicano la percentuale lungo la linea 3D. 
		/// plane1 e plane2 sono i piani con cui p1 e p2 hanno avuto intersezione.
		/// </summary>
		/// <param name="line"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static bool IntersectionLine(this AABBox3D box, Line3D line, out double p1, out double p2, out Plane3D plane1, out Plane3D plane2)
		{
			bool result = false;

			p1 = p2 = 0;
			OBBox3D oBBox = box.GetOBBOX();
			Point3D point1 = Point3D.NullPoint;
			Point3D point2 = Point3D.NullPoint;
			plane1 = Plane3D.ZeroPlane;
			plane2 = Plane3D.ZeroPlane;

			// per ogni piano
			foreach (BoxFace face in Enum.GetValues(typeof(BoxFace)))
			{
				Plane3D plane;
				oBBox.GetPlane(face, out plane);

				// trova l'intersezione
				Point3D intersectionPoint;
				bool isInside;
				bool intersect = plane.IntersectLine(line, out isInside, out intersectionPoint);
				// se la linea non è parallela al piano
				if (intersect)
				{
					AABBox3D enlargedBox = box.Clone();
					enlargedBox.Enlarge(MathUtils.FineTolerance, MathUtils.FineTolerance, MathUtils.FineTolerance);

					if (enlargedBox.Contains(intersectionPoint))
					{
						// trovato punto di intersezione;
						result = true;
						if (point1.IsNull())
						{
							point1 = intersectionPoint;
							plane1 = plane;
						}
						else if (point2.IsNull())
						{
							point2 = intersectionPoint;
							plane2 = plane;
						}
					}
				}
			}
			if (result == true)
			{
				line.IsOnCurve(point1, 0.5, out p1);
				line.IsOnCurve(point2, 0.5, out p2);
			}

			return result;
		}
	}
}
