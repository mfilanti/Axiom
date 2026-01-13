using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Models
{
	public class Star : CelestialBody
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Luminosità della stella in Watt
		/// </summary>
		public double Luminosity { get; set; }

		
		#endregion

		#region Constructors
		public Star(string name, double mass, double radius, Vector3D position, double luminosity)
			: base(name, mass, radius, position) { Luminosity = luminosity; }
		#endregion

		#region Methods
		public override void DisplayInfo()
		{
			Console.WriteLine($"Star: {Name}, Planets: {Nodes.Count}");
		}
		#endregion
	}
}
