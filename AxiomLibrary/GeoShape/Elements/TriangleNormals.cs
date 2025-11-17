using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Elements
{
	/// <summary>
	/// Semplice struttura che contiene 3 vettori che rappresentano 3 normali di un triangolo
	/// </summary>
	public class TriangleNormals
	{
		/// <summary>
		/// Prima normale
		/// </summary>
		public Vector3D N1 { get; set; }

		/// <summary>
		/// Seconda normale
		/// </summary>
		public Vector3D N2 { get; set; }

		/// <summary>
		/// Terza normale
		/// </summary>
		public Vector3D N3 { get; set; }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <param name="n3"></param>
		public TriangleNormals(Vector3D n1, Vector3D n2, Vector3D n3)
		{
			N1 = new(n1);
			N2 = new(n2);
			N3 = new(n3);
		}

		/// <summary>
		/// Ruota le 3 normali
		/// </summary>
		/// <param name="matrix"></param>
		public void ApplyRT(RTMatrix matrix)
		{
			N1 = matrix * N1;
			N2 = matrix * N2;
			N3 = matrix * N3;
		}
	}
}
