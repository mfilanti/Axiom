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
	/// Definizioni dei tipi delegate (punti di estensione) usati nel progetto GeoShape.
	/// </summary>
	/// <remarks>
	/// <b>Nota di design.</b> In precedenza questa classe esponeva anche i campi statici globali
	/// <c>DelegateEvaluator</c> e <c>ComputeTriangulation</c>, impostati una volta e letti ovunque.
	/// Erano stato mutabile globale (non thread-safe, e fonte di flakiness nei test paralleli).
	/// Sono stati RIMOSSI: ora l'evaluator e il triangolatore vengono <b>iniettati come parametri</b>
	/// nei metodi che ne hanno bisogno (es. <c>Node3D.Update(variables, evaluator, out error)</c>,
	/// <c>Entity3DExtensions.From*(..., triangulator)</c>, <c>Mesh3D.CutByPlane(..., triangulator)</c>).
	/// Restano qui solo le DEFINIZIONI dei tipi delegate.
	/// </remarks>
	public static class Delegates
	{
		/// <summary>
		/// Permette di valutare le espressioni delle varie formule.
		/// </summary>
		public delegate double EvaluatorDelegate(Dictionary<string, Variable> variables, string expression, out string errorDescription);

		/// <summary>
		/// Funzione che permette di calcolare una triangolazione a partire da una Figure2D regolare.
		/// Regolare significa chiusa (con uno o più loop).
		/// </summary>
		public delegate List<Triangle3D> ComputeTriangulationDelegate(Figure3D profile);

		/// <summary>
		/// Collisione tra un raggio e una mesh
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public delegate bool RayMeshCollision(Ray3D ray, Mesh3D mesh);
	}
}
