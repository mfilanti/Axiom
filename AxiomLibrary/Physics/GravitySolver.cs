using Axiom.GeoMath;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axiom.Physics
{
	/// <summary>
	/// Motore di integrazione gravitazionale (Velocity Verlet completo) per un insieme di corpi.
	/// È separato dai modelli di dominio: questi descrivono "cosa" c'è nella scena, il solver descrive
	/// "come" evolve nel tempo. Tutta l'integrazione avviene nel frame di mondo (inerziale) così che la
	/// dinamica dei corpi annidati resti coerente pur preservando la gerarchia dello scene-graph.
	/// </summary>
	public sealed class GravitySolver
	{
		/// <summary>
		/// Esegue un passo di Velocity Verlet completo con gravità diretta O(n²) usando il
		/// <paramref name="gravityField"/> indicato per il calcolo delle accelerazioni.
		/// </summary>
		public void StepDirect(IReadOnlyList<PhysicsBody> bodies, IGravityField gravityField, double deltaTime)
		{
			if (bodies.Count == 0) return;

			// 1. a(t)
			ComputeAccelerationsDirect(bodies, gravityField);
			// 2. x(t+dt) e mezza velocità (frame di mondo)
			IntegratePositionsWorld(bodies, deltaTime);
			// 3. a(t+dt) con le nuove posizioni
			ComputeAccelerationsDirect(bodies, gravityField);
			// 4. seconda metà della velocità (completamento Verlet)
			CompleteVelocities(bodies, deltaTime);
		}

		/// <summary>
		/// Esegue un passo di Velocity Verlet completo usando un Octree (Barnes-Hut) per il calcolo
		/// delle accelerazioni e restituisce l'Octree coerente con le posizioni finali (riusabile, ad
		/// esempio, come campo gravitazionale per altri corpi).
		/// </summary>
		/// <returns>L'Octree costruito sulle posizioni aggiornate.</returns>
		public GravityOctree<PhysicsBody> StepBarnesHut(IReadOnlyList<PhysicsBody> bodies, double deltaTime)
		{
			if (bodies.Count == 0) return null;

			// 1. Octree e accelerazioni iniziali a(t) con Barnes-Hut
			GravityOctree<PhysicsBody> octree = BuildOctree(bodies);
			ComputeAccelerationsBarnesHut(bodies, octree);

			// 2. Integrazione posizioni (frame di mondo) + mezza velocità
			IntegratePositionsWorld(bodies, deltaTime);

			// 3. Ricostruzione dell'Octree sulle nuove posizioni e a(t+dt)
			GravityOctree<PhysicsBody> octreeAfter = BuildOctree(bodies);
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
		private static void ComputeAccelerationsDirect(IReadOnlyList<PhysicsBody> bodies, IGravityField gravityField)
		{
			if (gravityField == null) return;
			foreach (var body in bodies)
				if (body.Motion != null && body.Dynamics != null)
					body.Dynamics.Acceleration = gravityField.ComputeAcceleration(body, bodies);
		}

		/// <summary>
		/// Calcola le accelerazioni gravitazionali tramite Barnes-Hut sull'Octree indicato.
		/// </summary>
		private static void ComputeAccelerationsBarnesHut(IReadOnlyList<PhysicsBody> bodies, GravityOctree<PhysicsBody> octree)
		{
			// Parallel.ForEach per sfruttare più core; ogni corpo scrive solo la propria accelerazione,
			// l'Octree è usato in sola lettura.
			Parallel.ForEach(bodies, body =>
			{
				if (body.Motion != null && body.Dynamics != null)
					body.Dynamics.Acceleration = octree.GetAcceleration(body, PhysicalConstants.G);
			});
		}

		/// <summary>
		/// Costruisce un Octree contenente tutti i corpi, sulle loro posizioni di mondo correnti.
		/// </summary>
		private static GravityOctree<PhysicsBody> BuildOctree(IReadOnlyList<PhysicsBody> bodies)
		{
			AABBox3D bounds = AABBox3D.FromPoints(bodies.Select(b => (Point3D)b.WorldMatrix.Translation));
			// Espansione per evitare errori di precisione ai bordi.
			bounds.Enlarge(1.1);

			var octree = new GravityOctree<PhysicsBody>(bounds);
			foreach (var body in bodies)
				octree.Insert(body);
			return octree;
		}

		/// <summary>
		/// Integra le posizioni (primo half-step di Verlet) nel frame di mondo.
		/// Le nuove posizioni di mondo sono calcolate tutte dallo snapshot corrente (integrazione
		/// simultanea), poi riscritte in ordine padre→figlio: grazie alla propagazione del
		/// ParentRTMatrix, ogni figlio vede già la nuova posa del padre quando la propria posizione di
		/// mondo viene riconvertita in traslazione locale.
		/// </summary>
		private static void IntegratePositionsWorld(IReadOnlyList<PhysicsBody> bodies, double dt)
		{
			var newWorldPositions = new Dictionary<PhysicsBody, Vector3D>();

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
		private static void CompleteVelocities(IReadOnlyList<PhysicsBody> bodies, double dt)
		{
			foreach (var body in bodies)
				if (body.Motion is VelocityVerletMotion verlet && body.Dynamics != null)
					verlet.CompleteStep(body.Dynamics, dt);
		}

		/// <summary>
		/// Imposta la posizione di mondo desiderata convertendola nella traslazione locale relativa al
		/// padre. Il setter di Translation propaga automaticamente il ParentRTMatrix ai figli.
		/// </summary>
		private static void SetWorldTranslation(PhysicsBody body, Vector3D worldTranslation)
		{
			RTMatrix parentInverse = body.ParentRTMatrix.InverseRT();
			Point3D localTranslation = parentInverse.Multiply((Point3D)worldTranslation);
			body.Translation = localTranslation;
		}
		#endregion
	}
}
