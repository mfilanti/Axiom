using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axiom.GeoShape.Delegates;
using static Axiom.GeoShape.Entities.Mesh3D;

namespace Axiom.GeoShape.Entities
{
	/// <summary>
	/// Estensioni per la classe Mesh3D.
	/// </summary>
	public static class Mesh3DExtensions
	{
		/// <summary>
		/// Indica se creare l'outline verticale nelle mesh riconducibili alla rivoluzione 
		/// (Revolution3D, Torus3D, Cylinder3D, Sphere3D)
		/// </summary>
		public static bool RevolutionVerticalOutline = true;

		/// <summary>
		/// Crea un file STL a partire da una Mesh3D. 
		/// Specifiche prese da questo link: 
		/// http://www.ennex.com/~fabbers/StL.asp
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="fileName"></param>
		public static void ToSTLFile(this Mesh3D mesh, string fileName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("solid bar");
			foreach (Triangle3D tri in mesh.Triangles)
			{
				stringBuilder.AppendLine($" facet normal {tri.Normal.X.RoundToString(3)} {tri.Normal.Y.RoundToString(3)} {tri.Normal.Z.RoundToString(3)}");
				stringBuilder.AppendLine("  outer loop");

				stringBuilder.AppendLine($"   vertex {tri.P1.X.RoundToString(3)} {tri.P1.Y.RoundToString(3)} {tri.P1.Z.RoundToString(3)}");
				stringBuilder.AppendLine($"   vertex {tri.P2.X.RoundToString(3)} {tri.P2.Y.RoundToString(3)} {tri.P2.Z.RoundToString(3)}");
				stringBuilder.AppendLine($"   vertex {tri.P3.X.RoundToString(3)} {tri.P3.Y.RoundToString(3)} {tri.P3.Z.RoundToString(3)}");
				stringBuilder.AppendLine("  endloop");
				stringBuilder.AppendLine(" endfacet");
			}
			stringBuilder.AppendLine("endsolid bar");
			File.WriteAllText(fileName, stringBuilder.ToString());
		}

		/// <summary>
		/// Carica un file STL e crea una Mesh3D. 
		/// Se errorMessage == "" allora tutto ok. 
		/// Specifiche prese da questo link: 
		/// http://www.ennex.com/~fabbers/StL.asp
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static Mesh3D FromSTLFile(string fileName)
		{
			// Le normali nel file STL le ignoro: è un dato ridondante come indicato anche nelle specifiche
			Mesh3D result = new Mesh3D();
			// Prima controllo se si tratta di un STL di testo
			bool binary = true;
			#region CHECK TYPE
			try
			{
				// Controllo se entro le prime 10 righe esiste il testo "facet"
				using (StreamReader sr = new StreamReader(fileName))
				{
					string line;
					int cnt = 0;
					while ((line = sr.ReadLine()) != null || cnt < 10)
					{
						if (line.Contains("facet"))
						{
							binary = false;
							break;
						}
						cnt++;
					}
				}
			}
			catch  { }
			#endregion CHECK TYPE

			if (!binary)
			{
				#region TEXT
				using (StreamReader sr = new StreamReader(fileName))
				{
					List<Point3D> points = new List<Point3D>(3);
					string line;
					string[] separator = new string[] { " " };
					while ((line = sr.ReadLine()) != null)
					{
						if (line.Contains("vertex"))
						{
							string[] arr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
							Point3D vertex = new Point3D();
							vertex.X = ToDouble(arr[1]);
							vertex.Y = ToDouble(arr[2]);
							vertex.Z = ToDouble(arr[3]);
							points.Add(vertex);
							if (points.Count == 3)
							{
								result.Triangles.Add(new Triangle3D(points[0], points[1], points[2]));
								points.Clear();
							}
						}
					}
				}
				#endregion TEXT
			}
			else
			{
				#region BINARY
				FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				BinaryReader r = new BinaryReader(fs);
				// Header di 80 bytes ignorato
				r.ReadBytes(80);
				// Numero di triangoli
				uint nFacets = r.ReadUInt32();

				for (int i = 0; i < nFacets; i++)
				{
					// Ignoriamo i 12 bytes della normale
					r.ReadBytes(12);
					Point3D vertex1 = new Point3D();
					vertex1.X = (double)r.ReadSingle();
					vertex1.Y = r.ReadSingle();
					vertex1.Z = r.ReadSingle();
					Point3D vertex2 = new Point3D();
					vertex2.X = r.ReadSingle();
					vertex2.Y = r.ReadSingle();
					vertex2.Z = r.ReadSingle();
					Point3D vertex3 = new Point3D();
					vertex3.X = r.ReadSingle();
					vertex3.Y = r.ReadSingle();
					vertex3.Z = r.ReadSingle();
					r.ReadBytes(2);

					result.Triangles.Add(new Triangle3D(vertex1, vertex2, vertex3));
				}
				r.Close();
				fs.Close();
				#endregion BINARY
			}

			return result;
		}

