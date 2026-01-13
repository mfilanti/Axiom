using Axiom.GeoShape;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Models
{
	public static class GalaxyExtensions
	{
		public static IEnumerable<CelestialBody> GetAllBodies(this Galaxy galaxy)
		{
			foreach (var star in galaxy.Stars)
			{
				yield return star;

				foreach (var body in GetChildrenRecursive(star))
					yield return body;
			}
		}

		private static IEnumerable<CelestialBody> GetChildrenRecursive(Node3D node)
		{
			foreach (var child in node.Nodes.Values)
			{
				if (child is CelestialBody body)
				{
					yield return body;

					foreach (var sub in GetChildrenRecursive(child))
						yield return sub;
				}
			}
		}
	}
}
