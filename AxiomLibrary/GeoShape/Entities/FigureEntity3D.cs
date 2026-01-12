using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Entities
{
	/// <summary>
	/// Rappresenta una entità 3D che ha internamente una Figure3D. 
	/// Eredita dalla Entity3D l'Id la RTMatrix, il colore, ecc...
	/// </summary>
	public class FigureEntity3D : Entity3D
	{
		#region Properties
		/// <summary>
		/// Figure3D contenuta internamente
		/// </summary>
		public Figure3D Figure { get; set; }


		#endregion PUBLIC FIELDS

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public FigureEntity3D()
			: base()
		{
			Figure = new Figure3D();
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="figure"></param>
		/// <param name="lY"></param>
		/// <param name="lZ"></param>
		public FigureEntity3D(Figure3D figure)
			: base()
		{
			Figure = figure.Clone();
		}

		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Clona il OBBox3D
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			FigureEntity3D result = new FigureEntity3D(Figure.Clone());
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			Figure3D clone = Figure.Clone();
			clone.ApplyRT(WorldMatrix);

			AABBox3D result = clone.GetABBox();
			return result;
		}

		#endregion PUBLIC METHODS

	}
}
