using Axiom.Formula;
using Axiom.GeoShape;
using System.Collections.Generic;

namespace Axiom.Formula.Tests
{
	[TestClass]
	public class FormulaEvaluatorTests
	{
		private readonly FormulaEvaluator _eval = new FormulaEvaluator();

		private static Dictionary<string, Variable> Vars(params Variable[] vs)
		{
			var d = new Dictionary<string, Variable>();
			foreach (var v in vs) d[v.Name] = v;
			return d;
		}

		private static Variable Val(string name, double value) => new Variable { Name = name, Value = value };
		private static Variable Formula(string name, string formula) => new Variable { Name = name, Formula = formula };

		private double Eval(string expr, Dictionary<string, Variable>? vars = null)
		{
			double r = _eval.Evaluate(vars ?? new Dictionary<string, Variable>(), expr, out string error);
			Assert.AreEqual(string.Empty, error, $"Errore inatteso valutando '{expr}': {error}");
			return r;
		}

		// ---- Aritmetica di base ----

		[TestMethod]
		public void Arithmetic_RespectsPrecedenceAndParentheses()
		{
			Assert.AreEqual(14, Eval("2 + 3 * 4"), 1e-9);
			Assert.AreEqual(20, Eval("(2 + 3) * 4"), 1e-9);
			Assert.AreEqual(2, Eval("7 % 5"), 1e-9);
			Assert.AreEqual(-1.5, Eval("-3 / 2"), 1e-9);
		}

		[TestMethod]
		public void EmptyExpression_ReturnsZero_NoError()
		{
			double r = _eval.Evaluate(new Dictionary<string, Variable>(), "  ", out string error);
			Assert.AreEqual(0.0, r);
			Assert.AreEqual(string.Empty, error);
		}

		// ---- Variabili primarie ----

		[TestMethod]
		public void PrimaryVariable_UsesValue()
		{
			var vars = Vars(Val("a", 5), Val("b", 3));
			Assert.AreEqual(13, Eval("a * 2 + b", vars), 1e-9);
		}

		[TestMethod]
		public void VariableNames_AreCaseInsensitive()
		{
			var vars = Vars(Val("Width", 10));
			Assert.AreEqual(20, Eval("width * 2", vars), 1e-9);
		}

		// ---- Variabili secondarie (formule che dipendono da altre) ----

		[TestMethod]
		public void SecondaryVariable_IsResolvedFromItsFormula()
		{
			// b = a + 1, con a = 5  =>  b = 6
			var vars = Vars(Val("a", 5), Formula("b", "a + 1"));
			Assert.AreEqual(6, Eval("b", vars), 1e-9);
			Assert.AreEqual(12, Eval("b * 2", vars), 1e-9);
		}

		[TestMethod]
		public void SecondaryVariable_ChainOfDependencies()
		{
			// c = b * 2, b = a + 1, a = 5  =>  c = 12
			var vars = Vars(Val("a", 5), Formula("b", "a + 1"), Formula("c", "b * 2"));
			Assert.AreEqual(12, Eval("c", vars), 1e-9);
		}

		[TestMethod]
		public void SecondaryVariable_UsedInsideAnExpressionWithFunctions()
		{
			// area = pi * r^2 (approssimato con Pow), r secondaria = diametro / 2
			var vars = Vars(Val("diametro", 10), Formula("r", "diametro / 2"));
			Assert.AreEqual(System.Math.PI * 25, Eval("pi * Pow(r, 2)", vars), 1e-6);
		}

		// ---- Funzioni e costanti ----

		[TestMethod]
		public void BuiltInFunctions_AreSupported_AndCaseInsensitive()
		{
			Assert.AreEqual(4, Eval("Sqrt(16)"), 1e-9);
			Assert.AreEqual(3, Eval("sqrt(9)"), 1e-9, "nome funzione case-insensitive");
			Assert.AreEqual(3, Eval("Abs(-3)"), 1e-9);
			Assert.AreEqual(7, Eval("Max(2, 7)"), 1e-9);
			Assert.AreEqual(2, Eval("Min(2, 7)"), 1e-9);
			Assert.AreEqual(1024, Eval("Pow(2, 10)"), 1e-9);
		}

		[TestMethod]
		public void Constants_PiAndE_AreKnown()
		{
			Assert.AreEqual(System.Math.PI, Eval("pi"), 1e-12);
			Assert.AreEqual(System.Math.E, Eval("e"), 1e-12);
		}

		// ---- Gestione errori ----

		[TestMethod]
		public void CyclicSecondaryVariables_ReportError_NoStackOverflow()
		{
			// a = b + 1, b = a + 1  => ciclo
			var vars = Vars(Formula("a", "b + 1"), Formula("b", "a + 1"));
			double r = _eval.Evaluate(vars, "a", out string error);

			Assert.AreEqual(0.0, r);
			Assert.IsFalse(string.IsNullOrEmpty(error), "un ciclo deve produrre un errore");
			StringAssert.Contains(error, "Ciclo");
		}

		[TestMethod]
		public void UndefinedIdentifier_ReportsError()
		{
			double r = _eval.Evaluate(new Dictionary<string, Variable>(), "x + 1", out string error);
			Assert.AreEqual(0.0, r);
			Assert.IsFalse(string.IsNullOrEmpty(error));
		}

		[TestMethod]
		public void InvalidSyntax_ReportsError()
		{
			double r = _eval.Evaluate(new Dictionary<string, Variable>(), "2 +", out string error);
			Assert.AreEqual(0.0, r);
			Assert.IsFalse(string.IsNullOrEmpty(error));
		}

		// ---- Compatibilità con il contratto di GeoShape ----

		[TestMethod]
		public void AsDelegate_MatchesGeoShapeEvaluatorDelegate()
		{
			Delegates.EvaluatorDelegate del = new FormulaEvaluator().AsDelegate();
			var vars = Vars(Val("a", 4), Formula("b", "a * a"));

			double result = del(vars, "b + 1", out string error);

			Assert.AreEqual(string.Empty, error);
			Assert.AreEqual(17, result, 1e-9);
		}
	}
}
