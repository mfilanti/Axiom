using Axiom.GeoMath;
using Axiom.Physics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axiom.Physics.Tests
{
	[TestClass]
	public class GravitySolverTests
	{
		private const double G = PhysicalConstants.G;
		private const double SunMass = 1.989e30;

		/// <summary>
		/// Costruisce un sistema a due corpi: sole fermo all'origine (Motion = null) e un corpo su
		/// un'orbita circolare a distanza <paramref name="r"/>. I corpi sono fratelli di primo livello,
		/// quindi world-frame = local-frame (nessuna gerarchia): il test è puramente sul motore.
		/// </summary>
		private static (List<PhysicsBody> bodies, TestBody planet) TwoBodySystem(double r)
		{
			double v = Math.Sqrt(G * SunMass / r);
			var sun = new TestBody(SunMass, Vector3D.Zero, "Sun");
			sun.Dynamics = new DynamicsState();
			sun.Motion = null; // il sole resta fermo

			var planet = new TestBody(5.972e24, new Vector3D(r, 0, 0), new Vector3D(0, v, 0), new VelocityVerletMotion(), "Planet");

			return (new List<PhysicsBody> { sun, planet }, planet);
		}

		[TestMethod]
		public void StepDirect_EmptyList_DoesNotThrow()
		{
			new GravitySolver().StepDirect(new List<PhysicsBody>(), new NewtonianGravity(), 1.0);
		}

		[TestMethod]
		public void StepBarnesHut_EmptyList_ReturnsNull()
		{
			Assert.IsNull(new GravitySolver().StepBarnesHut(new List<PhysicsBody>(), 1.0));
		}

		[TestMethod]
		public void StepDirect_MovesBody_AndComputesAcceleration()
		{
			var (bodies, planet) = TwoBodySystem(1.496e11);
			double initialX = planet.WorldMatrix.Translation.X;

			new GravitySolver().StepDirect(bodies, new NewtonianGravity(), 3600.0);

			Assert.AreNotEqual(initialX, planet.WorldMatrix.Translation.X, "il corpo dovrebbe essersi mosso");
			Assert.IsGreaterThan(0, planet.Dynamics.Acceleration.Length, "l'accelerazione gravitazionale dovrebbe essere non nulla");
		}

		[TestMethod]
		public void StepDirect_CircularOrbit_ConservesRadius()
		{
			double r0 = 1.496e11;
			var (bodies, planet) = TwoBodySystem(r0);
			var solver = new GravitySolver();
			var field = new NewtonianGravity();

			for (int i = 0; i < 200; i++)
				solver.StepDirect(bodies, field, 3600.0);

			double rFinal = planet.WorldMatrix.Translation.Length;
			double relError = Math.Abs(rFinal - r0) / r0;
			Assert.IsLessThan(0.05, relError, $"raggio orbitale non conservato (errore {relError:P2})");
		}

		[TestMethod]
		public void StepBarnesHut_ReturnsOctreeConsistentWithUpdatedPositions()
		{
			var (bodies, planet) = TwoBodySystem(1.496e11);

			GravityOctree<PhysicsBody> octree = new GravitySolver().StepBarnesHut(bodies, 3600.0);

			Assert.IsNotNull(octree);
			// L'Octree restituito deve "vedere" il pianeta nella sua nuova posizione.
			var detected = octree.QueryRange((Point3D)planet.WorldMatrix.Translation, 1.0);
			Assert.IsTrue(detected.Any(b => b.Name == "Planet"),
				"l'Octree deve essere coerente con le posizioni aggiornate");
		}

		[TestMethod]
		public void StepBarnesHut_CircularOrbit_ConservesRadius()
		{
			double r0 = 1.496e11;
			var (bodies, planet) = TwoBodySystem(r0);
			var solver = new GravitySolver();

			for (int i = 0; i < 200; i++)
				solver.StepBarnesHut(bodies, 3600.0);

			double rFinal = planet.WorldMatrix.Translation.Length;
			double relError = Math.Abs(rFinal - r0) / r0;
			Assert.IsLessThan(0.05, relError, $"raggio orbitale non conservato (Barnes-Hut, errore {relError:P2})");
		}
	}
}
