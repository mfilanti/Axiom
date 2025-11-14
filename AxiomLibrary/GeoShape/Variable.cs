using System.Runtime.Serialization;

namespace Axiom.GeoShape
{
	/// <summary>
	/// Definisce una variabile utilizzata nelle forme geometriche.
	/// </summary>
	public class Variable
	{
		/// <summary>
		/// Nome
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Valore
		/// </summary>		
		public double Value { get; set; } = 0.0;

		/// <summary>
		/// Se è diversa da stringa vuota (""), allora si tratta di variabile "Secondaria", cioè che dipende da altre
		/// </summary>
		public string Formula { get; set; } = string.Empty;

		/// <summary>
		/// Indica se si tratta di una variabile a cui applicare l'unità di misura (Uom) lineare. 
		/// Vale a livello di interfaccia utente, il valore viene sempre mantenuto in mm.
		/// </summary>
		public bool ApplyUomFactor { get; set; } = false;

		/// <summary>
		/// Indica se è una variabile non modificabile
		/// </summary>
		public bool ReadOnly {get; set; } = false;

		/// <summary>
		/// Descrizione
		/// </summary>
		public string Description { get; set; } = string.Empty;
	}
}
