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
		/// Costante di conversione Radianti -> Gradi
		/// </summary>
		public const double RadToDeg = 180 / Math.PI;

		/// <summary>
		/// Costante di conversione Gradi -> Radianti
		/// </summary>
		public const double DegToRad = Math.PI / 180;

		/// <summary>
		/// Precisione usata per confronti.
		/// </summary>
		public const double FineTolerance = 0.00001;


		/// <summary>
		/// Esegue lo swap di due variabili di tipo generico.
		/// </summary>
		/// <typeparam name="T">Tipo</typeparam>
		/// <param name="val1">variabile 1</param>
		/// <param name="val2">Variabile 2</param>
		public static void Swap<T>(ref T val1, ref T val2)
		{
			var temp = val1;
			val1 = val2;
			val2 = temp;
		}
	}
}
