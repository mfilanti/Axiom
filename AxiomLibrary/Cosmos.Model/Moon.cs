using Axiom.GeoMath;
using System;

namespace Axiom.Cosmos.Model
{
	public class Moon : CelestialBody
	{
		#region Constructors
		public Moon(string name, double mass, double radius, Vector3D position)
			: base(name, mass, radius, position)
		{
		}
		#endregion

		#region Methods
		public override void DisplayInfo()
		{
			Console.WriteLine($"Moon: {Name}");
		}
		#endregion
	}
}
