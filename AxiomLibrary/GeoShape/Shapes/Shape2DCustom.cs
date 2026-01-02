using Axiom.GeoShape.Curves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Axiom.GeoShape.Shapes
{
	/// <summary>
	/// Classe shape figura custom (Es. da dxf)
	/// </summary>
	public class Shape2DCustom : Shape2D
	{
		#region Properties
		/// <summary>
		/// Figura dello shape
		/// </summary>
		[DataMember]
		public Figure3D Figure { get; set; }

		#endregion 

		#region Ctor
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Shape2DCustom()
		{
			Figure = new Figure3D();
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="figure">Figura dello shape</param>
		public Shape2DCustom(Figure3D figure)
		{
			Figure = figure.Clone();
		}
		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Indica se può essere svuotato
		/// </summary>
		public override bool CanEmpty
		{
			get
			{
				bool result = true;
				List<Figure3D> loops = Figure.Loops();
				if (loops.Count == 1 && loops[0].IsClosed() == false)
					result = false;

				return result;
			}
		}

		/// <summary>
		/// Restituisce la figura dello shape
		/// </summary>
		/// <returns></returns>
		public override Figure3D GetFigure()
		{
			Figure3D result = Figure.Clone();

			if (InverseVersus)
				result = result.Inverse();

			//if (MirrorY)
			//	result = result.MirrorY();

			return result;
		}

		/// <summary>
		/// Valida lo Shape2D
		/// </summary>
		/// <returns></returns>
		public override bool Validate() => Figure.Count > 0;

		/// <summary>
		/// Clona lo Shape2D
		/// </summary>
		/// <returns></returns>
		public override Shape2D CloneShape()
		{
			Shape2D result = new Shape2DCustom(Figure.Clone());
			CloneTo(result);
			return result;
		}
		#endregion 
	}
}
