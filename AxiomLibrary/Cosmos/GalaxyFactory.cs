using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;

namespace Axiom.Cosmos
{
	public static class GalaxyFactory
	{
		private const double G = 6.67430e-11;

		public static Universe CreateStarWarsUniverse()
		{
			var universe = new Universe("Deep Space");

			// Creiamo la Via Lattea (con il Sistema Solare che abbiamo fatto prima)
			Galaxy milkyWay = GalaxyFactory.CreateSolarSystem();
			universe.Galaxies.Add(milkyWay);

			// Creiamo una galassia vicina (es. Andromeda)
			// Possiamo posizionarla a distanze intergalattiche (es. 2.5 milioni di anni luce)
			Galaxy andromeda = new Galaxy("Andromeda");
			andromeda.GravityField = new NewtonianGravity();

			// Aggiungiamo una stella massiccia ad Andromeda per test
			var star = new Star("Andromeda Alpha", 5.0e30, 800000000, new Vector3D(2.365e22, 0, 0), 5.0e26);
			andromeda.AddCelestialBody(star);

			universe.Galaxies.Add(andromeda);

			return universe;
		}

		public static Galaxy CreateSolarSystem()
		{
			var solarSystem = new Galaxy("Solar System")
			{
				GravityField = new NewtonianGravity()
			};

			// ===== IL SOLE (Centro del sistema) =====
			var sun = new Star("Sun", 1.989e30, 696340000, Vector3D.Zero, 3.828e26)
			{
				Dynamics = new DynamicsState { Velocity = Vector3D.Zero, Acceleration = Vector3D.Zero },
				Motion = new VelocityVerletMotion()
			};

			// ===== AGGIUNTA PIANETI =====
			// Parametri: (Nome, Massa, Raggio, Distanza dal Sole)
			sun.AddNode(CreatePlanet("Mercury", 3.301e23, 2439700, 57.91e9, sun));
			sun.AddNode(CreatePlanet("Venus", 4.867e24, 6051800, 108.2e9, sun));

			// Terra con Luna
			var earth = CreatePlanet("Earth", 5.972e24, 6371000, 149.6e9, sun);
			earth.AddNode(CreateSatellite("Moon", 7.347e22, 1737100, 384400000, earth));
			sun.AddNode(earth);

			// Marte con lune
			var mars = CreatePlanet("Mars", 6.39e23, 3389500, 227.9e9, sun);
			mars.AddNode(CreateSatellite("Phobos", 1.065e16, 11200, 9377000, mars));
			mars.AddNode(CreateSatellite("Deimos", 1.476e15, 6200, 23460000, mars));
			sun.AddNode(mars);

			// Fascia di Asteroidi (Esempio: Cerere)
			sun.AddNode(CreatePlanet("Ceres", 9.39e20, 473000, 413.7e9, sun));

			// Giganti Gassosi
			var jupiter = CreatePlanet("Jupiter", 1.898e27, 69911000, 778.5e9, sun);
			jupiter.AddNode(CreateSatellite("Europa", 4.8e22, 1560000, 670900000, jupiter));
			sun.AddNode(jupiter);

			sun.AddNode(CreatePlanet("Saturn", 5.683e26, 58232000, 1.434e12, sun));
			sun.AddNode(CreatePlanet("Uranus", 8.681e25, 25362000, 2.871e12, sun));
			sun.AddNode(CreatePlanet("Neptune", 1.024e26, 24622000, 4.495e12, sun));

			solarSystem.AddCelestialBody(sun);
			return solarSystem;
		}

		/// <summary>
		/// Crea un pianeta calcolando automaticamente la velocità orbitale necessaria per un'orbita circolare.
		/// </summary>
		private static Planet CreatePlanet(string name, double mass, double radius, double distance, Star parent)
		{
			double speed = Math.Sqrt(G * parent.Mass / distance);
			var planet = new Planet(name, mass, radius, new Vector3D(distance, 0, 0))
			{
				Dynamics = new DynamicsState
				{
					Velocity = new Vector3D(0, speed, 0),
					Acceleration = Vector3D.Zero
				},
				Motion = new VelocityVerletMotion()
			};
			return planet;
		}

		/// <summary>
		/// Crea un satellite (Luna o Asteroide orbitante)
		/// </summary>
		private static Moon CreateSatellite(string name, double mass, double radius, double distance, CelestialBody parent)
		{
			double speed = Math.Sqrt(G * parent.Mass / distance);
			var moon = new Moon(name, mass, radius, new Vector3D(distance, 0, 0))
			{
				Dynamics = new DynamicsState
				{
					// La velocità è relativa al pianeta padre
					Velocity = new Vector3D(0, speed, 0),
					Acceleration = Vector3D.Zero
				},
				Motion = new VelocityVerletMotion()
			};
			return moon;
		}
	}
}