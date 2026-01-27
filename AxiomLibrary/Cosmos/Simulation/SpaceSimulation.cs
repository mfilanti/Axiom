using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.Cosmos.Starships;
using Axiom.Cosmos.Utils;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axiom.Cosmos.Simulation
{
	public sealed class SpaceSimulation
	{
		#region Fields
		/// <summary>
		/// Lista di piloti e relative navi
		/// </summary>
		private readonly List<ShipPilot> _pilots = new();
		/// <summary>
		/// Engine di fisica celeste
		/// </summary>
		private readonly CosmosPhysicsEngine _physics = new();
		/// <summary>
		/// Universo in simulazione
		/// </summary>
		private Universe _universe;
		/// <summary>
		/// Galassia corrente in simulazione
		/// </summary>
		private Galaxy _currentGalaxy;

		#endregion

		#region Properties
		public Universe Universe => _universe;

		public ShipPilot CurrentShip { get; set; }
		#endregion


		#region Constructors
		public SpaceSimulation(Universe universe)
		{
			_universe = universe;
			_currentGalaxy = universe.Galaxies.FirstOrDefault(); // Per ora usiamo la prima galassia
		}
		public SpaceSimulation()
		{
			_universe = new Universe("Universe");
			_universe.Galaxies.Add(new Galaxy("Milky Way"));
			_currentGalaxy = _universe.Galaxies.FirstOrDefault(); 
		}
		#endregion


		#region Methods
		/// <summary>
		/// Aggiorna la simulazione spaziale
		/// </summary>
		/// <param name="deltaTime"></param>
		public void Update(double deltaTime)
		{
			if (_currentGalaxy == null) return;

			// 1. Fisica Celeste
			var gravityField = _currentGalaxy.UpdatePhysics(deltaTime);

			// 2. Loop Navi
			foreach (var pilot in _pilots)
			{
				pilot.UpdatePhysics(gravityField, deltaTime);
			}
		}


		/// <summary>
		/// Aggiunge una nave alla simulazione con il relativo pilota
		/// </summary>
		/// <param name="ship"></param>
		/// <param name="input"></param>
		public ShipPilot AddShip(Starship ship, IInputProvider input)
		{
			ShipPilot r = new ShipPilot(ship, input);
			_pilots.Add(r);
			if (CurrentShip == null)
			{
				CurrentShip = r;
			}
			return r;
		}
        /// <summary>
        /// Aggiunge un corpo celeste alla simulazione
        /// </summary>
        /// <param name="planet"></param>
        public void AddCelestialBody(CelestialBody planet) => _currentGalaxy.AddCelestialBody(planet);

        #endregion
    }
}
