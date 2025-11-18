using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Elements
{
	/// <summary>
	/// Lato di collegamento tra triangoli appartenenti ad una mesh. 
	/// </summary>
	public class Edge3D
	{
		/// <summary>
		/// Primo vertice dell'edge
		/// </summary>
		public Point3D Vertex1 { get; set; }

		/// <summary>
		/// Secondo vertice dell'edge
		/// </summary>
		public Point3D Vertex2 { get; set; }

		/// <summary>
		/// Lista di indici di triangoli (indici di una mesh) che indicano i triangoli che hanno 
		/// questo edge in comune. In una mesh chiusa e "corretta", ci saranno sempre 2 indici.
		/// </summary>
		public List<int> TriangleIndexes { get; set; }

		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Edge3D()
		{
			Vertex1 = Point3D.NullPoint;
			Vertex2 = Point3D.NullPoint;
			TriangleIndexes = [];
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="vertex1"></param>
		/// <param name="vertex2"></param>
		/// <param name="triangleIndexes"></param>
		public Edge3D(Point3D vertex1, Point3D vertex2, List<int> triangleIndexes)
		{
			Vertex1 = vertex1;
			Vertex2 = vertex2;
			TriangleIndexes = triangleIndexes;
		}

		/// <summary>
		/// Clona l'Edge3D
		/// </summary>
		/// <returns></returns>
		public Edge3D Clone()=> new Edge3D(Vertex1, Vertex2, new List<int>(TriangleIndexes));

		/// <summary>
		/// Spesso l'edge è inserito in un dictionary, e qui viene proposta una chiave univoca, che non 
		/// tiene conto dell'orientamento dell'edge, considerando il punto medio. 
		/// Il parametro precision indica quante cifre dopo la virgola devono essere considerate nella 
		/// generazione della chiave.
		/// </summary>
		/// <param name="precision"></param>
		/// <returns></returns>
		public static string GetEdgeKey(Point3D vertex1, Point3D vertex2, short precision)
		{
			Point3D midPoint = vertex1 + 0.5 * (vertex2 - vertex1);
			double prec = Math.Pow(10, precision);
			string result = "X" + Math.Round(midPoint.X * prec, 0).ToString() +
							"Y" + Math.Round(midPoint.Y * prec, 0).ToString() +
							"Z" + Math.Round(midPoint.Z * prec, 0).ToString();

			return result;
		}

	}
}
