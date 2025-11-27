using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;
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
	public abstract class Shape2D : Entity3D, ICloneable
	{
		#region fields
		/// <summary>
		/// Lista dei parametri di forma dello Shape2D
		/// </summary>
		protected Dictionary<string, Parameter> _geometricParameters = new();
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

		#region Costructor
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
		public IEnumerable<Parameter> Parameters => _geometricParameters.Values;
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
				shape._geometricParameters.Add(item.Key, item.Value.Clone());
			}
		}

		/// <summary>
		/// Clona lo Shape2D
		/// </summary>
		/// <returns></returns>
		public abstract override Shape2D Clone();

		/// <summary>
		/// Box orientato
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			var abox = GetFigure().GetABBox();
			return new AABBox3D(abox.MinPoint, abox.MaxPoint);
		}

		/// <summary>
		/// Controlla se la figura è antioraria. 
		/// Se la figura è aperta la chiude per controllare il verso. 
		/// Se la figura è una linea, allora controlla solo la tangente (true se t.X>0 se circa zero guarda t.Y)
		/// </summary>
		/// <returns></returns>
		public bool IsCounterClockWise()
		{
			bool result = false;
			Figure3D clone = GetFigure();
			clone.Reduce(true, false);
			if (clone.Count == 1 && clone[0] is Line3D)
			{
				Vector3D tangent = clone[0].StartTangent;
				if (tangent.X.IsEquals(0))
					result = tangent.Y > 0;
				else
					result = tangent.X > 0;
			}
			else
			{
				Figure3D figureApprox = clone.ApproxFigure(false, true, true, 2, 2, 2, false, false);
				if (figureApprox.IsClosed() == false)
					figureApprox.Add(new Line3D(figureApprox.EndPoint, figureApprox.StartPoint));

				Polygon3D figurePolygon = figureApprox.ToPolygon();
				result = figurePolygon.IsCounterClockWise();
			}
			return result;
		}
		#endregion

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

	}
}
