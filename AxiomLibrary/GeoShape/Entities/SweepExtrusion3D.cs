using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Axiom.GeoShape.Entities
{
	/// <summary>
	/// Estrusione sweep 3D.
	/// Rappresenta un'estrusione nello spazio lungo un percorso qualsiasi
	/// E' definito con:
	/// - uno Shape2D che rappresenta la sezione
	/// - il percorso di estrusione (Figure3D)
	/// - una lista di tagli iniziali
	/// - una lista di tagli finali
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity3D)
	/// </summary>
	public class SweepExtrusion3D : Entity3D
	{
		#region Properties

		/// <summary>
		/// Rappresenta la figura piana da estrudere
		/// </summary>
		public Shape2D Shape { get; set; }

		/// <summary>
		/// Percorso di estrusione (dovrà essere continuo). 
		/// Il profilo, durante l'estrusione, sarà sempre perpendicolare al percorso.
		/// </summary>
		public Figure3D ExtrusionPath { get; set; }

		/// <summary>
		/// Lista di tagli vicino allo zero dell'estruso
		/// </summary>
		public List<Plane3D> StartCuts { get; set; }

		/// <summary>
		/// Lista di tagli vicino alla fine dell'estruso
		/// </summary>
		public List<Plane3D> EndCuts { get; set; }

		#endregion

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public SweepExtrusion3D()
			: base()
		{
			Shape = new Shape2DCustom();
			ExtrusionPath = new();
			StartCuts = [];
			EndCuts = [];
		}

		/// <summary>
		/// Costruttore. 
		/// Le due liste di tagli possono essere null.
		/// </summary>
		/// <param name="profile"></param>
		/// <param name="extrusionPath"></param>
		/// <param name="startCuts"></param>
		/// <param name="endCuts"></param>
		public SweepExtrusion3D(Shape2D profile, Figure3D extrusionPath, List<Plane3D> startCuts, List<Plane3D> endCuts)
			: base()
		{
			Shape = profile.CloneShape();
			ExtrusionPath = extrusionPath.Clone();
			StartCuts = [];
			EndCuts = [];
			if (startCuts != null)
				StartCuts = startCuts;

			if (endCuts != null)
				EndCuts = endCuts;
		}

		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Clona lo SweepExtrusion3D
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			SweepExtrusion3D result = new SweepExtrusion3D(Shape.CloneShape(), ExtrusionPath, new List<Plane3D>(StartCuts), new List<Plane3D>(EndCuts));
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente. 
		/// N.B. Calcolo molto approssimato: da rifare
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			// TODO
			Figure3D profile1 = Shape.GetFigure().Clone();
			profile1.Move((Vector3D)ExtrusionPath.StartPoint);
			Figure3D profile2 = profile1.Clone();
			profile2.Move((Vector3D)ExtrusionPath.EndPoint);
			Figure3D all = new Figure3D();
			all.AddFigure(profile1);
			all.AddFigure(profile2);
			all.AddFigure(ExtrusionPath.Clone());

			RTMatrix matrix = new(WorldMatrix);
			all.ApplyRT(matrix);
			AABBox3D result = all.GetABBox();

			return result;
		}
		#endregion 
	}
}
