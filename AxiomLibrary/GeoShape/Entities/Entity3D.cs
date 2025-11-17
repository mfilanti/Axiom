using Axiom.GeoMath;
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
	/// Entità geometrica generica.
	/// </summary>
	public abstract class Entity3D : ICloneable
	{
		#region Fields
		/// <summary>
		/// Parametri specifici della sottoclasse
		/// </summary>
		protected Dictionary<string, Parameter> _parameters = new();

		#endregion
		#region Properties
		/// <summary>
		/// Identificativo entità
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Identifica la posizione del nodo all'interno dell'eventuale albero
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Matrice che identifica la posizione assoulta del padre
		/// </summary>
		public RTMatrix ParentRTMatrix { get; set; }

		/// <summary>
		///  Matrice che identifica la posizione relativa al padre. 
		/// </summary>
		public RTMatrix RTMatrix { get; set; }

		/// <summary>
		/// Usato internamente per mantenere coerenza sui metodi SetRotation e GetRotation. 
		/// Infatti la rotazione viene convertita sempre in matrice, ma quando da matrice si torna 
		/// ai tre angoli, ci sono 2 soluzioni. 
		/// Questo booleano serve per scegliere la soluzione coerente. 
		/// Non va usato direttamente. 
		/// </summary>
		public bool RotationYSimmetricRange { get; set; }

		/// <summary>
		/// Formula posizione X
		/// </summary>
		public string XFormula { get; set; }

		/// <summary>
		/// Formula posizione Y
		/// </summary>
		public string YFormula { get; set; }

		/// <summary>
		/// Formula posizione Z
		/// </summary>
		public string ZFormula { get; set; }

		/// <summary>
		/// Formula della rotazione attorno a X (intesa in gradi)
		/// </summary>
		public string RotXFormula { get; set; }

		/// <summary>
		/// Formula della rotazione attorno a Y (intesa in gradi)
		/// </summary>
		public string RotYFormula { get; set; }

		/// <summary>
		/// Formula della rotazione attorno a Z (intesa in gradi)
		/// </summary>
		public string RotZFormula { get; set; }

		/// <summary>
		/// Posizione X
		/// </summary>
		[XmlIgnore()]
		public double X
		{
			get => RTMatrix[0, 3]; set => RTMatrix[0, 3] = value;
		}

		/// <summary>
		/// Posizione Y
		/// </summary>
		[XmlIgnore()]
		public double Y
		{
			get => RTMatrix[1, 3]; set => RTMatrix[1, 3] = value;
		}

		/// <summary>
		/// Posizione Z
		/// </summary>
		[XmlIgnore()]
		public double Z
		{
			get => RTMatrix[2, 3]; set => RTMatrix[2, 3] = value;
		}

		/// <summary>
		/// Concatenazione Path + "/" + Id;
		/// </summary>
		public string PathId
		{
			get { return Path + "/" + Id; }
		}

		/// <summary>
		/// Lista di parametri che devono essere valutati con eventuali formule. 
		/// Se nel set passo una lista con numero di elementi non corretto, viene restituita una eccezione.
		/// </summary>
		[XmlIgnore()]
		public List<Parameter> ParametersFormula
		{
			get => [.. _parameters.Values];
			set
			{
				if (value.Count != _parameters.Count)
					throw new Exception("Wrong parameters count.");
				for (int i = 0; i < _parameters.Count; i++)
				{
					var key = _parameters.Keys.ElementAt(i);
					_parameters[key].Value = value[i].Value;
					_parameters[key].Formula = value[i].Formula;
				}
			}
		}

		/// <summary>
		/// Matrice che indica la posizione assoluta. 
		/// Retituisce ParentRTMatrix * RTMatrix. (Sola lettura).
		/// </summary>
		public RTMatrix WorldMatrix => ParentRTMatrix.Multiply(RTMatrix);

		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore di default comune alle sottoclassi
		/// </summary>
		public Entity3D()
		{
			Id = "";
			Path = "";
			ParentRTMatrix = RTMatrix.Identity;
			RTMatrix = RTMatrix.Identity;
			RotationYSimmetricRange = true;
			XFormula = "";
			YFormula = "";
			ZFormula = "";
			RotXFormula = "";
			RotYFormula = "";
			RotZFormula = "";
		}
		#endregion


		#region PUBLIC METHODS
		/// <summary>
		/// Clona l'entità
		/// </summary>
		/// <param name="entity"></param>
		public void CloneTo(Entity3D entity)
		{
			entity.Id = Id;
			entity.Path = Path;
			entity.ParentRTMatrix = ParentRTMatrix;
			entity.RTMatrix = RTMatrix;
			entity.RotationYSimmetricRange = RotationYSimmetricRange;
			entity.XFormula = XFormula;
			entity.YFormula = YFormula;
			entity.ZFormula = ZFormula;
			entity.RotXFormula = RotXFormula;
			entity.RotYFormula = RotYFormula;
			entity.RotZFormula = RotZFormula;
		}

		/// <summary>
		/// Clona l'entità
		/// </summary>
		/// <returns></returns>
		public abstract Entity3D Clone();

		/// <summary>
		/// Restituisce l'AABBox corrispondente
		/// </summary>
		/// <returns></returns>
		public abstract AABBox3D GetAABBox();

		/// <summary>
		/// Effettua l'update di tutti i valori, valutando le eventuali formule. 
		/// Restituisce true o false indicando se ci sono stati errori (true: ok, false: errori)
		/// </summary>
		/// <param name="variables">Variabili del padre</param>
		/// <param name="errorDescription">Restituisce un messaggio con la descrizione dell'eventuale errore</param>
		/// <returns>Indica se ci sono stati errori (true: ok, false: errori)</returns>
		public virtual bool Update(Dictionary<string, Variable> variables, out string errorDescription)
		{
			bool result = true;
			string id = Path.Remove(0, Path.LastIndexOf("/") + 1) + "/" + Id;
			errorDescription = "Entity " + id + "\n";
			if (Delegates.DelegateEvaluator != null)
			{
				string localErrorDescription;
				#region X, Y, Z
				if (XFormula.Length > 0)
				{
					double value = Delegates.DelegateEvaluator(variables, XFormula, out localErrorDescription);
					if (value.CompareTo(double.NaN) == 0)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
					else
						X = value;
				}
				if (YFormula.Length > 0)
				{
					double value = Delegates.DelegateEvaluator(variables, YFormula, out localErrorDescription);
					if (value.CompareTo(double.NaN) == 0)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
					else
						Y = value;
				}
				if (ZFormula.Length > 0)
				{
					double value = Delegates.DelegateEvaluator(variables, ZFormula, out localErrorDescription);
					if (value.CompareTo(double.NaN) == 0)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
					else
						Z = value;
				}
				#endregion X, Y, Z

				#region RotX, RotY, RotZ
				double xRadAngle, yRadAngle, zRadAngle;
				GetRotation(out xRadAngle, out yRadAngle, out zRadAngle);
				double xDegAngle = MathUtils.RadToDeg * xRadAngle;
				double yDegAngle = MathUtils.RadToDeg * yRadAngle;
				double zDegAngle = MathUtils.RadToDeg * zRadAngle;

				if (RotXFormula.Length > 0)
				{
					double value = Delegates.DelegateEvaluator(variables, RotXFormula, out localErrorDescription);
					if (value.CompareTo(double.NaN) == 0)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
					else
						xDegAngle = value;
				}
				if (RotYFormula.Length > 0)
				{
					double value = Delegates.DelegateEvaluator(variables, RotYFormula, out localErrorDescription);
					if (value.CompareTo(double.NaN) == 0)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
					else
						yDegAngle = value;
				}
				if (RotZFormula.Length > 0)
				{
					double value = Delegates.DelegateEvaluator(variables, RotZFormula, out localErrorDescription);
					if (value.CompareTo(double.NaN) == 0)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
					else
						zDegAngle = value;
				}
				xRadAngle = MathUtils.DegToRad * xDegAngle;
				yRadAngle = MathUtils.DegToRad * yDegAngle;
				zRadAngle = MathUtils.DegToRad * zDegAngle;
				SetRotation(xRadAngle, yRadAngle, zRadAngle);
				#endregion RotX, RotY, RotZ

				// Valuto anche ogni parametro specifico della sottoclasse
				List<Parameter> parameters = ParametersFormula;
				for (int i = 0; i < parameters.Count; i++)
				{
					Parameter parameter = parameters[i];
					if (parameter.Formula != null && parameter.Formula.Length > 0)
					{
						double value = Delegates.DelegateEvaluator(variables, parameter.Formula, out localErrorDescription);
						if (value.CompareTo(double.NaN) == 0)
						{
							result = false;
							errorDescription += localErrorDescription + "\n";
						}
						else
						{
							parameter.Value = value;
							parameters[i] = parameter;
						}
					}
					ParametersFormula = parameters;
				}
			}
			if (result)
				errorDescription = "";

			return result;
		}

		/// <summary>
		/// Restituisce la rotazione dell'entità (angoli di eulero X Y Z)
		/// </summary>
		public void GetRotation(out double xRadAngle, out double yRadAngle, out double zRadAngle) => RTMatrix.ToEulerAnglesXYZ(RotationYSimmetricRange, out xRadAngle, out yRadAngle, out zRadAngle);

		/// <summary>
		/// Setta la rotazione mantenendo la traslazione (angoli di eulero X Y Z). 
		/// </summary>
		/// <param name="xRadAngle"></param>
		/// <param name="yRadAngle"></param>
		/// <param name="zRadAngle"></param>
		public void SetRotation(double xRadAngle, double yRadAngle, double zRadAngle)
		{
			RotationYSimmetricRange = yRadAngle <= Math.PI / 2 && yRadAngle >= -Math.PI / 2;
			RTMatrix.SetRotation(xRadAngle, yRadAngle, zRadAngle);
		}

		#endregion PUBLIC METHODS

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
