using Axiom.Cosmos.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axiom.Cosmos.Models
{
	public class Galaxy
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Nome della galassia
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Stelle presenti nella galassia
		/// </summary>
		public List<Star> Stars { get; set; } = new List<Star>();

		/// <summary>
		/// Gravità della galassia
		/// </summary>
		public IGravityField GravityField { get; set; }
		#endregion

		#region Constructors
		public Galaxy(string name) { Name = name; }
		#endregion

		#region Methods
		public void DisplayInfo()
		{
			Console.WriteLine($"Galaxy: {Name}, Stars: {Stars.Count}");
		}

        internal void AddStar(Star star)
        {
            Stars.Add(star);	
		}

		public void Step(double deltaTime)
		{
			var bodies = this.GetAllBodies().ToList();

			// 1. Calcolo accelerazioni globali
			if (GravityField != null)
			{
				// 1. accelerazioni iniziali
				foreach (var body in bodies)
					body.Dynamics.Acceleration =
						GravityField.ComputeAcceleration(body, bodies);
			}

			// 2. primo half-step (posizione)
			foreach (var body in bodies)
				body.Motion.Integrate(body, body.Dynamics, deltaTime);

			// 3. ricalcolo accelerazioni
			if (GravityField != null)
			{
				foreach (var body in bodies)
					body.Dynamics.Acceleration =
						GravityField.ComputeAcceleration(body, bodies);
			}

			// 4. secondo half-step (velocità)
			foreach (var body in bodies)
				if (body.Motion is VelocityVerletMotion verlet)
					verlet.CompleteStep(body.Dynamics, deltaTime);
		}
		#endregion
	}
}
