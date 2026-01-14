using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.Cosmos.Starships;
using Axiom.Cosmos.StartshipsControls;
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

		#endregion

		#region Constructors

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
				// A. Prima l'input (orienta la nave)
				pilot.ProcessInput(deltaTime);

				// B. Poi la fisica (muove la nave)
				_physics.ApplyShipPhysics(pilot.Ship, gravityField, deltaTime);

				// C. Infine sincronizza
				pilot.Ship.UpdateRTMatrix();
			}
		}


		/// <summary>
		/// Aggiunge una nave alla simulazione con il relativo pilota
		/// </summary>
		/// <param name="ship"></param>
		/// <param name="input"></param>
		public void AddShip(Starship ship, IInputProvider input)
			=> _pilots.Add(new ShipPilot(ship, input));

		#endregion

		#region Constructors
		public SpaceSimulation(Universe universe)
		{
			_universe = universe;
			_currentGalaxy = universe.Galaxies.FirstOrDefault(); // Per ora usiamo la prima galassia
		}
		#endregion
	}
}
