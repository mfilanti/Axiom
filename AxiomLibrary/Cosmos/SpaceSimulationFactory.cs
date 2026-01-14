using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Simulation;
using Axiom.Cosmos.Starships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos
{
    public static class SpaceSimulationFactory
    {
        public static SpaceSimulation CreateDefaultSimulation()
        {
            // Creazione di un universo di default
            var universe = GalaxyFactory.CreateStarWarsUniverse();
			// Creazione della simulazione spaziale con l'universo di default
            var simulation = new SpaceSimulation(universe);
			// Creiamo l'X-Wing del giocatore
			var xWing = new Starship("X-Wing Red 5", mass: 5000)
			{
				MaxThrust = 150000, // Newton
				X = 149.6e9, // Partiamo vicino alla Terra
				Y = 5000000 // In orbita
			};
			xWing.Motion = new VelocityVerletMotion();

			simulation.AddShip(xWing,null);
			return simulation;
		}
	}
}
