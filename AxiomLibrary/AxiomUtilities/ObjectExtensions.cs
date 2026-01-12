using System;
using System.Collections.Generic;
using System.Text;

namespace AxiomUtilities
{
	/// <summary>
	/// Estensioni per gli oggetti
	/// </summary>
	public static class ObjectExtensions
    {
		/// <summary>
		/// Fa una copia dell'oggetto.
		/// Funziona solo per ValueType o ICloneable. 
		/// Aggiunta gestione anche per List<string>, List<int>, List<double>.
		/// </summary>
		/// <param name="toClone"></param>
		/// <returns></returns>
		public static object MakeCopyOf(this object toClone)
		{
			if (toClone is ICloneable)
			{
				// Restituisce una deep copy dell'oggetto
				return ((ICloneable)toClone).Clone();
			}
			else if (toClone is ValueType)
			{
				// Restituisce una shallow copy del ValueType
				return ((ValueType)toClone);
			}
			else if (toClone is List<string>)
			{
				// Restituisce una nuova lista, glie elementi vengono copiati essendo ValueType
				// La string in realtà non lo è ma si comporta come ValueType
				return new List<string>((List<string>)toClone);
			}
			else if (toClone is List<int>)
			{
				// Restituisce una nuova lista, glie elementi vengono copiati essendo ValueType
				return new List<int>((List<int>)toClone);
			}
			else if (toClone is List<double>)
			{
				// Restituisce una nuova lista, glie elementi vengono copiati essendo ValueType
				return new List<double>((List<double>)toClone);
			}
			else if (toClone == null)
			{
				return null;
			}
			else
			{

				// Senza ricorrere a reflection o serializzazione, non possiamo fare un clone
				// dell'oggetto, e quindi restituiamo un'eccezione
				throw new System.NotSupportedException("object not cloneable");
			}
		}
	}
}
