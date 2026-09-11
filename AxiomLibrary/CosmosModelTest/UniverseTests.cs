using Axiom.Cosmos.Model;
using Axiom.GeoMath;

namespace Axiom.Cosmos.Model.Tests
{
	[TestClass]
	public class UniverseTests
	{
		[TestMethod]
		public void NewUniverse_HasNameAndEmptyCollections()
		{
			var u = new Universe("Deep Space");
			Assert.AreEqual("Deep Space", u.Name);
			Assert.IsNotNull(u.Galaxies);
			Assert.IsEmpty(u.Galaxies);
			Assert.IsNotNull(u.IntergalacticBodies);
			Assert.IsEmpty(u.IntergalacticBodies);
		}

		[TestMethod]
		public void Step_AdvancesEachGalaxy()
		{
			var galaxy = ModelSystems.SunAndPlanet(out _, out var planet);
			var u = new Universe("U");
			u.Galaxies.Add(galaxy);

			double x0 = planet.WorldMatrix.Translation.X;
			u.Step(3600.0);

			Assert.AreNotEqual(x0, planet.WorldMatrix.Translation.X,
				"lo Step dell'universo deve propagarsi alle galassie contenute");
		}
	}
}
