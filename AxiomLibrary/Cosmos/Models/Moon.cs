using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Models
{
	public class Moon : CelestialBody
	{
		#region Fields

		#endregion

		#region Properties
		#endregion

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
