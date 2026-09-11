using System.Collections.Generic;

namespace Axiom.Cosmos.Model
{
	public class Universe
	{
		#region Properties
		/// <summary>
		/// Nome dell'universo
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Galassie contenute nell'universo
		/// </summary>
		public List<Galaxy> Galaxies { get; set; } = new List<Galaxy>();

		/// <summary>
		/// Corpi che non appartengono a nessuna galassia (es. pianeti vaganti o buchi neri intergalattici)
		/// </summary>
		public List<CelestialBody> IntergalacticBodies { get; set; } = new List<CelestialBody>();

		#endregion

		#region Constructors
		/// <summary>
		/// Universo
		/// </summary>
		public Universe(string name)
		{
			Name = name;
		}
		#endregion

		#region Methods
		public void Step(double deltaTime)
		{
			// L'universo coordina la fisica globale: avanza ogni galassia.
			foreach (var galaxy in Galaxies)
			{
				galaxy.Step(deltaTime);
			}

			// (Qui andrebbe un Octree globale se i corpi intergalattici sono molti.)
		}
		#endregion
	}
}
