namespace Axiom.Physics
{
	/// <summary>
	/// Costanti fisiche condivise dal motore.
	/// Punto unico di verità: evita la ripetizione del valore di G nelle varie classi.
	/// </summary>
	public static class PhysicalConstants
	{
		/// <summary>
		/// Costante di gravitazione universale (m³·kg⁻¹·s⁻²).
		/// </summary>
		public const double G = 6.67430e-11;
	}
}
