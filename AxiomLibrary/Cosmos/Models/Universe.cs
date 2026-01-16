using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Models
{
	public class Universe
	{
		#region Fields

		#endregion

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
			// 1. L'universo coordina la fisica globale
			// In un gioco Star Wars, qui potresti gestire il passaggio da un sistema solare all'altro
			foreach (var galaxy in Galaxies)
			{
				galaxy.Step(deltaTime);
			}

			// 2. Aggiornamento corpi fuori dalle galassie
			// (Qui andrebbe un Octree globale se i corpi intergalattici sono molti)
		}
		#endregion

		
	}
}
