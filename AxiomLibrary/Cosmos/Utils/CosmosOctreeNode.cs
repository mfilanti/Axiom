using Axiom.Cosmos.Model;
using Axiom.GeoMath;
using Axiom.Physics;

namespace Axiom.Cosmos.Utils
{
	/// <summary>
	/// Octree gravitazionale specializzato sui <see cref="CelestialBody"/>. È una sottile
	/// specializzazione del <see cref="GravityOctree{T}"/> generico di <c>Axiom.Physics</c>: eredita
	/// inserimento, query di raggio ("radar") e accelerazione Barnes-Hut, fissando il tipo del corpo.
	/// </summary>
	public sealed class CosmosOctreeNode : GravityOctree<CelestialBody>
	{
		public CosmosOctreeNode(AABBox3D boundary) : base(boundary) { }
	}
}
