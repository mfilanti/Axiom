using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoMath
{
	/// <summary>
	/// Classe di utilità matematica.
	/// </summary>
	public static class MathUtils
	{
		/// <summary>
		/// Precisione nei confronti.
		/// </summary>
		private static double _epsilon = 0.0001; // Precision.Confusion() * 100;

		/// <summary>
		/// Precisione nei confronti.
		/// </summary>
		public static double Epsilon => _epsilon;

		/// <summary>
		/// Funzione che verifica se i due valori sono uguali a meno della 
		/// precisione definita da OpenCascade PrecisionExtension.Confusion.
		/// </summary>
		/// <param name="val1">Valore 1</param>
		/// <param name="val2">Valore 2</param>
		/// <returns>True se i valori sono uguali a meno della precisione, false altrimenti</returns>
		public static bool IsEqual(double val1, double val2)
		{
			return IsEqual(val1, val2, _epsilon);
		}

		/// <summary>
		/// Funzione che verifica se i due valori sono uguali a meno della 
		/// precisione specificata.
		/// </summary>
		/// <param name="val1">Valore 1</param>
		/// <param name="val2">Valore 2</param>
		/// <param name="precision">Precisione</param>
		/// <returns>True se i valori sono uguali a meno della precisione, false altrimenti</returns>
		public static bool IsEqual(double val1, double val2, double precision)
		{
			bool result = true;
			if (Math.Abs(val1 - val2) > precision)
			{
				result = false;
			}
			return result;
		}
	}
}
