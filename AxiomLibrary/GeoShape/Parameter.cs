using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape
{
	/// <summary>
	/// Classe che contiene le informazioni di un singolo parametro geometrico
	/// </summary>
	public class Parameter
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Formula del parametro geometrico.
		/// </summary>
		public string Formula { get; set; } = string.Empty;

		/// <summary>
		/// Valore calcolato del parametro geometrico.
		/// </summary>
		public double Value { get; set; } = 0.0;

		/// <summary>
		/// Nome
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Indica se si tratta di un parametro a cui applicare l'unità di misura (Uom) lineare. 
		/// Vale a livello di interfaccia utente, il valore viene sempre mantenuto in mm.
		/// </summary>
		public bool ApplyLinearUom { get; set; } = false;
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore
		/// </summary>
		public Parameter(string name, bool applyLinearUom, string formula, double value)
		{
			Name = name;
			ApplyLinearUom = applyLinearUom;
			Formula = formula;
			Value = value;
		}
		#endregion

		#region Methods

		#endregion




	}
}
