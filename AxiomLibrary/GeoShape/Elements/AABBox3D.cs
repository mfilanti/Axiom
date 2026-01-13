using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Axiom.GeoShape.Elements
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
		[XmlIgnore]
		[JsonIgnore]
		public List<Point3D> Points => [
					MinPoint,
					XmaxYminZminPoint,
					XmaxYmaxZminPoint,
					XminYmaxZminPoint,
					MaxPoint,
					XminYmaxZmaxPoint,
					XminYminZmaxPoint,
					XmaxYminZmaxPoint,
				];

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
		/// Restituisce la figura che rappresenta gli edge del box
		/// </summary>
		[XmlIgnore]
		public Figure3D Figure
		{
			get
			{
				Figure3D result = new Figure3D();
				result.AddPolygon(MinPoint, XmaxYminZminPoint, XmaxYmaxZminPoint, XminYmaxZminPoint, MinPoint);
				result.AddPolygon(XminYminZmaxPoint, XmaxYminZmaxPoint, MaxPoint, XminYmaxZmaxPoint, XminYminZmaxPoint);
				result.AddPolygon(MinPoint, XminYminZmaxPoint);
				result.AddPolygon(XmaxYminZminPoint, XmaxYminZmaxPoint);
				result.AddPolygon(XmaxYmaxZminPoint, MaxPoint);
				result.AddPolygon(XminYmaxZminPoint, XminYmaxZmaxPoint);
				return result;
			}
		}

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
			if (boxToAdd.MinPoint.X < MinPoint.X) MinPoint.X = boxToAdd.MinPoint.X;
			if (boxToAdd.MinPoint.Y < MinPoint.Y) MinPoint.Y = boxToAdd.MinPoint.Y;
			if (boxToAdd.MinPoint.Z < MinPoint.Z) MinPoint.Z = boxToAdd.MinPoint.Z;
			if (boxToAdd.MaxPoint.X > MaxPoint.X) MaxPoint.X = boxToAdd.MaxPoint.X;
			if (boxToAdd.MaxPoint.Y > MaxPoint.Y) MaxPoint.Y = boxToAdd.MaxPoint.Y;
			if (boxToAdd.MaxPoint.Z > MaxPoint.Z) MaxPoint.Z = boxToAdd.MaxPoint.Z;
		}

		/// <summary>
		/// Considera il punto ed eventualmente allarga il box per comprenderlo
		/// </summary>
		/// <param name="point"></param>
		public void EnlargeByPoint(Point3D point)
		{
			if (point.X < MinPoint.X) MinPoint.X = point.X;
			if (point.Y < MinPoint.Y) MinPoint.Y = point.Y;
			if (point.Z < MinPoint.Z) MinPoint.Z = point.Z;
			if (point.X > MaxPoint.X) MaxPoint.X = point.X;
			if (point.Y > MaxPoint.Y) MaxPoint.Y = point.Y;
			if (point.Z > MaxPoint.Z) MaxPoint.Z = point.Z;
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
			MinPoint.X -= offsetX;
			MinPoint.Y -= offsetY;
			MinPoint.Z -= offsetZ;
			MaxPoint.X += offsetX;
			MaxPoint.Y += offsetY;
			MaxPoint.Z += offsetZ;
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
		/// <returns></returns>
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
		/// <returns></returns>
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
				Point3D min = MinPoint;
				Point3D max = MaxPoint;
				if (bBox.MinPoint.X > min.X) min.X = bBox.MinPoint.X;
				if (bBox.MinPoint.Y > min.Y) min.Y = bBox.MinPoint.Y;
				if (bBox.MinPoint.Z > min.Z) min.Z = bBox.MinPoint.Z;
				if (bBox.MaxPoint.X < max.X) max.X = bBox.MaxPoint.X;
				if (bBox.MaxPoint.Y < max.Y) max.Y = bBox.MaxPoint.Y;
				if (bBox.MaxPoint.Z < max.Z) max.Z = bBox.MaxPoint.Z;

				intersection = new AABBox3D(min, max);
			}

			return result;
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
		/// Crea e restituisce un OBBox corrispondente
		/// </summary>
		/// <returns></returns>
		public OBBox3D GetOBBOX()
		{
			double lX = MaxPoint.X - MinPoint.X;
			double lY = MaxPoint.Y - MinPoint.Y;
			double lZ = MaxPoint.Z - MinPoint.Z;
			OBBox3D result = new OBBox3D(lX, lY, lZ);
			result.RTMatrix.Translation = (Vector3D)Center;

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
		public bool IntersectionLine(Line3D line, out double p1, out double p2, out Plane3D plane1, out Plane3D plane2)
		{
			bool result = false;

			p1 = p2 = 0;
			OBBox3D oBBox = GetOBBOX();
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
					AABBox3D enlargedBox = Clone();
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
			AABBox3D result = AABBox3D.NullAABBox;
			foreach (Point3D point in points)
			{
				if (point.X < result.MinPoint.X)
					result.MinPoint.X = point.X;
				if (point.X > result.MaxPoint.X)
					result.MaxPoint.X = point.X;

				if (point.Y < result.MinPoint.Y)
					result.MinPoint.Y = point.Y;
				if (point.Y > result.MaxPoint.Y)
					result.MaxPoint.Y = point.Y;

				if (point.Z < result.MinPoint.Z)
					result.MinPoint.Z = point.Z;
				if (point.Z > result.MaxPoint.Z)
					result.MaxPoint.Z = point.Z;
			}

			return result;
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
