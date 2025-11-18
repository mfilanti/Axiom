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
	/// Entity che rappresenta una figura 2D.
	/// </summary>
	public abstract class Shape2D : ICloneable
	{
		#region fields
		/// <summary>
		/// Lista dei parametri di forma dello Shape2D
		/// </summary>
		private List<Parameter> _geometricParameters = new();

		#endregion
		#region Properties

		/// <summary>
		/// Verso di percorrenza inverso
		/// </summary>
		public bool InverseVersus { get; set; }

		/// <summary>
		/// Indica se specchiare attorno all'asse Y
		/// </summary>
		public bool MirrorY { get; set; }

		#endregion

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default comune alle sottoclassi
		/// </summary>
		public Shape2D()
		{
			InverseVersus = false;
			MirrorY = false;
		}
		#endregion CONSTRUCTORS

		#region PUBLIC PROPERTIES
		/// <summary>
		/// Indica se può essere svuotato
		/// </summary>
		public abstract bool CanEmpty
		{
			get;
		}

		/// <summary>
		/// Parametri geometrici dello shape
		/// </summary>
		public IEnumerable<Parameter> Parameters => _geometricParameters;
		#endregion PUBLIC PROPERTIES

		#region PUBLIC METHODS

		/// <summary>
		/// Restituisce la figura dello shape
		/// </summary>
		/// <returns></returns>
		public abstract Figure3D GetFigure();

		/// <summary>
		/// Valida lo Shape2D
		/// </summary>
		/// <returns></returns>
		public abstract bool Validate();

		/// <summary>
		/// Clona lo Shape2D
		/// </summary>
		/// <param name="entity"></param>
		public void CloneTo(Shape2D shape)
		{
			shape.InverseVersus = InverseVersus;
			shape.MirrorY = MirrorY;
			shape._geometricParameters.Clear();
			foreach (var item in _geometricParameters)
			{
				shape._geometricParameters.Add(item.Clone());
			}
		}

		/// <summary>
		/// Clona lo Shape2D
		/// </summary>
		/// <returns></returns>
		public abstract Shape2D Clone();

		#endregion

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

	}
}
