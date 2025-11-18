using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
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
	/// Toro 3D.
	/// Rappresenta un toroide nello spazio.
	/// E' definito con:
	/// - il raggio interno (della circonferenza che viene "estrusa")
	/// - il raggio esterno (di estrusione)
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity3D)
	/// Se la matrice è un'identità allora lo zero è al centro del toro. 
	/// </summary>
	public class Torus3D : Entity3D
	{
		private const string INNER_RADIUS = "innerRadius";
		private const string OUTER_RADIUS = "outerRadius";
		#region Properties
		/// <summary>
		/// Raggio interno (della circonferenza che viene "estrusa")
		/// </summary>
		public double InnerRadius { get => _parameters[INNER_RADIUS].Value; set => _parameters[INNER_RADIUS].Value = value; }

		/// <summary>
		/// Raggio esterno (di estrusione)
		/// </summary>
		public double OuterRadius { get => _parameters[OUTER_RADIUS].Value; set => _parameters[OUTER_RADIUS].Value = value; }

		/// <summary>
		/// Formula raggio interno
		/// </summary>
		public string InnerRadiusFormula { get => _parameters[INNER_RADIUS].Formula; set => _parameters[INNER_RADIUS].Formula = value; }

		/// <summary>
		/// Formula raggio esterno
		/// </summary>
		public string OuterRadiusFormula { get => _parameters[OUTER_RADIUS].Formula; set => _parameters[OUTER_RADIUS].Formula = value; }
		#endregion

		#region Ctor
		/// <summary>
		/// Costruttore
		/// </summary>
		public Torus3D()
			: this(0, 0, "", "")
		{
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="innerRadius"></param>
		/// <param name="outerRadius"></param>
		public Torus3D(double innerRadius, double outerRadius)
			: this(innerRadius, outerRadius, "", "")
		{
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="innerRadius"></param>
		/// <param name="outerRadius"></param>
		/// <param name="innerRadiusFormula"></param>
		/// <param name="outerRadiusFormula"></param>
		public Torus3D(double innerRadius, double outerRadius, string innerRadiusFormula, string outerRadiusFormula)
			: base()
		{
			_parameters.Add(INNER_RADIUS, new Parameter(INNER_RADIUS, true, innerRadiusFormula, innerRadius));
			_parameters.Add(OUTER_RADIUS, new Parameter(OUTER_RADIUS, true, outerRadiusFormula, outerRadius));
		}
		#endregion

		#region Methods
		/// <summary>
		/// Clona il cilindro
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			Torus3D result = new Torus3D(InnerRadius, OuterRadius, InnerRadiusFormula, OuterRadiusFormula);
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente. 
		/// N.B. Calcolo approssimato per eccesso: AABBox dell'OBBOx rototraslato.
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			Figure3D section =
			[
				new Arc3D(new Point3D(OuterRadius, 0), InnerRadius, 0, Math.PI, true, RTMatrix.Identity),
				new Arc3D(new Point3D(OuterRadius, 0), InnerRadius, Math.PI, 0, true, RTMatrix.Identity),
				new Arc3D(),
			];
			AABBox3D box2D = section.GetABBox();
			Point3D minPoint = new Point3D(-box2D.MaxPoint.X, -box2D.MaxPoint.X, box2D.MinPoint.Y);
			Point3D maxPoint = new Point3D(box2D.MaxPoint.X, box2D.MaxPoint.X, box2D.MaxPoint.Y);
			AABBox3D abbox = new AABBox3D(minPoint, maxPoint);
			OBBox3D obbox = abbox.GetOBBOX();
			RTMatrix matrix = ParentRTMatrix.Multiply(RTMatrix);
			obbox.RTMatrix = matrix.Multiply(obbox.RTMatrix);
			AABBox3D result = obbox.GetAABBox();
			return result;
		}
		#endregion 
	}
}
