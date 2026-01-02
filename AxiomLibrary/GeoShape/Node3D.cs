using Axiom.GeoMath;
using Axiom.GeoShape.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Axiom.GeoShape
{
	/// <summary>
	/// Elemento base di un albero di entità 3D. 
	/// Può contenere direttamente delle Entity3D o altri Node3D. 
	/// RTMatrix è la matrice locale di rototraslazione, che indica la RT rispetto al nodo padre. 
	/// Ogni entità o nodo figlio è associato ad una chiave univoca per dare la possibilità 
	/// di ricerca successiva (la chiave è univoca sullo stesso livello).
	/// </summary>
	[Serializable]
	public class Node3D
	{
		#region Properties
		/// <summary>
		/// Identificativo nodo. 
		/// Sul set c'è RICORSIONE per aggiornare tutti i Path dei figli.
		/// </summary>
		public string Id
		{
			get { return _Id; }
			set
			{
				_Id = value;
				Path = _Path;
			}
		}
		private string _Id;

		/// <summary>
		/// Identifica la posizione del nodo all'interno dell'albero. 
		/// Sul set c'è RICORSIONE.
		/// </summary>
		public string Path
		{
			get
			{
				return _Path;
			}

			set
			{
				_Path = value;
				foreach (KeyValuePair<string, Node3D> kvp in Nodes)
					kvp.Value.Path = PathId;

				foreach (KeyValuePair<string, Entity3D> kvp in Entities)
					kvp.Value.Path = PathId;
			}
		}
		private string _Path;

		/// <summary>
		/// Insieme di entità figlie del nodo
		/// </summary>
		public Dictionary<string, Entity3D> Entities { get; set; } = new();

		/// <summary>
		/// Insieme di nodi figli del nodo
		/// </summary>
		public Dictionary<string, Node3D> Nodes { get; set; } = new();

		/// <summary>
		/// Matrice che identifica la posizione assoulta del padre. 
		/// Sul set c'è RICORSIONE (normalmente il set non è da utilizzare direttamente).
		/// </summary>
		public RTMatrix ParentRTMatrix
		{
			get { return _ParentRTMatrix; }
			set
			{
				_ParentRTMatrix = value;
				RTMatrix absoluteMatrix = _ParentRTMatrix.Multiply(_rtMatrix);
				foreach (KeyValuePair<string, Node3D> kvp in Nodes)
					kvp.Value.ParentRTMatrix = absoluteMatrix;

				foreach (KeyValuePair<string, Entity3D> kvp in Entities)
					kvp.Value.ParentRTMatrix = absoluteMatrix;
			}
		}
		private RTMatrix _ParentRTMatrix;

		/// <summary>
		/// Matrice che identifica la posizione relativa al padre. 
		/// Sul set c'è RICORSIONE.
		/// </summary>
		public RTMatrix RTMatrix
		{
			get { return _rtMatrix; }
			set
			{
				_rtMatrix = value;
				if (Node3D.DoRTRecursion)
				{
					RTMatrix absoluteMatrix = _ParentRTMatrix.Multiply(_rtMatrix);
					foreach (KeyValuePair<string, Node3D> kvp in Nodes)
						kvp.Value.ParentRTMatrix = absoluteMatrix;

					foreach (KeyValuePair<string, Entity3D> kvp in Entities)
						kvp.Value.ParentRTMatrix = absoluteMatrix;
				}
			}
		}
		private RTMatrix _rtMatrix;

		/// <summary>
		/// Usato internamente per mantenere coerenza sui metodi SetRotation e GetRotation. 
		/// Infatti la rotazione viene convertita sempre in matrice, ma quando da matrice si torna 
		/// ai tre angoli, ci sono 2 soluzioni. 
		/// Questo booleano serve per scegliere la soluzione coerente. 
		/// Non va usato direttamente. 
		/// </summary>
		public bool RotationYSimmetricRange;

		

		/// <summary>
		/// Formula della posizione X
		/// </summary>
		public string XFormula;

		/// <summary>
		/// Formula della posizione Y
		/// </summary>
		public string YFormula;

		/// <summary>
		/// Formula della posizione Z
		/// </summary>
		public string ZFormula;

		/// <summary>
		/// Formula della rotazione attorno a X (intesa in gradi)
		/// </summary>
		public string RotXFormula;

		/// <summary>
		/// Formula della rotazione attorno a Y (intesa in gradi)
		/// </summary>
		public string RotYFormula;

		/// <summary>
		/// Formula della rotazione attorno a Z (intesa in gradi)
		/// </summary>
		public string RotZFormula;

		/// <summary>
		/// Dictionary che rappresenta l'elenco delle variabili locali al nodo
		/// </summary>
		public Dictionary<string, Variable> Variables { get; set; } = new();

		/// <summary>
		/// Lista di variabili "secondarie", cioè che hanno una formula e quindi dipendono da altre variabili. 
		/// Qui l'ordine è importante, per questo si usa una lista.
		/// </summary>
		public List<Variable> SecondaryVariables;

		/// <summary>
		/// Indica se è visibile. 
		/// In certe (rare) occasioni, il nodo viene rappresentato graficamente e 
		/// per questo motivo esiste questa proprietà (vedi SceneEditor).
		/// </summary>
		public bool Visible;

		#endregion PUBLIC FIELDS

		#region PUBLIC PROPERTIES
		/// <summary>
		/// Posizione X. 
		/// Sul set c'è ricorsione.
		/// </summary>
		[XmlIgnore()]
		public double X
		{
			get => _rtMatrix[0, 3]; set => _rtMatrix[0, 3] = value;
		}

		/// <summary>
		/// Posizione Y. 
		/// Sul set c'è ricorsione.
		/// </summary>
		[XmlIgnore()]
		public double Y
		{
			get => _rtMatrix[1, 3]; set => _rtMatrix[1, 3] = value;
		}

		/// <summary>
		/// Posizione Z. 
		/// Sul set c'è ricorsione.
		/// </summary>
		[XmlIgnore()]
		public double Z
		{
			get => _rtMatrix[2, 3]; set => _rtMatrix[2, 3] = value;
		}

		/// <summary>
		/// Traslazione. 
		/// Sul set c'è ricorsione.
		/// </summary>
		[XmlIgnore()]
		public Vector3D Translation
		{
			get => _rtMatrix.Translation;
			set => _rtMatrix.Translation = value;
		}

		/// <summary>
		/// Concatenazione Path + "/" + Id (sola lettura);
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
		public IEnumerable<Parameter> ParametersFormula { get; private set; }

		/// <summary>
		/// Matrice che indica la posizione assoluta. 
		/// Retituisce ParentRTMatrix * RTMatrix. (Sola lettura).
		/// </summary>
		public RTMatrix WorldMatrix => ParentRTMatrix.Multiply(RTMatrix);

		#endregion PUBLIC PROPERTIES

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Node3D()
		{
			_Id = "";
			_ParentRTMatrix = RTMatrix.Identity;
			_rtMatrix = RTMatrix.Identity;
			RotationYSimmetricRange = true;
			Entities = new();
			Nodes = new();
			XFormula = "";
			YFormula = "";
			ZFormula = "";
			RotXFormula = "";
			RotYFormula = "";
			RotZFormula = "";
			Variables = new();
			SecondaryVariables = new List<Variable>();
			Visible = true;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		public Node3D(string id) : this()
		{
			_Id = id;
		}
		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Aggiunge un nodo alla lista assegnando correttamente il path e il ParentRTMatrix
		/// </summary>
		/// <param name="node"></param>
		public void AddNode(Node3D node)
		{
			node.Path = PathId;
			node.ParentRTMatrix = _ParentRTMatrix.Multiply(_rtMatrix);
			Nodes.Add(node.Id, node);
		}

		/// <summary>
		/// Aggiunge una entità alla lista assegnando correttamente il path e il ParentRTMatrix
		/// </summary>
		/// <param name="node"></param>
		public void AddEntity(Entity3D entity)
		{
			entity.Path = PathId;
			entity.ParentRTMatrix = _ParentRTMatrix.Multiply(_rtMatrix);
			Entities.Add(entity.Id, entity);
		}


        /// <summary>
        /// Clona il nodo
        /// </summary>
        /// <returns></returns>
        public virtual Node3D Clone()
		{
			Node3D result = new Node3D(_Id);
			CloneTo(result);

			return result;
		}

		/// <summary>
		/// Clona il nodo
		/// </summary>
		/// <returns></returns>
		public void CloneTo(Node3D node)
		{
			node.Id = _Id;
			node.Path = Path;
			node._ParentRTMatrix = _ParentRTMatrix;
			node._rtMatrix = _rtMatrix;
			node.RotationYSimmetricRange = RotationYSimmetricRange;
			node.Entities = new();
			node.Nodes = new();
			foreach (KeyValuePair<string, Entity3D> kvp in Entities)
				node.Entities.Add(kvp.Key, kvp.Value.Clone());

			foreach (KeyValuePair<string, Node3D> kvp in Nodes)
				node.Nodes.Add(kvp.Key, kvp.Value.Clone());

			node.XFormula = XFormula;
			node.YFormula = YFormula;
			node.ZFormula = ZFormula;
			node.RotXFormula = RotXFormula;
			node.RotYFormula = RotYFormula;
			node.RotZFormula = RotZFormula;

			if (Variables != null)
			{
				node.Variables = new();
				foreach (KeyValuePair<string, Variable> pair in Variables)
					node.Variables.Add(pair.Key, pair.Value.Clone());
			}

			if (SecondaryVariables != null)
			{
				node.SecondaryVariables = new List<Variable>();
				foreach (Variable variable in SecondaryVariables)
					node.SecondaryVariables.Add(variable.Clone());
			}

			node.Visible = Visible;


		}

		/// <summary>
		/// Restituisce la rotazione del nodo (angoli di eulero X Y Z)
		/// </summary>
		public void GetRotation(out double xRadAngle, out double yRadAngle, out double zRadAngle)
		{
			_rtMatrix.ToEulerAnglesXYZ(RotationYSimmetricRange, out xRadAngle, out yRadAngle, out zRadAngle);
		}

		/// <summary>
		/// Setta la rotazione mantenendo la traslazione (angoli di eulero X Y Z). 
		/// C'è ricorsione.
		/// </summary>
		/// <param name="xRadAngle"></param>
		/// <param name="yRadAngle"></param>
		/// <param name="zRadAngle"></param>
		public void SetRotation(double xRadAngle, double yRadAngle, double zRadAngle)
		{
			RTMatrix matrix = _rtMatrix;
			RotationYSimmetricRange = yRadAngle <= Math.PI / 2 && yRadAngle >= -Math.PI / 2;
			matrix.SetRotation(xRadAngle, yRadAngle, zRadAngle);
			RTMatrix = matrix;
		}

		/// <summary>
		/// Restituisce il Node3D corrispondente al pathId indicato. 
		/// Path di un Node3D.
		/// </summary>
		/// <param name="nodePath"></param>
		/// <returns></returns>
		public Node3D GetNodeByPathId(string nodePathId)
		{
			Node3D result = null;
			char[] separator = { '/' };
			string[] pathArray = nodePathId.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (pathArray.Length > 1)
			{
				result = this;
				for (int i = 1; i < pathArray.Length; i++)
				{
					string pathItem = pathArray[i];
					if (result.Nodes.ContainsKey(pathItem))
					{
						result = result.Nodes[pathItem];
					}
					else
					{
						result = null;
						break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Restituisce la Entity3D corrispondente al pathId indicato. 
		/// Path di una Entity3D.
		/// </summary>
		/// <param name="entityPath"></param>
		/// <returns></returns>
		public Entity3D GetEntityByPathId(string entityPathId)
		{
			Entity3D result = null;
			bool error = false;
			char[] separator = { '/' };
			string[] pathArray = entityPathId.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (pathArray.Length > 1)
			{
				Node3D node = this;
				for (int i = 1; i < pathArray.Length - 1; i++)
				{
					string pathItem = pathArray[i];
					if (node.Nodes.ContainsKey(pathItem))
					{
						node = node.Nodes[pathItem];
					}
					else
					{
						error = true;
						break;
					}
				}
				if (!error && node.Entities.ContainsKey(pathArray[pathArray.Length - 1]))
					result = node.Entities[pathArray[pathArray.Length - 1]];
			}
			return result;
		}

		/// <summary>
		/// Restituisce il Node3D corrispondente al padre del pathId indicato. 
		/// Path di un Node3D o di una Entity3D.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public Node3D GetParentNodeByPathId(string pathId)
		{
			Node3D result = null;
			char[] separator = { '/' };
			string[] pathArray = pathId.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (pathArray.Length > 1)
			{
				result = this;
				for (int i = 1; i < pathArray.Length - 1; i++)
				{
					string pathItem = pathArray[i];
					if (result.Nodes.ContainsKey(pathItem))
					{
						result = result.Nodes[pathItem];
					}
					else
					{
						result = null;
						break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Effettua l'update di tutti i valori, valutando le eventuali formule del nodo e delle 
		/// Entity3D associate. 
		/// Viene chiamato l'update ricorsivamente di tutti i sottonodi. 
		/// Restituisce true o false indicando se ci sono stati errori (true: ok, false: errori)
		/// </summary>
		/// <param name="variables">Variabili del padre</param>
		/// <param name="secondaryVariables">Variabili secondarie del padre</param>
		/// <param name="errorDescription">Restituisce un messaggio con la descrizione dell'eventuale errore</param>
		/// <returns>Indica se ci sono stati errori (true: ok, false: errori)</returns>
		public virtual bool Update(Dictionary<string, Variable> variables, out string errorDescription)
		{
			bool result = true;
			errorDescription = "Node " + Id + "\n";
			if (Delegates.DelegateEvaluator != null)
			{
				string localErrorDescription;
				Dictionary<string, Variable> allVariables = new Dictionary<string, Variable>();
				if (variables != null)
				{
					foreach (KeyValuePair<string, Variable> kvp in variables)
						allVariables.Add(kvp.Key, kvp.Value);
				}
				foreach (KeyValuePair<string, Variable> kvp in Variables)
				{
					// Aggiorno anche le variabili con formule
					if (kvp.Value.Formula != null && kvp.Value.Formula.Length > 0)
					{
						double value = Delegates.DelegateEvaluator(allVariables, kvp.Value.Formula, out localErrorDescription);
						if (value.CompareTo(double.NaN) == 0)
						{
							result = false;
							errorDescription += localErrorDescription + "\n";
						}
						else
						{
							kvp.Value.Value = value;
						}
					}

					if (allVariables.ContainsKey(kvp.Key) == false)
						allVariables.Add(kvp.Key, kvp.Value);
				}

				#region X, Y, Z
				if (XFormula.Length > 0)
				{
					double value = Delegates.DelegateEvaluator(allVariables, XFormula, out localErrorDescription);
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
					double value = Delegates.DelegateEvaluator(allVariables, YFormula, out localErrorDescription);
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
					double value = Delegates.DelegateEvaluator(allVariables, ZFormula, out localErrorDescription);
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
					double value = Delegates.DelegateEvaluator(allVariables, RotXFormula, out localErrorDescription);
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
					double value = Delegates.DelegateEvaluator(allVariables, RotYFormula, out localErrorDescription);
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
					double value = Delegates.DelegateEvaluator(allVariables, RotZFormula, out localErrorDescription);
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
				List<Parameter> parameters = ParametersFormula.ToList();
				for (int i = 0; i < parameters.Count; i++)
				{
					Parameter parameter = parameters[i];
					if (parameter.Formula != null && parameter.Formula.Length > 0)
					{
						double value = Delegates.DelegateEvaluator(allVariables, parameter.Formula, out localErrorDescription);
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
				// Faccio l'update di tutte le Entity3D
				foreach (Entity3D entity in Entities.Values)
				{
					bool localResult = entity.Update(allVariables, out localErrorDescription);
					if (localResult == false)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
				}
				if (result)
					errorDescription = "";

				// Ora faccio l'update di tutti i sotto nodi
				foreach (Node3D node in Nodes.Values)
				{
					bool localResult = node.Update(allVariables, out localErrorDescription);
					if (localResult == false)
					{
						result = false;
						errorDescription += localErrorDescription + "\n";
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Retituisce tutti i Node3D del nodo e di tutti i sottonodi. 
		/// Effettua una ricorsione.
		/// </summary>
		/// <returns></returns>
		public List<Node3D> GetSubNodes()
		{
			List<Node3D> result = new List<Node3D>();
			updateSubNodes(this, ref result);

			return result;
		}

		private void updateSubNodes(Node3D node, ref List<Node3D> nodes)
		{
			nodes.AddRange(node.Nodes.Values);
			foreach (Node3D sibling in node.Nodes.Values)
				updateSubNodes(sibling, ref nodes);
		}

		/// <summary>
		/// Retituisce tutte le Entity3D del nodo e di tutti i sottonodi. 
		/// Effettua una ricorsione.
		/// </summary>
		/// <returns></returns>
		public List<Entity3D> GetSubEntities()
		{
			List<Entity3D> result = new List<Entity3D>();
			updateSubEntities(this, ref result);

			return result;
		}

		private void updateSubEntities(Node3D node, ref List<Entity3D> entities)
		{
			entities.AddRange(node.Entities.Values);
			foreach (Node3D sibling in node.Nodes.Values)
				updateSubEntities(sibling, ref entities);
		}

		#endregion PUBLIC METHODS

		#region STATICS
		/// <summary>
		/// Indica se effettuare la ricorsione durante ogni aggiornamento della RTMatrix. 
		/// E di tutte le proprietà che indirettamente lo fanno (X, Y, Z, ...). 
		/// Di default è true, portarlo a false solo nei casi in cui vanno fatti molti aggiornamenti RT 
		/// e si desidera effettuare l'update di tutti i figli una sola volta alla fine.
		/// </summary>
		public static bool DoRTRecursion = true;

		/// <summary>
		/// Fa una copia dell'oggetto.
		/// Funziona solo per ValueType o ICloneable. 
		/// Aggiunta gestione anche per List<string>, List<int>, List<double>.
		/// </summary>
		/// <param name="toClone"></param>
		/// <returns></returns>
		public static object MakeCopyOf(object toClone)
		{
			if (toClone is ICloneable)
			{
				// Restituisce una deep copy dell'oggetto
				return ((ICloneable)toClone).Clone();
			}
			else if (toClone is ValueType)
			{
				// Restituisce una shallow copy del ValueType
				return ((ValueType)toClone);
			}
			else if (toClone is List<string>)
			{
				// Restituisce una nuova lista, glie elementi vengono copiati essendo ValueType
				// La string in realtà non lo è ma si comporta come ValueType
				return new List<string>((List<string>)toClone);
			}
			else if (toClone is List<int>)
			{
				// Restituisce una nuova lista, glie elementi vengono copiati essendo ValueType
				return new List<int>((List<int>)toClone);
			}
			else if (toClone is List<double>)
			{
				// Restituisce una nuova lista, glie elementi vengono copiati essendo ValueType
				return new List<double>((List<double>)toClone);
			}
			else if (toClone == null)
			{
				return null;
			}
			else
			{

				// Senza ricorrere a reflection o serializzazione, non possiamo fare un clone
				// dell'oggetto, e quindi restituiamo un'eccezione
				throw new System.NotSupportedException("object not cloneable");
			}
		}
		#endregion STATICS

	}
}
