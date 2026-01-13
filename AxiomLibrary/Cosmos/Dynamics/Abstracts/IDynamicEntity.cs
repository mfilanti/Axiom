using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Cosmos.Dynamics.Abstracts
{
    public interface IDynamicEntity
    {
        Vector3D AbsolutePosition { get; }
    }
}
