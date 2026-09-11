using Axiom.GeoMath;
using System.Collections.Generic;

namespace Axiom.Physics
{
	/// <summary>
	/// Campo gravitazionale su un insieme di corpi. Definito sull'astrazione <see cref="PhysicsBody"/>,
	/// non su tipi di dominio, così da restare riusabile fuori dal contesto astrofisico.
	/// </summary>
	public interface IGravityField
	{
		Vector3D ComputeForce(PhysicsBody body);

		Vector3D ComputeAcceleration(
			PhysicsBody target,
			IReadOnlyCollection<PhysicsBody> allBodies);
	}
}
