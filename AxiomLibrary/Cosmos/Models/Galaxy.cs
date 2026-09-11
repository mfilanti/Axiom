using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Utils;
using Axiom.GeoShape;
using System;
using System.Linq;

namespace Axiom.Cosmos.Models
{
	public class Galaxy : Node3D
	{
		#region Fields
		/// <summary>
		/// Costante di gravitazione universale (alias di <see cref="PhysicalConstants.G"/>, mantenuto
		/// per retro-compatibilità dell'API pubblica).
		/// </summary>
		public const double G = PhysicalConstants.G;

		/// <summary>
		/// Motore di integrazione gravitazionale. La galassia descrive la scena; il solver la fa evolvere.
		/// </summary>
		private static readonly GravitySolver _solver = new GravitySolver();
		#endregion

		#region Properties
		/// <summary>
		/// Nome della galassia
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gravità della galassia
		/// </summary>
		public IGravityField GravityField { get; set; }

		#endregion

		#region Constructors
		public Galaxy(string name) { Name = name; }
		#endregion

		#region Methods
		public void DisplayInfo()
		{
			Console.WriteLine($"Galaxy: {Name}, Stars: {Nodes.Count}");
		}

		/// <summary>
		/// Aggiunge un corpo celeste alla galassia
		/// </summary>
		/// <param name="body"></param>
		public void AddCelestialBody(CelestialBody body) => AddNode(body);

		/// <summary>
		/// Aggiorna la fisica della galassia con gravità diretta O(n²) (Velocity Verlet completo).
		/// </summary>
		/// <param name="deltaTime"></param>
		public void Step(double deltaTime)
			=> _solver.StepDirect(this.GetAllBodies().ToList(), GravityField, deltaTime);

		/// <summary>
		/// Aggiorna la fisica della galassia usando un Octree (Barnes-Hut) e restituisce l'Octree
		/// coerente con le posizioni finali (riusato, ad esempio, come campo gravitazionale per le navi).
		/// </summary>
		/// <param name="deltaTime">Delta di tempo</param>
		/// <returns>L'Octree costruito sulle posizioni aggiornate, oppure null se non ci sono corpi.</returns>
		public CosmosOctreeNode UpdatePhysics(double deltaTime)
			=> _solver.StepBarnesHut(this.GetAllBodies().ToList(), deltaTime);
		#endregion
	}
}
