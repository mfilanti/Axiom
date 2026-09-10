using Axiom.Cosmos.Dynamics;
using Axiom.Cosmos.Starships;
using Axiom.Cosmos.Utils;
using Axiom.GeoMath;
using Axiom.GeoShape;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.Cosmos.Models
{
	public class Galaxy : Node3D
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
			Console.WriteLine($"Galaxy: {Name}, Stars: {Nodes.Count}");
		}

        /// <summary>
        /// Aggiunge una stella alla galassia
        /// </summary>
        /// <param name="star"></param>
        public void AddCelestialBody(CelestialBody body) => AddNode(body);


        /// <summary>
        /// Aggiorna la fisica della galassia con il metodo Velocity Verlet completo e gravità diretta O(n²).
        /// L'integrazione avviene interamente nel frame di mondo (inerziale): le posizioni sono lette e
        /// riscritte come coordinate di mondo, così la dinamica dei corpi annidati (es. lune figlie dei
        /// pianeti) resta fisicamente coerente pur preservando la gerarchia dello scene-graph.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Step(double deltaTime)
		{
			var bodies = this.GetAllBodies().ToList();
			if (bodies.Count == 0) return;

			// 1. a(t)
			ComputeAccelerationsDirect(bodies);
			// 2. x(t+dt) e mezza velocità (frame di mondo)
			IntegratePositionsWorld(bodies, deltaTime);
			// 3. a(t+dt) con le nuove posizioni
			ComputeAccelerationsDirect(bodies);
			// 4. seconda metà della velocità (completamento Verlet)
			CompleteVelocities(bodies, deltaTime);
		}

		/// <summary>
		/// Aggiorna la fisica della galassia usando un Octree (Barnes-Hut) per l'ottimizzazione.
		/// Esegue un Velocity Verlet completo nel frame di mondo e restituisce l'Octree coerente con
		/// le posizioni finali (riusato, ad esempio, come campo gravitazionale per le navi).
		/// </summary>
		/// <param name="deltaTime">Delta di tempo</param>
		/// <returns>L'Octree costruito sulle posizioni aggiornate, oppure null se non ci sono corpi.</returns>
		public CosmosOctreeNode UpdatePhysics(double deltaTime)
		{
			var bodies = this.GetAllBodies().ToList();
			if (bodies.Count == 0) return null;

			// 1. Octree e accelerazioni iniziali a(t) con Barnes-Hut
			CosmosOctreeNode octree = BuildOctree(bodies);
			ComputeAccelerationsBarnesHut(bodies, octree);

			// 2. Integrazione posizioni (frame di mondo) + mezza velocità
			IntegratePositionsWorld(bodies, deltaTime);

			// 3. Ricostruzione dell'Octree sulle nuove posizioni e a(t+dt)
			CosmosOctreeNode octreeAfter = BuildOctree(bodies);
			ComputeAccelerationsBarnesHut(bodies, octreeAfter);

			// 4. Completamento della velocità (Velocity Verlet completo)
			CompleteVelocities(bodies, deltaTime);

			// L'Octree restituito è coerente con le posizioni finali.
			return octreeAfter;
		}

		#region Physics helpers
		/// <summary>
		/// Calcola le accelerazioni gravitazionali dirette (O(n²)) per i corpi che si muovono.
		/// </summary>
		private void ComputeAccelerationsDirect(List<CelestialBody> bodies)
		{
			if (GravityField == null) return;
			foreach (var body in bodies)
				if (body.Motion != null && body.Dynamics != null)
					body.Dynamics.Acceleration = GravityField.ComputeAcceleration(body, bodies);
		}

		/// <summary>
		/// Calcola le accelerazioni gravitazionali tramite Barnes-Hut sull'Octree indicato.
		/// </summary>
		private void ComputeAccelerationsBarnesHut(List<CelestialBody> bodies, CosmosOctreeNode octree)
		{
			// Parallel.ForEach per sfruttare più core su Unity/Desktop; ogni corpo scrive solo la
			// propria accelerazione, l'Octree è usato in sola lettura.
			Parallel.ForEach(bodies, body =>
			{
				if (body.Motion != null && body.Dynamics != null)
					body.Dynamics.Acceleration = octree.GetAcceleration(body, G);
			});
		}

		/// <summary>
		/// Costruisce un Octree contenente tutti i corpi, sulle loro posizioni di mondo correnti.
		/// </summary>
		private static CosmosOctreeNode BuildOctree(List<CelestialBody> bodies)
		{
			AABBox3D bounds = AABBox3D.FromPoints(bodies.Select(b => (Point3D)b.WorldMatrix.Translation));
			// Espansione per evitare errori di precisione ai bordi.
			bounds.Enlarge(1.1);

			var octree = new CosmosOctreeNode(bounds);
			foreach (var body in bodies)
				octree.Insert(body);
			return octree;
		}

		/// <summary>
		/// Integra le posizioni (primo half-step di Verlet) nel frame di mondo.
		/// Le nuove posizioni di mondo sono calcolate tutte dallo snapshot corrente (integrazione
		/// simultanea), poi riscritte in ordine padre→figlio (GetAllBodies è pre-order): grazie alla
		/// propagazione del ParentRTMatrix, ogni figlio vede già la nuova posa del padre quando la
		/// propria posizione di mondo viene riconvertita in traslazione locale.
		/// </summary>
		private static void IntegratePositionsWorld(List<CelestialBody> bodies, double dt)
		{
			var newWorldPositions = new Dictionary<CelestialBody, Vector3D>();

			// Fase di lettura: calcola le nuove posizioni di mondo dallo stato corrente (immutato).
			foreach (var body in bodies)
			{
				if (body.Motion == null || body.Dynamics == null) continue;

				Vector3D worldPos = body.WorldMatrix.Translation;
				Vector3D a = body.Dynamics.Acceleration;
				Vector3D v = body.Dynamics.Velocity;

				worldPos += v * dt + a * (0.5 * dt * dt);
				body.Dynamics.Velocity = v + a * (0.5 * dt);

				newWorldPositions[body] = worldPos;
			}

			// Fase di scrittura: padre prima dei figli.
			foreach (var body in bodies)
				if (newWorldPositions.TryGetValue(body, out Vector3D worldPos))
					SetWorldTranslation(body, worldPos);
		}

		/// <summary>
		/// Completa la velocità (secondo half-step) per i corpi che usano il Velocity Verlet.
		/// </summary>
		private static void CompleteVelocities(List<CelestialBody> bodies, double dt)
		{
			foreach (var body in bodies)
				if (body.Motion is VelocityVerletMotion verlet && body.Dynamics != null)
					verlet.CompleteStep(body.Dynamics, dt);
		}

		/// <summary>
		/// Imposta la posizione di mondo desiderata convertendola nella traslazione locale relativa al
		/// padre. Il setter di Translation propaga automaticamente il ParentRTMatrix ai figli.
		/// </summary>
		private static void SetWorldTranslation(CelestialBody body, Vector3D worldTranslation)
		{
			RTMatrix parentInverse = body.ParentRTMatrix.InverseRT();
			Point3D localTranslation = parentInverse.Multiply((Point3D)worldTranslation);
			body.Translation = localTranslation;
		}
		#endregion

        #endregion
    }
}
