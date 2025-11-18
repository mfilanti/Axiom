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
	/// Estrusione 3D.
	/// Rappresenta un'estrusione nello spazio.
	/// E' definito con:
	/// - uno Shape2D che rappresenta il profilo iniziale
	/// - la direzione di estrusione (tipicamente Z)
	/// - la lunghezza
	/// - una lista di tagli iniziali
	/// - una lista di tagli finali
	/// - una matrice che ne determina l'orientamento e la posizione nello spazio (ereditata da Entity3D)
	/// </summary>

	public class Extrusion3D : Entity3D
	{
		#region Const
		private const string LENGHT = "Length";
		#endregion
		#region Properties

		/// <summary>
		/// Rappresenta la figura piana da estrudere
		/// </summary>
		public Shape2D Shape { get; set; }

		/// <summary>
		/// Direzione di estrusione
		/// </summary>
		public Vector3D ExtrusionDirection { get; set; }

		/// <summary>
		/// Lunghezza dell'estruso
		/// </summary>
		public double Length { get => _parameters[LENGHT].Value; set => _parameters[LENGHT].Value = value; }

		/// <summary>
		/// Formula lunghezza
		/// </summary>
		public string LengthFormula { get => _parameters[LENGHT].Formula; set => _parameters[LENGHT].Formula = value; }

		/// <summary>
		/// Lista di tagli vicino allo zero dell'estruso
		/// </summary>
		public List<Plane3D> StartCuts { get; set; }

		/// <summary>
		/// Lista di tagli vicino alla fine dell'estruso
		/// </summary>
		public List<Plane3D> EndCuts { get; set; }

		#endregion PUBLIC FIELDS

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Extrusion3D()
			: base()
		{
			Shape = new Shape2DCustom();
			ExtrusionDirection = Vector3D.UnitZ;
			StartCuts = new List<Plane3D>();
			EndCuts = new List<Plane3D>();
			_parameters.Add(LENGHT, new Parameter(LENGHT, true, "", 0.0));
		}

		/// <summary>
		/// Costruttore. 
		/// Le due liste di tagli possono essere null.
		/// </summary>
		/// <param name="profile"></param>
		/// <param name="extrusionDirection"></param>
		/// <param name="length"></param>
		/// <param name="startCuts"></param>
		/// <param name="endCuts"></param>
		public Extrusion3D(Shape2D profile, Vector3D extrusionDirection, double length, List<Plane3D> startCuts, List<Plane3D> endCuts, string lengthFormula)
			: this()
		{
			Shape = profile.Clone();
			ExtrusionDirection = new(extrusionDirection);
			Length = length;
			LengthFormula = lengthFormula;

			StartCuts = new List<Plane3D>();
			EndCuts = new List<Plane3D>();
			if (startCuts != null) StartCuts = startCuts;
			if (endCuts != null) EndCuts = endCuts;

			foreach (var item in Shape.Parameters)
			{
				_parameters.Add(item.Name, item);
			}
		}


		#endregion

		#region Methods
		/// <summary>
		/// Clona l'Extrusion3D
		/// </summary>
		/// <returns></returns>
		public override Entity3D Clone()
		{
			Extrusion3D result = new Extrusion3D(Shape.Clone(), ExtrusionDirection, Length,
												 [.. StartCuts], [.. EndCuts], LengthFormula);
			CloneTo(result);
			return result;
		}

		/// <summary>
		/// Restituisce l'AABBox corrispondente. 
		/// N.B. Calcolo non esatto: non tiene in considerazione i tagli.
		/// </summary>
		/// <returns></returns>
		public override AABBox3D GetAABBox()
		{
			Figure3D profile1 = Shape.GetFigure();
			Figure3D profile2 = profile1.Clone();
			profile2.Move(Length * ExtrusionDirection);
			RTMatrix matrix = ParentRTMatrix.Multiply(RTMatrix);
			profile1.ApplyRT(matrix);
			profile2.ApplyRT(matrix);
			AABBox3D result = profile1.GetABBox();
			result.Union(profile2.GetABBox());

			return result;
		}
		#endregion PUBLIC METHODS
	}
}
