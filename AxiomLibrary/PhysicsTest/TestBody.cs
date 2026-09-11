using Axiom.GeoMath;
using Axiom.Physics;

namespace Axiom.Physics.Tests
{
	/// <summary>
	/// Corpo concreto minimale usato per collaudare il motore in isolamento, senza dipendere dai tipi
	/// di dominio (CelestialBody/Starship). Rappresenta un punto materiale con massa, posizione e
	/// (opzionalmente) stato dinamico + modello di moto.
	/// </summary>
	internal sealed class TestBody : PhysicsBody
	{
		public TestBody(double mass, Vector3D position, string name = "body")
		{
			Name = name;
			Mass = mass;
			X = position.X;
			Y = position.Y;
			Z = position.Z;
		}

		public TestBody(double mass, Vector3D position, Vector3D velocity, IMotionModel motion, string name = "body")
			: this(mass, position, name)
		{
			Dynamics = new DynamicsState { Velocity = velocity };
			Motion = motion;
		}
	}
}
