using Axiom.Cosmos.Starships;
using Axiom.Cosmos.StartshipsControls;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosTest
{
    [TestClass]
    public class StarshipTest
    {
		[TestMethod]
		public void TestShip_Damping_ShouldStopRotation()
		{
			var ship = new Starship("X-Wing", 5000);
			var controller = new ShipFlightController(ship) { AngularDamping = 10.0 };

			// 1. Diamo un input per farla girare
			controller.HandleInput(0, 1.0, 0, 0, 0.1);
			ship.GetRotation(out _, out double yawInMotion, out _);

			// 2. Rilasciamo l'input (yaw = 0) e facciamo passare del tempo
			for (int i = 0; i < 100; i++)
				controller.HandleInput(0, 0, 0, 0, 0.01);

			ship.GetRotation(out _, out double yawStopped, out _);

			// 3. Verifichiamo che la rotazione sia quasi ferma (la differenza tra due frame deve tendere a zero)
			controller.HandleInput(0, 0, 0, 0, 0.01);
			ship.GetRotation(out _, out double yawFinal, out _);

			Assert.AreEqual(yawStopped, yawFinal, 0.0001, "Il damping non ha fermato la rotazione!");
		}
	}
}
