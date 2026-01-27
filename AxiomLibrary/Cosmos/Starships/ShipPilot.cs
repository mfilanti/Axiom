using Assets.AxiomCore.Cosmos_Link.Starships;
using Axiom.Cosmos.Models;
using Axiom.Cosmos.Simulation;
using Axiom.Cosmos.Utils;
using Axiom.GeoMath;
using System;

namespace Axiom.Cosmos.Starships
{
    public class ShipPilot
    {
        public Starship Ship { get; }
        public ShipFlightController Controller { get; }
        public IInputProvider Input { get; }

        /// <summary>
        /// Vettore di gravità attuale applicato alla nave.
        /// </summary>
        public Vector3D Gravity = Vector3D.Zero;

        public ShipPilot(Starship ship, IInputProvider input)
        {
            Ship = ship;
            Input = input;
            Controller = new ShipFlightController(ship);
        }
        public void UpdatePhysics(CosmosOctreeNode gravityField, double deltaTime)
        {
            // A. Prima l'input (orienta la nave)
            ProcessInput(deltaTime);

            // B. Poi la fisica (muove la nave)
            ApplyShipPhysics(Ship, gravityField, deltaTime);

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

        /// <summary>
        /// Applica la fisica alla nave, considerando il campo gravitazionale e la spinta dei motori.
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="gravityField"></param>
        /// <param name="dt"></param>
        private void ApplyShipPhysics(Starship ship, CosmosOctreeNode gravityField, double dt)
        {
            // Gravità + Motore
            Gravity = (gravityField == null) ? Vector3D.Zero : gravityField.GetAcceleration(ship, Galaxy.G);
            Vector3D engineAcc = ship.GetThrustForce() / ship.Mass;
            ship.Dynamics.Acceleration = Gravity + engineAcc;

            //// Damping Lineare
            ApplyLinearDamping(ship, dt);

            ship.Motion?.Integrate(ship, ship.Dynamics, dt);
        }

        // All'interno della logica di movimento della nave
        public void ApplyLinearDamping(Starship ship, double dt)
        {
            double dampingFactor = 0.50; // Perde il 5% di velocità al secondo

            // Se non stiamo accelerando o se vogliamo un feeling "frenato"
            if (ship.CurrentThrottle < 0.1)
            {
                ship.Dynamics.Velocity *= Math.Pow(dampingFactor, dt);
            }
        }
    }
}
