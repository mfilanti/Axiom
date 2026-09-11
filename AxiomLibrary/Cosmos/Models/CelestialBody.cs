using Axiom.GeoMath;

namespace Axiom.Cosmos.Models
{
	public abstract class CelestialBody : PhysicsBody
	{
		#region Properties
		/// <summary>
		/// Raggio del corpo celeste in metri
		/// </summary>
		public double Radius { get; set; }
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore del corpo celeste
		/// </summary>
		/// <param name="name"></param>
		/// <param name="mass"></param>
		/// <param name="radius"></param>
		/// <param name="position"></param>
		protected CelestialBody(string name, double mass, double radius, Vector3D position) : base()
		{
			Name = name;
			Mass = mass;
			Radius = radius;
			X = position.X;
			Y = position.Y;
			Z = position.Z;
		}
		#endregion

		#region Methods
		public void Step(double dt)
		{
			Motion?.Integrate(this, Dynamics, dt);

			// 2. Propago lo step ai figli
			foreach (var node in Nodes.Values)
			{
				if (node is CelestialBody body)
					body.Step(dt);
			}
		}
		public abstract void DisplayInfo();
		#endregion
	}
}
