using Axiom.Cosmos.Simulation;
using Axiom.Cosmos.StartshipsControls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Starships
{
	public class ShipPilot
	{
		public Starship Ship { get; }
		public ShipFlightController Controller { get; }
		public IInputProvider Input { get; }

		public ShipPilot(Starship ship, IInputProvider input)
		{
			Ship = ship;
			Input = input;
			Controller = new ShipFlightController(ship);
		}

		public void ProcessInput(double dt)
		{
			Controller.HandleInput(Input.Pitch, Input.Yaw, Input.Roll, Input.ThrottleDelta, dt);
		}
	}
}
