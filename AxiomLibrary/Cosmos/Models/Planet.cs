using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Models
{
	public class Planet : CelestialBody
	{
		#region Fields

		#endregion

		#region Constructors
		public Planet(string name, double mass, double radius, Vector3D position)
			: base(name, mass, radius, position) { }
		#endregion

		#region Methods
		public override void DisplayInfo()
		{
			Console.WriteLine($"Planet: {Name}, Moons: {Nodes.Count}");
		}
		#endregion
	}
}
