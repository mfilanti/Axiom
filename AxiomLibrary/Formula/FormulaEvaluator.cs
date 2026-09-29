using Axiom.GeoShape;
using NCalc;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Axiom.Formula
{
	/// <summary>
	/// Valutatore di formule basato su <b>NCalc</b>, compatibile con
	/// <see cref="Delegates.EvaluatorDelegate"/> di GeoShape: si aggancia quindi direttamente a
	/// <c>Node3D.Update(variables, evaluator, out error)</c> e simili.
	/// <para>
	/// Gestisce le <b>variabili secondarie</b>: una <see cref="Variable"/> la cui
	/// <see cref="Variable.Formula"/> non è vuota viene calcolata valutando la sua formula, risolvendo
	/// ricorsivamente le variabili da cui dipende. I cicli di dipendenza sono rilevati e segnalati
	/// (<see cref="FormulaCycleException"/>) invece di causare ricorsione infinita.
	/// </para>
	/// </summary>
	public sealed class FormulaEvaluator
	{
		/// <summary>
		/// Costanti matematiche riconosciute quando un identificatore non corrisponde a una variabile.
		/// </summary>
		private static readonly IReadOnlyDictionary<string, double> Constants =
			new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
			{
				["pi"] = Math.PI,
				["e"] = Math.E,
			};

		/// <summary>
		/// Restituisce questo valutatore come delegate compatibile con GeoShape, pronto da iniettare
		/// (es. <c>node.Update(vars, evaluator.AsDelegate(), out err)</c>).
		/// </summary>
		public Delegates.EvaluatorDelegate AsDelegate() => Evaluate;

		/// <summary>
		/// Valuta <paramref name="expression"/> risolvendo le <paramref name="variables"/> (incluse le
		/// secondarie). In caso di errore restituisce 0 e popola <paramref name="errorDescription"/>.
		/// </summary>
		public double Evaluate(Dictionary<string, Variable> variables, string expression, out string errorDescription)
		{
			errorDescription = string.Empty;

			if (string.IsNullOrWhiteSpace(expression))
				return 0.0;

			try
			{
				var byName = BuildLookup(variables);
				var resolved = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
				var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				return EvaluateExpression(expression, byName, resolved, visiting);
			}
			catch (Exception ex)
			{
				errorDescription = ex.Message;
				return 0.0;
			}
		}

		/// <summary>
		/// Valuta una singola espressione, delegando la risoluzione di ogni identificatore a
		/// <see cref="Resolve"/> (variabili primarie/secondarie e costanti).
		/// </summary>
		private double EvaluateExpression(
			string expression,
			IReadOnlyDictionary<string, Variable> byName,
			Dictionary<string, double> resolved,
			HashSet<string> visiting)
		{
			// NCalc analizza i letterali numerici in cultura invariante ('.' decimale); rendiamo inoltre
			// i nomi delle funzioni built-in (Sqrt, Sin, ...) insensibili al maiuscolo/minuscolo.
			var expr = new Expression(expression, ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
			expr.EvaluateParameter += (name, args) => args.Result = Resolve(name, byName, resolved, visiting);
			object result = expr.Evaluate();
			return Convert.ToDouble(result, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Risolve il valore di un identificatore: variabile primaria (usa <see cref="Variable.Value"/>),
		/// variabile secondaria (valuta ricorsivamente <see cref="Variable.Formula"/> con rilevazione dei
		/// cicli) oppure costante matematica.
		/// </summary>
		private double Resolve(
			string name,
			IReadOnlyDictionary<string, Variable> byName,
			Dictionary<string, double> resolved,
			HashSet<string> visiting)
		{
			if (resolved.TryGetValue(name, out double cached))
				return cached;

			if (byName.TryGetValue(name, out Variable variable))
			{
				double value;
				if (string.IsNullOrWhiteSpace(variable.Formula))
				{
					value = variable.Value;
				}
				else
				{
					if (!visiting.Add(name))
						throw new FormulaCycleException($"Ciclo di dipendenze tra le variabili in corrispondenza di '{name}'.");
					try
					{
						value = EvaluateExpression(variable.Formula, byName, resolved, visiting);
					}
					finally
					{
						visiting.Remove(name);
					}
				}

				resolved[name] = value;
				return value;
			}

			if (Constants.TryGetValue(name, out double constant))
				return constant;

			throw new ArgumentException($"Variabile o costante non definita: '{name}'.");
		}

		/// <summary>
		/// Costruisce una tabella di lookup case-insensitive delle variabili, indicizzata per
		/// <see cref="Variable.Name"/> (con fallback alla chiave del dizionario).
		/// </summary>
		private static IReadOnlyDictionary<string, Variable> BuildLookup(Dictionary<string, Variable> variables)
		{
			var byName = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);
			if (variables == null) return byName;

			foreach (var kv in variables)
			{
				if (kv.Value == null) continue;
				string key = string.IsNullOrWhiteSpace(kv.Value.Name) ? kv.Key : kv.Value.Name;
				if (!string.IsNullOrWhiteSpace(key))
					byName[key] = kv.Value; // in caso di duplicati, vince l'ultimo
			}
			return byName;
		}
	}
}
