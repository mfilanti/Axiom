using Axiom.GeoShape;
using System.Collections.Generic;

namespace Axiom.Cosmos.Model
{
	public static class GalaxyExtensions
	{
		public static IEnumerable<CelestialBody> GetAllBodies(this Galaxy galaxy)
		{
			foreach (var node in galaxy.Nodes.Values)
			{
				if (node is CelestialBody celestialBody) yield return celestialBody;

				foreach (var body in GetChildrenRecursive(node))
					yield return body;
			}
		}

		private static IEnumerable<CelestialBody> GetChildrenRecursive(Node3D node)
		{
			foreach (var child in node.Nodes.Values)
			{
				if (child is CelestialBody body) yield return body;

				foreach (var sub in GetChildrenRecursive(child))
					yield return sub;
			}
		}
	}
}
