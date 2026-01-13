using Axiom.Cosmos.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Dynamics
{
	public sealed class UniverseSimulator
	{
		/// <summary>
		/// Corpi celesti presenti nell'universo simulato
		/// </summary>
		public List<CelestialBody> Bodies { get; } = new();

		/// <summary>
		/// Gets or sets the force field used for physical simulations.
		/// </summary>
		public IGravityField ForceField { get; set; }

		public void Step(double deltaTime)
		{
			foreach (var body in Bodies)
				body.Dynamics.Acceleration = ForceField.ComputeForce(body) / body.Mass;

			foreach (var body in Bodies)
				body.Step(deltaTime);
		}
	}
}
