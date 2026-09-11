namespace Axiom.Cosmos.Dynamics
{
	/// <summary>
	/// Costanti fisiche condivise dalla simulazione Cosmos.
	/// Punto unico di verità: evita la ripetizione del valore di G in più classi.
	/// </summary>
	public static class PhysicalConstants
	{
		/// <summary>
		/// Costante di gravitazione universale (m³·kg⁻¹·s⁻²).
		/// </summary>
		public const double G = 6.67430e-11;
	}
}
