using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Dynamics
{
	public sealed class DynamicsState
	{
		#region Fields

		#endregion

		#region Properties

		/// <summary>
		/// Velocità del corpo celeste in m/s
		/// </summary>
		public Vector3D Velocity { get; set; }

		/// <summary>
		/// Accelerazione del corpo celeste in m/s²
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

		#region Methods

		#endregion
	}
}
