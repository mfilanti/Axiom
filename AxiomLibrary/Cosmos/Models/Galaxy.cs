using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Starships;
using Axiom.Cosmos.Utils;
using Axiom.GeoMath;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.Cosmos.Models
{
	public class Galaxy
	{
		#region Fields
		public const double G = 6.67430e-11;
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

		/// <summary>
		/// Navicelle dei giocatori presenti nella galassia
		/// </summary>
		public List<Starship> ActiveShips { get; set; } = new List<Starship>();
		#endregion

		#region Constructors
		public Galaxy(string name) { Name = name; }
		#endregion

		#region Methods
		public void DisplayInfo()
		{
			Console.WriteLine($"Galaxy: {Name}, Stars: {Stars.Count}");
		}

		/// <summary>
		/// Aggiunge una stella alla galassia
		/// </summary>
		/// <param name="star"></param>
		public void AddStar(Star star)
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

		/// <summary>
		/// Aggiorna la fisica della galassia utilizzando un Octree per l'ottimizzazione
		/// </summary>
		/// <param name="deltaTime">Delta di tempo</param>
		/// <returns></returns>
		public CosmosOctreeNode UpdatePhysics(double deltaTime)
		{
			// 1. Recupero di tutti i corpi celesti nella gerarchia
			var bodies = this.GetAllBodies().ToList();
			if (bodies.Count == 0) return null;

			// 2. COSTRUZIONE DELL'OCTREE
			// Calcoliamo il box che contiene l'intero "universo" corrente
			AABBox3D galaxyBounds = AABBox3D.FromPoints(bodies.Select(b => (Point3D)b.WorldMatrix.Translation));

			// Espandiamo leggermente il box per evitare errori di precisione ai bordi
			galaxyBounds.Enlarge(1.1);

			CosmosOctreeNode rootNode = new CosmosOctreeNode(galaxyBounds);
			foreach (var body in bodies)
			{
				rootNode.Insert(body);
			}

			// 3. CALCOLO ACCELERAZIONI (Utilizzando l'algoritmo Barnes-Hut)
			// Usiamo Parallel.ForEach per massimizzare le prestazioni su Unity/Desktop
			if (GravityField != null)
			{
				Parallel.ForEach(bodies, body =>
				{
					// L'Octree ora decide se calcolare la forza di ogni corpo 
					// o usare il Centro di Massa per i gruppi lontani
					body.Dynamics.Acceleration = rootNode.GetAcceleration(body, G);

				});
			}
			// 3. FISICA DELLE NAVI (Logica separata)
			foreach (var ship in ActiveShips)
			{
				// La nave interroga l'Octree per la gravità
				Vector3D gravityAcc = rootNode.GetAcceleration(ship, G);

				// Aggiunge la spinta dei motori (Thrust)
				Vector3D engineAcc = ship.GetThrustForce() / ship.Mass;

				// Applica l'accelerazione totale
				ship.Dynamics.Acceleration = gravityAcc + engineAcc;

				// Integra il movimento
				ship.Motion?.Integrate(ship, ship.Dynamics, deltaTime);
			}

			// 4. INTEGRAZIONE DEL MOTO (Velocity Verlet o altro)
			// Primo step: Posizione e velocità intermedia
			foreach (var body in bodies)
			{
				body.Motion?.Integrate(body, body.Dynamics, deltaTime);
			}

			// 5. AGGIORNAMENTO MATRICI 3D (Per Unity)
			// Dopo aver mosso i corpi, aggiorniamo la struttura Node3D 
			// per riflettere le nuove WorldMatrix
			foreach (var star in Stars)
			{
				// La ricorsione di Node3D aggiorna i figli (pianeti, lune, navicelle)
				star.Update(new(),out _);
			}
			return rootNode;
		}
		#endregion
	}
}
