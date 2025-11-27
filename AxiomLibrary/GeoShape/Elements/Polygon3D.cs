using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Elements
{
	/// <summary>
	/// Tipo di vertice
	/// </summary>
	[DataContract]
	public enum VertexType
	{
		Convex,
		Concave
	}

	/// <summary>
	/// Classe poligono 2D (CHIUSO).
	/// Mantiene un insieme di punti che rappresentano i vertici del poligono. 
	/// Almeno 3 vertici. 
	/// N.B. L'ultimo punto non va inserito uguale al primo. O almeno non è necessario. 
	/// </summary>
	[DataContract]
	public class Polygon3D : ICloneable
	{
		#region Properties
		/// <summary>
		/// Lista contenente i vertici del poligono
		/// </summary>
		[DataMember]
		public List<Point3D> Vertices { get; set; }
		#endregion 

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Polygon3D()
		{
			Vertices = new List<Point3D>();
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="points"></param>
		public Polygon3D(IEnumerable<Point3D> points)
		{
			Vertices = [.. points];
		}
		#endregion

		#region Methods
		/// <summary>
		/// Clona in Polygon3D
		/// </summary>
		/// <returns></returns>
		public Polygon3D Clone() => new Polygon3D(Vertices);

		/// <summary>
		/// Dato un punto, restituisce il suo indice. 
		/// Restituisce -1 se il punto non è un vertice del poligono.
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		public int VertexIndex(Point3D vertex)
		{
			int result = -1;

			for (int i = 0; i < Vertices.Count; i++)
			{
				if (Vertices[i] == vertex)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Calcola l'area del poligono. 
		/// Area maggiore di 0: poligono antiorario
		/// Area minore di 0: poligono orario
		/// Restrizione: il poligono non deve essere auto-intersecante. 
		/// www.swin.edu.au/astronomy/pbourke/geometry/polyarea/
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public double Area()
		{
			double area = 0;

			int j;
			for (int i = 0; i < Vertices.Count; i++)
			{
				j = (i + 1) % Vertices.Count;
				area += Vertices[i].X * Vertices[j].Y;
				area -= Vertices[i].Y * Vertices[j].X;
			}

			area = area / 2;
			return area;
		}

		/// <summary>
		/// Restituisce il punto precedente. 
		/// Nel caso di primo punto della lista, restituisce l'ultimo.
		/// </summary>
		/// <param name="vertexIndex"></param>
		/// <returns></returns>
		public Point3D PreviousPoint(int vertexIndex)
		{
			Point3D result;
			if (vertexIndex == 0)
				result = Vertices[Vertices.Count - 1];
			else
				result = Vertices[vertexIndex - 1];

			return result;
		}

		/// <summary>
		/// Restituisce il punto successivo. 
		/// Nel caso di ultimo punto della lista, restituisce il primo.
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		public Point3D NextPoint(int vertexIndex)
		{
			Point3D result;

			if (vertexIndex == Vertices.Count - 1)
				result = Vertices[0];
			else
				result = Vertices[vertexIndex + 1];

			return result;
		}

		/// <summary>
		/// Indica il tipo di vertice (concavo o convesso). 
		/// Si suppone che il poligono sia orario.
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		public VertexType PolygonVertexType(int indexVertex)
		{
			VertexType vertexType;
			Point3D vertex = Vertices[indexVertex];
			Point3D previous = PreviousPoint(indexVertex);
			Point3D next = NextPoint(indexVertex);

			double area = (new Polygon3D(new Point3D[] { previous, vertex, next })).Area();

			if (area <= 0)
				vertexType = VertexType.Convex;
			else //if (area > 0)
				vertexType = VertexType.Concave;

			return vertexType;
		}

		/// <summary>
		/// Indica se la linea formata dai due vertici è diagonale o meno. 
		/// Per essere diagonale la linea non deve intersecare le linee del poligono. 
		/// http://www.swin.edu.au/astronomy/pbourke/geometry/lineline2d
		/// </summary>
		/// <param name="vertex1"></param>
		/// <param name="vertex2"></param>
		/// <returns></returns>
		public bool Diagonal(Point3D vertex1, Point3D vertex2)
		{
			bool result = false;
			int nVertices = Vertices.Count;
			// Per le intersezioni non uso il metodo di Line2D ma
			// riscrivo il codice qui per ottimizzare
			int j = 0;
			for (int i = 0; i < nVertices; i++)
			{
				result = true;
				j = (i + 1) % nVertices;

				// Linea da testare
				double x1 = vertex1.X;
				double y1 = vertex1.Y;
				double x2 = vertex1.X;
				double y2 = vertex1.Y;

				// Linea del poligono
				double x3 = Vertices[i].X;
				double y3 = Vertices[i].Y;
				double x4 = Vertices[j].X;
				double y4 = Vertices[j].Y;

				double den = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
				double uB = -1;

				if (den.IsEquals( 0) == false)
					uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / den;

				if ((uB > 0) && (uB < 1))
					result = false;
			}
			return result;
		}

		/// <summary>
		/// Indica se il vertice è o meno un vertice principale: 
		/// un vertice pi del poligono P è un vertice principale se la 
		/// diagonale (pi-1, pi+1) interseca il contorno di P solo in pi-1 e pi+1. 
		/// http://www-cgrl.cs.mcgill.ca/~godfried/teaching/cg-projects/97/Ian/glossay.html
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		public bool PrincipalVertex(int vertexIndex)
		{
			bool result = false;
			Point3D previous = PreviousPoint(vertexIndex);
			Point3D next = NextPoint(vertexIndex);

			if (Diagonal(previous, next))
				result = true;

			return result;
		}

		/// <summary>
		/// Inverte i punti del poligono. 
		/// N.B. Cambia il verso (orario/antiorario)
		/// </summary>
		public void Inverse() => Vertices.Reverse();

		/// <summary>
		/// Indica se il poligono è orario o antiorario
		/// </summary>
		/// <returns></returns>
		public bool IsCounterClockWise()
		{
			return (Area() > 0);
		}

		/// <summary>
		/// Restituisce la figura corrispondente
		/// </summary>
		/// <returns></returns>
		public Figure3D ToFigure()
		{
			List<Point3D> points = new List<Point3D>(Vertices);
			points.Add(Vertices[0]);
			return new Figure3D(points);
		}

		/// <summary>
		/// Restituisce il baricentro del poligono
		/// </summary>
		/// <returns></returns>
		public Point3D GetBarycenter()
		{
			Point3D result = new Point3D(0, 0);
			double signedArea = 0.0;
			double x0 = 0.0; // Current vertex X
			double y0 = 0.0; // Current vertex Y
			double x1 = 0.0; // Next vertex X
			double y1 = 0.0; // Next vertex Y
			double a = 0.0;  // Partial signed area

			// Per tutti i vertici tranne l'ultimo (ricordo che il poligono è sempre chiuso)
			for (int i = 0; i < Vertices.Count - 1; i++)
			{
				x0 = Vertices[i].X;
				y0 = Vertices[i].Y;
				x1 = Vertices[i + 1].X;
				y1 = Vertices[i + 1].Y;
				a = x0 * y1 - x1 * y0;
				signedArea += a;
				result.X += (x0 + x1) * a;
				result.Y += (y0 + y1) * a;
			}

			// Per l'ultimo vertice (assieme al primo)
			x0 = Vertices[Vertices.Count - 1].X;
			y0 = Vertices[Vertices.Count - 1].Y;
			x1 = Vertices[0].X;
			y1 = Vertices[0].Y;
			a = x0 * y1 - x1 * y0;
			signedArea += a;
			result.X += (x0 + x1) * a;
			result.Y += (y0 + y1) * a;

			signedArea *= 0.5;
			result.X /= (6 * signedArea);
			result.Y /= (6 * signedArea);

			return result;
		}
		#endregion 

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
