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
	/// Cilindro 3D.
	/// Rappresenta un cilindro nello spazio.
	/// E' definito con:
	/// - il raggio
	/// - l'altezza (in direzione Z) 
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity3D)
	/// Se la matrice è un'identità allora lo zero è al centro 
	/// del cilindro e in basso
	/// </summary>
	public class Cylinder3D : Entity3D
	{
		#region Constants
		/// <summary>
		/// Raggio
		/// </summary>
		private const string RADIUS = "radius";
		/// <summary>
		/// Altezza
		/// </summary>
		private const string HEIGHT = "height";
		#endregion
		#region Fields
		#endregion
		#region Properties
		/// <summary>
		/// Raggio
		/// </summary>
		public double Radius { get => _parameters[RADIUS].Value; set => _parameters[RADIUS].Value = value; }

		/// <summary>
		/// Altezza (in Z)
		/// </summary>
		public double Height { get => _parameters[HEIGHT].Value; set => _parameters[HEIGHT].Value = value; }

		/// <summary>
		/// Formula raggio
		/// </summary>
		public string RadiusFormula { get => _parameters[RADIUS].Formula; set => _parameters[RADIUS].Formula = value; }

		/// <summary>
		/// Formula altezza
		/// </summary>
		public string HeightFormula { get => _parameters[HEIGHT].Formula; set => _parameters[HEIGHT].Formula = value; }
		#endregion

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Cylinder3D()
			: this(0, 0, "", "")
		{
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="height"></param>
		public Cylinder3D(double radius, double height)
			: this(radius, height, "", "")
		{
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="height"></param>
		/// <param name="radiusFormula"></param>
		/// <param name="heightFormula"></param>
		public Cylinder3D(double radius, double height, string radiusFormula, string heightFormula)
			: base()
		{
			_parameters.Clear();
			_parameters.Add("radius", new Parameter("radius", true, radiusFormula, radius));
			_parameters.Add("height", new Parameter("height", true, heightFormula, height));
		}
		#endregion

		#region Methods
		/// <summary>
		/// Clona il cilindro
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			Cylinder3D result = new Cylinder3D(Radius, Height, RadiusFormula, HeightFormula);
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			Figure3D profile = new Figure3D()
			{
				new Arc3D(Point3D.Zero, Radius, 0, Math.PI, true, RTMatrix.Identity),
				new Arc3D(Point3D.Zero, Radius, Math.PI, 0, true, RTMatrix.Identity),
			};
			Figure3D profile2 = profile.Clone();
			profile2.Move(Height * Vector3D.UnitZ);
			RTMatrix matrix = ParentRTMatrix.Multiply(RTMatrix);
			profile.ApplyRT(matrix);
			profile2.ApplyRT(matrix);
			AABBox3D result = profile.GetABBox();
			result.Union(profile2.GetABBox());

			return result;
		}


		#endregion

	}
}
