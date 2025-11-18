using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Entities
{
	/// <summary>
	/// Estensioni per le entità 3D
	/// </summary>
	public static class Entity3DExtensions
	{
		/// <summary>
		/// Mesh3D da Entity3D. 
		/// Il parametro maxError viene considerato opportunamente. 
		/// Nel caso di Mesh3D viene restituito un clone.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromEntity3D(this Entity3D entity, double maxError)
		{
			Mesh3D result = null;

			if (entity is Mesh3D)
				result = entity.Clone() as Mesh3D;
			else if (entity is Cylinder3D cylinder)
				result = FromCylinder3D(cylinder, maxError);
			else if (entity is Extrusion3D extrusion)
				result = FromExtrusion3D(extrusion, maxError);
			else if (entity is SweepExtrusion3D sweep)
				result = FromSweepExtrusion3D(sweep, maxError);
			else if (entity is OBBox3D oBBox)
				result = FromOBBox3D(oBBox);
			else if (entity is Sphere3D sphere)
				result = FromSphere3D(sphere, maxError);
			else if (entity is Torus3D torus)
				result = FromTorus3D(torus, maxError);
			else if (entity is Revolution3D revolution)
				result = FromRevolution3D(revolution, maxError);
			else if (entity is PlanarFace3D planar)
				result = FromPlanarFace3D(planar, maxError);
			else if (entity is FigureEntity3D figureEntity)
				result = FromFigureEntity3D(figureEntity, maxError);

			return result;
		}



		#region CYLINDER3D
		/// <summary>
		/// Mesh3D da Cylinder3D. 
		/// Il parametro slices è meglio che sia divisibile per 4, così l'Outline corrisponderà 
		/// agli angoli 0, 90, 180 e 270. 
		/// Se slices è minore di 3 viene restituito null.
		/// </summary>
		/// <param name="cylinder"></param>
		/// <param name="slices"></param>
		/// <returns></returns>
		public static Mesh3D FromCylinder3D(this Cylinder3D cylinder, int slices)
		{
			Mesh3D result = null;
			if (cylinder != null && slices >= 3)
			{
				Point3D point;
				List<Point3D> points = new List<Point3D>();
				List<Vector3D> normals = new List<Vector3D>();
				List<Point3D> outlinePoints = new List<Point3D>();

				point = new Point3D(0, cylinder.Height);
				points.Add(point);
				normals.Add(Vector3D.UnitY);

				point = new Point3D(cylinder.Radius, cylinder.Height);
				points.Add(point);
				normals.Add(Vector3D.UnitY);
				outlinePoints.Add(point);

				point = new Point3D(cylinder.Radius, cylinder.Height);
				points.Add(point);
				normals.Add(Vector3D.UnitX);

				point = new Point3D(cylinder.Radius, 0);
				points.Add(point);
				normals.Add(Vector3D.UnitX);
				outlinePoints.Add(point);

				point = new Point3D(cylinder.Radius, 0);
				points.Add(point);
				normals.Add(Vector3D.NegativeUnitY);

				point = new Point3D(0, 0);
				points.Add(point);
				normals.Add(Vector3D.NegativeUnitY);

				// Snap points
				List<Point3D> snapEndPoints = new List<Point3D>();
				List<Point3D> snapMiddlePoints = new List<Point3D>();
				snapEndPoints.Add(new Point3D(0, cylinder.Height));
				snapMiddlePoints.Add(new Point3D(cylinder.Radius / 2, cylinder.Height));
				snapEndPoints.Add(new Point3D(cylinder.Radius, cylinder.Height));
				snapMiddlePoints.Add(new Point3D(cylinder.Radius, cylinder.Height / 2));
				snapEndPoints.Add(new Point3D(cylinder.Radius, 0));
				snapMiddlePoints.Add(new Point3D(cylinder.Radius / 2, 0));
				snapEndPoints.Add(new Point3D(0, 0));


				result = Mesh3DExtensions.FromRevolution(slices, points, normals, outlinePoints, snapEndPoints, snapMiddlePoints);

				// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
				cylinder.CloneTo(result);
			}
			return result;
		}

		/// <summary>
		/// Mesh3D da Cylinder3D. 
		/// Il parametro maxError viene considerato per calcolare gli slices, che comunque 
		/// verrà portato al primo numero maggiore divisibile per 4.
		/// </summary>
		/// <param name="cylinder"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromCylinder3D(this Cylinder3D cylinder, double maxError)
		{
			int slices = 0;
			if (maxError < cylinder.Radius)
			{
				double stepRadAngle = 0;
				stepRadAngle = 2 * Math.Acos((cylinder.Radius - maxError) / cylinder.Radius);
				slices = ((int)(2 * Math.PI / stepRadAngle) + 1);
				int r = slices % 4;
				if (r > 0)
					slices += 4 - r;
			}

			if (slices < 4)
				slices = 4;

			return FromCylinder3D(cylinder, slices);
		}

		#endregion CYLINDER3D

		#region EXTRUSION3D
		/// <summary>
		/// Mesh3D da Extrusion3D. 
		/// Il parametro maxError indica come approssimare la Figure3D del profilo. 
		/// </summary>
		/// <param name="extrusion"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromExtrusion3D(Extrusion3D extrusion, double maxError)
		{
			return FromExtrusion3D(extrusion, true, maxError);
		}

		/// <summary>
		/// Mesh3D da Extrusion3D. 
		/// Il parametro createStartEndSurfaces indica se creare le facce di chiusura 
		/// (la crea se il delegate è stato impostato e se il profilo è chiuso). 
		/// Il parametro maxError indica come approssimare la Figure3D del profilo. 
		/// </summary>
		/// <param name="extrusion"></param>
		/// <param name="createStartEndSurfaces"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromExtrusion3D(Extrusion3D extrusion, bool createStartEndSurfaces, double maxError)
		{
			Mesh3D result = null;
			if (extrusion != null)
			{
				Figure3D figure = extrusion.Shape.GetFigure();
				figure.IsClosed();
				figure.Reduce(true, false);
				if (figure.Count > 0)
				{
					result = new Mesh3D();
					result.VertexNormals = new List<TriangleNormals>();
					result.Outline = new List<Point3D>();
					double length = extrusion.Length;
					Vector3D direction = extrusion.ExtrusionDirection;

					List<Vector3D> tangents;
					Figure3D approx = figure.ApproxFigureMaxChordalDeviation(true, true, maxError, maxError, out tangents);

					List<Triangle3D> closingFace = null;
					if (createStartEndSurfaces && Delegates.ComputeTriangulation != null)
						closingFace = Delegates.ComputeTriangulation(approx);

					if (extrusion.StartCuts != null && extrusion.EndCuts != null)
						ModifyProfileByCuts(ref approx, ref tangents, direction, length, extrusion.StartCuts, extrusion.EndCuts);

					if (closingFace != null && extrusion.StartCuts != null && extrusion.EndCuts != null)
						closingFace = ModifyClosingFaceByCuts(closingFace, direction, length, extrusion.StartCuts, extrusion.EndCuts);

					#region Facce di estrusione
					for (int i = 0; i < approx.Count; i++)
					{
						Curve3D curve = approx[i];
						Point3D p1 = new(curve.StartPoint);
						Point3D p2 = new(curve.EndPoint);
						Point3D p3 = p2 + length * direction;
						Point3D p4 = p1 + length * direction;
						Vector3D n1 = (Vector3D)tangents[2 * i].Perpendicular();
						Vector3D n2 = (Vector3D)tangents[2 * i + 1].Perpendicular();

						Triangle3D triangle;
						triangle = new Triangle3D(GetPointFromCuts(p1, direction, extrusion.StartCuts),
												  GetPointFromCuts(p2, direction, extrusion.StartCuts),
												  GetPointFromCuts(p4, -direction, extrusion.EndCuts));
						result.Triangles.Add(triangle);
						triangle = new Triangle3D(GetPointFromCuts(p3, -direction, extrusion.EndCuts),
												  GetPointFromCuts(p4, -direction, extrusion.EndCuts),
												  GetPointFromCuts(p2, direction, extrusion.StartCuts));
						result.Triangles.Add(triangle);

						result.VertexNormals.Add(new TriangleNormals(n1, n2, n1));
						result.VertexNormals.Add(new TriangleNormals(n2, n1, n2));
					}
					#endregion Facce di estrusione

					#region Facce di chiusura
					if (closingFace != null)
					{
						foreach (Triangle3D closeTriangle in closingFace)
						{
							Point3D p1 = closeTriangle.P1;
							Point3D p2 = closeTriangle.P2;
							Point3D p3 = closeTriangle.P3;
							Point3D p4 = p1 + length * direction;
							Point3D p5 = p2 + length * direction;
							Point3D p6 = p3 + length * direction;

							Triangle3D triangle;

							// Faccia sotto
							triangle = new Triangle3D(GetPointFromCuts(p1, direction, extrusion.StartCuts),
													  GetPointFromCuts(p3, direction, extrusion.StartCuts),
													  GetPointFromCuts(p2, direction, extrusion.StartCuts));
							result.Triangles.Add(triangle);
							Vector3D n1 = triangle.Normal;

							// Faccia sopra
							triangle = new Triangle3D(GetPointFromCuts(p4, -direction, extrusion.EndCuts),
													  GetPointFromCuts(p5, -direction, extrusion.EndCuts),
													  GetPointFromCuts(p6, -direction, extrusion.EndCuts));
							result.Triangles.Add(triangle);
							Vector3D n2 = triangle.Normal;

							result.VertexNormals.Add(new TriangleNormals(n1, n1, n1));
							result.VertexNormals.Add(new TriangleNormals(n2, n2, n2));
						}
					}
					#endregion Facce di chiusura

					#region Outline
					// Outline del profilo
					foreach (Curve3D curve in approx)
					{
						Point3D point1 = new(curve.StartPoint);
						Point3D point2 = new(curve.EndPoint);
						Point3D point3 = point1 + length * direction;
						Point3D point4 = point2 + length * direction;
						Point3D p1 = GetPointFromCuts(point1, direction, extrusion.StartCuts);
						Point3D p2 = GetPointFromCuts(point2, direction, extrusion.StartCuts);
						Point3D p3 = GetPointFromCuts(point3, -direction, extrusion.EndCuts);
						Point3D p4 = GetPointFromCuts(point4, -direction, extrusion.EndCuts);

						result.Outline.Add(p1);
						result.Outline.Add(p2);
						result.Outline.Add(p3);
						result.Outline.Add(p4);
					}

					// Outline lungo la direzione di estrusione
					foreach (Curve3D curve in figure)
					{
						Point3D point1 = new(curve.StartPoint);
						Point3D point2 = new(curve.EndPoint);
						Point3D point3 = point1 + length * direction;
						Point3D point4 = point2 + length * direction;
						Point3D p1 = GetPointFromCuts(point1, direction, extrusion.StartCuts);
						Point3D p2 = GetPointFromCuts(point2, direction, extrusion.StartCuts);
						Point3D p3 = GetPointFromCuts(point3, -direction, extrusion.EndCuts);
						Point3D p4 = GetPointFromCuts(point4, -direction, extrusion.EndCuts);

						result.Outline.Add(p1);
						result.Outline.Add(p3);
						result.Outline.Add(p2);
						result.Outline.Add(p4);
					}
					#endregion Outline

					#region SnapPoints
					result.SnapEndPoints = new List<Point3D>();
					result.SnapMiddlePoints = new List<Point3D>();
					Curve3D prevCurve = figure[figure.Count - 1];
					foreach (Curve3D curve in figure)
					{
						if (curve.StartPoint.IsEquals(prevCurve.EndPoint) == false)
						{
							Point3D pS1 = new(curve.StartPoint);
							Point3D pS2 = pS1 + length * direction;
							Point3D pS3 = pS1 + length / 2 * direction;
							pS1 = GetPointFromCuts(pS1, direction, extrusion.StartCuts);
							pS2 = GetPointFromCuts(pS2, -direction, extrusion.EndCuts);
							result.SnapEndPoints.Add(pS1);
							result.SnapEndPoints.Add(pS2);
							result.SnapMiddlePoints.Add(pS3);
						}
						Point3D pM1 = new(curve.Evaluate(0.5));
						Point3D pM2 = pM1 + length * direction;
						pM1 = GetPointFromCuts(pM1, direction, extrusion.StartCuts);
						pM2 = GetPointFromCuts(pM2, -direction, extrusion.EndCuts);
						result.SnapMiddlePoints.Add(pM1);
						result.SnapMiddlePoints.Add(pM2);

						Point3D pE1 = new(curve.EndPoint);
						Point3D pE2 = pE1 + length * direction;
						Point3D pE3 = pE1 + length / 2 * direction;
						pE1 = GetPointFromCuts(pE1, direction, extrusion.StartCuts);
						pE2 = GetPointFromCuts(pE2, -direction, extrusion.EndCuts);
						result.SnapEndPoints.Add(pE1);
						result.SnapEndPoints.Add(pE2);
						result.SnapMiddlePoints.Add(pE3);

						prevCurve = curve;
					}

					#endregion SnapPoints

					// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
					extrusion.CloneTo(result);
				}
			}
			return result;
		}

		#region Private Methods
		/// <summary>
		/// Proietta i punti sui piani di taglio e prende quello più lontano
		/// </summary>
		private static Point3D GetPointFromCuts(Point3D point, Vector3D direction, List<Plane3D> cuts)
		{
			Point3D result = point;
			if (cuts != null)
			{
				double maxDist = 0; // N.B. Qui è importante zero
				foreach (Plane3D plane in cuts)
				{
					Point3D intersection;
					plane.IntersectRay(new Ray3D(point, direction), out intersection);
					double dist = (intersection - point).Dot(direction); // N.B. Qui non usare la distanza perchè ci serve una distanza CON segno
					if (dist > maxDist)
					{
						result = intersection;
						maxDist = dist;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Divide in due ogni linea del profilo che si trova lungo le intersezioni dei cuts
		/// </summary>
		private static void ModifyProfileByCuts(ref Figure3D profile, ref List<Vector3D> tangents, Vector3D direction, double length, List<Plane3D> startCuts, List<Plane3D> endCuts)
		{
			List<Point3D> newProfile = new List<Point3D>();
			List<Vector3D> newTangents = new List<Vector3D>();
			List<Line3D> cutLines = GetCutLines(direction, length, startCuts, endCuts);
			for (int i = 0; i < profile.Count; i++)
			{
				Curve3D curve = profile[i];
				Line3D line = curve as Line3D;
				List<Point3D> intersections = GetOrderedIntersections(line, cutLines);
				// Aggiungo il primo
				newProfile.Add(line.PStart);
				newTangents.Add(tangents[2 * i]);
				foreach (Point3D point in intersections)
				{
					// Aggiungo 2 volte ogni intersezione
					newProfile.Add(point);
					newProfile.Add(point);
					// Replico le tangenti 
					newTangents.Add(tangents[2 * i]);
					newTangents.Add(tangents[2 * i]);
				}
				// Aggiungo l'ultimo
				newProfile.Add(line.PEnd);
				newTangents.Add(tangents[2 * i + 1]);
			}

			profile = new Figure3D();
			for (int i = 0; i < newProfile.Count - 1; i += 2)
				profile.Add(new Line3D(newProfile[i], newProfile[i + 1]));

			tangents = newTangents;
		}

		/// <summary>
		/// Restituisce la lista di linee che rappresentano l'intersezione tra i vari piani
		/// </summary>
		private static List<Line3D> GetCutLines(Vector3D direction, double length, List<Plane3D> startCuts, List<Plane3D> endCuts)
		{
			List<Line3D> result = new List<Line3D>();
			// Copio i piani perchè ne aggiungo quello a 90°
			List<Plane3D> lStartCuts = new List<Plane3D>(startCuts);
			List<Plane3D> lEndCuts = new List<Plane3D>(endCuts);
			// Start
			lStartCuts.Add(new Plane3D(Vector3D.NegativeUnitZ, Point3D.Zero));
			for (int i = 0; i < lStartCuts.Count; i++)
			{
				for (int j = i + 1; j < lStartCuts.Count; j++)
				{
					Plane3D plane1 = lStartCuts[i];
					Plane3D plane2 = lStartCuts[j];
					Point3D origin;
					Vector3D intDirection;
					if (plane1.IntersectPlane(plane2, out origin, out intDirection))
						result.Add(new Line3D((Point3D)origin, (Point3D)(origin + 100 * intDirection)));
				}
			}
			// End
			lEndCuts.Add(new Plane3D(Vector3D.UnitZ, Point3D.Zero + length * direction));
			for (int i = 0; i < lEndCuts.Count; i++)
			{
				for (int j = i + 1; j < lEndCuts.Count; j++)
				{
					Plane3D plane1 = lEndCuts[i];
					Plane3D plane2 = lEndCuts[j];
					Point3D origin;
					Vector3D intDirection;
					if (plane1.IntersectPlane(plane2, out origin, out intDirection))
						result.Add(new Line3D((Point3D)origin, (Point3D)(origin + 100 * intDirection)));
				}
			}

			return result;
		}

		/// <summary>
		/// Il segmento line viene intersecato con le linee (considerate infinite) contenute in cutLines
		/// </summary>
		private static List<Point3D> GetOrderedIntersections(Line3D line, List<Line3D> cutLines)
		{
			List<Point3D> result = new List<Point3D>();
			List<double> uList = new List<double>();
			foreach (Line3D cutLine in cutLines)
			{
				double u;
				if (Intersection(line, cutLine, out u))
					uList.Add(u);
			}
			uList.Sort();
			foreach (float u in uList)
				result.Add(line.Evaluate(u));

			return result;
		}

		/// <summary>
		/// Il segmento line viene intersecato con la linea infLine (considerata infinita). 
		/// Il risultato u indica il punto di intersezione lungo line.
		/// </summary>
		private static bool Intersection(Line3D line, Line3D infLine, out double u)
		{
			bool result = false;
			double x1 = line.PStart.X, y1 = line.PStart.Y;
			double x2 = line.PEnd.X, y2 = line.PEnd.Y;
			double x3 = infLine.PStart.X, y3 = infLine.PStart.Y;
			double x4 = infLine.PEnd.X, y4 = infLine.PEnd.Y;
			double uA;
			double numA = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
			double den = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

			u = 0;
			if (den.IsEquals(0.001) == false)
			{
				uA = numA / den;
				if (uA >= 0 && uA <= 1)
				{
					// Intersezione appartiene al primo segmento
					result = true;
					u = uA;
				}
			}

			return result;
		}

		/// <summary>
		/// Dividere in due tutti i triangoli lungo le intersezioni dei cuts, 
		/// ritriangolando eventualmente i trapezoidi ottenuti
		/// </summary>
		private static List<Triangle3D> ModifyClosingFaceByCuts(List<Triangle3D> closingFace, Vector3D direction, double length,
																List<Plane3D> startCuts, List<Plane3D> endCuts)
		{
			List<Triangle3D> result = new List<Triangle3D>();
			List<Line3D> cutLines = GetCutLines(direction, length, startCuts, endCuts);

			List<Plane3D> cutPlanes = new List<Plane3D>();
			foreach (Line3D line in cutLines)
			{
				List<Triangle3D> locResult = new List<Triangle3D>();
				cutPlanes.Add(new Plane3D(direction.Cross((Vector3D)line.StartTangent), (Point3D)line.StartPoint));
			}
			// Per ogni taglio suddivido i triangoli            
			foreach (Triangle3D triangle in closingFace)
			{
				List<Triangle3D> triangles = triangle.MultiSubdivide(cutPlanes);
				result.AddRange(triangles);
			}

			return result;
		}
		#endregion

		#endregion EXTRUSION3D

		#region SWEEPEXTRUSION3D
		/// <summary>
		/// Mesh3D da SweepExtrusion3D. 
		/// Il parametro maxError indica come approssimare la Figure3D del profilo e la Figure3D del percorso. 
		/// </summary>
		/// <param name="extrusion"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromSweepExtrusion3D(SweepExtrusion3D extrusion, double maxError)
		{
			return FromSweepExtrusion3D(extrusion, true, maxError);
		}

		/// <summary>
		/// Mesh3D da SweepExtrusion3D. 
		/// Il parametro createStartEndSurfaces indica se creare le facce di chiusura 
		/// (la crea se il delegate è stato impostato e se il profilo è chiuso). 
		/// Il parametro maxError indica come approssimare la Figure3D del profilo e la Figure3D del percorso. 
		/// </summary>
		/// <param name="extrusion"></param>
		/// <param name="createStartEndSurfaces"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromSweepExtrusion3D(SweepExtrusion3D sweep, bool createStartEndSurfaces, double maxError)
		{
			Mesh3D result = null;
			if (sweep != null)
			{
				Figure3D section = sweep.Shape.GetFigure();
				section.IsClosed();
				section.Reduce(true, false);
				if (section.Count > 0)
				{
					result = new Mesh3D();
					result.VertexNormals = new List<TriangleNormals>();
					result.Outline = new List<Point3D>();

					Figure3D section3 = section.Clone();
					List<Vector3D> sectionTangents;
					Figure3D sectionApprox = section3.ApproxFigureMaxChordalDeviation(true, true, maxError, maxError, out sectionTangents);

					List<Triangle3D> closingFace = null;
					if (createStartEndSurfaces && Delegates.ComputeTriangulation != null)
						closingFace = Delegates.ComputeTriangulation(sectionApprox);

					List<Vector3D> pathTangents;
					Figure3D pathApprox = sweep.ExtrusionPath.ApproxFigureMaxChordalDeviation(true, true, maxError, maxError, out pathTangents);
					List<Point3D> outlinePoints = new List<Point3D>() { section3.StartPoint };
					foreach (Curve3D curve in section3)
					{
						if (curve.StartPoint != outlinePoints[outlinePoints.Count - 1])
							outlinePoints.Add(curve.StartPoint);

						if (curve.EndPoint != outlinePoints[outlinePoints.Count - 1])
							outlinePoints.Add(curve.EndPoint);
					}

					Figure3D previousSection;
					List<Vector3D> previousSectionTangents;
					List<Point3D> previousOutlinePoints;
					Vector3D lZ = -pathTangents[0];
					Vector3D lX = Vector3D.UnitZ.Cross(lZ);
					Plane3D plane = new Plane3D(lZ, lX, pathApprox[0].StartPoint);
					RTMatrix matrix = plane.GetRTMatrix();
					GetSection(matrix, sectionApprox, out previousSection);
					GetTangents(matrix, sectionTangents, out previousSectionTangents);
					GetOutLine(matrix, outlinePoints, out previousOutlinePoints);
					AddOutline(previousSection, ref result);
					if (closingFace != null) AddClosingFace(closingFace, false, matrix, ref result);

					for (int i = 0; i < pathApprox.Count; i++)
					{
						Vector3D actualPathTangent = pathTangents[2 * i + 1];
						Vector3D previousPathTangent = pathTangents[2 * i];
						Figure3D actualSection;
						List<Vector3D> actualSectionTangents;
						List<Point3D> actualOutlinePoints;
						lZ = -actualPathTangent;
						lX = Vector3D.UnitZ.Cross(lZ);
						plane = new Plane3D(lZ, lX, pathApprox[i].EndPoint);
						matrix = plane.GetRTMatrix();
						GetSection(matrix, sectionApprox, out actualSection);
						GetTangents(matrix, sectionTangents, out actualSectionTangents);
						GetOutLine(matrix, outlinePoints, out actualOutlinePoints);
						AddTriangles(previousSection, actualSection, previousPathTangent, actualPathTangent, previousSectionTangents, actualSectionTangents, ref result);
						AddOutline(previousOutlinePoints, actualOutlinePoints, ref result);
						previousSection = actualSection;
						previousSectionTangents = actualSectionTangents;
						previousOutlinePoints = actualOutlinePoints;
					}
					AddOutline(previousSection, ref result);
					if (closingFace != null) AddClosingFace(closingFace, true, matrix, ref result);

					// Tagli
					if (sweep.StartCuts != null && sweep.StartCuts.Count > 0)
					{
						foreach (Plane3D cutPlane in sweep.StartCuts)
						{
							Plane3D cutClone = cutPlane;
							// La normale del piano è importante, eventualmente la inverto
							if (sweep.ExtrusionPath.StartTangent.Dot(cutClone.Normal) > 0)
								cutClone.Normal = -cutClone.Normal;

							result.CutByPlane(cutClone, true);
						}
					}
					if (sweep.EndCuts != null && sweep.EndCuts.Count > 0)
					{
						foreach (Plane3D cutPlane in sweep.EndCuts)
						{
							Plane3D cutClone = cutPlane;
							// La normale del piano è importante, eventualmente la inverto
							if (sweep.ExtrusionPath.StartTangent.Dot(cutClone.Normal) < 0)
								cutClone.Normal = -cutClone.Normal;

							result.CutByPlane(cutClone, true);
						}
					}

					// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
					sweep.CloneTo(result);
				}
			}
			return result;
		}

		#region Private Static Methods for SweepExtrusion3D
		private static void AddClosingFace(List<Triangle3D> closingFace, bool up, RTMatrix matrix, ref Mesh3D result)
		{
			foreach (Triangle3D triangle in closingFace)
			{
				Triangle3D tClone = triangle;
				if (!up)
					tClone = new Triangle3D(triangle.P1, triangle.P3, triangle.P2);

				tClone.ApplyRT(matrix);
				result.Triangles.Add(tClone);
				Vector3D n = tClone.Normal;
				result.VertexNormals.Add(new TriangleNormals(n, n, n));
			}
		}

		private static void AddOutline(Figure3D section, ref Mesh3D mesh)
		{
			foreach (Curve3D curve in section)
			{
				mesh.Outline.Add(curve.StartPoint);
				mesh.Outline.Add(curve.EndPoint);
			}
		}

		private static void AddTriangles(Figure3D previousSection, Figure3D actualSection, Vector3D previousPathTangent, Vector3D actualPathTangent,
										 List<Vector3D> previousTangents, List<Vector3D> actualTangents, ref Mesh3D result)
		{
			for (int i = 0; i < previousSection.Count; i++)
			{
				Point3D p1 = previousSection[i].StartPoint;
				Point3D p2 = previousSection[i].EndPoint;
				Point3D p3 = actualSection[i].EndPoint;
				Point3D p4 = actualSection[i].StartPoint;
				Vector3D n1 = previousPathTangent.Cross(previousTangents[2 * i]);
				Vector3D n2 = previousPathTangent.Cross(previousTangents[2 * i + 1]);
				Vector3D n3 = actualPathTangent.Cross(actualTangents[2 * i + 1]);
				Vector3D n4 = actualPathTangent.Cross(actualTangents[2 * i]);

				Triangle3D triangle;
				triangle = new Triangle3D(p1, p2, p4);
				result.Triangles.Add(triangle);
				triangle = new Triangle3D(p3, p4, p2);
				result.Triangles.Add(triangle);

				result.VertexNormals.Add(new TriangleNormals(n1, n2, n4));
				result.VertexNormals.Add(new TriangleNormals(n3, n4, n2));
			}
		}

		private static void AddOutline(List<Point3D> previousOutlinePoints, List<Point3D> actualOutlinePoints, ref Mesh3D mesh)
		{
			for (int i = 0; i < previousOutlinePoints.Count; i++)
			{
				mesh.Outline.Add(previousOutlinePoints[i]);
				mesh.Outline.Add(actualOutlinePoints[i]);
			}
		}

		private static void GetSection(RTMatrix matrix, Figure3D sectionApprox, out Figure3D previousSection)
		{
			previousSection = sectionApprox.Clone();
			previousSection.ApplyRT(matrix);
		}

		private static void GetTangents(RTMatrix matrix, List<Vector3D> refTangents, out List<Vector3D> tangents)
		{
			tangents = new List<Vector3D>();
			foreach (Vector3D tangent in refTangents)
				tangents.Add(matrix.Multiply(tangent));
		}

		private static void GetOutLine(RTMatrix matrix, List<Point3D> refOutlinePoints, out List<Point3D> outlinePoints)
		{
			outlinePoints = new List<Point3D>();
			foreach (Point3D point in refOutlinePoints)
				outlinePoints.Add(matrix.Multiply(point));
		}
		#endregion
		#endregion

		#region OOBBOX3D
		/// <summary>
		/// Mesh3D da OBBox3D
		/// </summary>
		/// <param name="bbox"></param>
		/// <returns></returns>
		public static Mesh3D FromOBBox3D(OBBox3D bbox)
		{
			Mesh3D result = null;
			if (bbox != null)
			{
				result = new Mesh3D();
				result.Outline = [];
				Point3D p1 = new(-bbox.LX / 2, -bbox.LY / 2, -bbox.LZ / 2);
				Point3D p2 = new(bbox.LX / 2, -bbox.LY / 2, -bbox.LZ / 2);
				Point3D p3 = new(bbox.LX / 2, bbox.LY / 2, -bbox.LZ / 2);
				Point3D p4 = new(-bbox.LX / 2, bbox.LY / 2, -bbox.LZ / 2);
				Point3D p5 = new(-bbox.LX / 2, -bbox.LY / 2, bbox.LZ / 2);
				Point3D p6 = new(bbox.LX / 2, -bbox.LY / 2, bbox.LZ / 2);
				Point3D p7 = new(bbox.LX / 2, bbox.LY / 2, bbox.LZ / 2);
				Point3D p8 = new(-bbox.LX / 2, bbox.LY / 2, bbox.LZ / 2);
				// Sotto
				result.Triangles.Add(new(p4, p3, p1));
				result.Triangles.Add(new(p2, p1, p3));
				// Sopra
				result.Triangles.Add(new(p5, p6, p8));
				result.Triangles.Add(new(p7, p8, p6));
				// Anteriore
				result.Triangles.Add(new(p1, p2, p5));
				result.Triangles.Add(new(p6, p5, p2));
				// Posteriore
				result.Triangles.Add(new(p3, p4, p7));
				result.Triangles.Add(new(p8, p7, p4));
				// Sinistra
				result.Triangles.Add(new(p4, p1, p8));
				result.Triangles.Add(new(p5, p8, p1));
				// Destra
				result.Triangles.Add(new(p2, p3, p6));
				result.Triangles.Add(new(p7, p6, p3));
				// Outline
				result.Outline.AddRange([p1, p2, p2, p3, p3, p4, p4, p1]);
				result.Outline.AddRange([p5, p6, p6, p7, p7, p8, p8, p5]);
				result.Outline.AddRange([p1, p5, p2, p6, p3, p7, p4, p8]);

				#region SnapPoints
				result.SnapEndPoints = [.. new Point3D[] { p1, p2, p3, p4, p5, p6, p7, p8 }];
				result.SnapMiddlePoints = [];
				Figure3D figure = new Figure3D();
				figure.AddPolygon(p1, p2, p3, p4, p1);
				figure.AddPolygon(p5, p6, p7, p8, p5);
				figure.AddPolygon(p1, p5);
				figure.AddPolygon(p2, p6);
				figure.AddPolygon(p3, p7);
				figure.AddPolygon(p4, p8);
				foreach (Curve3D curve in figure)
					result.SnapMiddlePoints.Add(curve.Evaluate(0.5));

				#endregion SnapPoints

				// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
				bbox.CloneTo(result);
			}
			return result;
		}
		#endregion
		#region SPHERE3D
		/// <summary>
		/// Mesh3D da Sphere3D. 
		/// Il parametro maxError viene considerato per calcolare gli slices, che comunque 
		/// verrà portato al primo numero maggiore divisibile per 4.
		/// </summary>
		/// <param name="sphere"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromSphere3D(Sphere3D sphere, double maxError)
		{
			int slices = 0;
			if (maxError < sphere.Radius)
			{
				double stepRadAngle = 0;
				stepRadAngle = 2 * Math.Acos((sphere.Radius - maxError) / sphere.Radius);
				slices = ((int)(2 * Math.PI / stepRadAngle) + 1);
				int r = slices % 4;
				if (r > 0)
					slices += 4 - r;
			}

			if (slices < 4)
				slices = 4;

			return FromSphere3D(sphere, slices, slices / 2);
		}

		/// <summary>
		/// Mesh3D da Sphere3D. 
		/// Il parametro slices è meglio che sia divisibile per 4, così l'Outline corrisponderà 
		/// agli angoli 0, 90, 180 e 270. 
		/// Se slices è minore di 3 viene restituito null.
		/// Il parametro stacks è meglio che sia pari così l'Outline orizzontale corrisponderà 
		/// all'equatore. 
		/// Se stacks è minore di 2 viene restituito null. 
		/// </summary>
		/// <param name="sphere"></param>
		/// <param name="slices"></param>
		/// <param name="stacks"></param>
		/// <returns></returns>
		public static Mesh3D FromSphere3D(Sphere3D sphere, int slices, int stacks)
		{
			Mesh3D result = null;
			if (sphere != null && slices >= 3 && stacks >= 2)
			{
				Point3D point = Point3D.NullPoint;
				Vector3D tangent;
				Vector3D normal = Vector3D.NullVector;
				Arc3D arc = new Arc3D(Point3D.Zero, sphere.Radius, Math.PI / 2, -Math.PI / 2, false, RTMatrix.Identity);
				double offsetRadAngle = Math.PI / stacks;
				List<Point3D> points = new List<Point3D>();
				List<Vector3D> normals = new List<Vector3D>();
				List<Point3D> outlinePoints = new List<Point3D>();
				int equator = stacks / 2;
				for (int i = 0; i <= stacks; i++)
				{
					if (i > 1)
					{
						points.Add(point);
						normals.Add(normal);
					}
					point = arc.EvaluateAngle(i * offsetRadAngle, out tangent);
					normal = tangent.Perpendicular();
					points.Add(point);
					normals.Add(normal);
					// Equatore
					if (i == equator)
						outlinePoints.Add(point);
				}

				// Snap points
				List<Point3D> snapEndPoints = new List<Point3D>();
				List<Point3D> snapMiddlePoints = new List<Point3D>();
				snapEndPoints.Add(arc.StartPoint);
				snapMiddlePoints.Add(arc.Evaluate(0.25));
				snapEndPoints.Add(arc.Evaluate(0.5));
				snapMiddlePoints.Add(arc.Evaluate(0.75));
				snapEndPoints.Add(arc.EndPoint);

				result = Mesh3DExtensions.FromRevolution(slices, points, normals, outlinePoints, snapEndPoints, snapMiddlePoints);

				// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
				sphere.CloneTo(result);
			}
			return result;
		}
		#endregion



		/// <summary>
		/// Mesh3D da Torus3D. 
		/// Il parametro maxError viene considerato per calcolare gli slices, che comunque 
		/// verrà portato al primo numero maggiore divisibile per 4.
		/// </summary>
		/// <param name="torus"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromTorus3D(Torus3D torus, double maxError)
		{
			int slices = 0;
			if (maxError < torus.OuterRadius)
			{
				double stepRadAngle = 0;
				stepRadAngle = 2 * Math.Acos((torus.OuterRadius - maxError) / torus.OuterRadius);
				slices = ((int)(2 * Math.PI / stepRadAngle) + 1);
				int r = slices % 4;
				if (r > 0)
					slices += 4 - r;
			}

			if (slices < 4)
				slices = 4;

			int sides = 0;
			if (maxError < torus.InnerRadius)
			{
				double stepRadAngle = 0;
				stepRadAngle = 2 * Math.Acos((torus.InnerRadius - maxError) / torus.InnerRadius);
				sides = ((int)(2 * Math.PI / stepRadAngle) + 1);
				int r = slices % 4;
				if (r > 0)
					sides += 4 - r;
			}

			if (sides < 4)
				sides = 4;

			return FromTorus3D(torus, slices, sides);
		}

		/// <summary>
		/// Mesh3D da Torus3D. 
		/// Il parametro slices è meglio che sia divisibile per 4, così l'Outline corrisponderà 
		/// agli angoli 0, 90, 180 e 270. 
		/// Se slices è minore di 3 viene restituito null.
		/// Il parametro sides è meglio che sia divisibile per 4 così l'Outline orizzontale 
		/// corrisponderà ai 4 equatori. 
		/// Se sides è minore di 3 viene restituito null. 
		/// </summary>
		/// <param name="torus"></param>
		/// <param name="sides"></param>
		/// <param name="slices"></param>
		/// <returns></returns>
		public static Mesh3D FromTorus3D(Torus3D torus, int sides, int slices)
		{
			Mesh3D result = null;
			if (torus != null && slices >= 3 && sides >= 3)
			{
				Point3D point = Point3D.NullPoint;
				Vector3D tangent;
				Vector3D normal = Vector3D.NullVector;
				Arc3D arc = new Arc3D(new Point3D(torus.OuterRadius, 0), torus.InnerRadius, Math.PI / 2, Math.PI / 2 - 0.001, true, RTMatrix.Identity);
				double offsetRadAngle = 2 * Math.PI / sides;
				List<Point3D> points = new List<Point3D>();
				List<Vector3D> normals = new List<Vector3D>();
				List<Point3D> outlinePoints = new List<Point3D>();
				int[] equators = new int[] { 0, sides / 2, sides / 4, sides * 3 / 4 };
				for (int i = 0; i <= sides; i++)
				{
					if (i > 1)
					{
						points.Add(point);
						normals.Add(normal);
					}
					point = arc.EvaluateAngle(i * offsetRadAngle, out tangent);
					normal = tangent.Perpendicular();
					points.Add(point);
					normals.Add(normal);
					// Equatori
					if (i == equators[0] || i == equators[1] || i == equators[2] || i == equators[3])
						outlinePoints.Add(point);
				}

				// Snap points
				List<Point3D> snapEndPoints = new List<Point3D>();
				List<Point3D> snapMiddlePoints = new List<Point3D>();
				snapEndPoints.Add(arc.StartPoint);
				snapMiddlePoints.Add(arc.Evaluate(0.125));
				snapEndPoints.Add(arc.Evaluate(0.25));
				snapMiddlePoints.Add(arc.Evaluate(0.375));
				snapEndPoints.Add(arc.Evaluate(0.5));
				snapMiddlePoints.Add(arc.Evaluate(0.627));
				snapEndPoints.Add(arc.Evaluate(0.75));
				snapMiddlePoints.Add(arc.Evaluate(0.875));

				result = Mesh3DExtensions.FromRevolution(slices, points, normals, outlinePoints, snapEndPoints, snapMiddlePoints);

				// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
				torus.CloneTo(result);
			}
			return result;
		}

		/// <summary>
		/// Mesh3D da Revolution3D. 
		/// Il parametro maxError indica come approssimare la Figure3D della sezione. 
		/// Il parametro maxError viene anche considerato per calcolare gli slices, che comunque 
		/// verrà portato al primo numero maggiore divisibile per 4.
		/// </summary>
		/// <param name="revolution"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromRevolution3D(Revolution3D revolution, double maxError)
		{
			int slices = 0;
			AABBox3D box = revolution.Shape.GetFigure().GetABBox();
			double maxRadius = Math.Max(Math.Abs(box.MaxPoint.X), Math.Abs(box.MinPoint.X));
			if (maxError < maxRadius)
			{
				double stepRadAngle = 0;
				stepRadAngle = 2 * Math.Acos((maxRadius - maxError) / maxRadius);
				slices = ((int)(2 * Math.PI / stepRadAngle) + 1);
				int r = slices % 4;
				if (r > 0)
					slices += 4 - r;
			}

			if (slices < 4)
				slices = 4;

			return Mesh3DExtensions.FromRevolution3D(revolution, maxError, slices);
		}

		/// <summary>
		/// Mesh3D da Revolution3D. 
		/// Il parametro maxError indica come approssimare la Figure3D della sezione. 
		/// Il parametro slices è meglio che sia divisibile per 4, così l'Outline corrisponderà 
		/// agli angoli 0, 90, 180 e 270. 
		/// Se slices è minore di 3 viene restituito null.
		/// </summary>
		/// <param name="revolution"></param>
		/// <param name="maxError"></param>
		/// <param name="slices"></param>
		/// <returns></returns>
		public static Mesh3D FromRevolution3D(Revolution3D revolution, double maxError, int slices)
		{
			Mesh3D result = null;
			if (revolution != null && slices >= 3)
			{
				Figure3D figure = revolution.Shape.GetFigure();
				figure.Reduce(true, true);
				if (figure.Count > 0)
				{
					List<Vector3D> tangents;
					Figure3D approx = figure.ApproxFigureMaxChordalDeviation(true, true, maxError, maxError, out tangents);
					List<Point3D> points = new List<Point3D>();
					List<Vector3D> normals = new List<Vector3D>();
					for (int i = 0; i < approx.Count; i++)
					{
						Curve3D curve = approx[i];
						points.Add(curve.StartPoint);
						points.Add(curve.EndPoint);
						normals.Add(tangents[2 * i].Perpendicular());
						normals.Add(tangents[2 * i + 1].Perpendicular());
					}

					// Outline orizzontale
					List<Point3D> outlinePoints = new List<Point3D>();
					foreach (Curve3D curve in figure)
					{
						Point3D start = curve.StartPoint;
						if (start.X.IsEquals(0) == false)
							outlinePoints.Add(start);
					}
					Point3D end = figure.EndPoint;
					if (end.X.IsEquals(0) == false)
						outlinePoints.Add(end);

					// Snap points
					List<Point3D> snapEndPoints = new List<Point3D>();
					List<Point3D> snapMiddlePoints = new List<Point3D>();
					Curve3D prevCurve = figure[figure.Count - 1];
					foreach (Curve3D curve in figure)
					{
						if (curve.StartPoint.IsEquals(curve.StartPoint) == false)
							snapEndPoints.Add(curve.StartPoint);

						snapEndPoints.Add(curve.EndPoint);
						snapMiddlePoints.Add(curve.Evaluate(0.5));
					}

					result = Mesh3DExtensions.FromRevolution(slices, points, normals, outlinePoints, snapEndPoints, snapMiddlePoints);

					// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
					revolution.CloneTo(result);
				}
			}
			return result;
		}

		/// <summary>
		/// Mesh3D da PlanarFace3D. 
		/// Il parametro maxError indica come approssimare la Figure3D che rappresenta la faccia. 
		/// </summary>
		/// <param name="planarFace"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromPlanarFace3D(PlanarFace3D planarFace, double maxError)
		{
			Mesh3D result = null;
			if (planarFace != null)
			{
				Figure3D figure = planarFace.Shape.GetFigure();
				figure.Reduce(true, true);
				if (figure.Count > 0)
				{
					result = new Mesh3D();
					//result.VertexNormals = new List<TriangleNormals>();
					result.Outline = new List<Point3D>();

					Figure3D approx = figure.ApproxFigureMaxChordalDeviation(true, true, maxError, maxError);

					if (Delegates.ComputeTriangulation != null)
					{
						List<Triangle3D> triangulation = Delegates.ComputeTriangulation(approx);
						if (triangulation != null)
							result.Triangles = triangulation;
					}

					// Outline del profilo
					foreach (Curve3D curve in approx)
					{
						Point3D point1 = new(curve.StartPoint);
						Point3D point2 = new(curve.EndPoint);
						result.Outline.Add(point1);
						result.Outline.Add(point2);
					}

					#region SnapPoints
					result.SnapEndPoints = new List<Point3D>();
					result.SnapMiddlePoints = new List<Point3D>();
					Curve3D prevCurve = figure[figure.Count - 1];
					foreach (Curve3D curve in figure)
					{
						if (curve.StartPoint.IsEquals(prevCurve.EndPoint) == false)
						{
							Point3D pS = new(curve.StartPoint);
							result.SnapEndPoints.Add(pS);
						}
						Point3D pM = new(curve.Evaluate(0.5));
						result.SnapMiddlePoints.Add(pM);

						Point3D pE = new(curve.EndPoint);
						result.SnapEndPoints.Add(pE);
					}

					#endregion SnapPoints

					// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
					planarFace.CloneTo(result);
				}
			}
			return result;
		}

		/// <summary>
		/// Mesh3D da FigureEntity3D. 
		/// Viene restituita una Mesh con 0 triangoli e con una outline che rappresenta la Figure3D interna. 
		/// Il parametro maxError indica come approssimare la Figure3D interna.
		/// </summary>
		/// <param name="figureEntity3D"></param>
		/// <param name="maxError"></param>
		/// <returns></returns>
		public static Mesh3D FromFigureEntity3D(FigureEntity3D figureEntity3D, double maxError)
		{
			Mesh3D result = null;
			if (figureEntity3D != null)
			{
				Figure3D figure = figureEntity3D.Figure.Clone();
				if (figure.Count > 0)
				{
					result = new Mesh3D();
					result.Outline = new List<Point3D>();

					Figure3D approx = figure.ApproxFigureMaxChordalDeviation(true, true, maxError, maxError);

					// Outline del profilo
					foreach (Curve3D curve in approx)
					{
						result.Outline.Add(curve.StartPoint);
						result.Outline.Add(curve.EndPoint);
					}

					#region SnapPoints
					result.SnapEndPoints = new List<Point3D>();
					result.SnapMiddlePoints = new List<Point3D>();
					Curve3D prevCurve = figure[figure.Count - 1];
					foreach (Curve3D curve in figure)
					{
						if (curve.StartPoint.IsEquals(prevCurve.EndPoint) == false)
							result.SnapEndPoints.Add(curve.StartPoint);

						result.SnapMiddlePoints.Add(curve.Evaluate(0.5));
						result.SnapEndPoints.Add(curve.EndPoint);
					}

					#endregion SnapPoints

					// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
					figureEntity3D.CloneTo(result);
				}
			}
			return result;
		}
	}
}
