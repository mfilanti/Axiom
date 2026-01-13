using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.Cosmos.Starships;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axiom.Cosmos.Simulation
{
	public sealed class SpaceSimulation
	{
		// Il contesto astronomico (Pianeti, Stelle, Octree)
		public Universe Universe { get; set; }

		// Il contesto astronomico (Pianeti, Stelle, Octree)
		public Galaxy CurrentGalaxy { get; set; }

		// Gli attori dinamici (Navi del giocatore, NPC, proiettili)
		public List<Starship> ActiveShips { get; set; } = new List<Starship>();

		public SpaceSimulation(Universe universe)
		{
			Universe = universe;
			CurrentGalaxy = universe.Galaxies.FirstOrDefault(); // Per ora usiamo la prima galassia
		}

		public void AddShip(Starship ship)
		{
			if (!ActiveShips.Contains(ship))
				ActiveShips.Add(ship);
		}

		public void Update(double deltaTime)
		{
			if (CurrentGalaxy == null) return;

			// 1. Chiediamo alla galassia di aggiornare i suoi pianeti
			// Passiamo un deltaTime per la fisica celeste
			// 2. Otteniamo l'Octree della galassia per calcolare la gravità sulle navi
			var gravityField = CurrentGalaxy.UpdatePhysics(deltaTime);

			// 3. Aggiorniamo le navi separatamente
			foreach (var ship in ActiveShips)
			{
				// A. Calcolo Gravità (La nave interroga l'Octree della Galassia)
				Vector3D gravityAcc = gravityField.GetAcceleration(ship, Galaxy.G);

				// B. Calcolo Spinta Motori (Thrust)
				// F = m * a  =>  a = F / m
				Vector3D engineForce = ship.GetEngineForce();
				Vector3D engineAcc = engineForce / ship.Mass;

				// C. Somma delle Accelerazioni
				ship.Dynamics.Acceleration = gravityAcc + engineAcc;

				// D. Integrazione del Moto (Velocity Verlet o Eulero)
				// Usiamo il modello di moto assegnato alla nave
				ship.Motion?.Integrate(ship, ship.Dynamics, deltaTime);
			}

			// 4. SINCRONIZZAZIONE MATRICI (Per il rendering in Unity)
			foreach (var ship in ActiveShips)
			{
				ship.Update(new(), out _);
			}
		}
	}
}
