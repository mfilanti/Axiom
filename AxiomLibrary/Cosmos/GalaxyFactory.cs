using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos
{
	public static class GalaxyFactory
	{
		public static Galaxy CreateSampleGalaxy()
        {
            const double G = 6.67430e-11;

            // ===== GALASSIA =====
            var milkyWay = new Galaxy("Milky Way")
            {
                GravityField = new NewtonianGravity()
            };

            // ===== STELLA =====
            var sun = new Star(
                "Sun",
                mass: 1.989e30,
                radius: 696340,
                position: Vector3D.Zero,
                luminosity: 3.828e26
            );

            sun.Dynamics = new DynamicsState
            {
                Velocity = Vector3D.Zero,
                Acceleration = Vector3D.Zero
            };

            sun.Motion = new VelocityVerletMotion();

            // ===== TERRA =====
            Planet earth = AddEarth(G, sun);
            sun.AddNode(earth);


			// ===== ASSEMBLAGGIO =====
			milkyWay.AddStar(sun);

            return milkyWay;
        }

        private static Planet AddEarth(double G, Star sun)
        {
            double earthDistance = 149.6e9; // metri
            double earthSpeed = Math.Sqrt(G * sun.Mass / earthDistance);

            var earth = new Planet(
                "Earth",
                mass: 5.972e24,
                radius: 6371,
                position: new Vector3D(earthDistance, 0, 0)
            );

            earth.Dynamics = new DynamicsState
            {
                Velocity = new Vector3D(0, earthSpeed, 0),
                Acceleration = Vector3D.Zero
            };

            earth.Motion = new VelocityVerletMotion();

			// ===== LUNA =====
			double moonDistance = 384_400_000; // metri
			double moonSpeed = Math.Sqrt(G * earth.Mass / moonDistance);

			var moon = new Moon(
				"Moon",
				mass: 7.35e22,
				radius: 1737,
				position: new Vector3D(moonDistance, 0, 0)
			);

			moon.Dynamics = new DynamicsState
			{
				Velocity = new Vector3D(0, moonSpeed, 0),
				Acceleration = Vector3D.Zero
			};

			moon.Motion = new VelocityVerletMotion();

			earth.AddNode(moon);
			return earth;
        }
    }
}