		private static double ToDouble(string value)
		{
			double result;
			string valueDot = value.Replace(",", ".");
			if (double.TryParse(valueDot, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out result) == false)
				result = 0;
			return result;
		}

		/// <summary>
		/// Permette di ottenere una mesh dalla rivoluzione dei punti passati applicando le normali indicate. 
		/// slices: indica quante sezioni devono essere effettuate(è meglio che sia divisibile per 4, 
		/// così l'Outline corrisponderà agli angoli 0, 90, 180 e 270)
		/// points: punti 2D intesi nel piano YZ dai quali partire con la rivoluzione
		/// normals: vettori 2D che indicano la normale da applicare a ciascun punto
		/// outlinePoints: punti dell'outline
		/// snapEndPoints: punti snap agli estremi
		/// snapMiddlePoints: punti snap medi
		/// </summary>
		public static Mesh3D FromRevolution(int slices, List<Point3D> points, List<Vector3D> normals,
							  List<Point3D> outlinePoints, List<Point3D> snapEndPoints, List<Point3D> snapMiddlePoints)
		{
			Mesh3D result = new Mesh3D();
			result.VertexNormals = new List<TriangleNormals>();
			result.Outline = new List<Point3D>();
			result.SnapEndPoints = new List<Point3D>();
			result.SnapMiddlePoints = new List<Point3D>();
			double offsetRadAngle = 2 * Math.PI / slices;
			Point3D[,] points3 = new Point3D[points.Count, slices];
			Vector3D[,] normals3 = new Vector3D[normals.Count, slices];
			for (int i = 0; i < points.Count; i++)
			{
				Point3D point3 = new Point3D(0, points[i].X, points[i].Y);
				Vector3D normal3 = new Vector3D(0, normals[i].X, normals[i].Y);

				for (int j = 0; j < slices; j++)
				{
					RTMatrix rotMatrix = RTMatrix.FromEulerAnglesXYZ(0, 0, j * offsetRadAngle);
					points3[i, j] = rotMatrix.Multiply(point3);
					normals3[i, j] = rotMatrix.Multiply(normal3);
				}
			}
			for (int i = 0; i < points.Count - 1; i += 2)
			{
				int nexti = i + 1;
				for (int j = 0; j < slices; j++)
				{
					int nextj = (j + 1) % slices;
					Point3D p1 = points3[i, j];
					Point3D p2 = points3[nexti, j];
					Point3D p3 = points3[nexti, nextj];
					Point3D p4 = points3[i, nextj];
					Vector3D n1 = normals3[i, j];
					Vector3D n2 = normals3[nexti, j];
					Vector3D n3 = normals3[nexti, nextj];
					Vector3D n4 = normals3[i, nextj];
					if (p1.IsEquals(p4))
					{
						result.Triangles.Add(new Triangle3D(p1, p2, p3));
						result.VertexNormals.Add(new TriangleNormals(n1, n2, n3));
					}
					else if (p2.IsEquals(p3))
					{
						result.Triangles.Add(new Triangle3D(p1, p2, p4));
						result.VertexNormals.Add(new TriangleNormals(n1, n2, n4));
					}
					else
					{
						result.Triangles.Add(new Triangle3D(p1, p2, p3));
						result.Triangles.Add(new Triangle3D(p4, p1, p3));
						result.VertexNormals.Add(new TriangleNormals(n1, n2, n3));
						result.VertexNormals.Add(new TriangleNormals(n4, n1, n3));
					}
				}
			}

			// Outline orizzontale
			foreach (Point3D point in outlinePoints)
			{
				Point3D point3 = new Point3D(0, point.X, point.Y);
				result.Outline.Add(point3);
				for (int j = 1; j < slices; j++)
				{
					RTMatrix rotMatrix = RTMatrix.FromEulerAnglesXYZ(0, 0, j * offsetRadAngle);
					result.Outline.Add(rotMatrix * point3);
					result.Outline.Add(rotMatrix * point3);
				}
				result.Outline.Add(point3);
			}

			// Outline verticale + snap points
			if (RevolutionVerticalOutline == true)
			{
				int[] vecticalOutLine = new int[] { 0, slices / 4, slices / 2, slices * 3 / 4 };
				for (int i = 0; i <= slices; i++)
				{
					if (i == vecticalOutLine[0] || i == vecticalOutLine[1] || i == vecticalOutLine[2] || i == vecticalOutLine[3])
					{
						for (int j = 0; j < points.Count - 1; j++)
						{
							result.Outline.Add(points3[j, i]);
							result.Outline.Add(points3[j + 1, i]);
						}
					}
				}

				int[] snapsMiddle = new int[] { slices / 8, slices * 3 / 8, slices * 5 / 8, slices * 7 / 8 };
				for (int i = 0; i <= slices; i++)
				{
					RTMatrix rotMatrix = RTMatrix.FromEulerAnglesXYZ(0, 0, i * offsetRadAngle);
					if (i == vecticalOutLine[0] || i == vecticalOutLine[1] || i == vecticalOutLine[2] || i == vecticalOutLine[3])
					{
						foreach (Point3D point in snapEndPoints)
						{
							Point3D p3 = new Point3D(0, point.X, point.Y);
							p3 = rotMatrix.Multiply(p3);
							result.SnapEndPoints.Add(p3);
						}
						foreach (Point3D point in snapMiddlePoints)
						{
							Point3D p3 = new Point3D(0, point.X, point.Y);
							p3 = rotMatrix.Multiply(p3);
							result.SnapMiddlePoints.Add(p3);
						}
					}
					else if (i == snapsMiddle[0] || i == snapsMiddle[1] || i == snapsMiddle[2] || i == snapsMiddle[3])
					{
						foreach (Point3D point in snapEndPoints)
						{
							Point3D p3 = new Point3D(0, point.X, point.Y);
							p3 = rotMatrix.Multiply(p3);
							result.SnapMiddlePoints.Add(p3);
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Mesh3D da Revolution3D. 
		/// Il parametro maxError indica come approssimare la Figure2D della sezione. 
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

			return FromRevolution3D(revolution, maxError, slices);
		}

		/// <summary>
		/// Mesh3D da Revolution3D. 
		/// Il parametro maxError indica come approssimare la Figure2D della sezione. 
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
						if (start.X.IsEquals( 0) == false)
							outlinePoints.Add(start);
					}
					Point3D end = figure.EndPoint;
					if (end.X.IsEquals( 0) == false)
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

					result = FromRevolution(slices, points, normals, outlinePoints, snapEndPoints, snapMiddlePoints);

					// Porto tutte le proprietà della Entity3D (Id, Path, RTMatrix, Color, ecc...) su Mesh3D 
					revolution.CloneTo(result);
				}
			}
			return result;
		}

