using Axiom.GeoMath;
using System;

namespace Axiom.Cosmos.Model
{
	public class Planet : CelestialBody
	{
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
