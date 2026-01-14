using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Simulation
{
	public interface IInputProvider
	{
		double Pitch { get; }
		double Yaw { get; }
		double Roll { get; }
		double ThrottleDelta { get; }
	}
}