		/// <summary>
		/// Calcola la Figura2D ottenuta dall'outline delle meshes visibile da una certa posizione della telecamera.
		/// Nasconde le linee non in vista.
		/// </summary>
		/// <param name="meshes">Meshes dalle quali estrarre l'outlines</param>
		/// <param name="cameraDirection">Vettore direzione della telecamera</param>
		/// <param name="cameraUpVector">Vettore verticale della telecamera</param>
		/// <param name="collider">Delegate facoltativo (se != null) per calcolare l'intersezione ray-mesh</param>
		/// <returns>Figure2D calcolata</returns>
		public static Figure3D GetVisibleOutline(List<Mesh3D> meshes, Vector3D cameraDirection, Vector3D cameraUpVector, RayMeshCollision collider)
		{
			Vector3D direction = cameraDirection.Negate();
			Vector3D xAxis = cameraUpVector.Cross(direction).Normalize();
			List<Mesh3D> clonedMeshes = new List<Mesh3D>(meshes.Count);

			// applica le matrici alle mesh
			foreach (Mesh3D mesh in meshes)
				clonedMeshes.Add(mesh.ApplyMatrixToGeometry());

			// piano di proiezione
			Plane3D plane = new Plane3D(direction, xAxis, Point3D.Zero);

			// crea la figura 2d vista dalla telecamera,
			// e collega le linee originali con quelle proiettate mediante il dizionario relationedLines
			Figure3D figure = new Figure3D();
			Dictionary<Line3D, Line3D> relationedLines = new Dictionary<Line3D, Line3D>();
			foreach (Mesh3D mesh in clonedMeshes)
			{
				for (int i = 0; i < mesh.Outline.Count; i = i + 2)
				{
					Point3D pStart = mesh.Outline[i];
					Point3D pEnd = mesh.Outline[i + 1];
					Line3D line = new Line3D(pStart, pEnd);
					Line3D lineProjection = new Line3D(plane.Project2D(pStart), plane.Project2D(pEnd));
					if (!lineProjection.Length.IsEquals(0, MathUtils.FineTolerance))
					{
						relationedLines.Add(lineProjection, line);
						figure.Add(lineProjection);
					}
				}
			}

			// suddivide tutte le linee estraendo gli offset di suddivisione
			Dictionary<Line3D, List<double>> offsets;
			figure.SubdivideCrossCurve(out offsets);

			// suddivide la figura 3d in base agli offset ed elimina i segmenti non visibili
			Figure3D figure3d = new Figure3D();
			Triangle3D lastTriangle = new Triangle3D();
			Figure3D trims = new Figure3D();
			foreach (KeyValuePair<Line3D, List<double>> pair in offsets)
			{
				Line3D line = pair.Key;
				Line3D line3d = relationedLines[line];
				trims.AddRange(GetTrims(line3d, pair.Value));
			}
			Ray3D ray = new Ray3D();
			ray.Direction = direction;

			foreach (Line3D trim in trims)
			{
				ray.Location = trim.MiddlePoint;
				ray.Location = ray.Location + (MathUtils.FineTolerance * ray.Direction);

				bool intersect = false;

				foreach (Mesh3D mesh in clonedMeshes)
				{
					if (collider != null)
					{
						intersect = collider(ray, mesh);
					}
					else
					{
						if (lastTriangle.Normal != Vector3D.Zero)
						{
							Point3D intersection;
							intersect = lastTriangle.IntersectRay(ray, true, out intersection);
							if (intersect)
								break;
							else
								lastTriangle = new Triangle3D();
						}
						foreach (Triangle3D triangle in mesh.Triangles)
						{
							Point3D intersection;
							intersect = triangle.IntersectRay(ray, true, out intersection);
							if (intersect)
							{
								lastTriangle = triangle;
								break;
							}
						}
					}
					if (intersect)
						break;
				}
				if (!intersect)
					figure3d.Add(trim);
			}

			Figure3D result = new Figure3D();
			// crea le linee 3d e la linnee 2d della figure, relazionando linee 3d e 2d
			foreach (Curve3D line in figure3d)
			{
				Line3D lineProjection = new Line3D(plane.Project2D(line.StartPoint), plane.Project2D(line.EndPoint));
				result.Add(lineProjection);
			}
			result.DeleteDuplicates();
			result.DeleteNulls(); // todo, non dovrebbe servire
			return result;
		}

		/// <summary>
		/// Esegue una più trim della stessa linea
		/// </summary>
		/// <param name="line">Linea su cui eseguire i trim</param>
		/// <param name="offsets">Lista di offset ORDINATI in maniera crescente, espressi in percentuale 0-1.
		/// Indicano i punti in cui tagliare la linea</param>
		/// <returns>Lista dei trim ottenuti dal taglio</returns>
		private static List<Curve3D> GetTrims(Line3D line, List<double> offsets)
		{
			List<Curve3D> result = new List<Curve3D>();
			double length = line.Length;
			double precOffset = 0;
			for (int i = 1; i < offsets.Count; i++)
			{
				double offset = offsets[i];
				Line3D trim = (Line3D)line.Trim(precOffset * length, offset * length);
				result.Add(trim);
				precOffset = offset;
			}
			return result;
		}
	}
}
