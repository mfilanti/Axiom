using System;
using System.Collections.Generic;

namespace Axiom.GeoMath
{
	/// <summary>
	/// Box 3D Allineato agli assi
	/// </summary>
	/// <remark>
	/// E' un parallelepipedo con gli assi allineati a X, Y e Z. 
	/// Generalmente rappresenta il minimo parallelepipedo che contiene un entità.
	/// </remark>
	public class AABBox3D : ICloneable
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Punto con x massima, y minima e z minima
		/// </summary>
		public Point3D XmaxYminZminPoint => new(MaxPoint.X, MinPoint.Y, MinPoint.Z);

		/// <summary>
		/// Punto con x massima, y massima e z minima
		/// </summary>
		public Point3D XmaxYmaxZminPoint => new(MaxPoint.X, MaxPoint.Y, MinPoint.Z);

		/// <summary>
		/// Punto con x minima, y massima e z minima
		/// </summary>
		public Point3D XminYmaxZminPoint => new(MinPoint.X, MaxPoint.Y, MinPoint.Z);

		/// <summary>
		/// Punto con x massima, y minima e z massima
		/// </summary>
		public Point3D XmaxYminZmaxPoint => new(MaxPoint.X, MinPoint.Y, MaxPoint.Z);


		/// <summary>
		/// Punto con x minima, y massima e z minima
		/// </summary>
		public Point3D XminYmaxZmaxPoint => new(MinPoint.X, MaxPoint.Y, MaxPoint.Z);


		/// <summary>
		/// Punto con x minima, y massima e z massima
		/// </summary>
		public Point3D XminYminZmaxPoint => new(MinPoint.X, MinPoint.Y, MaxPoint.Z);


		/// <summary>
		/// Lista degli 8 punti. 
		/// Dal punto minimo in senso antiorario (prima Zmin poi Zmax)
		/// </summary>
		public List<Point3D> Points => new List<Point3D>
		{
			MinPoint,
			XmaxYminZminPoint,
			XmaxYmaxZminPoint,
			XminYmaxZminPoint,
			MaxPoint,
			XminYmaxZmaxPoint,
			XminYminZmaxPoint,
			XmaxYminZmaxPoint,
		};

		/// <summary>
		/// Estensione X
		/// </summary>
		public double LX => MaxPoint.X - MinPoint.X;

		/// <summary>
		/// Estensione Y
		/// </summary>
		public double LY => MaxPoint.Y - MinPoint.Y;

		/// <summary>
		/// Estensione Z
		/// </summary>
		public double LZ => MaxPoint.Z - MinPoint.Z;

		/// <summary>
		/// Lato maggiore
		/// </summary>
		public double MaxSide
		{
			get
			{
				double max = LX;
				if (LY > max)
					max = LY;

				if (LZ > max)
					max = LZ;

				return max;
			}
		}

		/// <summary>
		/// Lato minore
		/// </summary>
		public double MinSide
		{
			get
			{
				double min = LX;
				if (LY < min)
					min = LY;

				if (LZ < min)
					min = LZ;

				return min;
			}
		}

		/// <summary>
		/// Centro del bounding box
		/// </summary>
		public Point3D Center => MinPoint + 0.5 * (MaxPoint - MinPoint);

		/// <summary>
		/// Area box (delle 6 facce)
		/// </summary>
		public double Area => 2 * LX * LY + 2 * LX * LZ + 2 * LY * LZ;

		/// <summary>
		/// Volume box
		/// </summary>
		public double Volume => LX * LY * LZ;

		/// <summary>
		/// Punto minimo
		/// </summary>
		public Point3D MinPoint { get; set; }

