using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using System.Numerics;

namespace Axiom.GeoShape.Elements
{

	/// <summary>
	/// Struttura che rappresenta il piano in 3D. 
	/// Descritto con una normale una direzione X e un punto. 
	/// La direzione X può anche non essere indicata (ne viene presa una in maniera arbitraria).
	/// </summary>
	public class Plane3D
	{
		#region properties
		/// <summary>
		/// Normale al piano (direzione Z locale)
		/// </summary>
		public Vector3D Normal { get; set; }

		/// <summary>
		/// Direzione X locale
		/// </summary>
		public Vector3D XAxis { get; set; }

		/// <summary>
		/// Location del piano
		/// </summary>
		public Point3D Location { get; set; }

		/// <summary>
		/// Direzione Y locale (calcolata come Normal x XAxis)
		/// </summary>
		public Vector3D YAxis => Normal.Cross(XAxis);
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore. 
		/// La direzione X locale viene individuata in maniera arbitraria. 
		/// Regola: [(Normal x UnitX) x Normal] con eccezioni nel caso Normal sia UnitX o NegativeUnitX.
		/// </summary>
		/// <param name="normal"></param>
		/// <param name="location"></param>
		public Plane3D(Vector3D normal, Point3D location)
		{
			Normal = normal.Normalize();
			XAxis = Vector3D.Zero;
			Location = location;
			ResetXAxis();
		}

