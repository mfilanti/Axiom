using Axiom.Cosmos.Model.Starships;
using Axiom.Cosmos.Simulation;
using Axiom.GeoMath;
using Axiom.Physics;

namespace Axiom.Cosmos
{
	/// <summary>
	/// Accoppia una nave, il suo controller di volo e la sorgente di input, e ne aggiorna lo stato per
	/// ogni passo delegando la fisica al motore unico <see cref="CosmosPhysicsEngine"/>.
	/// </summary>
	public class ShipPilot
	{
		public Starship Ship { get; }
		public ShipFlightController Controller { get; }
		public IInputProvider Input { get; }

		/// <summary>
		/// Vettore di gravità attuale applicato alla nave (per HUD/telemetria).
		/// </summary>
		public Vector3D Gravity = Vector3D.Zero;

		public ShipPilot(Starship ship, IInputProvider input)
		{
			Ship = ship;
			Input = input;
			Controller = new ShipFlightController(ship);
		}

		/// <summary>
		/// Aggiorna la nave per un passo: prima l'input (orientamento), poi la fisica delegata al
		/// motore unico, infine la sincronizzazione della matrice.
		/// </summary>
		public void UpdatePhysics(CosmosPhysicsEngine engine, GravityOctree<PhysicsBody> gravityField, double deltaTime)
		{
			// A. Prima l'input (orienta la nave)
			ProcessInput(deltaTime);

			// B. Poi la fisica (muove la nave) — un solo punto di verità nel motore
			Gravity = engine.ApplyShipPhysics(Ship, gravityField, deltaTime);

			// C. Infine sincronizza
			Ship.UpdateRTMatrix();
		}

		private void ProcessInput(double dt)
		{
			double pitch = 0.0;
			double yaw = 0.0;
			double roll = 0.0;
			double throttleDelta = 0.0;
			if (Input != null)
			{
				pitch = Input.Pitch;
				yaw = Input.Yaw;
				roll = Input.Roll;
				throttleDelta = Input.ThrottleDelta;
			}
			Controller.HandleInput(pitch, yaw, roll, throttleDelta, dt);
		}
	}
}
