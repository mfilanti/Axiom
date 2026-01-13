using Axiom.Cosmos.Models;
using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Axiom.Cosmos.Dynamics
{
	public interface IGravityField
	{
		Vector3D ComputeForce(CelestialBody body);

		Vector3D ComputeAcceleration(
		CelestialBody target,
		IReadOnlyCollection<CelestialBody> allBodies);
	}
}
