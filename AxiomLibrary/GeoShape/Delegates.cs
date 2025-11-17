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

	}
}
