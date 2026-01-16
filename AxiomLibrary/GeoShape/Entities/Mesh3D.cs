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
	/// Mesh 3D.
	/// Rappresenta un'insieme di triangoli nello spazio.
	/// E' definito con:
	/// - Lista di triangoli
	/// - Lista di punti che rappresentano il contorno (può essere null)
	/// - Lista di normali dei vertici (può essere null)
	/// - una matrice che ne determina l'orientamento 
	///   e la posizione nello spazio (ereditata da Entity3D)
	/// </summary>
	public class Mesh3D : Entity3D
	{
		#region PUBLIC FIELDS
		/// <summary>
		/// Lista di triangoli che formano la mesh
		/// </summary>
		public List<Triangle3D> Triangles { get; set; }

		/// <summary>
		/// Lista di punti che formano il contorno della mesh. 
		/// Avrà un numero pari di punti (ogni 2 punti una linea). 
		/// Può essere null: niente contorno.
		/// </summary>
		public List<Point3D> Outline { get; set; }

		/// <summary>
		/// Lista di normali dei vertici. 
		/// Avrà Count = Triangles.Count. 
		/// Può essere anche null, in tal caso le normali saranno quelle dei triangoli.
		/// </summary>
		public List<TriangleNormals> VertexNormals { get; set; }

		/// <summary>
		/// Indica la lista di punti snap di inizio/fine. 
		/// Può essere anche null, nessun punto di snap.
		/// </summary>
		public List<Point3D> SnapEndPoints { get; set; }

		/// <summary>
		/// Indica la lista di punti snap medi. 
		/// Può essere anche null, nessun punto di snap.
		/// </summary>
		public List<Point3D> SnapMiddlePoints { get; set; }
		#endregion PUBLIC FIELDS

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Mesh3D()
		{
			Triangles = new List<Triangle3D>();
			Outline = null;
			VertexNormals = null;
			SnapEndPoints = null;
			SnapMiddlePoints = null;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		public Mesh3D(List<Triangle3D> triangles) : base()
		{
			Triangles = triangles;
			Outline = null;
			VertexNormals = null;
			SnapEndPoints = null;
			SnapMiddlePoints = null;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		public Mesh3D(List<Triangle3D> triangles, List<Point3D> outline, List<TriangleNormals> vertexNormals,
					  List<Point3D> snapEndPoints, List<Point3D> snapMiddlePoints)
		{
			Triangles = triangles;
			Outline = outline;
			VertexNormals = vertexNormals;
			SnapEndPoints = snapEndPoints;
			SnapMiddlePoints = snapMiddlePoints;
		}
		#endregion CONSTRUCTORS

		#region Methods
		/// <summary>
		/// Imposta le VertexNormals pari alla normale dei triangoli
		/// </summary>
		public void SetNormals()
		{
			VertexNormals = new List<TriangleNormals>();
			for (int i = 0; i < Triangles.Count; i++)
			{
				Triangle3D triangle = Triangles[i];
				Vector3D normal = triangle.Normal;
				VertexNormals.Add(new TriangleNormals(normal, normal, normal));
			}
		}

		/// <summary>
		/// Clona la mesh
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			Mesh3D result = new Mesh3D
			{
				Triangles = new List<Triangle3D>(Triangles),
				Outline = (Outline != null) ? new List<Point3D>(Outline) : new List<Point3D>(),
				VertexNormals = (VertexNormals is not null) ? new List<TriangleNormals>(VertexNormals) : new List<TriangleNormals>(),
				SnapEndPoints = (SnapEndPoints is not null) ? new List<Point3D>(SnapEndPoints) : new List<Point3D>(),
				SnapMiddlePoints = (SnapMiddlePoints is not null) ? new List<Point3D>(SnapMiddlePoints) : new List<Point3D>()
			};
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Clona la mesh impostando le normali di VertexNormals nel caso sia a null. 
		/// Le imposta pari alla normale del triangolo.
		/// </summary>
		/// <returns></returns>
		public Mesh3D CloneWithNormals()
		{
			Mesh3D result;
			if (VertexNormals != null)
			{
				result = (Mesh3D)Clone();
			}
			else
			{
				result = new Mesh3D();
				result.Triangles = new List<Triangle3D>();
				result.VertexNormals = new List<TriangleNormals>();
				for (int i = 0; i < Triangles.Count; i++)
				{
					Triangle3D triangle = Triangles[i];
					Vector3D normal = triangle.Normal;
					result.Triangles.Add(triangle);
					result.VertexNormals.Add(new TriangleNormals(normal, normal, normal));
				}

				if (Outline != null)
					result.Outline = new List<Point3D>(Outline);

				if (SnapEndPoints != null)
					result.SnapEndPoints = new List<Point3D>( SnapEndPoints);

				if (SnapMiddlePoints != null)
					result.SnapMiddlePoints = new List<Point3D>(SnapMiddlePoints);

				base.CloneTo(result);
			}
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			List<Point3D> points = new List<Point3D>();
			RTMatrix matrix = ParentRTMatrix.Multiply(RTMatrix);
			if (matrix != RTMatrix.Identity)
			{
				foreach (Triangle3D triangle in Triangles)
				{
					points.Add(matrix.Multiply(triangle.P1));
					points.Add(matrix.Multiply(triangle.P2));
					points.Add(matrix.Multiply(triangle.P3));
				}
			}
			else
			{
				foreach (Triangle3D triangle in Triangles)
				{
					points.Add(triangle.P1);
					points.Add(triangle.P2);
					points.Add(triangle.P3);
				}
			}
			return AABBox3D.FromPoints(points);
		}

		/// <summary>
		/// Restituisce un dictionary contenente tutti gli edge della mesh
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, Edge3D> GetEdges()
		{
			Dictionary<string, Edge3D> result = new Dictionary<string, Edge3D>();
			for (int i = 0; i < Triangles.Count; i++)
			{
				Triangle3D triangle = Triangles[i];
				AddUpdateEdge(triangle.P1, triangle.P2, i, result);
				AddUpdateEdge(triangle.P2, triangle.P3, i, result);
				AddUpdateEdge(triangle.P1, triangle.P3, i, result);
			}

			return result;
		}

		// Crea un edge e lo aggiunge nel dictionary. 
		// Se l'edge è già stato aggiunto, allora aggiunge solo l'indice del triangolo
		private void AddUpdateEdge(Point3D vertex1, Point3D vertex2, int index, Dictionary<string, Edge3D> edges)
		{
			string key = Edge3D.GetEdgeKey(vertex1, vertex2, 2);
			if (edges.ContainsKey(key) == false)
			{
				Edge3D edge = new Edge3D(vertex1, vertex2, new List<int>());
				edge.TriangleIndexes.Add(index);
				edges.Add(key, edge);
			}
			else
			{
				edges[key].TriangleIndexes.Add(index);
			}
		}

		/// <summary>
		/// Controlla se la mesh è chiusa e non ha buchi o giunzioni a T tra i triangoli. 
		/// Il parametro edges può essere null, in tal caso viene ricalcolato. 
		/// edges1: indica il numero di edge che hanno un solo triangolo (mesh aperta o giunzioni a T)
		/// edges2: indica il numero di edge che hanno 2 triangoli (mesh corretta) 
		/// edges3: indica il numero di edge che hanno più di 2 triangoli (mesh gravemente corrotta)
		/// </summary>
		/// <param name="edges"></param>
		/// <param name="edges1"></param>
		/// <param name="edges2"></param>
		/// <param name="edges3"></param>
		/// <returns></returns>
		public bool CheckCorrectness(Dictionary<string, Edge3D> edges, out int edges1, out int edges2, out int edges3)
		{
			bool result = false;
			edges1 = 0;
			edges2 = 0;
			edges3 = 0;
			if (edges == null)
				edges = GetEdges();

			foreach (Edge3D edge in edges.Values)
			{
				if (edge.TriangleIndexes.Count == 1)
					edges1++;
				else if (edge.TriangleIndexes.Count == 2)
					edges2++;
				else //if (edge.TriangleIndexes.Count > 2)
					edges3++;
			}
			result = (edges1 == 0) && (edges3 == 0);
			return result;
		}

		/// <summary>
		/// Cerca di semplificare la mesh unendo i triangoli adiacenti che hanno un lato colineare. 
		/// Il parametro edges può essere null, in tal caso viene ricalcolato. 
		/// N.B. Per ora è una routine molto stupida: può rendere una mesh non corretta
		/// </summary>
		public void Simplify(Dictionary<string, Edge3D> edges)
		{
			if (edges == null)
				edges = GetEdges();

			Dictionary<int, int> oldNewLink = new Dictionary<int, int>();
			List<Triangle3D> newTriangles = new List<Triangle3D>();
			List<TriangleNormals> newNormals = new List<TriangleNormals>();
			foreach (Edge3D edge in edges.Values)
			{
				if (edge.TriangleIndexes.Count == 2)
				{
					Triangle3D t1 = Triangles[edge.TriangleIndexes[0]];
					Triangle3D t2 = Triangles[edge.TriangleIndexes[1]];
					if (oldNewLink.ContainsKey(edge.TriangleIndexes[0]))
						t1 = newTriangles[oldNewLink[edge.TriangleIndexes[0]]];
					if (oldNewLink.ContainsKey(edge.TriangleIndexes[1]))
						t2 = newTriangles[oldNewLink[edge.TriangleIndexes[1]]];

					Point3D otherP1, otherP2;
					if (edge.Vertex1.IsEquals(t1.P1) && edge.Vertex2.IsEquals(t1.P2) ||
						edge.Vertex2.IsEquals(t1.P1) && edge.Vertex1.IsEquals(t1.P2))
						otherP1 = t1.P3;
					else if (edge.Vertex1.IsEquals(t1.P1) && edge.Vertex2.IsEquals(t1.P3) ||
							 edge.Vertex2.IsEquals(t1.P1) && edge.Vertex1.IsEquals(t1.P3))
						otherP1 = t1.P2;
					else
						otherP1 = t1.P1;

					if (edge.Vertex1.IsEquals(t2.P1) && edge.Vertex2.IsEquals(t2.P2) ||
						edge.Vertex2.IsEquals(t2.P1) && edge.Vertex1.IsEquals(t2.P2))
						otherP2 = t2.P3;
					else if (edge.Vertex1.IsEquals(t2.P1) && edge.Vertex2.IsEquals(t2.P3) ||
							 edge.Vertex2.IsEquals(t2.P1) && edge.Vertex1.IsEquals(t2.P3))
						otherP2 = t2.P2;
					else
						otherP2 = t2.P1;

					bool newTri = false;
					Point3D point3 = Point3D.NullPoint;
					if ((otherP1 - edge.Vertex1).IsParallel(otherP2 - edge.Vertex1, 0.0001))
					{
						newTri = true;
						point3 = edge.Vertex2;
					}
					else if ((otherP1 - edge.Vertex2).IsParallel(otherP2 - edge.Vertex2))
					{
						newTri = true;
						point3 = edge.Vertex1;
					}
					if (newTri)
					{
						Triangle3D newT = new Triangle3D(otherP2, otherP1, point3);
						Vector3D normal = newT.Normal;
						TriangleNormals newN = new TriangleNormals(normal, normal, normal);
						newTriangles.Add(newT);
						newNormals.Add(newN);
						if (oldNewLink.ContainsKey(edge.TriangleIndexes[0]) == false)
							oldNewLink.Add(edge.TriangleIndexes[0], newTriangles.Count - 1);
						else
						{
							int oldNew = oldNewLink[edge.TriangleIndexes[0]];
							List<int> keys = new List<int>(oldNewLink.Keys);
							foreach (int key in keys)
								if (oldNewLink[key] == oldNew)
									oldNewLink[key] = newTriangles.Count - 1;
						}
						if (oldNewLink.ContainsKey(edge.TriangleIndexes[1]) == false)
							oldNewLink.Add(edge.TriangleIndexes[1], newTriangles.Count - 1);
						else
						{
							int oldNew = oldNewLink[edge.TriangleIndexes[1]];
							List<int> keys = new List<int>(oldNewLink.Keys);
							foreach (int key in keys)
								if (oldNewLink[key] == oldNew)
									oldNewLink[key] = newTriangles.Count - 1;
						}
					}
				}
			}
			List<int> indexToRemove = new List<int>(oldNewLink.Keys);
			HashSet<int> indexToAdd = new HashSet<int>(oldNewLink.Values);
			indexToRemove.Sort();
			for (int i = indexToRemove.Count - 1; i >= 0; i--)
			{
				Triangles.RemoveAt(indexToRemove[i]);
				if (VertexNormals != null)
					VertexNormals.RemoveAt(indexToRemove[i]);
			}
			foreach (int index in indexToAdd)
			{
				Triangles.Add(newTriangles[index]);
				if (VertexNormals != null)
					VertexNormals.Add(newNormals[index]);
			}
		}

		/// <summary>
		/// Ricrea l'outline della mesh. 
		/// Determina se un edge fa parte dell'outline in base all'angolo tra i 2 triangoli. 
		/// Se supera il valore indicato l'edge viene inserito tra l'outline. 
		/// Può essere utile quando si carica un STL e quindi non si hanno informazioni sull'outline. 
		/// Da per scontato che la mesh sia regolare. 
		/// Il parametro edges può essere null, in tal caso viene ricalcolato.
		/// </summary>
		/// <param name="radAngleMin"></param>
		/// <param name="edges"></param>
		public void AutomaticSetOutline(double radAngleMin, Dictionary<string, Edge3D> edges)
		{
			Outline = new List<Point3D>();
			if (edges == null)
				edges = GetEdges();

			foreach (Edge3D edge in edges.Values)
			{
				if (edge.TriangleIndexes.Count == 2)
				{
					int index1 = edge.TriangleIndexes[0];
					int index2 = edge.TriangleIndexes[1];
					Vector3D normal1 = Triangles[index1].Normal;
					Vector3D normal2 = Triangles[index2].Normal;
					double angle = normal1.Angle(normal2);
					if (angle > radAngleMin)
					{
						Outline.Add(edge.Vertex1);
						Outline.Add(edge.Vertex2);
					}
				}
				else if (edge.TriangleIndexes.Count == 1)
				{
					Outline.Add(edge.Vertex1);
					Outline.Add(edge.Vertex2);
				}
			}
		}

		/// <summary>
		/// Effettua la differenza booleana tra this e il parametro mesh
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public Mesh3D CsgDifference(Mesh3D mesh)
		{
			Mesh3D result = null;
			Mesh3D meshA = ApplyMatrixToGeometry();
			Mesh3D meshB = mesh.ApplyMatrixToGeometry();

			AABBox3D boxA = meshA.GetAABBox();
			AABBox3D boxB = meshB.GetAABBox();
			AABBox3D boxInt;
			if (boxA.Intersect(boxB, out boxInt))
			{
				Mesh3D cloneA = meshA.CloneWithNormals();
				Mesh3D cloneB = meshB.CloneWithNormals();

				// Lista di indici dei triangoli di A che intersecano il box intersezione
				List<int> indexInBoxA = new List<int>();
				for (int i = 0; i < meshA.Triangles.Count; i++)
				{
					AABBox3D boxTri = meshA.Triangles[i].GetAABBox();
					if (boxInt.Intersect(boxTri))
						indexInBoxA.Add(i);
				}
				// Lista di indici dei triangoli di B che intersecano il box intersezione
				List<int> indexInBoxB = new List<int>();
				for (int i = 0; i < meshB.Triangles.Count; i++)
				{
					AABBox3D boxTri = meshB.Triangles[i].GetAABBox();
					if (boxInt.Intersect(boxTri))
						indexInBoxB.Add(i);
				}

				#region Suddivisione
				// Suddivido i triangoli di A con B
				Dictionary<int, List<Triangle3D>> subA = new Dictionary<int, List<Triangle3D>>();
				Dictionary<int, List<TriangleNormals>> outTriNA = new Dictionary<int, List<TriangleNormals>>();
				foreach (int indexA in indexInBoxA)
				{
					Triangle3D triA = cloneA.Triangles[indexA];
					TriangleNormals inTriNA = cloneA.VertexNormals[indexA];
					AABBox3D boxTriA = triA.GetAABBox();
					List<Triangle3D> trianglesB = new List<Triangle3D>();
					foreach (int indexB in indexInBoxB)
					{
						Triangle3D triB = cloneB.Triangles[indexB];
						AABBox3D boxTriB = triB.GetAABBox();
						if (boxTriA.Intersect(boxTriB))
						{
							Line3D line;
							if (triA.IntersectTriangle(triB, out line))
								trianglesB.Add(triB);
						}
					}
					if (trianglesB.Count > 0)
					{
						List<TriangleNormals> outTriN;
						List<Triangle3D> sub = triA.MultiSubdivide(trianglesB, inTriNA, out outTriN);
						if (sub.Count > 1)
						{
							subA.Add(indexA, sub);
							outTriNA.Add(indexA, outTriN);
						}
						else { }
					}
				}
				// Suddivido i triangoli di B con A
				Dictionary<int, List<Triangle3D>> subB = new Dictionary<int, List<Triangle3D>>();
				Dictionary<int, List<TriangleNormals>> outTriNB = new Dictionary<int, List<TriangleNormals>>();
				foreach (int indexB in indexInBoxB)
				{
					Triangle3D triB = cloneB.Triangles[indexB];
					TriangleNormals inTriNB = cloneB.VertexNormals[indexB];
					AABBox3D boxTriB = triB.GetAABBox();
					List<Triangle3D> trianglesA = new List<Triangle3D>();
					foreach (int indexA in indexInBoxA)
					{
						Triangle3D triA = cloneA.Triangles[indexA];
						AABBox3D boxTriA = triA.GetAABBox();
						if (boxTriB.Intersect(boxTriA))
						{
							Line3D line;
							if (triB.IntersectTriangle(triA, out line))
								trianglesA.Add(triA);
						}
					}
					if (trianglesA.Count > 0)
					{
						List<TriangleNormals> outTriN;
						List<Triangle3D> sub = triB.MultiSubdivide(trianglesA, inTriNB, out outTriN);
						if (sub.Count > 1)
						{
							subB.Add(indexB, sub);
							outTriNB.Add(indexB, outTriN);
						}
						else { }
					}
				}

				List<int> indexToRemoveA = new List<int>(subA.Keys);
				indexToRemoveA.Sort();
				for (int i = indexToRemoveA.Count - 1; i >= 0; i--)
				{
					int index = indexToRemoveA[i];
					cloneA.Triangles.RemoveAt(index);
					cloneA.VertexNormals.RemoveAt(index);
				}
				foreach (KeyValuePair<int, List<Triangle3D>> pair in subA)
				{
					cloneA.Triangles.AddRange(subA[pair.Key]);
					cloneA.VertexNormals.AddRange(outTriNA[pair.Key]);
				}

				List<int> indexToRemoveB = new List<int>(subB.Keys);
				indexToRemoveB.Sort();
				for (int i = indexToRemoveB.Count - 1; i >= 0; i--)
				{
					int index = indexToRemoveB[i];
					cloneB.Triangles.RemoveAt(index);
					cloneB.VertexNormals.RemoveAt(index);
				}
				foreach (KeyValuePair<int, List<Triangle3D>> pair in subB)
				{
					cloneB.Triangles.AddRange(subB[pair.Key]);
					cloneB.VertexNormals.AddRange(outTriNB[pair.Key]);
				}
				#endregion Suddivisione

				// Ora faccio il merge di B su A eliminando opportunamente i triangoli
				for (int i = cloneA.Triangles.Count - 1; i >= 0; i--)
				{
					bool contained = false;
					Triangle3D triangle = cloneA.Triangles[i];
					AABBox3D boxTri = triangle.GetAABBox();
					if (boxB.Intersect(boxTri))
					{
						if (meshB.Contains(triangle.Center))
							contained = true;
					}
					if (contained)
					{
						cloneA.Triangles.RemoveAt(i);
						cloneA.VertexNormals.RemoveAt(i);
					}
				}
				for (int i = cloneB.Triangles.Count - 1; i >= 0; i--)
				{
					bool contained = false;
					Triangle3D triangle = cloneB.Triangles[i];
					AABBox3D boxTri = triangle.GetAABBox();
					if (boxA.Intersect(boxTri))
					{
						if (meshA.Contains(triangle.Center))
							contained = true;
					}
					if (!contained)
					{
						cloneB.Triangles.RemoveAt(i);
						cloneB.VertexNormals.RemoveAt(i);
					}
				}
				cloneA.Triangles.AddRange(cloneB.Triangles);
				cloneA.VertexNormals.AddRange(cloneB.VertexNormals);

				//cloneA.Simplify(null);
				result = cloneA;
			}
			else
			{
				result = (Mesh3D)Clone();
			}

			return result;
		}

		/// <summary>
		/// Effettua la differenza booleana tra this e il parametro mesh
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public Mesh3D CsgDifference2(Mesh3D mesh)
		{
			Mesh3D result = null;
			Mesh3D meshA = ApplyMatrixToGeometry();
			Mesh3D meshB = mesh.ApplyMatrixToGeometry();

			AABBox3D boxA = meshA.GetAABBox();
			AABBox3D boxB = meshB.GetAABBox();
			AABBox3D boxInt;
			if (boxA.Intersect(boxB, out boxInt))
			{
				Mesh3D cloneA = meshA.CloneWithNormals();
				Mesh3D cloneB = meshB.CloneWithNormals();

				// Lista di indici dei triangoli di A che intersecano il box intersezione
				List<int> indexInBoxA = new List<int>();
				for (int i = 0; i < cloneA.Triangles.Count; i++)
				{
					AABBox3D boxTri = cloneA.Triangles[i].GetAABBox();
					if (boxInt.Intersect(boxTri))
						indexInBoxA.Add(i);
				}
				// Lista di indici dei triangoli di B che intersecano il box intersezione
				List<int> indexInBoxB = new List<int>();
				for (int i = 0; i < cloneB.Triangles.Count; i++)
				{
					AABBox3D boxTri = cloneB.Triangles[i].GetAABBox();
					if (boxInt.Intersect(boxTri))
						indexInBoxB.Add(i);
				}

				#region Suddivisione
				// Suddivido i triangoli di A con B e viceversa
				List<int> indexAToRemove = new List<int>();
				List<int> indexBToRemove = new List<int>();
				for (int i = 0; i < indexInBoxA.Count; i++)
				{
					int indexA = indexInBoxA[i];
					Triangle3D triA = cloneA.Triangles[indexA];
					TriangleNormals triNA = cloneA.VertexNormals[indexA];
					AABBox3D boxTriA = triA.GetAABBox();
					for (int j = 0; j < indexInBoxB.Count; j++)
					{
						int indexB = indexInBoxB[j];
						Triangle3D triB = cloneB.Triangles[indexB];
						TriangleNormals triNB = cloneB.VertexNormals[indexB];
						AABBox3D boxTriB = triB.GetAABBox();
						if (boxTriA.Intersect(boxTriB))
						{
							Line3D line;
							if (triA.IntersectTriangle(triB, out line))
							{
								List<TriangleNormals> outTriNA, outTriNB;
								List<Triangle3D> subA = triA.Subdivide(line, triNA, out outTriNA);
								List<Triangle3D> subB = triB.Subdivide(line, triNB, out outTriNB);
								if (subA.Count > 1 || subB.Count > 1)
								{
									indexAToRemove.Add(indexA);
									int countA = cloneA.Triangles.Count;
									cloneA.Triangles.AddRange(subA);
									cloneA.VertexNormals.AddRange(outTriNA);

									indexInBoxA.RemoveAt(i);
									for (int k = 0; k < subA.Count; k++)
										indexInBoxA.Add(countA + k);

									indexBToRemove.Add(indexB);
									int countB = cloneB.Triangles.Count;
									cloneB.Triangles.AddRange(subB);
									cloneB.VertexNormals.AddRange(outTriNB);

									indexInBoxB.RemoveAt(j);
									for (int k = 0; k < subB.Count; k++)
										indexInBoxB.Add(countB + k);

									i--;
									break;
								}
							}
						}
					}
				}
				// Rimuovo i triangoli che sono stati suddivisi
				indexAToRemove.Sort();
				for (int i = indexAToRemove.Count - 1; i >= 0; i--)
				{
					cloneA.Triangles.RemoveAt(indexAToRemove[i]);
					cloneA.VertexNormals.RemoveAt(indexAToRemove[i]);
				}

				indexBToRemove.Sort();
				for (int i = indexBToRemove.Count - 1; i >= 0; i--)
				{
					cloneB.Triangles.RemoveAt(indexBToRemove[i]);
					cloneB.VertexNormals.RemoveAt(indexBToRemove[i]);
				}
				#endregion Suddivisione

				// Ora faccio il merge di B su A eliminando opportunamente i triangoli
				for (int i = cloneA.Triangles.Count - 1; i >= 0; i--)
				{
					bool contained = false;
					Triangle3D triangle = cloneA.Triangles[i];
					AABBox3D boxTri = triangle.GetAABBox();
					if (boxB.Intersect(boxTri))
					{
						if (meshB.Contains(triangle.Center))
							contained = true;
					}
					if (contained)
					{
						cloneA.Triangles.RemoveAt(i);
						cloneA.VertexNormals.RemoveAt(i);
					}
				}
				for (int i = cloneB.Triangles.Count - 1; i >= 0; i--)
				{
					bool contained = false;
					Triangle3D triangle = cloneB.Triangles[i];
					AABBox3D boxTri = triangle.GetAABBox();
					if (boxA.Intersect(boxTri))
					{
						if (meshA.Contains(triangle.Center))
							contained = true;
					}
					if (!contained)
					{
						cloneB.Triangles.RemoveAt(i);
						cloneB.VertexNormals.RemoveAt(i);
					}
				}
				cloneA.Triangles.AddRange(cloneB.Triangles);
				cloneA.VertexNormals.AddRange(cloneB.VertexNormals);

				result = cloneA;
			}
			else
			{
				result = (Mesh3D)Clone();
			}

			return result;
		}

		/// <summary>
		/// Rende la RTMatrix della mesh identità applicandola prima a tutte le componenti. 
		/// - triangoli
		/// - normali
		/// - outline
		/// </summary>
		/// <returns></returns>
		public Mesh3D ApplyMatrixToGeometry()
		{
			Mesh3D result = new Mesh3D();
			// Clono tutti i dati di Entity3D, sotto vengono ricreati i triangoli, le normali e l'outline
			CloneTo(result as Entity3D);
			result.RTMatrix = RTMatrix.Identity;
			RTMatrix matrix = RTMatrix;
			if (matrix != RTMatrix.Identity)
			{
				foreach (Triangle3D triangle in Triangles)
				{
					Triangle3D clone = triangle;
					clone.ApplyRT(matrix);
					result.Triangles.Add(clone);
				}
				if (VertexNormals != null)
				{
					result.VertexNormals = new List<TriangleNormals>();
					foreach (TriangleNormals triNormal in VertexNormals)
					{
						TriangleNormals clone = triNormal;
						clone.ApplyRT(matrix);
						result.VertexNormals.Add(clone);
					}
				}
				if (Outline != null)
				{
					result.Outline = new List<Point3D>();
					foreach (Point3D point in Outline)
					{
						Point3D clone = point;
						result.Outline.Add(matrix.Multiply(point));
					}
				}
				if (SnapEndPoints != null)
				{
					result.SnapEndPoints = new List<Point3D>();
					foreach (Point3D point in SnapEndPoints)
					{
						Point3D clone = point;
						result.SnapEndPoints.Add(matrix.Multiply(point));
					}
				}
				if (SnapMiddlePoints != null)
				{
					result.SnapMiddlePoints = new List<Point3D>();
					foreach (Point3D point in SnapMiddlePoints)
					{
						Point3D clone = point;
						result.SnapMiddlePoints.Add(matrix.Multiply(point));
					}
				}
			}
			else
			{
				result = (Mesh3D)Clone();
			}

			return result;
		}

		/// <summary>
		/// Clona this su un altra mesh
		/// </summary>
		/// <param name="mesh"></param>
		public void CloneTo(Mesh3D mesh)
		{
			base.CloneTo(mesh);
			mesh.Triangles = Triangles == null ? null : new List<Triangle3D>(Triangles);
			mesh.Outline = Outline == null ? null : new List<Point3D>(Outline);
			mesh.VertexNormals = VertexNormals == null ? null : new List<TriangleNormals>(VertexNormals);
			mesh.SnapEndPoints = SnapEndPoints == null ? null : new List<Point3D>(SnapEndPoints);
			mesh.SnapMiddlePoints = SnapMiddlePoints == null ? null : new List<Point3D>(SnapMiddlePoints);
		}

		/// <summary>
		/// Indica se la mesh contiene il punto
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Contains(Point3D point)
		{
			// Test con UnitZ perturbato
			Vector3D testDirection = new Vector3D(0.001, 0.001, 0.99);
			testDirection.SetNormalize();
			return Contains(point, Vector3D.UnitZ);
		}

		/// <summary>
		/// Indica se la mesh contiene il punto
		/// </summary>
		/// <param name="point"></param>
		/// <param name="testDirection"></param>
		/// <returns></returns>
		public bool Contains(Point3D point, Vector3D testDirection)
		{
			bool result;
			Ray3D ray;
			RTMatrix identity = RTMatrix.Identity;
			if (RTMatrix == identity)
			{
				ray = new Ray3D(point, testDirection);
			}
			else
			{
				RTMatrix inverse = RTMatrix.Inverse();
				ray = new Ray3D(inverse * point, inverse * testDirection);
			}

			AABBox3D rayBox = AABBox3D.FromPoints(new List<Point3D>(new Point3D[] { ray.Location, ray.Location + ray.Direction * 1000000000 }));
			Point3D intPoint;
			List<Point3D> intersections = new List<Point3D>();
			foreach (Triangle3D triangle in Triangles)
			{
				AABBox3D triBox = triangle.GetAABBox();
				if (rayBox.Intersect(triBox))
					if (triangle.IntersectRay(ray, out intPoint))
						intersections.Add(intPoint);
			}
			int count = intersections.Count;
			// Controllo che non ci siano intersezioni doppie
			for (int i = 0; i < intersections.Count; i++)
			{
				for (int j = i + 1; j < intersections.Count; j++)
				{
					if (intersections[i].IsEquals(intersections[j]) == true)
						count--;
				}
			}

			result = (count % 2) != 0;
			return result;
		}

		/// <summary>
		/// Unisce la la mesh passata a this
		/// </summary>
		/// <param name="mesh"></param>
		public void Unite(Mesh3D mesh)
		{
			// Devo fare comunque un clone quindi ne approfitto e chiamo ApplyMatrixToGeometry che mi semplifica il codice
			Mesh3D meshClone = mesh.ApplyMatrixToGeometry();
			// Rendo il clone relativo alla WorldMatrix di this (impostando la RTMatrix e chiamando nuovamente ApplyMatrixToGeometry)
			meshClone.RTMatrix = WorldMatrix.Inverse();
			meshClone = meshClone.ApplyMatrixToGeometry();

			if (VertexNormals != null && VertexNormals.Count == Triangles.Count)
			{
				if (meshClone.VertexNormals != null && meshClone.VertexNormals.Count == meshClone.Triangles.Count)
				{
					VertexNormals.AddRange(meshClone.VertexNormals);
				}
				else
				{
					foreach (Triangle3D triangle in meshClone.Triangles)
					{
						Vector3D n = triangle.Normal;
						VertexNormals.Add(new TriangleNormals(n, n, n));
					}
				}
			}
			Triangles.AddRange(meshClone.Triangles);
			if (Outline != null)
			{
				if (meshClone.Outline != null)
					Outline.AddRange(meshClone.Outline);
			}
			if (meshClone.SnapEndPoints != null)
			{
				if (SnapEndPoints == null)
					SnapEndPoints = new List<Point3D>();

				SnapEndPoints.AddRange(meshClone.SnapEndPoints);
			}
			if (meshClone.SnapMiddlePoints != null)
			{
				if (SnapMiddlePoints == null)
					SnapMiddlePoints = new List<Point3D>();

				SnapMiddlePoints.AddRange(meshClone.SnapMiddlePoints);
			}
		}

		/// <summary>
		/// Interseca tutti i triangoli della mesh con il piano eliminando 
		/// quelli che stanno dalla parte della normale.
		/// </summary>
		/// <param name="cutPlane">Piano</param>
		/// <param name="addOutline">Aggiungi la linea di chiusura</param>
		public void CutByPlane(Plane3D cutPlane, bool addClosingFace)
		{
			Figure3D cutFigure = new Figure3D();
			Mesh3D meshDummy = new Mesh3D();
			if (VertexNormals != null)
				meshDummy.VertexNormals = new List<TriangleNormals>();

			for (int i = 0; i < Triangles.Count; i++)
			{
				Triangle3D triangle = Triangles[i];
				List<TriangleNormals> triNormalsList;
				Line3D line;
				List<Triangle3D> cutted = triangle.Subdivide(cutPlane, VertexNormals[i], out triNormalsList, out line);
				meshDummy.Triangles.AddRange(cutted);
				meshDummy.VertexNormals.AddRange(triNormalsList);
				if (cutted.Count > 1 && line != null)
					cutFigure.Add(line);
			}
			Triangles.Clear();
			if (VertexNormals != null)
				VertexNormals.Clear();

			// Ora filtro i triangoli
			for (int i = 0; i < meshDummy.Triangles.Count; i++)
			{
				Triangle3D triangle = meshDummy.Triangles[i];
				double signedDistance = cutPlane.SignedDistance(triangle.Center);
				if (signedDistance < 0)
				{
					Triangles.Add(triangle);
					if (VertexNormals != null)
						VertexNormals.Add(meshDummy.VertexNormals[i]);
				}
			}

			if (addClosingFace && Delegates.ComputeTriangulation != null)
			{
				Figure3D figureClone = cutFigure.Clone();
				figureClone.ApplyRT(cutPlane.GetRTMatrix().Inverse());
				figureClone.AutomaticSort();
				List<Triangle3D> closeTriList = Delegates.ComputeTriangulation(figureClone);
				foreach (Triangle3D tri in closeTriList)
				{
					Triangle3D triClone = tri;
					triClone.ApplyRT(cutPlane.GetRTMatrix());

					Triangles.Add(triClone);
					if (VertexNormals != null)
						VertexNormals.Add(new TriangleNormals(triClone.Normal, triClone.Normal, triClone.Normal));
				}
			}
			#region OUTLINE
			// Se c'è l'outline taglio anche quella
			if (Outline != null && Outline.Count > 0)
			{
				List<Point3D> dummyOutLine = new List<Point3D>();
				for (int i = 0; i < Outline.Count; i += 2)
				{
					Point3D p1 = Outline[i];
					Point3D p2 = Outline[i + 1];
					if (cutPlane.SignedDistance(p1) < 0 && cutPlane.SignedDistance(p2) < 0)
					{
						dummyOutLine.Add(p1);
						dummyOutLine.Add(p2);
					}
					else if (cutPlane.SignedDistance(p1) < 0 && cutPlane.SignedDistance(p2) > 0)
					{
						dummyOutLine.Add(p1);
						Point3D pInt;
						cutPlane.IntersectRay(new Ray3D(p1, p2 - p1), out pInt);
						dummyOutLine.Add(pInt);
					}
					else if (cutPlane.SignedDistance(p1) > 0 && cutPlane.SignedDistance(p2) < 0)
					{
						dummyOutLine.Add(p2);
						Point3D pInt;
						cutPlane.IntersectRay(new Ray3D(p1, p2 - p1), out pInt);
						dummyOutLine.Add(pInt);
					}
				}
				Outline.Clear();
				Outline.AddRange(dummyOutLine);
				// Aggiungo quella calcolata in fase di taglio
				foreach (Curve3D curve in cutFigure)
				{
					Outline.Add(curve.StartPoint);
					Outline.Add(curve.EndPoint);
				}
			}
			#endregion OUTLINE
		}

		/// <summary>
		/// Determina l'intersezione tra la mesh e un piano
		/// </summary>
		/// <param name="plane"></param>
		/// <returns></returns>
		public Figure3D Intersect(Plane3D plane)
		{
			Figure3D result = new Figure3D();
			for (int i = 0; i < Triangles.Count; i++)
			{
				Triangle3D triangle = Triangles[i];
				triangle.ApplyRT(WorldMatrix);
				TriangleNormals triN;
				if (VertexNormals != null)
				{
					triN = VertexNormals[i];
				}
				else
				{
					Vector3D n = triangle.Normal;
					triN = new TriangleNormals(n, n, n);
				}
				List<TriangleNormals> triNormalsList;
				Line3D line;
				List<Triangle3D> cutted = triangle.Subdivide(plane, triN, out triNormalsList, out line);
				if (cutted.Count > 1 && line != null)
					result.Add(line);
			}
			return result;
		}

		/// <summary>
		/// Indica se le due mesh si intersecano, esce alla prima intersezione
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public bool Intersect(Mesh3D mesh)
		{
			bool result = false;
			foreach (Triangle3D t1 in Triangles)
			{
				t1.ApplyRT(WorldMatrix);
				foreach (Triangle3D t2 in mesh.Triangles)
				{
					t2.ApplyRT(mesh.WorldMatrix);
					if (t1.IntersectTriangle(t2))
					{
						result = true;
						break;
					}
				}
				if (result)
					break;
			}
			return result;
		}

		/// <summary>
		/// Aggiunge una figura nell'Outline, approssimata con tolleranza indicata
		/// </summary>
		/// <param name="figure"></param>
		/// <param name="tolerance"></param>
		public void AddFigureToOutline(Figure3D figure, double tolerance)
		{
			if (Outline == null)
				Outline = new List<Point3D>();

			Figure3D approx = figure.ApproxFigureMaxChordalDeviation(true, true, tolerance, tolerance);
			foreach (Curve3D curve in approx)
			{
				Outline.Add(curve.StartPoint);
				Outline.Add(curve.EndPoint);
			}
		}

		#endregion PUBLIC METHODS
	}
}
