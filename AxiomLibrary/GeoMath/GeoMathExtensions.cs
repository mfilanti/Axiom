using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoMath
{
	/// <summary>
	/// Estensioni matematiche per la geometria
	/// </summary>
	public static class GeoMathExtensions
	{
		#region Double Extensions
		/// <summary>
		/// Funzione che verifica se i due valori sono uguali a meno della 
		/// precisione definita da OpenCascade PrecisionExtension.Confusion.
		/// </summary>
		/// <param name="val1">Valore 1</param>
		/// <param name="val2">Valore 2</param>
		/// <returns>True se i valori sono uguali a meno della precisione, false altrimenti</returns>
		public static bool IsEquals(this double val1, double val2) => IsEquals(val1, val2, MathUtils.FineTolerance);

		/// <summary>
		/// Funzione che verifica se i due valori sono uguali a meno della 
		/// precisione specificata.
		/// </summary>
		/// <param name="val1">Valore 1</param>
		/// <param name="val2">Valore 2</param>
		/// <param name="precision">Precisione</param>
		/// <returns>True se i valori sono uguali a meno della precisione, false altrimenti</returns>
		public static bool IsEquals(this double val1, double val2, double precision) => Math.Abs(val1 - val2) < precision;


		/// <summary>
		/// Confronto tra 2 double con tolleranza.
		/// </summary>
		/// <param name="n1">primo valore</param>
		/// <param name="n2">secondo valore</param>
		/// <returns><c>true</c> Se è uguale o maggiore</returns>
		public static bool IsEqualsOrGreater(this double n1, double n2) => IsEqualsOrGreater(n1, n2, MathUtils.FineTolerance);

		/// <summary>
		/// Confronto tra 2 double con tolleranza passata come parametro.
		/// </summary>
		/// <param name="n1">primo valore</param>
		/// <param name="n2">secondo valore</param>
		/// <param name="tolerance">Tolleranza</param>
		/// <returns><c>true</c> Se è uguale o maggiore</returns>
		public static bool IsEqualsOrGreater(this double n1, double n2, double tolerance) => IsEquals(n1, n2, tolerance) || n1 > n2;

		/// <summary>
		/// Confronto tra 2 double con tolleranza uguale a MathUtils.Tolerance.
		/// </summary>
		/// <param name="n1">primo valore</param>
		/// <param name="n2">secondo valore</param>
		/// <returns><c>true</c> Se è uguale o minore</returns>
		public static bool IsEqualsOrLesser(this double n1, double n2) => IsEqualsOrLesser(n1, n2, MathUtils.FineTolerance);

		/// <summary>
		/// Confronto tra 2 double con tolleranza passata come parametro.
		/// </summary>
		/// <param name="n1">primo valore</param>
		/// <param name="n2">secondo valore</param>
		/// <param name="tolerance">Tolleranza</param>
		/// <returns><c>true</c> Se è uguale o minore</returns>
		public static bool IsEqualsOrLesser(this double n1, double n2, double tolerance) => IsEquals(n1, n2, tolerance) || n1 < n2;

		/// <summary>
		/// Converte l'angolo (in radianti) in ingresso nel range (0, 2*PI)
		/// </summary>
		/// <param name="radAngle">Angolo in radianti</param>
		/// <returns>Angolo nel range  (0, 2*PI)</returns>
		public static double AngleToRange02PI(this double radAngle)
		{
			double result = radAngle % (2 * Math.PI);
			if (result < 0) result += 2 * Math.PI;
			return result;
		}

		/// <summary>
		/// Converte l'angolo (in gradi) in ingresoo nel range (0, 360)
		/// </summary>
		/// <param name="degAngle">Angolo in gradi</param>
		/// <returns>Angolo nel range  (0, 360)</returns>
		public static double AngleToRange0360(this double degAngle)
		{
			double result = degAngle % 360;
			if (result < 0) result += 360;
			return result;
		}

		/// <summary>
		/// Angolo interno compreso tra i due angoli passati (risultato in gradi e sempre positivo). 
		/// Gli angoli passati possono essere qualsiasi: 
		/// riporta internamente in un range opportuno per determinare l'angolo compreso
		/// </summary>
		/// <param name="degAngle1">Primo Angolo</param>
		/// <param name="degAngle2">Secondo angolo</param>
		/// <returns>angolo interno</returns>
		public static double InternalAngle(this double degAngle1, double degAngle2)
		{
			double degAngleR1 = AngleToRange0360(degAngle1);
			double degAngleR2 = AngleToRange0360(degAngle2);
			double result = Math.Abs(degAngleR2 - degAngleR1);
			if (result > 180)
				result = 360 - result;
			return result;
		}
		#endregion

		#region Points Extensions

		/// <summary>
		/// Indica se il punto non è nullo
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static bool IsNotNull(this Point3D point) => point is not null;

		/// <summary>
		/// Indica se il punto è nullo
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static bool IsNull(this Point3D point) => point is null;
		#endregion
	}
}