		/// <summary>
		/// Costruttore. 
		/// La normale e l'asse x vengono comunque normalizzati internamente.
		/// </summary>
		/// <param name="normal"></param>
		/// <param name="xAxis"></param>
		/// <param name="location"></param>
		public Plane3D(Vector3D normal, Vector3D xAxis, Point3D location)
		{
			Normal = normal.Normalize();
			XAxis = xAxis.Normalize();
			Location = location;
		}
		#endregion
		#region Overrides
		
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override bool Equals(object? obj)
		{
			if(obj is Plane3D plane)
			{

				return Normal == plane.Normal && XAxis == plane.XAxis && Location == plane.Location;
			}
			return false;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
		#region Methods
		/// <summary>
		/// Reimposta l'asse X in maniera arbitraria, mantenendo fissa la normale. 
		/// Regola: [(Normal x UnitX) x Normal] con eccezioni nel caso Normal sia UnitX o NegativeUnitX.
		/// </summary>
		/// <returns></returns>
		public void ResetXAxis()
		{
			if (Normal.IsEquals(Vector3D.NegativeUnitX))
				XAxis = Vector3D.NegativeUnitY;
			else if (Normal.IsEquals(Vector3D.UnitX))
				XAxis = Vector3D.UnitY;
			else
				XAxis = (Normal.Cross(Vector3D.UnitX)).Cross(Normal).Normalize();
		}

		/// <summary>
		/// Intersezione piano - linea. 
		/// Restituisce true o false. 
		/// Come parametri di uscita vengono restituiti anche l'eventuale punto di intersezione e 
		/// un booleano che indica se il punto appartiene alla linea di ingresso.
		/// </summary>
		/// <param name="line">Segmento con cui fare l'intersezione</param>
		/// <param name="insideLine">Indica se l'intersezione appartiene al segmento</param>
		/// <param name="intersection">Eventuale punto di intersezione</param>
		/// <returns>Indica se c'è intersezione (false solo se linea e piano sono paralleli)</returns>
		public bool IntersectLine(Line3D line, out bool insideLine, out Point3D intersection)
		{
			bool result = false;
			insideLine = false;
			intersection = new Point3D();

			Vector3D vLine = line.PEnd - line.PStart;
			double d = vLine.Dot(Normal);
			if (d.IsEquals(0) == false)
			{
				Vector3D vPlaneLine = Location - line.PStart;
				result = true;
				double n = vPlaneLine.Dot(Normal);
				double u = n / d;
				intersection = line.PStart + u * vLine;
				if ((u >= 0) && (u <= 1))
					insideLine = true;
			}

			return result;
		}

		/// <summary>
		/// Intersezione piano - raggio. 
		/// Restituisce true o false. 
		/// Come parametro di uscita viene restituito anche l'eventuale punto di intersezione.
		/// </summary>
		/// <param name="ray">Raggio con cui fare l'intersezione</param>
		/// <param name="intersection">Eventuale punto di intersezione</param>
		/// <returns>Indica se c'è intersezione (false solo se raggio e piano sono paralleli)</returns>
		public bool IntersectRay(Ray3D ray, out Point3D intersection)
		{
			bool result = false;
			intersection = new Point3D();

			double d = ray.Direction.Dot(Normal);
			if (d.IsEquals(0) == false)
			{
				Vector3D vPlaneRay = Location - ray.Location;
				double n = vPlaneRay.Dot(Normal);
				double u = n / d;
				intersection = ray.Location + u * ray.Direction;
				if (u >= 0)
					result = true;
			}

			return result;
		}

		/// <summary>
		/// Intersezione piano - triangolo. 
		/// Restituisce true o false. 
		/// Come parametro di uscita viene restituita l'eventuale Line. 
		/// intL1: indica che il primo punto di intersezione appartiene a P1-P2 
		/// intL2: indica che il primo punto di intersezione appartiene a P1-P3 
		/// intL3: indica che il primo punto di intersezione appartiene a P2-P3 
		/// </summary>
		/// <param name="triangle"></param>
		/// <param name="intersection"></param>
		/// <returns></returns>
		public bool IntersectTriangle(Triangle3D triangle, out Line3D intersection)
		{
			bool result = false;
			intersection = null;
			Line3D line1 = new Line3D(triangle.P1, triangle.P2);
			Line3D line2 = new Line3D(triangle.P1, triangle.P3);
			Point3D point1, point2;
			bool insideLine1, insideLine2;
			bool inters1 = IntersectLine(line1, out insideLine1, out point1);
			bool inters2 = IntersectLine(line2, out insideLine2, out point2);

			// Controllo che point1 non sia uguale a p2
			if ((inters1 && insideLine1 && inters2 && insideLine2) && point1.IsEquals(point2))
				inters2 = false;

			if (inters1 && insideLine1 && inters2 && insideLine2)
			{
				result = true;
				intersection = new Line3D(point1, point2);
			}
			else
			{
				if ((inters1 && insideLine1) || (inters2 && insideLine2))
				{
					Line3D line3 = new Line3D(triangle.P2, triangle.P3);
					Point3D point3;
					bool insideLine3;
					bool inters3 = IntersectLine(line3, out insideLine3, out point3);
					// Se siamo qui, uno dei due ha intersecato e quindi questo intersecherà di sicuro 
					// cioè inters3 e insideLine3 saranno a true (non li controllo nemmeno)
					if (inters3 && insideLine3)
					{
						if (inters2 && insideLine2)
							point1 = point2;

						result = true;
						intersection = new Line3D(point1, point3);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Intersezione piano - piano. 
		/// Restituisce true o false. 
		/// Come parametri di uscita vengono restituiti il punto più vicino alle 2 location e la direzione 
		/// della retta di intersezione.
		/// </summary>
		/// <param name="line">Piano con cui fare l'intersezione</param>
		/// <param name="intersectionPoint">Punto di intersezione (il più vicino alle 2 location)</param>
		/// <param name="intersectionDirection">Direzione della linea di intersezione</param>
		/// <returns>Indica se c'è intersezione (false solo se i due piani sono paralleli)</returns>
		public bool IntersectPlane(Plane3D plane, out Point3D intersectionPoint, out Vector3D intersectionDirection)
		{
			bool result = false;
			intersectionPoint = Point3D.Zero;
			intersectionDirection = Normal.Cross(plane.Normal);
			// Controllo che non siano 2 piani paralleli
			if (intersectionDirection.LengthSquared > 0.001)
			{
				result = true;
				Vector3D lineToIntersection = Normal.Cross(intersectionDirection);
				plane.IntersectLine(new Line3D(Location, Location + 100 * lineToIntersection), out _, out intersectionPoint);
			}

			return result;
		}

		/// <summary>
		/// Distanza punto - piano
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public double Distance(Point3D point) => Math.Abs((point - Location).Dot(Normal));

		/// <summary>
		/// Distanza punto - piano, con segno (positiva sul lato della normale)
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public double SignedDistance(Point3D point) => (point - Location).Dot(Normal);

		/// <summary>
		/// Proiezione del punto sul piano
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Point3D Project(Point3D point)
		{
			Point3D result = point + Normal * (-(point - Location).Dot(Normal));
			return result;
		}

		/// <summary>
		/// Proiezione del punto sul piano, per ottenere un punto 2D in coordinate piano
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Point3D Project2D(Point3D point)
		{
			Point3D projection3 = Project(point);
			Point3D result = new Point3D();
			result.X = (projection3 - Location).Dot(XAxis);
			result.Y = (projection3 - Location).Dot(YAxis);
			return result;
		}

		/// <summary>
		/// Dato il punto in coordinate piano restituisce il punto 3D
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Point3D UnProject2D(Point3D point)
		{
			RTMatrix matrix = GetRTMatrix();
			Point3D result = matrix * new Point3D(point);
			return result;
		}

		/// <summary>
		/// Proiezione del vettore sul piano, per ottenere un vettore 2D in coordinate piano. 
		/// La lunghezza del vettore può cambiare.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vector3D Project2D(Vector3D vector)
		{
			Point3D point = Location + vector;
			Point3D projection3 = Project(point);
			Vector3D result = new Vector3D(
				(projection3 - Location).Dot(XAxis),
				(projection3 - Location).Dot(YAxis),
				0);
			return result;
		}

		/// <summary>
		/// Matrice che rappresenta il piano
		/// </summary>
		/// <returns></returns>
		public RTMatrix GetRTMatrix() => RTMatrix.FromVectors(XAxis, YAxis, Normal, (Vector3D)Location);

		/// <summary>
		/// Ruota e trasla il piano
		/// </summary>
		/// <param name="matrix"></param>
		public void ApplyRT(RTMatrix matrix)
		{
			Location = matrix * Location;
			Normal = matrix * Normal;
			XAxis = matrix * XAxis;
		}
		#endregion 

		#region OPERATORS
		/// <summary>
		/// Uguaglianza precisa
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(Plane3D left, Plane3D right)
		{
			return (left.Location == right.Location && left.Normal == right.Normal && left.XAxis == right.XAxis);
		}

		/// <summary>
		/// Disuguaglianza precisa
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(Plane3D left, Plane3D right)
		{
			return (left.Location != right.Location || left.Normal != right.Normal || left.XAxis != right.XAxis);
		}
		#endregion OPERATORS

		#region STATICS
		/// <summary>
		/// Restituisce un asse X arbitrario che sta sul piano individuato da normal. 
		/// Regola: [(Normal x UnitX) x Normal] con eccezioni nel caso Normal sia UnitX o NegativeUnitX.
		/// </summary>
		/// <param name="normal"></param>
		/// <returns></returns>
		public static Vector3D GetXAxis(Vector3D normal)
		{
			Plane3D plane = new(normal, Point3D.Zero);
			return plane.XAxis;
		}

		/// <summary>
		/// Restituisce un asse Y arbitrario che sta sul piano indivisuato da normal. 
		/// Regola: Normal x GetXAxis
		/// </summary>
		/// <param name="normal"></param>
		/// <returns></returns>
		public static Vector3D GetYAxis(Vector3D normal)
		{
			Plane3D plane = new (normal, Point3D.Zero);
			return plane.YAxis;
		}

		/// <summary>
		/// Piano standard XY, normale Z
		/// </summary>
		public static Plane3D XYPlane => new Plane3D(Vector3D.UnitZ, Vector3D.UnitX, Point3D.Zero);

		/// <summary>
		/// Piano standard YZ, normale -Z
		/// </summary>
		public static Plane3D YXPlane => new Plane3D(Vector3D.NegativeUnitZ, Vector3D.UnitY, Point3D.Zero);

		/// <summary>
		/// Piano standard YZ, normale X
		/// </summary>
		public static Plane3D YZPlane => new Plane3D(Vector3D.UnitX, Vector3D.UnitY, Point3D.Zero);

		/// <summary>
		/// Piano standard ZY, normale -X
		/// </summary>
		public static Plane3D ZYPlane => new Plane3D(Vector3D.NegativeUnitX, Vector3D.UnitZ, Point3D.Zero);

		/// <summary>
		/// Piano standard ZX, normale Y
		/// </summary>
		public static Plane3D ZXPlane => new Plane3D(Vector3D.UnitY, Vector3D.UnitZ, Point3D.Zero);

		/// <summary>
		/// Piano standard XZ, normale -Y
		/// </summary>
		public static Plane3D XZPlane => new Plane3D(Vector3D.NegativeUnitY, Vector3D.UnitX, Point3D.Zero);

		/// <summary>
		/// Piano nullo
		/// </summary>
		public static Plane3D ZeroPlane => new Plane3D(Vector3D.Zero, Vector3D.Zero, Point3D.Zero);

		/// <summary>
		/// Restituisce un piano definito dai punti passati. 
		/// Se i punti non appartengono tutti al piano, o sono colineari, o in numero non sufficiente 
		/// allora viene restituito Plane3D.ZeroPlane. 
		/// N.B. Ci sono due soluzioni (normali opposte), ne viene restituita una, senza una particolare regola. 
		/// Spetta al chiamante eventualmente invertire la normale.
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Plane3D FromPoints(IEnumerable<Point3D> points)
		{
			Plane3D result = Plane3D.ZeroPlane;
			// Prima verifico se i punti appartengono ad uno dei piani standard
			bool standard = false;
			#region Standard
			bool sameX = true;
			double x = double.NaN;
			bool sameY = true;
			double y = double.NaN;
			bool sameZ = true;
			double z = double.NaN;
			foreach (Point3D point in points)
			{
				if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(z))
				{
					x = point.X;
					y = point.Y;
					z = point.Z;
				}
				else
				{
					if (sameX && point.X.IsEquals(x) == false)
						sameX = false;

					if (sameY && point.Y.IsEquals(y) == false)
						sameY = false;

					if (sameZ && point.Z.IsEquals(z) == false)
						sameZ = false;
				}
				if (!sameX && !sameY && !sameZ)
					break;
			}
			if (!sameX && !sameY && !sameZ)
			{
				standard = false;
			}
			else
			{
				standard = true;
				if ((sameX && sameY) || (sameX && sameZ) || (sameY && sameZ))
				{
					result = Plane3D.ZeroPlane;
				}
				else
				{
					if (sameX)
					{
						result = Plane3D.YZPlane;
						result.Location.X = x;
					}
					if (sameY)
					{
						result = Plane3D.XZPlane;
						result.Location.Y = y;
					}
					if (sameZ)
					{
						result = Plane3D.XYPlane;
						result.Location.Z = z;
					}
				}
			}
			#endregion Standard
			if (!standard)
			{
				#region Generico
				Point3D p1 = Point3D.NullPoint;
				Point3D p2 = Point3D.NullPoint;
				Vector3D normal = Vector3D.Zero;
				double distP2 = 0;
				double distP3 = 0;
				bool foundP1 = false;
				bool foundP2 = false;
				bool foundP3 = false;

				foreach (Point3D point in points)
				{
					if (!foundP1)
					{
						p1 = point;
						foundP1 = true;
						continue;
					}
					double dist = point.Distance(p1);
					if (dist > distP2)
					{
						distP2 = dist;
						p2 = point;
						foundP2 = true;
					}
				}
				if (foundP2)
				{
					Vector3D vx = p2 - p1;
					Vector3D vxN = vx.Normalize();
					foreach (Point3D point in points)
					{
						if (point.IsEquals(p1) == false && point.IsEquals(p2) == false)
						{
							Vector3D v = point - p1;
							Vector3D vN = v.Normalize();
							if (vN.IsEquals(vxN) == false)
							{
								double dist = v.Length * Math.Sin(vx.Angle(v));
								if (dist > distP3)
								{
									foundP3 = true;
									distP3 = dist;
									normal = vx.Cross(v).Normalize();
								}
							}
						}
					}
					if (foundP3)
					{
						result = new Plane3D(normal, p1);
					}
				}

				if (result != Plane3D.ZeroPlane)
				{
					foreach (Point3D point in points)
					{
						if (MathExtensions.IsEquals(result.Distance(point), 0, 0.05) == false)
						{
							result = Plane3D.ZeroPlane;
							break;
						}
					}
				}
				#endregion Generico
			}
			return result;
		}

		/// <summary>
		/// Restituisce un piano definito dalla matrice indicata. 
		/// La matrice viene considerata una rtMatrix, cioè con i vettori colonna normalizzati.
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public static Plane3D FromMatrix(RTMatrix rtMatrix)
		{
			Plane3D result = Plane3D.ZeroPlane;
			result.Normal = rtMatrix.GetVector(2);
			result.XAxis = rtMatrix.GetVector(0);
			result.Location = (Point3D)rtMatrix.Translation;

			return result;
		}
		#endregion STATICS
	}
}