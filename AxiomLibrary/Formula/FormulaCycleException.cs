using System;

namespace Axiom.Formula
{
	/// <summary>
	/// Sollevata quando le variabili secondarie (formule che dipendono da altre variabili) formano un
	/// ciclo di dipendenze (es. A = B + 1, B = A · 2), che renderebbe la valutazione non terminante.
	/// </summary>
	public sealed class FormulaCycleException : Exception
	{
		public FormulaCycleException(string message) : base(message) { }
	}
}
