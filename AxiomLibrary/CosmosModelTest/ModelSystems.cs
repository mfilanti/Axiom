using Axiom.Cosmos.Model;
using Axiom.GeoMath;
using Axiom.Physics;
using System;

namespace Axiom.Cosmos.Model.Tests
{
	/// <summary>
	/// Costruzione di sistemi di prova <b>senza</b> la <c>GalaxyFactory</c> (che vive nel progetto di
	/// composizione <c>Axiom.Cosmos</c>): qui si collauda il dominio in isolamento, quindi i corpi sono
	/// assemblati a mano.
	/// </summary>
	internal static class ModelSystems
	{
		public const double G = PhysicalConstants.G;

		/// <summary>
		/// Sole fermo all'origine + pianeta su orbita circolare a distanza <paramref name="r"/>.
		/// </summary>
		public static Galaxy SunAndPlanet(out Star sun, out Planet planet, double r = 1.496e11)
		{
			var galaxy = new Galaxy("Test") { GravityField = new NewtonianGravity() };

			sun = new Star("Sun", 1.989e30, 6.9e8, Vector3D.Zero, 1.0)
			{
				Dynamics = new DynamicsState(),
				Motion = null // il sole resta fermo
			};

			double v = Math.Sqrt(G * sun.Mass / r);
			planet = new Planet("Earth", 5.972e24, 6.4e6, new Vector3D(r, 0, 0))
			{
				Dynamics = new DynamicsState { Velocity = new Vector3D(0, v, 0) },
				Motion = new VelocityVerletMotion()
			};

			sun.AddNode(planet);
			galaxy.AddCelestialBody(sun);
			return galaxy;
		}
	}
}
