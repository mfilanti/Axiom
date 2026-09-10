using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape
{
	/// <summary>
	/// Classe contenente i delegati usati nel progetto GeoShape.
	/// </summary>
	/// <remarks>
	/// <b>Thread-safety.</b> I campi <see cref="DelegateEvaluator"/> e <see cref="ComputeTriangulation"/>
	/// sono punti di estensione GLOBALI e vanno impostati UNA sola volta in fase di inizializzazione
	/// dell'applicazione (tipicamente sul thread principale), PRIMA di usare la libreria da più thread.
	/// Non vanno modificati mentre operazioni geometriche sono in esecuzione in parallelo (es. meshing).
	/// La lettura concorrente del riferimento è sicura; la riscrittura concorrente NO.
	/// </remarks>
	public static class Delegates
	{
		/// <summary>
		/// Permette di valutare le espressioni delle varie formule.
		/// </summary>
		public delegate double EvaluatorDelegate(Dictionary<string, Variable> variables, string expression, out string errorDescription);

		/// <summary>
		/// Permette di valutare le espressioni delle varie formule
		/// </summary>
		public static EvaluatorDelegate DelegateEvaluator = null;

		/// <summary>
		/// Funzione che permette di calcolare una triangolazione a partire da una Figure2D regolare. 
		/// Regolare significa chiusa (con uno o più loop). 
		/// </summary>
		public delegate List<Triangle3D> ComputeTriangulationDelegate(Figure3D profile);

		/// <summary>
		/// Funzione che permette di calcolare una triangolazione a partire da una Figure2D regolare. 
		/// Regolare significa chiusa (con uno o più loop). 
		/// </summary>
		public static ComputeTriangulationDelegate ComputeTriangulation = null;

		/// <summary>
		/// Collisione tra un raggio e una mesh
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public delegate bool RayMeshCollision(Ray3D ray, Mesh3D mesh);
	}
}