		/// <summary>
		/// Punto massimo
		/// </summary>
		public Point3D MaxPoint { get; set; }
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public AABBox3D()
		{
			MinPoint = new Point3D();
			MaxPoint = new Point3D();
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="minPoint"></param>
		/// <param name="maxPoint"></param>
		public AABBox3D(Point3D minPoint, Point3D maxPoint)
		{
			MinPoint = new(minPoint);
			MaxPoint = new(maxPoint);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Clona l'AABBox3D
		/// </summary>
		/// <returns></returns>
		public AABBox3D Clone() => new AABBox3D(MinPoint, MaxPoint);

		/// <summary>
		/// Traslazione del box
		/// </summary>
		/// <param name="vector2D"></param>
		public void Move(Vector3D movement)
		{
			MinPoint += movement;
			MaxPoint += movement;
		}

		/// <summary>
		/// Fa l'unione con il box passato
		/// </summary>
		/// <param name="boxToAdd"></param>
		public void Union(AABBox3D boxToAdd)
		{
			// Point3D è immutabile: si ricostruiscono i punti anziché mutarne le coordinate.
			MinPoint = new Point3D(
				Math.Min(MinPoint.X, boxToAdd.MinPoint.X),
				Math.Min(MinPoint.Y, boxToAdd.MinPoint.Y),
				Math.Min(MinPoint.Z, boxToAdd.MinPoint.Z));
			MaxPoint = new Point3D(
				Math.Max(MaxPoint.X, boxToAdd.MaxPoint.X),
				Math.Max(MaxPoint.Y, boxToAdd.MaxPoint.Y),
				Math.Max(MaxPoint.Z, boxToAdd.MaxPoint.Z));
		}

		/// <summary>
		/// Considera il punto ed eventualmente allarga il box per comprenderlo
		/// </summary>
		/// <param name="point"></param>
		public void EnlargeByPoint(Point3D point)
		{
			MinPoint = new Point3D(
				Math.Min(MinPoint.X, point.X), Math.Min(MinPoint.Y, point.Y), Math.Min(MinPoint.Z, point.Z));
			MaxPoint = new Point3D(
				Math.Max(MaxPoint.X, point.X), Math.Max(MaxPoint.Y, point.Y), Math.Max(MaxPoint.Z, point.Z));
		}

		/// <summary>
		/// Ingrandisce il bbox delle quantità indicate. 
		/// Il singolo offset viene applicato 2 volte in ciascuna direzione (sx/dx, ...).
		/// </summary>
		/// <param name="offsetX">X</param>
		/// <param name="offsetY">Y</param>
		/// <param name="offsetZ">Z</param>
		public void Enlarge(double offsetX, double offsetY, double offsetZ)
		{
			MinPoint = new Point3D(MinPoint.X - offsetX, MinPoint.Y - offsetY, MinPoint.Z - offsetZ);
			MaxPoint = new Point3D(MaxPoint.X + offsetX, MaxPoint.Y + offsetY, MaxPoint.Z + offsetZ);
		}

		/// <summary>
		/// Ingrandisce il bbox delle quantità indicate. 
		/// Il singolo offset viene applicato 2 volte in ciascuna direzione (sx/dx, ...).
		/// </summary>
		/// <param name="scaleFactor">Scala</param>
		public void Enlarge(double scaleFactor)
		{
			// Calcoliamo quanto è grande il box ora
			double sizeX = MaxPoint.X - MinPoint.X;
			double sizeY = MaxPoint.Y - MinPoint.Y;
			double sizeZ = MaxPoint.Z - MinPoint.Z;

			// L'offset è la metà della differenza di dimensione desiderata
			double offsetX = (sizeX * scaleFactor - sizeX) / 2.0;
			double offsetY = (sizeY * scaleFactor - sizeY) / 2.0;
			double offsetZ = (sizeZ * scaleFactor - sizeZ) / 2.0;
			Enlarge(offsetX, offsetY, offsetZ);
		}

		/// <summary>
		/// Controlla se this si interseca anche parzialmente con un altro AABBox3D. 
		/// True se A contiene B, se B contiene A e se uno spigolo è contenuto nell'altro. 
		/// False se sono separati.
		/// </summary>
		/// <param name="bBox"></param>
		/// <returns>true intersecato, false non intersecato</returns>
		public bool Intersect(AABBox3D bBox) =>
			!(bBox.MaxPoint.X < MinPoint.X || bBox.MaxPoint.Y < MinPoint.Y || bBox.MaxPoint.Z < MinPoint.Z ||
			bBox.MinPoint.X > MaxPoint.X || bBox.MinPoint.Y > MaxPoint.Y || bBox.MinPoint.Z > MaxPoint.Z);

		/// <summary>
		/// Controlla se this si interseca anche parzialmente con un altro AABBox3D. 
		/// True se A contiene B, se B contiene A e se uno spigolo è contenuto nell'altro. 
		/// False se sono separati. 
		/// Restituisce l'eventuale BBox intersezione come parametro di uscita.
		/// </summary>
		/// <param name="bBox"></param>
		/// <returns>true intersecato, false non intersecato</returns>
		public bool Intersect(AABBox3D bBox, out AABBox3D intersection)
		{
			bool result = true;
			intersection = null;
			if (bBox.MaxPoint.X < MinPoint.X || bBox.MaxPoint.Y < MinPoint.Y || bBox.MaxPoint.Z < MinPoint.Z ||
				bBox.MinPoint.X > MaxPoint.X || bBox.MinPoint.Y > MaxPoint.Y || bBox.MinPoint.Z > MaxPoint.Z)
			{
				result = false;
			}
			else
			{
				Point3D min = new Point3D(
					Math.Max(MinPoint.X, bBox.MinPoint.X),
					Math.Max(MinPoint.Y, bBox.MinPoint.Y),
					Math.Max(MinPoint.Z, bBox.MinPoint.Z));
				Point3D max = new Point3D(
					Math.Min(MaxPoint.X, bBox.MaxPoint.X),
					Math.Min(MaxPoint.Y, bBox.MaxPoint.Y),
					Math.Min(MaxPoint.Z, bBox.MaxPoint.Z));

				intersection = new AABBox3D(min, max);
			}

			return result;
		}

		/// <summary>
		/// Intersezione con una sfera
		/// </summary>
		/// <param name="sphereCenter">Centro della sfera</param>
		/// <param name="radius">Raggio</param>
		/// <returns>true intersecato, false non intersecato</returns>
		public bool IntersectsSphere(Point3D sphereCenter, double radius)
		{
			// Troviamo il punto più vicino al centro della sfera all'interno del box (Clamping)
			double closestX = Math.Max(MinPoint.X, Math.Min(sphereCenter.X, MaxPoint.X));
			double closestY = Math.Max(MinPoint.Y, Math.Min(sphereCenter.Y, MaxPoint.Y));
			double closestZ = Math.Max(MinPoint.Z, Math.Min(sphereCenter.Z, MaxPoint.Z));

			// Calcoliamo la distanza tra il centro della sfera e questo punto più vicino
			double distanceX = sphereCenter.X - closestX;
			double distanceY = sphereCenter.Y - closestY;
			double distanceZ = sphereCenter.Z - closestZ;

			// Usiamo il quadrato della distanza per evitare il calcolo costoso della radice quadrata (Math.Sqrt)
			double distanceSquared = (distanceX * distanceX) + (distanceY * distanceY) + (distanceZ * distanceZ);

			// Se la distanza quadrata è minore del raggio al quadrato, c'è intersezione
			return distanceSquared <= (radius * radius);
		}

		/// <summary>
		/// Controlla se this contiene un altro AABBox3D
		/// </summary>
		/// <param name="bBox">box</param>
		/// <returns></returns>
		public bool Contains(AABBox3D bBox) => !(bBox.MinPoint.X < MinPoint.X || bBox.MinPoint.Y < MinPoint.Y || bBox.MinPoint.Z < MinPoint.Z ||
				bBox.MaxPoint.X > MaxPoint.X || bBox.MaxPoint.Y > MaxPoint.Y || bBox.MaxPoint.Z > MaxPoint.Z);

		/// <summary>
		/// Controlla se this contiene il punto passato
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Contains(Point3D point) => !(point.X < MinPoint.X || point.Y < MinPoint.Y || point.Z < MinPoint.Z ||
				point.X > MaxPoint.X || point.Y > MaxPoint.Y || point.Z > MaxPoint.Z);

		/// <summary>
		/// Confronta i due box considerando la tolleranza uguale a MathUtils.Tolerance
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public bool ApproxEquals(AABBox3D box) => MinPoint.IsEquals(box.MinPoint) && MaxPoint.IsEquals(box.MaxPoint);

		/// <summary>
		/// Confronta i due box considerando la tolleranza indicata
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public bool ApproxEquals(AABBox3D box, double tolerance) => MinPoint.IsEquals(box.MinPoint, tolerance) && MaxPoint.IsEquals(box.MaxPoint, tolerance);

		

		/// <summary>
		/// Indica se si tratta di un NullAABBox
		/// </summary>
		/// <returns></returns>
		public bool IsNullAABBox() => MinPoint.Equals(new Point3D(double.MaxValue, double.MaxValue, double.MaxValue)) &&
										MaxPoint.Equals(new Point3D(double.MinValue, double.MinValue, double.MinValue));

		#endregion PUBLIC METHODS

		#region STATICS
		/// <summary>
		/// AABBox3D nullo, cioè ha come punto minimo 3 double.MaxValue e come punto 
		/// massimo 3 double.MinValue. 
		/// In questo modo qualsiasi box con cui verrà fatto l'Union rimarrà inalterato.
		/// </summary>
		public static AABBox3D NullAABBox
		{
			get
			{
				Point3D minPoint = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
				Point3D maxPoint = new Point3D(double.MinValue, double.MinValue, double.MinValue);
				return new AABBox3D(minPoint, maxPoint);
			}
		}

		/// <summary>
		/// Restituisce il box corrispondente
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static AABBox3D FromPoints(IEnumerable<Point3D> points)
		{
			// Si accumulano min/max su variabili locali e si costruiscono i punti (immutabili) alla fine.
			// Con sequenza vuota si ottiene il NullAABBox (min = MaxValue, max = MinValue).
			double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
			foreach (Point3D point in points)
			{
				if (point.X < minX) minX = point.X;
				if (point.X > maxX) maxX = point.X;
				if (point.Y < minY) minY = point.Y;
				if (point.Y > maxY) maxY = point.Y;
				if (point.Z < minZ) minZ = point.Z;
				if (point.Z > maxZ) maxZ = point.Z;
			}

			return new AABBox3D(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));
		}
		#endregion STATICS

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}

}
