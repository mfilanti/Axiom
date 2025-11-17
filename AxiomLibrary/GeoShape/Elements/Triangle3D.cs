using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Elements
{
	/// <summary>
	/// Struttura che rappresenta un triangolo nello spazio
	/// </summary>
	public class Triangle3D
	{
		#region properties
		/// <summary>
		/// Normale uscente. 
		/// Considerando i 3 punti in ordine antiorario.
		/// </summary>
		public Vector3D Normal => (P2 - P1).Cross(P3 - P1).Normalize();

		/// <summary>
		/// Centro del triangolo, inteso come baricentro.
		/// </summary>
		public Point3D Center
			// Sarebbe: 
			// P1 + (2.0 / 3.0) * ((P2 + 0.5 * (P3 - P2)) - P1)
			// Semplificato con algebra vettoriale
			=> (1 / 3.0) * (P1 + (P2 - (-P3)));

		/// <summary>
		/// Restituisce l'area del triangolo
		/// </summary>
		public double Area
		{
			get
			{
				double result = 0;
				Vector3D u = P2 - P1;
				Vector3D v = P3 - P1;
				result = 0.5 * u.Cross(v).Length;
				return result;
			}
		}

		/// <summary>
		/// Primo punto
		/// </summary>
		public Point3D P1 { get; set; }

		/// <summary>
		/// Secondo punto
		/// </summary>
		public Point3D P2 { get; set; }

		/// <summary>
		/// Terzo punto
		/// </summary>
		public Point3D P3 { get; set; }
		#endregion 

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="p3"></param>
		public Triangle3D(Point3D p1, Point3D p2, Point3D p3)
		{
			P1 = new(p1);
			P2 = new(p2);
			P3 = new(p3);
		}
		#endregion CONSTRUCTORS

		#region Overrides

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override bool Equals(object? obj)
		{
			if (obj is Triangle3D triangle3D)
			{
				return 
					P1 == triangle3D.P1 && 
					P2 == triangle3D.P2 &&
					P3 == triangle3D.P3;
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

		#region PUBLIC METHODS
		/// <summary>
		/// Confronta i due triangoli considerando la tolleranza uguale a MathUtils.Tolerance
		/// </summary>
		/// <param name="triangle"></param>
		/// <returns></returns>
		public bool IsEquals(Triangle3D triangle) => P1.IsEquals(triangle.P2) && P2.IsEquals(triangle.P2) && P3.IsEquals(triangle.P3);

		/// <summary>
		/// Confronta i due triangoli considerando la tolleranza indicata
		/// </summary>
		/// <param name="triangle"></param>
		/// <returns></returns>
		public bool IsEquals(Triangle3D triangle, double tolerance) => P1.IsEquals(triangle.P2, tolerance) && P2.IsEquals(triangle.P2, tolerance) && P3.IsEquals(triangle.P3, tolerance);

		/// <summary>
		/// Restituisce l'AABBox corrispondente
		/// </summary>
		/// <returns></returns>
		public AABBox3D GetAABBox() => AABBox3D.FromPoints([P1, P2, P3]);

		/// <summary>
		/// Ruota e trasla il triangolo
		/// </summary>
		/// <param name="matrix"></param>
		public void ApplyRT(RTMatrix matrix)
		{
			P1 = matrix * P1;
			P2 = matrix * P2;
			P3 = matrix * P3;
		}

		/// <summary>
		/// Suddivide il triangolo in più triangoli in base al piano passato. 
		/// Se il piano non interseca restituisce una lista con un solo triangolo uguale a 
		/// </summary>
		/// <param name="cutPlane"></param>
		/// <returns></returns>
		public List<Triangle3D> Subdivide(Plane3D cutPlane)
		{
			Vector3D n = Normal;
			TriangleNormals triNormals = new TriangleNormals(n, n, n);
			List<TriangleNormals> triNormalsList;
			return Subdivide(cutPlane, triNormals, out triNormalsList, out _);
		}

		/// <summary>
		/// Suddivide il triangolo in più triangoli in base al piano passato. 
		/// Se il piano non interseca restituisce una lista con un solo triangolo uguale a  
		/// Effettua il calcolo anche per le normali associate ai vertici del triangolo.
		/// </summary>
		/// <param name="cutPlane"></param>
		/// <param name="triNormals"></param>
		/// <param name="triNormalsList"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		public List<Triangle3D> Subdivide(Plane3D cutPlane, TriangleNormals triNormals, out List<TriangleNormals> triNormalsList, out Line3D line)
		{
			List<Triangle3D> result = new List<Triangle3D>();
			triNormalsList = new List<TriangleNormals>();
			line = null;
			Line3D line1 = new Line3D(P1, P2);
			Line3D line2 = new Line3D(P1, P3);
			Point3D int1, int2, int3;
			Point3D pA, pB, pC, iAB, iAC;
			Vector3D nA, nB, nC, nAB, nAC;
			bool insideLine1, insideLine2, insideLine3;
			bool inters1 = cutPlane.IntersectLine(line1, out insideLine1, out int1);
			bool inters2 = cutPlane.IntersectLine(line2, out insideLine2, out int2);
			if ((inters1 && insideLine1) || (inters2 && insideLine2))
			{
				if (inters1 && insideLine1 && inters2 && insideLine2)
				{
					pA = P1;
					nA = triNormals.N1;
					pB = P2;
					nB = triNormals.N2;
					pC = P3;
					nC = triNormals.N3;
					iAB = int1;
					iAC = int2;
				}
				else
				{
					Line3D line3 = new Line3D(P2, P3);
					cutPlane.IntersectLine(line3, out insideLine3, out int3);
					// Se siamo qui, uno dei due ha intersecato e quindi questo intersecherà di sicuro 
					// cioè inters3 e insideLine3 saranno a true (non li controllo nemmeno)
					if (inters1 && insideLine1)
					{
						pA = P2;
						nA = triNormals.N2;
						pB = P3;
						nB = triNormals.N3;
						pC = P1;
						nC = triNormals.N1;
						iAB = int3;
						iAC = int1;
					}
					else
					{
						pA = P3;
						nA = triNormals.N3;
						pB = P1;
						nB = triNormals.N1;
						pC = P2;
						nC = triNormals.N2;
						iAB = int2;
						iAC = int3;
					}
				}
				nAB = nA.Slerp(nB, (iAB - pA).Normalize().Dot((pB - pA).Normalize()), nA);
				nAC = nA.Slerp(nC, (iAC - pA).Normalize().Dot((pC - pA).Normalize()), nA);

				// Ora mi sono ricondotto a un unico caso
				bool pB_diff_iAB = !pB.IsEquals(iAB);
				bool pC_diff_iAC = !pC.IsEquals(iAC);
				bool iAB_diff_iAC = !iAB.IsEquals(iAC);
				if (iAB_diff_iAC)
				{
					line = new Line3D(iAB, iAC);
					result.Add(new Triangle3D(pA, iAB, iAC));
					triNormalsList.Add(new TriangleNormals(nA, nAB, nAC));
					if (pB_diff_iAB)
					{
						result.Add(new Triangle3D(pB, pC, iAB));
						triNormalsList.Add(new TriangleNormals(nB, nC, nAB));
					}
					if (pC_diff_iAC)
					{
						result.Add(new Triangle3D(pC, iAC, iAB));
						triNormalsList.Add(new TriangleNormals(nC, nAC, nAB));
					}
				}
				else
				{
					result.Add(this);
					triNormalsList.Add(triNormals);
				}
			}
			else
			{
				result.Add(this);
				triNormalsList.Add(triNormals);
			}
			return result;
		}

		/// <summary>
		/// Suddivide il triangolo ripetutamente con i piani passati
		/// </summary>
		/// <param name="planes"></param>
		/// <returns></returns>
		public List<Triangle3D> MultiSubdivide(List<Plane3D> planes)
		{
			List<Triangle3D> result = new List<Triangle3D>();
			result.Add(this);
			foreach (Plane3D plane in planes)
			{
				List<Triangle3D> locResult = new List<Triangle3D>();
				foreach (Triangle3D triangle in result)
					locResult.AddRange(triangle.Subdivide(plane));

				result = locResult;
			}
			return result;
		}

		/// <summary>
		/// Suddivide il triangolo in più triangoli in base alla linea passata. 
		/// Se la linea ha un punto fuori dal triangolo restituisce una lista con un solo triangolo uguale a  
		/// Se la linea corrisponde COMPLETAMENTE ad un lato del triangolo, restituisce una lista con un solo 
		/// triangolo uguale a 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="considerNormals"></param>
		/// <param name="inNormals"></param>
		/// <param name="outNormals"></param>
		/// <returns></returns>
		public List<Triangle3D> Subdivide(Line3D line, TriangleNormals inNormals, out List<TriangleNormals> outNormals)
		{
			List<Triangle3D> result = new List<Triangle3D>();
			outNormals = new List<TriangleNormals>();

			Point3D pA = line.PStart;
			Point3D pB = line.PEnd;
			// Calcolo le coordinate baricentriche dei due punti
			Vector3D v0 = P3 - P1;
			Vector3D v1 = P2 - P1;
			Vector3D v2A = pA - P1;
			Vector3D v2B = pB - P1;

			double dot00 = v0.Dot(v0);
			double dot01 = v0.Dot(v1);
			double dot11 = v1.Dot(v1);
			double dot0A = v0.Dot(v2A);
			double dot1A = v1.Dot(v2A);
			double dot0B = v0.Dot(v2B);
			double dot1B = v1.Dot(v2B);

			double invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
			double uA = (dot11 * dot0A - dot01 * dot1A) * invDenom;
			double vA = (dot00 * dot1A - dot01 * dot0A) * invDenom;
			double wA = 1 - vA - uA;
			double uB = (dot11 * dot0B - dot01 * dot1B) * invDenom;
			double vB = (dot00 * dot1B - dot01 * dot0B) * invDenom;
			double wB = 1 - vB - uB;

			Vector3D nA = wA * inNormals.N1 + vA * inNormals.N2 + uA * inNormals.N3;
			Vector3D nB = wB * inNormals.N1 + vB * inNormals.N2 + uB * inNormals.N3;

			bool vAIsZero = vA.IsEquals(0);
			bool uAIsZero = uA.IsEquals(0);
			bool vAIsOne = vA.IsEquals(1);
			bool uAIsOne = uA.IsEquals(1);
			bool wAIsZero = wA.IsEquals(0);
			bool vBIsZero = vB.IsEquals(0);
			bool uBIsZero = uB.IsEquals(0);
			bool vBIsOne = vB.IsEquals(1);
			bool uBIsOne = uB.IsEquals(1);
			bool wBIsZero = wB.IsEquals(0);

			if ((vA < 0 && !vAIsZero) || (vA > 1 && !vAIsOne) || (uA < 0 && !uAIsZero) || (uA > 1 && !uAIsOne) ||
				(vB < 0 && !vBIsZero) || (vB > 1 && !vBIsOne) || (uB < 0 && !uBIsZero) || (uB > 1 && !uBIsOne))
			{
				// - linea con un punto fuori dal triangolo
				result.Add(this);
				outNormals.Add(inNormals);
			}
			else
			{
				// Abbiamo diversi casi da considerare: 
				if ((vAIsZero && uAIsZero || vAIsZero && uAIsOne || vAIsOne && uAIsZero) &&
					(vBIsZero && uBIsZero || vBIsZero && uBIsOne || vBIsOne && uBIsZero))
				{
					// - linea completamente coincidente con un lato
					result.Add(this);
					outNormals.Add(inNormals);
				}
				else if (((vAIsZero && uAIsZero || vAIsZero && uAIsOne || vAIsOne && uAIsZero) && (vBIsZero || uBIsZero || wBIsZero)) ||
						 ((vBIsZero && uBIsZero || vBIsZero && uBIsOne || vBIsOne && uBIsZero) && (vAIsZero || uAIsZero || wAIsZero)))
				{
					// - linea con un punto coincidente con un vertice e l'altro su un lato
					#region 2 triangoli
					// - T1 (p1, p2, pI)
					// - T2 (p1, pI, p3)
					Point3D p1, p2, p3, pI;
					Vector3D n1, n2, n3, nI;

					if (vAIsZero && uAIsZero || vAIsZero && uAIsOne || vAIsOne && uAIsZero)
					{
						pI = pB;
						nI = nB;
						if (vBIsZero)
						{
							p1 = P2;
							p2 = P3;
							p3 = P1;
							n1 = inNormals.N2;
							n2 = inNormals.N3;
							n3 = inNormals.N1;
						}
						else if (uBIsZero)
						{
							p1 = P3;
							p2 = P1;
							p3 = P2;
							n1 = inNormals.N3;
							n2 = inNormals.N1;
							n3 = inNormals.N2;
						}
						else //if (wBIsZero)
						{
							p1 = P1;
							p2 = P2;
							p3 = P3;
							n1 = inNormals.N1;
							n2 = inNormals.N2;
							n3 = inNormals.N3;
						}
					}
					else //if(uBIsZero && vBIsZero || uBIsZero && vBIsOne || uBIsOne && vBIsZero)
					{
						pI = pA;
						nI = nA;
						if (vAIsZero)
						{
							p1 = P2;
							p2 = P3;
							p3 = P1;
							n1 = inNormals.N2;
							n2 = inNormals.N3;
							n3 = inNormals.N1;
						}
						else if (uAIsZero)
						{
							p1 = P3;
							p2 = P1;
							p3 = P2;
							n1 = inNormals.N3;
							n2 = inNormals.N1;
							n3 = inNormals.N2;
						}
						else //if (wAIsZero)
						{
							p1 = P1;
							p2 = P2;
							p3 = P3;
							n1 = inNormals.N1;
							n2 = inNormals.N2;
							n3 = inNormals.N3;
						}
					}

					result.Add(new Triangle3D(p1, p2, pI));
					result.Add(new Triangle3D(p1, pI, p3));
					outNormals.Add(new TriangleNormals(n1, n2, nI));
					outNormals.Add(new TriangleNormals(n1, nI, n3));

					#endregion 2 triangoli
				}
				else if (vAIsZero && vBIsZero || uAIsZero && uBIsZero || wAIsZero && wBIsZero)
				{
					// - linea con entrambi i punti sullo stesso lato del triangolo
					#region 3 triangoli
					// - T1 (p1, p2, pI1)
					// - T2 (p1, pI1, pI2)
					// - T3 (p1, pI2, p3)
					Point3D p1, p2, p3, pI1, pI2;
					Vector3D n1, n2, n3, nI1, nI2;

					if (vAIsZero && vBIsZero)
					{
						p1 = P2;
						p2 = P3;
						p3 = P1;
						n1 = inNormals.N2;
						n2 = inNormals.N3;
						n3 = inNormals.N1;
						if (uA > uB)
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
						else
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
					}
					else if (uAIsZero && uBIsZero)
					{
						p1 = P3;
						p2 = P1;
						p3 = P2;
						n1 = inNormals.N3;
						n2 = inNormals.N1;
						n3 = inNormals.N2;
						if (vA < vB)
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
						else
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
					}
					else //if (wAIsZero && wBIsZero)
					{
						p1 = P1;
						p2 = P2;
						p3 = P3;
						n1 = inNormals.N1;
						n2 = inNormals.N2;
						n3 = inNormals.N3;
						if (vA > uA)
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
						else
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
					}
					result.Add(new Triangle3D(p1, p2, pI1));
					result.Add(new Triangle3D(p1, pI1, pI2));
					result.Add(new Triangle3D(p1, pI2, p3));
					outNormals.Add(new TriangleNormals(n1, n2, nI1));
					outNormals.Add(new TriangleNormals(n1, nI1, nI2));
					outNormals.Add(new TriangleNormals(n1, nI2, n3));
					#endregion 3 triangoli
				}
				else if ((vAIsZero || uAIsZero || wAIsZero) && (vBIsZero || uBIsZero || wBIsZero))
				{
					// - linea con i punti su lati diversi del triangolo
					#region 3 triangoli
					// - T1 (p1, pI1, pI2)
					// - T2 (p2, p3, pI1)
					// - T3 (p3, pI2, pI1)
					Point3D p1, p2, p3, pI1, pI2;
					Vector3D n1, n2, n3, nI1, nI2;
					if (vAIsZero && uBIsZero || uAIsZero && vBIsZero)
					{
						p1 = P1;
						p2 = P2;
						p3 = P3;
						n1 = inNormals.N1;
						n2 = inNormals.N2;
						n3 = inNormals.N3;
						if (vAIsZero && uBIsZero)
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
						else
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
					}
					else if (vAIsZero && wBIsZero || wAIsZero && vBIsZero)
					{
						p1 = P3;
						p2 = P1;
						p3 = P2;
						n1 = inNormals.N3;
						n2 = inNormals.N1;
						n3 = inNormals.N2;
						if (vAIsZero && wBIsZero)
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
						else
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
					}
					else //if (vAIsZero && wBIsZero || wAIsZero && vBIsZero)
					{
						p1 = P2;
						p2 = P3;
						p3 = P1;
						n1 = inNormals.N2;
						n2 = inNormals.N3;
						n3 = inNormals.N1;
						if (uAIsZero && wBIsZero)
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
						else
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
					}
					result.Add(new Triangle3D(p1, pI1, pI2));
					result.Add(new Triangle3D(p2, p3, pI1));
					result.Add(new Triangle3D(p3, pI2, pI1));
					outNormals.Add(new TriangleNormals(n1, nI1, nI2));
					outNormals.Add(new TriangleNormals(n2, n3, nI1));
					outNormals.Add(new TriangleNormals(n3, nI2, nI1));
					#endregion 3 triangoli
				}
				else if ((vAIsZero && uAIsZero || vAIsZero && uAIsOne || vAIsOne && uAIsZero) ||
						 (vBIsZero && uBIsZero || vBIsZero && uBIsOne || vBIsOne && uBIsZero))
				{
					// - linea con un punto coincidente con un vertice e l'altro interno
					#region 3 triangoli
					// - T1 (p1, p2, pI)
					// - T2 (p2, p3, pI)
					// - T3 (p3, p1, pI)
					Point3D p1, p2, p3, pI;
					Vector3D n1, n2, n3, nI;
					if (vAIsZero && uAIsZero || vBIsZero && uBIsZero)
					{
						p1 = P1;
						p2 = P2;
						p3 = P3;
						n1 = inNormals.N1;
						n2 = inNormals.N2;
						n3 = inNormals.N3;
						if (vAIsZero && uAIsZero)
						{
							pI = pB;
							nI = nB;
						}
						else
						{
							pI = pA;
							nI = nA;
						}
					}
					else if (vAIsZero && uAIsOne || vBIsZero && uBIsOne)
					{
						p1 = P3;
						p2 = P1;
						p3 = P2;
						n1 = inNormals.N3;
						n2 = inNormals.N1;
						n3 = inNormals.N2;
						if (vAIsZero && uAIsOne)
						{
							pI = pB;
							nI = nB;
						}
						else
						{
							pI = pA;
							nI = nA;
						}
					}
					else //if (uAIsOne && vAIsZero || uBIsOne && vBIsZero)
					{
						p1 = P2;
						p2 = P3;
						p3 = P1;
						n1 = inNormals.N2;
						n2 = inNormals.N3;
						n3 = inNormals.N1;
						if (vAIsOne && uAIsZero)
						{
							pI = pB;
							nI = nB;
						}
						else
						{
							pI = pA;
							nI = nA;
						}
					}
					result.Add(new Triangle3D(p1, p2, pI));
					result.Add(new Triangle3D(p2, p3, pI));
					result.Add(new Triangle3D(p3, p1, pI));
					outNormals.Add(new TriangleNormals(n1, n2, nI));
					outNormals.Add(new TriangleNormals(n2, n3, nI));
					outNormals.Add(new TriangleNormals(n3, n1, nI));
					#endregion 3 triangoli
				}
				else if ((vAIsZero || uAIsZero || wAIsZero) || (vBIsZero || uBIsZero || wBIsZero))
				{
					// - linea con un punto su un lato e l'altro interno
					#region 4 triangoli
					// - T1 (p1, p2, pI2)
					// - T2 (p2, pI1, pI2)
					// - T3 (p3, pI2, pI1)
					// - T4 (p3, p1, pI2)
					Point3D p1, p2, p3, pI1, pI2;
					Vector3D n1, n2, n3, nI1, nI2;
					if (vAIsZero || vBIsZero)
					{
						p1 = P2;
						p2 = P3;
						p3 = P1;
						n1 = inNormals.N2;
						n2 = inNormals.N3;
						n3 = inNormals.N1;
						if (vAIsZero)
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
						else
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
					}
					else if (uAIsZero || uBIsZero)
					{
						p1 = P3;
						p2 = P1;
						p3 = P2;
						n1 = inNormals.N3;
						n2 = inNormals.N1;
						n3 = inNormals.N2;
						if (uAIsZero)
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
						else
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
					}
					else //if (wAIsZero || wBIsZero)
					{
						p1 = P1;
						p2 = P2;
						p3 = P3;
						n1 = inNormals.N1;
						n2 = inNormals.N2;
						n3 = inNormals.N3;
						if (wAIsZero)
						{
							pI1 = pA;
							pI2 = pB;
							nI1 = nA;
							nI2 = nB;
						}
						else
						{
							pI1 = pB;
							pI2 = pA;
							nI1 = nB;
							nI2 = nA;
						}
					}
					result.Add(new Triangle3D(p1, p2, pI2));
					result.Add(new Triangle3D(p2, pI1, pI2));
					result.Add(new Triangle3D(p3, pI2, pI1));
					result.Add(new Triangle3D(p3, p1, pI2));
					outNormals.Add(new TriangleNormals(n1, n2, nI2));
					outNormals.Add(new TriangleNormals(n2, nI1, nI2));
					outNormals.Add(new TriangleNormals(n3, nI2, nI1));
					outNormals.Add(new TriangleNormals(n3, n1, nI2));
					#endregion 3 triangoli
				}
				else
				{
					// - linea con entrambi i punti interni
					#region 5 triangoli
					// - T1 (p1, p2, pU)
					// - T2 (p2, p3, pW)
					// - T3 (p3, p1, pV)
					// - T4 (pA, pB, pX)
					// - T5 (pB, pA, pY)
					Point3D pX, pY;
					Vector3D nX, nY;
					bool a1 = false;
					bool a2 = false;
					bool a3 = false;
					bool b1 = false;
					bool b2 = false;
					bool b3 = false;

					if (uA < uB)
					{
						result.Add(new Triangle3D(P1, P2, pA));
						outNormals.Add(new TriangleNormals(inNormals.N1, inNormals.N2, nA));
						a1 = true;
						a2 = true;
					}
					else
					{
						result.Add(new Triangle3D(P1, P2, pB));
						outNormals.Add(new TriangleNormals(inNormals.N1, inNormals.N2, nB));
						b1 = true;
						b2 = true;
					}
					if (wA < wB)
					{
						result.Add(new Triangle3D(P2, P3, pA));
						outNormals.Add(new TriangleNormals(inNormals.N2, inNormals.N3, nA));
						a2 = true;
						a3 = true;
					}
					else
					{
						result.Add(new Triangle3D(P2, P3, pB));
						outNormals.Add(new TriangleNormals(inNormals.N2, inNormals.N3, nB));
						b2 = true;
						b3 = true;
					}

					if (vA < vB)
					{
						result.Add(new Triangle3D(P3, P1, pA));
						outNormals.Add(new TriangleNormals(inNormals.N3, inNormals.N1, nA));
						a3 = true;
						a1 = true;
					}
					else
					{
						result.Add(new Triangle3D(P3, P1, pB));
						outNormals.Add(new TriangleNormals(inNormals.N3, inNormals.N1, nB));
						b3 = true;
						b1 = true;
					}

					if (a1 && b1 && a2 && b2)
					{
						pX = P1;
						pY = P2;
						nX = inNormals.N1;
						nY = inNormals.N2;
					}
					else if (a1 && b1 && a3 && b3)
					{
						pX = P1;
						pY = P3;
						nX = inNormals.N1;
						nY = inNormals.N3;
					}
					else //if (a2 && b2 && a3 && b3)
					{
						pX = P2;
						pY = P3;
						nX = inNormals.N2;
						nY = inNormals.N3;
					}
					Vector3D n = (pB - pA).Cross(pX - pA);
					if (n.Dot(Normal) < 0)
					{
						MathUtils.Swap<Point3D>(ref pX, ref pY);
						MathUtils.Swap<Vector3D>(ref nX, ref nY);
					}

					result.Add(new Triangle3D(pA, pB, pX));
					result.Add(new Triangle3D(pB, pA, pY));
					outNormals.Add(new TriangleNormals(nA, nB, nX));
					outNormals.Add(new TriangleNormals(nB, nA, nY));
					#endregion 5 triangoli
				}
			}
			return result;
		}

		/// <summary>
		/// Suddivide il triangolo ripetutamente con i triangoli passati
		/// </summary>
		/// <param name="triangles"></param>
		/// <returns></returns>
		public List<Triangle3D> MultiSubdivide(List<Triangle3D> triangles, TriangleNormals inNormals, out List<TriangleNormals> outNormals)
		{
			List<Triangle3D> result = new List<Triangle3D>();
			result.Add(this);
			outNormals = new List<TriangleNormals>();
			outNormals.Add(inNormals);
			foreach (Triangle3D cutTriangle in triangles)
			{
				List<Triangle3D> locResult = new List<Triangle3D>();
				List<TriangleNormals> locOutNormals = new List<TriangleNormals>();
				for (int i = 0; i < result.Count; i++)
				{
					Triangle3D triangle = result[i];
					TriangleNormals normals = outNormals[i];
					Line3D line;
					if (triangle.IntersectTriangle(cutTriangle, out line))
					{
						List<TriangleNormals> currentOutNormals;
						locResult.AddRange(triangle.Subdivide(line, normals, out currentOutNormals));
						locOutNormals.AddRange(currentOutNormals);
					}
					else
					{
						locResult.Add(triangle);
						locOutNormals.Add(normals);
					}
				}
				result = locResult;
				outNormals = locOutNormals;
			}
			return result;
		}

		/// <summary>
		/// Intersezione triangolo - triangolo. 
		/// </summary>
		/// <param name="triangle"></param>
		/// <returns></returns>
		public bool IntersectTriangle(Triangle3D triangle)
		{
			Line3D intersection;
			return IntersectTriangle(triangle, out intersection);
		}

		/// <summary>
		/// Intersezione triangolo - triangolo. 
		/// </summary>
		/// <param name="triangle"></param>
		/// <param name="intersection"></param>
		/// <returns></returns>
		public bool IntersectTriangle(Triangle3D triangle, out Line3D intersection)
		{
			bool result = false;
			intersection = null;
			Plane3D planeA = new Plane3D(Normal, P1);
			Plane3D planeB = new Plane3D(triangle.Normal, triangle.P1);
			Line3D lineA, lineB;
			if (planeB.IntersectTriangle(this, out lineA) && planeA.IntersectTriangle(triangle, out lineB))
			{
				// Controllo che entrambe le linee siano di lunghezza > 0
				double lengthASqr = lineA.StartPoint.DistanceSqr(lineA.EndPoint);
				double lengthBSqr = lineB.StartPoint.DistanceSqr(lineB.EndPoint);
				if (!lengthASqr.IsEquals(0) && lengthASqr > 0 && !lengthBSqr.IsEquals(0) && lengthBSqr > 0)
				{
					// lineA e lineB sono per forza colineari, occorre individuare se si sovrappongono
					// Prendo lineA come riferimento
					Vector3D refVector = lineA.StartTangent;
					double uAS = 0;
					double uAE = lineA.Length;
					double uBS = (lineB.PStart - lineA.PStart).Dot(refVector);
					double uBE = (lineB.PEnd - lineA.PStart).Dot(refVector);
					if (!(uBS < 0 && uBE < 0) && !(uBS > uAE && uBE > uAE))
					{
						result = true;
						// Rendo lineB concorde a lineA
						if (uBS > uBE)
						{
							lineB.SetInverse();
							MathUtils.Swap<double>(ref uBS, ref uBE);
						}
						// Se siamo qui esistono 4 casi:
						if (uAS <= uBS && uAE >= uBE)
							intersection = lineB;
						else if (uBS <= uAS && uBE >= uAE)
							intersection = lineA;
						else if (uAS <= uBS && uAE < uBE)
							intersection = new Line3D(lineB.PStart, lineA.PEnd);
						else if (uBS <= uAS && uBE < uAE)
							intersection = new Line3D(lineA.PStart, lineB.PEnd);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Intersezione triangolo - raggio. 
		/// Restituisce true se il raggio interseca il piano del triangolo e se il punto è interno al 
		/// triangolo stesso. 
		/// N.B. True anche se è su un bordo del triangolo. 
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="intersection"></param>
		/// <returns></returns>
		public bool IntersectRay(Ray3D ray, out Point3D intersection)
		{
			return IntersectRay(ray, false, out intersection);
		}

		/// <summary>
		/// Intersezione triangolo - raggio. 
		/// Restituisce true se il raggio interseca il piano del triangolo e se il punto è interno al 
		/// triangolo stesso.
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="excludeBorder"></param>
		/// <param name="intersection"></param>
		/// <returns></returns>
		public bool IntersectRay(Ray3D ray, bool excludeBorder, out Point3D intersection)
		{
			Plane3D plane = new Plane3D(Normal, P1);
			bool result = plane.IntersectRay(ray, out intersection);
			if (result)
				result = Contains(intersection, excludeBorder);

			return result;
		}

		/// <summary>
		/// Indica se il punto passato è interno al triangolo. 
		/// N.B. True anche se è su un bordo del triangolo. 
		/// Non controlla che il punto sia sul piano del triangolo (lo da per scontato)
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Contains(Point3D point)
		{
			return Contains(point, false);
		}

		/// <summary>
		/// Indica se il punto passato è interno al triangolo. 
		/// Non controlla che il punto sia sul piano del triangolo (lo da per scontato)
		/// </summary>
		/// <param name="point"></param>
		/// <param name="excludeBorder"></param>
		/// <returns></returns>
		public bool Contains(Point3D point, bool excludeBorder)
		{
			bool result = false;

			Vector3D v0 = P3 - P1;
			Vector3D v1 = P2 - P1;
			Vector3D v2 = point - P1;

			double dot00 = v0.Dot(v0);
			double dot01 = v0.Dot(v1);
			double dot02 = v0.Dot(v2);
			double dot11 = v1.Dot(v1);
			double dot12 = v1.Dot(v2);

			// Calcola le coordinate baricentriche
			double invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
			double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
			double v = (dot00 * dot12 - dot01 * dot02) * invDenom;

			if (excludeBorder)
				result = (u > 0 && v > 0 && (u + v) < 1);
			else
				result = (u > 0 || u.IsEquals(0)) && (v > 0 || v.IsEquals(0)) && (u + v < 1 || (u + v).IsEquals(1));

			return result;
		}
		#endregion PUBLIC METHODS

		#region OPERATORS
		/// <summary>
		/// Uguaglianza precisa
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(Triangle3D left, Triangle3D right)
		{
			return (left.P1 == right.P1 && left.P2 == right.P2 && left.P3 == right.P3);
		}

		/// <summary>
		/// Disuguaglianza precisa
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(Triangle3D left, Triangle3D right)
		{
			return (left.P1 != right.P1 || left.P2 != right.P2 || left.P3 != right.P3);
		}
		#endregion OPERATORS
	}
}
