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
	/// Faccia piana 3D.
	/// Rappresenta una faccia piana nello spazio.
	/// E' definita con:
	/// - uno Shape2D che rappresenta il profilo
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity3D)
	/// </summary>
	public class PlanarFace3D : Entity3D
	{
		#region PUBLIC FIELDS
		/// <summary>
		/// Rappresenta la figura del piano
		/// </summary>
		public Shape2D Shape { get; set; }

		/// <summary>
		/// Texture che verrà disegnata dentro lo Shape. 
		/// Es. Avendo un Bitmap Gdi:
		/// MemoryStream ms = new MemoryStream();
		/// Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
		/// return ms.ToArray();
		/// </summary>
		public byte[] Texture { get; set; }

		#endregion PUBLIC FIELDS

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public PlanarFace3D()
			: base()
		{
			Shape = new Shape2DCustom();
			Texture = null;
		}

		/// <summary>
		/// Costruttore. 
		/// </summary>
		/// <param name="profile"></param>
		public PlanarFace3D(Shape2D customProfile)
			: base()
		{
			Shape = customProfile.Clone();
			Texture = null;
		}

		
		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Clona l'Extrusion3D
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			PlanarFace3D result = new PlanarFace3D(Shape.Clone());
			if (Texture != null)
				result.Texture = new List<byte>(Texture).ToArray();

			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente. 
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			Figure3D profile = Shape.GetFigure();
			RTMatrix matrix = ParentRTMatrix.Multiply(RTMatrix);
			profile.ApplyRT(matrix);
			AABBox3D result = profile.GetABBox();

			return result;
		}

		#endregion 

	}
}
