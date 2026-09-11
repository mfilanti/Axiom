using Axiom.GeoShape;

namespace Axiom.Physics
{
	/// <summary>
	/// Strategia di integrazione del moto di un nodo dello scene-graph.
	/// </summary>
	public interface IMotionModel
	{
		void Integrate(Node3D node, DynamicsState state, double deltaTime);
	}
}
