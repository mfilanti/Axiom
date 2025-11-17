using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using System.Numerics;
using System.Xml.Serialization;

namespace Axiom.GeoShape.Entities
{
	/// <summary>
	/// Oriented Bounding Box 3D.
	/// Rappresenta un bounding box (parallelepipedo) orientato nello spazio.
	/// E' definito con:
	/// - L in direzione X
	/// - L in direzione Y
	/// - L in direzione Z
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity)
	/// Se la matrice è un'identità allora lo zero coincide con il centro del box
	/// </summary>
	public class OBBox3D : Entity3D
	{
		#region Constants
		private const string LX_CONST = "lX";
		private const string LY_CONST = "lY";
		private const string LZ_CONST = "lZ";
		#endregion
		#region Properties
		/// <summary>
		/// L in direzione X
		/// </summary>
		public double LX { get => _parameters[LX_CONST].Value; set => _parameters[LX_CONST].Value = value; }

		/// <summary>
		/// L in direzione Y
		/// </summary>
		public double LY { get => _parameters[LY_CONST].Value; set => _parameters[LY_CONST].Value = value; }

		/// <summary>
		/// L in direzione Z
		/// </summary>
		public double LZ { get => _parameters[LZ_CONST].Value; set => _parameters[LZ_CONST].Value = value; }

		/// <summary>
		/// Formula LX
		/// </summary>
		public string LXFormula { get => _parameters[LX_CONST].Formula; set => _parameters[LX_CONST].Formula = value; }

		/// <summary>
		/// Formula LY
		/// </summary>
		public string LYFormula { get => _parameters[LY_CONST].Formula; set => _parameters[LY_CONST].Formula = value; }

		/// <summary>
		/// Formula LZ
		/// </summary>
		public string LZFormula { get => _parameters[LZ_CONST].Formula; set => _parameters[LZ_CONST].Formula = value; }
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public OBBox3D()
			: this(0, 0, 0, "", "", "")
		{
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="lX"></param>
		/// <param name="lY"></param>
		/// <param name="lZ"></param>
		public OBBox3D(double lX, double lY, double lZ)
			: this(lX, lY, lZ, "", "", "")
		{
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="lX"></param>
		/// <param name="lY"></param>
		/// <param name="lZ"></param>
		/// <param name="lXFormula"></param>
		/// <param name="lYFormula"></param>
		/// <param name="lZFormula"></param>
		public OBBox3D(double lX, double lY, double lZ, string lXFormula, string lYFormula, string lZFormula)
			: base()
		{
			_parameters.Add(LX_CONST, new Parameter(LX_CONST, true, lXFormula, lX));
			_parameters.Add(LY_CONST, new Parameter(LY_CONST, true, lYFormula, lY));
			_parameters.Add(LZ_CONST, new Parameter(LZ_CONST, true, lZFormula, lZ));
		}
		#endregion CONSTRUCTORS

		#region Methods
		/// <summary>
		/// Clona il OBBox3D
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			OBBox3D result = new OBBox3D(LX, LY, LZ, LXFormula, LYFormula, LZFormula);
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			List<Point3D> points =
			[
				// Faccia sotto
				new Point3D(-LX / 2, -LY / 2, -LZ / 2),
				new Point3D(LX / 2, -LY / 2, -LZ / 2),
				new Point3D(LX / 2, LY / 2, -LZ / 2),
				new Point3D(-LX / 2, LY / 2, -LZ / 2),
				// Faccia sopra
				new Point3D(-LX / 2, -LY / 2, LZ / 2),
				new Point3D(LX / 2, -LY / 2, LZ / 2),
				new Point3D(LX / 2, LY / 2, LZ / 2),
				new Point3D(-LX / 2, LY / 2, LZ / 2),
			];

			RTMatrix matrix = ParentRTMatrix.Multiply(RTMatrix);
			for (int i = 0; i < points.Count; i++)
				points[i] = matrix.Multiply(points[i]);

			AABBox3D result = AABBox3D.FromPoints(points);
			return result;
		}

		/// <summary>
		/// Restituisce il piano corrispondente alla faccia indicata
		/// </summary>
		/// <param name="face"></param>
		/// <param name="plane"></param>
		public void GetPlane(BoxFace face, out Plane3D plane)
		{
			switch (face)
			{
				case BoxFace.Top:
					plane = new Plane3D(Vector3D.UnitZ, new Point3D(0, 0, LZ / 2));
					break;
				case BoxFace.Bottom:
					plane = new Plane3D(Vector3D.NegativeUnitZ, new Point3D(0, 0, -LZ / 2));
					break;
				case BoxFace.Front:
					plane = new Plane3D(Vector3D.NegativeUnitY, new Point3D(0, -LY / 2, 0));
					break;
				case BoxFace.Rear:
					plane = new Plane3D(Vector3D.UnitY, new Point3D(0, LY / 2, 0));
					break;
				case BoxFace.Left:
					plane = new Plane3D(Vector3D.NegativeUnitX, new Point3D(-LX / 2, 0, 0));
					break;
				case BoxFace.Right:
					plane = new Plane3D(Vector3D.UnitX, new Point3D(LX / 2, 0, 0));
					break;
				default:
					plane = Plane3D.XYPlane;
					break;
			}
			plane.ApplyRT(RTMatrix);
		}

		/// <summary>
		/// Restituisce il piano corrispondente alla faccia indicata e una Figure3D 
		/// che ne rappresenta il bordo
		/// </summary>
		/// <param name="face"></param>
		/// <param name="plane"></param>
		/// <param name="borders"></param>
		public void GetPlane(BoxFace face, out Plane3D plane, out Figure3D borders)
		{
			GetPlane(face, out plane);
			borders = new Figure3D();
			Point3D pBRL = new Point3D(-LX / 2, LY / 2, -LZ / 2);
			Point3D pBRR = new Point3D(LX / 2, LY / 2, -LZ / 2);
			Point3D pBFL = new Point3D(-LX / 2, -LY / 2, -LZ / 2);
			Point3D pBFR = new Point3D(LX / 2, -LY / 2, -LZ / 2);
			Point3D pTRL = new Point3D(-LX / 2, LY / 2, LZ / 2);
			Point3D pTRR = new Point3D(LX / 2, LY / 2, LZ / 2);
			Point3D pTFL = new Point3D(-LX / 2, -LY / 2, LZ / 2);
			Point3D pTFR = new Point3D(LX / 2, -LY / 2, LZ / 2);

			switch (face)
			{
				case BoxFace.Top:
					borders.AddPolygon(pTRL, pTRR, pTFR, pTFL, pTRL);
					break;
				case BoxFace.Bottom:
					borders.AddPolygon(pBRL, pBRR, pBFR, pBFL, pBRL);
					break;
				case BoxFace.Front:
					borders.AddPolygon(pBFL, pBFR, pTFR, pTFL, pBFL);
					break;
				case BoxFace.Rear:
					borders.AddPolygon(pBRL, pBRR, pTRR, pTRL, pBRL);
					break;
				case BoxFace.Left:
					borders.AddPolygon(pBRL, pTRL, pTFL, pBFL, pBRL);
					break;
				case BoxFace.Right:
					borders.AddPolygon(pBRR, pTRR, pTFR, pBFR, pBRR);
					break;
				default:
					break;
			}
			borders.ApplyRT(RTMatrix);
		}

		/// <summary>
		/// Restituisce true se il punto passato è interno al OBBOx3D. 
		/// Anche se è sul bordo restituisce true.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Contains(Point3D point)
		{
			bool result = false;
			double d1, d2;
			Plane3D plane;

			GetPlane(BoxFace.Top, out plane);
			d1 = plane.Distance(point);
			GetPlane(BoxFace.Bottom, out plane);
			d2 = plane.Distance(point);
			// Se entrambe le distanze sono <= a LZ continuo altrimenti il punto è fuori
			if ((d1 <= LZ) && (d2 <= LZ))
			{
				GetPlane(BoxFace.Front, out plane);
				d1 = plane.Distance(point);
				GetPlane(BoxFace.Rear, out plane);
				d2 = plane.Distance(point);
				// Se entrambe le distanze sono <= a LY continuo altrimenti il punto è fuori
				if ((d1 <= LY) && (d2 <= LY))
				{
					GetPlane(BoxFace.Left, out plane);
					d1 = plane.Distance(point);
					GetPlane(BoxFace.Right, out plane);
					d2 = plane.Distance(point);
					// Se entrambe le distanze sono <= a LX il punto è dentro
					if ((d1 <= LX) && (d2 <= LX))
						result = true;
				}
			}

			return result;
		}

		/// <summary>
		/// Questa funzione testa se due OBBox3D sono separati. 
		/// L'idea alla base dell'algortimo è la seguente: 
		/// I due box sono separati se per ciascun asse la somma delle proiezioni dei raggi 
		/// è minore della distanza della proiezione dei due centri. 
		/// I raggi sono i punti più esterni al box lungo l'asse scelto. 
		/// Riferimento: "Realtime Collision Detection" - "4.4.1 OBB-OBB Intersection" pag.101
		/// </summary>
		/// <param name="box2"></param>
		/// <returns></returns>
		public bool Intersection(OBBox3D box)
		{
			bool result = true;
			// Creo 2 copie e metto dentro RTMatrix la moltiplicazione con la matrice ParentRTMatrix
			// perchè l'algoritmo che segue non ne teneva conto
			OBBox3D boxA = (OBBox3D)Clone();
			boxA.RTMatrix = boxA.ParentRTMatrix.Multiply(boxA.RTMatrix);
			boxA.ParentRTMatrix = RTMatrix.Identity;
			OBBox3D boxB = (OBBox3D)box.Clone();
			boxB.RTMatrix = boxB.ParentRTMatrix.Multiply(boxB.RTMatrix);
			boxB.ParentRTMatrix = RTMatrix.Identity;
			double EPSILON = 0.000001; // questo l'ho aggiunto io
			double ra, rb; // sono le proiezioni dei centri

			// Calcola la matrice di rotazione che esprime le coordinate del boxB rispetto le coordinate di boxA
			RTMatrix r = boxA.RTMatrix.Inverse().Multiply(boxB.RTMatrix);

			// Calcola la translazione tra il boxA e il boxB, nelle coordinate di a
			Vector3D t = r.Translation;

			// Calcolo le sottoespressioni comuni, aggiungendo un epsilon per evitare degli errori di calcolo
			// quando i due contorni sono paralleli ed il loro prodotto vettoriale è (vicino) allo zero
			RTMatrix absR = RTMatrix.Zero;
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					absR[i, j] = Math.Abs(r[i, j]) + EPSILON;

			// Testo gli assi L = A0, L = A1, L = A2
			double[] boxAe = new double[] { boxA.LX / 2, boxA.LY / 2, boxA.LZ / 2 };
			double[] boxBe = new double[] { boxB.LX / 2, boxB.LY / 2, boxB.LZ / 2 };
			for (int i = 0; i < 3; i++)
			{
				ra = boxAe[i];
				rb = boxBe[0] * absR[i, 0] + boxBe[1] * absR[i, 1] + boxBe[2] * absR[i, 2];
				if (Math.Abs(t[i]) > ra + rb)
				{
					result = false;
					break;
				}
			}

			if (result == true)
			{
				// Testo gli assi L = B0, L = B1, L = B2
				for (int i = 0; i < 3; i++)
				{
					ra = boxAe[0] * absR[0, i] + boxAe[1] * absR[1, i] + boxAe[2] * absR[2, i];
					rb = boxBe[i];
					if (Math.Abs(t[0] * r[0, i] + t[1] * r[1, i] + t[2] * r[2, i]) > ra + rb)
					{
						result = false;
						break;
					}
				}
			}
			if (result == true)
			{
				// Testo gli assi L = A0 x B0
				ra = boxAe[1] * absR[2, 0] + boxAe[2] * absR[1, 0];
				rb = boxBe[1] * absR[0, 2] + boxBe[2] * absR[0, 1];
				if (Math.Abs(t[2] * r[1, 0] - t[1] * r[2, 0]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A0 x B1
				ra = boxAe[1] * absR[2, 1] + boxAe[2] * absR[1, 1];
				rb = boxBe[0] * absR[0, 2] + boxBe[2] * absR[0, 0];
				if (Math.Abs(t[2] * r[1, 1] - t[1] * r[2, 1]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A0 x B2
				ra = boxAe[1] * absR[2, 2] + boxAe[2] * absR[1, 2];
				rb = boxBe[0] * absR[0, 1] + boxBe[1] * absR[0, 0];
				if (Math.Abs(t[2] * r[1, 2] - t[1] * r[2, 2]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A1 x B0
				ra = boxAe[0] * absR[2, 0] + boxAe[2] * absR[0, 0];
				rb = boxBe[1] * absR[1, 2] + boxBe[2] * absR[1, 1];
				if (Math.Abs(t[0] * r[2, 0] - t[2] * r[0, 0]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A1 x B1
				ra = boxAe[0] * absR[2, 1] + boxAe[2] * absR[0, 1];
				rb = boxBe[0] * absR[1, 2] + boxBe[2] * absR[1, 0];
				if (Math.Abs(t[0] * r[2, 1] - t[2] * r[0, 1]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A1 x B2
				ra = boxAe[0] * absR[2, 2] + boxAe[2] * absR[0, 2];
				rb = boxBe[0] * absR[1, 1] + boxBe[1] * absR[1, 0];
				if (Math.Abs(t[0] * r[2, 2] - t[2] * r[0, 2]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A2 x B0
				ra = boxAe[0] * absR[1, 0] + boxAe[1] * absR[0, 0];
				rb = boxBe[1] * absR[2, 2] + boxBe[2] * absR[2, 1];
				if (Math.Abs(t[1] * r[0, 0] - t[0] * r[1, 0]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A2 x B1
				ra = boxAe[0] * absR[1, 1] + boxAe[1] * absR[0, 1];
				rb = boxBe[0] * absR[2, 2] + boxBe[2] * absR[2, 0];
				if (Math.Abs(t[1] * r[0, 1] - t[0] * r[1, 1]) > ra + rb)
					result = false;
			}
			if (result == true)
			{
				// Test axis L = A2 x B2
				ra = boxAe[0] * absR[1, 2] + boxAe[1] * absR[0, 2];
				rb = boxBe[0] * absR[2, 1] + boxBe[1] * absR[2, 0];
				if (Math.Abs(t[1] * r[0, 2] - t[0] * r[1, 2]) > ra + rb)
					result = false;
			}
			// Dato che non sono stati trovati assi separati, gli OBB devono essere intersecati
			return result;
		}

		/// <summary>
		/// Calcola l'intersezione OBBox3D-Line
		/// Se la linea giace completamente all'interno di una faccia non 
		/// vengono restituite intersezioni
		/// </summary>
		/// <param name="line">Linea con cui calcolare l'intersezione</param>
		/// <param name="intersections">Eventuali punti di intersezione</param>
		/// <returns>Indica se c'è intersezione</returns>
		public bool IntersectionLine(Line3D line, out List<Point3D> intersections)
		{
			bool result = false;

			Plane3D plane;
			Figure3D borders;
			bool insideLine, insideFace;
			Point3D intPoint;

			intersections = new List<Point3D>();
			foreach (BoxFace face in Enum.GetValues(typeof(BoxFace)))
			{
				GetPlane(face, out plane, out borders);
				if (plane.IntersectLine(line, out insideLine, out intPoint))
				{
					if (insideLine)
					{
						// Ora controllo se il punto sta all'interno dei bordi
						insideFace = true;
						for (int i = 0; i < 4; i++)
						{
							Line3D border = (Line3D)borders[i];
							Vector3D vBorder = border.PEnd - border.PStart;
							Vector3D vP = intPoint - border.PStart;
							if (vP.Dot(vBorder) < 0)
							{
								insideFace = false;
								break;
							}
						}
						if (insideFace == true)
							intersections.Add(intPoint);
					}
				}
			}
			if (intersections.Count > 0)
				result = true;

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
			Point3D point1 = Point3D.NullPoint;
			Point3D point2 = Point3D.NullPoint;
			plane1 = Plane3D.ZeroPlane;
			plane2 = Plane3D.ZeroPlane;

			// per ogni piano
			foreach (BoxFace face in Enum.GetValues(typeof(BoxFace)))
			{
				Plane3D plane;
				GetPlane(face, out plane);

				// trova l'intersezione
				Point3D intersectionPoint;
				bool isInside;
				bool intersect = plane.IntersectLine(line, out isInside, out intersectionPoint);
				// se la linea non è parallela al piano
				if (intersect)
				{
					OBBox3D enlargedBox = (OBBox3D)Clone();
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
		/// Ingrandisce il bbox delle quantità indicate. 
		/// Il singolo offset viene applicato 2 volte in ciascuna direzione (sx/dx, ...).
		/// </summary>
		/// <param name="offsetX"></param>
		/// <param name="offsetY"></param>
		/// <param name="offsetZ"></param>
		public void Enlarge(double offsetX, double offsetY, double offsetZ)
		{
			LX += 2 * offsetX;
			LY += 2 * offsetY;
			LZ += 2 * offsetZ;
		}

		#endregion PUBLIC METHODS

	}
}