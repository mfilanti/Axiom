using Axiom.GeoMath;

namespace Axiom.Physics
{
	public sealed class DynamicsState
	{
		#region Properties

		/// <summary>
		/// Velocità del corpo in m/s
		/// </summary>
		public Vector3D Velocity { get; set; }

		/// <summary>
		/// Accelerazione del corpo in m/s²
		/// </summary>
		public Vector3D Acceleration { get; set; }

		#endregion

		#region Constructors
		public DynamicsState()
		{
			Velocity = Vector3D.Zero;
			Acceleration = Vector3D.Zero;
		}
		#endregion
	}
}
