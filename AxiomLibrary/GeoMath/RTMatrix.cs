using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Axiom.GeoMath
{

	/// <summary>
	/// Classe matrice di roto-traslazione 4x4
	/// </summary>
	[DataContract]
	public class RTMatrix
	{
		#region STATICS
		/// <summary>
		/// Matrice identità
		/// </summary>
		public static RTMatrix Identity
		{
			get { return new RTMatrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1); }
		}

		/// <summary>
		/// Matrice nulla
		/// </summary>
		public static RTMatrix Zero
		{
			get { return new RTMatrix(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0); }
		}

		/// <summary>
		/// Dati tre angoli di rotazione rispetto ai tre assi cartesiani. 
		/// Composizione rotazione Z * Y * X (convenzionale in robotica)
		/// Vengono chiamati anche angoli RPY: roll (x), pitch (y), yaw (z).
		/// </summary>
		/// <param name="xRadAngle">Angolo in radianti (Roll)</param>
		/// <param name="yRadAngle"></param>
		/// <param name="zRadAngle"></param>
		/// <returns></returns>
		public static RTMatrix FromEulerAnglesXYZ(double xRadAngle, double yRadAngle, double zRadAngle)
		{

			// Calcolo delle rotazioni elementari
			double cx = Math.Cos(xRadAngle);
			double sx = Math.Sin(xRadAngle);
			double cy = Math.Cos(yRadAngle);
			double sy = Math.Sin(yRadAngle);
			double cz = Math.Cos(zRadAngle);
			double sz = Math.Sin(zRadAngle);

			// Composizione rotazione Z * Y * X (convenzionale in robotica)
			double m11 = cz * cy;
			double m12 = cz * sy * sx - sz * cx;
			double m13 = cz * sy * cx + sz * sx;

			double m21 = sz * cy;
			double m22 = sz * sy * sx + cz * cx;
			double m23 = sz * sy * cx - cz * sx;

			double m31 = -sy;
			double m32 = cy * sx;
			double m33 = cy * cx;

			// Costruzione matrice 4x4 (rototraslazione con traslazione nulla)
			return new RTMatrix
			{
				_m11 = m11,
				_m12 = m12,
				_m13 = m13,
				_m14 = 0,
				_m21 = m21,
				_m22 = m22,
				_m23 = m23,
				_m24 = 0,
				_m31 = m31,
				_m32 = m32,
				_m33 = m33,
				_m34 = 0,
				_m41 = 0,
				_m42 = 0,
				_m43 = 0,
				_m44 = 1
			};

		}

		/// <summary>
		/// Dati due angoli di rotazione rispetto agli assi Z X in terna corrente. 
		/// Che poi equivale ai due angoli in X Z in terna fissa. 
		/// </summary>
		/// <param name="zRadAngle"></param>
		/// <param name="xRadAngle"></param>
		/// <returns></returns>
		public static RTMatrix FromEulerAnglesZX(double zRadAngle, double xRadAngle)
		{
			RTMatrix result = RTMatrix.Identity;
			double cosZ = (double)Math.Cos(zRadAngle);
			double sinZ = (double)Math.Sin(zRadAngle);
			double cosX = (double)Math.Cos(xRadAngle);
			double sinX = (double)Math.Sin(xRadAngle);
			result._m11 = cosZ;
			result._m21 = sinZ;
			result._m12 = -sinZ * cosX;
			result._m22 = cosZ * cosX;
			result._m32 = sinX;
			result._m13 = sinZ * sinX;
			result._m23 = -cosZ * sinX;
			result._m33 = cosX;

			return result;
		}

		/// <summary>
		/// Dati 3 vettori 
		/// </summary>
		/// <returns></returns>
		public static RTMatrix FromVectors(Vector3D x, Vector3D y, Vector3D z, Vector3D trasl) => new(x, y, z, trasl);

		/// <summary>
		/// Indetità con traslazione
		/// </summary>
		/// <param name="trasl"></param>
		/// <returns></returns>
		public static RTMatrix FromTraslation(Vector3D trasl)
		{
			RTMatrix result = RTMatrix.Identity;
			result.Translation = trasl;
			return result;
		}

		/// <summary>
		/// Data normale e traslazione. 
		/// Con vettori x e y ottenuti in maniera arbitraria. 
		/// Regola: [(this x UnitX) x this] con eccezioni nel caso this sia UnitX o NegativeUnitX.
		/// </summary>
		/// <returns></returns>
		public static RTMatrix FromNormal(Vector3D normal, Vector3D trasl)
		{
			normal.SetNormalized();
			Vector3D x = normal.Perpendicular();
			Vector3D y = normal.Cross(x);
			RTMatrix result = new RTMatrix(x, y, normal, trasl);
			return result;
		}

		#endregion STATICS
		#region Fields
		// Prima riga
		[DataMember]
		private double _m11, _m12, _m13, _m14;
		// Seconda riga
		[DataMember]
		private double _m21, _m22, _m23, _m24;
		// Terza riga
		[DataMember]
		private double _m31, _m32, _m33, _m34;
		// quarta riga
		[DataMember]
		private double _m41, _m42, _m43, _m44;
		#endregion

		#region Properties
		/// <summary>
		/// Traslazione X
		/// </summary>
		public double TraslationX => _m14;

		/// <summary>
		/// Traslazione Y
		/// </summary>
		public double TraslationY => _m24;

		/// <summary>
		/// Traslazione Z
		/// </summary>
		public double TraslationZ => _m34;

		/// <summary>
		/// Determinante
		/// </summary>
		public double Determinant =>
				_m11 * (_m22 * (_m33 * _m44 - _m43 * _m34) - _m23 * (_m32 * _m44 - _m42 * _m34) + _m24 * (_m32 * _m43 - _m42 * _m33)) -
				_m12 * (_m21 * (_m33 * _m44 - _m43 * _m34) - _m23 * (_m31 * _m44 - _m41 * _m34) + _m24 * (_m31 * _m43 - _m41 * _m33)) +
				_m13 * (_m21 * (_m32 * _m44 - _m42 * _m34) - _m22 * (_m31 * _m44 - _m41 * _m34) + _m24 * (_m31 * _m42 - _m41 * _m32)) -
				_m14 * (_m21 * (_m32 * _m43 - _m42 * _m33) - _m22 * (_m31 * _m43 - _m41 * _m33) + _m23 * (_m31 * _m42 - _m41 * _m32));

		/// <summary>
		/// Determinante per matrici affini (molto performante). 
		/// </summary>
		public double DeterminantAffine =>
					(_m11 * _m22 * _m33) +
					(_m12 * _m23 * _m31) +
					(_m13 * _m21 * _m32) -
					(_m13 * _m22 * _m31) -
					(_m12 * _m21 * _m33) -
					(_m11 * _m23 * _m32);

		/// <summary>
		///		Fa il Gets/Sets della parte Scale della matrice
		///		|Sx 0  0  0 |
		///		| 0 Sy 0  0 |
		///		| 0  0 Sz 0 |
		///		| 0  0  0 0 |
		/// </summary>

		[XmlIgnore]
		[JsonIgnore]
		public Vector3D Scale
		{
			get => new Vector3D(_m11, _m22, _m33);
			set
			{
				_m11 = value.X;
				_m22 = value.Y;
				_m33 = value.Z;
			}
		}

		/// <summary>
		///	Fa il Gets/Sets della parte Translation della matrice. 
		///	Rappresenta la colonna di indice 4.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public Vector3D Translation
		{
			get => new Vector3D(_m14, _m24, _m34);
			set
			{
				_m14 = value.X;
				_m24 = value.Y;
				_m34 = value.Z;
			}
		}

		/// <summary>
		/// In una matrice RT, rappresenta la colonna di indice 1
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public Vector3D XVector
		{
			get => this.GetVector(0); set => this.SetVector(0, value);
		}

		/// <summary>
		/// In una matrice RT, rappresenta la colonna di indice 2
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public Vector3D YVector
		{
			get => this.GetVector(1); set => this.SetVector(1, value);
		}

		/// <summary>
		/// In una matrice RT, rappresenta la colonna di indice 3
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public Vector3D ZVector
		{
			get => this.GetVector(2); set => this.SetVector(2, value);
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Matrice di rota-traslazione
		/// </summary>
		public RTMatrix()
		{
		}

		/// <summary>
		/// Matrice di rota-traslazione
		/// </summary>
		public RTMatrix(RTMatrix matrix)
			: this(matrix._m11, matrix._m12, matrix._m13, matrix._m14,
				  matrix._m21, matrix._m22, matrix._m23, matrix._m24,
				  matrix._m31, matrix._m32, matrix._m33, matrix._m34,
				  matrix._m41, matrix._m42, matrix._m43, matrix._m44)
		{
		}


		/// <summary>
		/// Costruttore con i vettori
		/// </summary>
		public RTMatrix(Vector3D x, Vector3D y, Vector3D z, Vector3D trasl)
			: this(x.X, y.X, z.X, trasl.X,
				  x.Y, y.Y, z.Y, trasl.Y,
				  x.Z, y.Z, z.Z, trasl.Z,
				  0, 0, 0, 1)
		{
		}

		public RTMatrix(double m11, double m12, double m13, double m14,
						double m21, double m22, double m23, double m24,
						double m31, double m32, double m33, double m34,
						double m41, double m42, double m43, double m44)
		{
			_m11 = m11;
			_m12 = m12;
			_m13 = m13;
			_m14 = m14;
			_m21 = m21;
			_m22 = m22;
			_m23 = m23;
			_m24 = m24;
			_m31 = m31;
			_m32 = m32;
			_m33 = m33;
			_m34 = m34;
			_m41 = m41;
			_m42 = m42;
			_m43 = m43;
			_m44 = m44;
		}
		#endregion

		#region Operators
		/// <summary>
		/// Accesso tramite 2 indici
		/// </summary>
		/// <param name="row">riga 0-base</param>
		/// <param name="col">colonna 0-base</param>
		public double this[int row, int col]
		{
			get
			{
				return this[row * 4 + col];
			}
			set
			{
				this[row * 4 + col] = value;
			}
		}

		/// <summary>
		/// Accesso tramite un indice
		/// </summary>
		/// <param name="index">indice 0-base</param>
		public double this[int index]
		{
			get
			{
				double result;
				switch (index)
				{
					case 0:
						result = _m11;
						break;
					case 1:
						result = _m12;
						break;
					case 2:
						result = _m13;
						break;
					case 3:
						result = _m14;
						break;
					case 4:
						result = _m21;
						break;
					case 5:
						result = _m22;
						break;
					case 6:
						result = _m23;
						break;
					case 7:
						result = _m24;
						break;
					case 8:
						result = _m31;
						break;
					case 9:
						result = _m32;
						break;
					case 10:
						result = _m33;
						break;
					case 11:
						result = _m34;
						break;
					case 12:
						result = _m41;
						break;
					case 13:
						result = _m42;
						break;
					case 14:
						result = _m43;
						break;
					case 15:
						result = _m44;
						break;
					default:
						result = 0;
						break;
				}
				return result;
			}
			set
			{
				switch (index)
				{
					case 0:
						_m11 = value;
						break;
					case 1:
						_m12 = value;
						break;
					case 2:
						_m13 = value;
						break;
					case 3:
						_m14 = value;
						break;
					case 4:
						_m21 = value;
						break;
					case 5:
						_m22 = value;
						break;
					case 6:
						_m23 = value;
						break;
					case 7:
						_m24 = value;
						break;
					case 8:
						_m31 = value;
						break;
					case 9:
						_m32 = value;
						break;
					case 10:
						_m33 = value;
						break;
					case 11:
						_m34 = value;
						break;
					case 12:
						_m41 = value;
						break;
					case 13:
						_m42 = value;
						break;
					case 14:
						_m43 = value;
						break;
					case 15:
						_m44 = value;
						break;
				}
			}
		}

		/// <summary>
		/// Set delle colonne della sottomatrice 3x3
		/// </summary>
		/// <param name="xAxis"></param>
		/// <param name="yAxis"></param>
		/// <param name="zAxis"></param>
		public void SetFromAxes(Vector3D xAxis, Vector3D yAxis, Vector3D zAxis)
		{
			_m11 = xAxis.X;
			_m21 = xAxis.Y;
			_m31 = xAxis.Z;
			_m12 = yAxis.X;
			_m22 = yAxis.Y;
			_m32 = yAxis.Z;
			_m13 = zAxis.X;
			_m23 = zAxis.Y;
			_m33 = zAxis.Z;
		}

		/// <summary>
		/// Setta la rotazione mantenendo la traslazione
		/// </summary>
		/// <param name="xRadAngle"></param>
		/// <param name="yRadAngle"></param>
		/// <param name="zRadAngle"></param>
		public void SetRotation(double xRadAngle, double yRadAngle, double zRadAngle)
		{
			Vector3D trasl = Translation;
			var matrix = FromEulerAnglesXYZ(xRadAngle, yRadAngle, zRadAngle);
			matrix.CloneTo(this);
			Translation = trasl;
		}


		#endregion

		#region Methods

		/// <summary>
		/// Aggiunge una traslazione x,y,z
		/// </summary>
		/// <param name="x">Traslazione X</param>
		/// <param name="y">Traslazione Y</param>
		/// <param name="z">Traslazione Z</param>
		public RTMatrix Traslate(double x, double y, double z)
		{
			RTMatrix result = new RTMatrix(this);
			result._m14 += x;
			result._m24 += y;
			result._m34 += z;
			return result;
		}

		/// <summary>
		/// Indica se la matrice ha qualche valore NaN
		/// </summary>
		/// <returns>Matrice valida</returns>
		public bool IsNaN()
		{
			var r =
				double.IsNaN(_m11) || double.IsNaN(_m12) || double.IsNaN(_m13) || double.IsNaN(_m14) ||
				double.IsNaN(_m21) || double.IsNaN(_m22) || double.IsNaN(_m23) || double.IsNaN(_m24) ||
				double.IsNaN(_m31) || double.IsNaN(_m32) || double.IsNaN(_m33) || double.IsNaN(_m34) ||
				double.IsNaN(_m41) || double.IsNaN(_m42) || double.IsNaN(_m43) || double.IsNaN(_m44);
			return r;
		}

		/// <summary>
		/// Esegue una copia della classe
		/// </summary>
		/// <returns>Copia della classe</returns>
		public RTMatrix Clone()
		{
			return new RTMatrix(this);
		}

		/// <summary>
		/// Metodo per clonare i valori in una matrice target
		/// </summary>
		/// <param name="target">target</param>
		public void CloneTo(RTMatrix target)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target._m11 = _m11; target._m12 = _m12; target._m13 = _m13; target._m14 = _m14;
			target._m21 = _m21; target._m22 = _m22; target._m23 = _m23; target._m24 = _m24;
			target._m31 = _m31; target._m32 = _m32; target._m33 = _m33; target._m34 = _m34;
			target._m41 = _m41; target._m42 = _m42; target._m43 = _m43; target._m44 = _m44;
		}

		/// <summary>
		/// Controlla se due matrici sono uguali con una certa tolleranza
		/// </summary>
		/// <param name="obj">seconda matrice</param>
		/// <returns></returns>
		public bool IsEquals(object obj)
		{
			if (obj is RTMatrix other)
			{
				bool row1 = MathUtils.IsEqual(_m11, other._m11) && MathUtils.IsEqual(_m12, other._m12) && MathUtils.IsEqual(_m13, other._m13) && MathUtils.IsEqual(_m14, other._m14);
				bool row2 = MathUtils.IsEqual(_m21, other._m21) && MathUtils.IsEqual(_m22, other._m22) && MathUtils.IsEqual(_m23, other._m23) && MathUtils.IsEqual(_m24, other._m24);
				bool row3 = MathUtils.IsEqual(_m31, other._m31) && MathUtils.IsEqual(_m32, other._m32) && MathUtils.IsEqual(_m33, other._m33) && MathUtils.IsEqual(_m34, other._m34);
				bool row4 = MathUtils.IsEqual(_m41, other._m41) && MathUtils.IsEqual(_m42, other._m42) && MathUtils.IsEqual(_m43, other._m43) && MathUtils.IsEqual(_m44, other._m44);
				return row1 && row2 && row3 && row4;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is RTMatrix)
				return (this == (RTMatrix)obj);
			else
				return false;
		}
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			int hashCode = 0;
			for (int i = 0; i < 16; i++)
				hashCode ^= this[i].GetHashCode();

			return hashCode;
		}
		#endregion
		#region Operations
		/// <summary>
		/// Somma elemento per elemento
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public RTMatrix Add(RTMatrix matrix)
		{
			return this + matrix;
		}

		/// <summary>
		/// Sottrazione elemento per elemento
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public RTMatrix Subtract(RTMatrix matrix)
		{
			return this - matrix;
		}

		/// <summary>
		/// Negazione elemento per elemento
		/// </summary>
		/// <returns></returns>
		public RTMatrix Negate()
		{
			return -this;
		}


		#endregion
		#region Multiply

		/// <summary>
		/// Moltiplicazione matrice-matrice
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public RTMatrix Multiply(RTMatrix matrix)
		{
			return this * matrix;
		}

		/// <summary>
		/// Applicazione della matrice a un punto 3D
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Point3D Multiply(Point3D point)
		{
			return this * point;
		}

		/// <summary>
		/// Applicazione della matrice a un vettore 3D
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public Vector3D Multiply(Vector3D vector)
		{
			return this * vector;
		}

		/// <summary>
		/// Moltiplicazione per uno scalare
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public RTMatrix Multiply(double scalar)
		{
			return this * scalar;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Inversa
		/// </summary>
		/// <returns></returns>
		public RTMatrix Inverse()
		{
			var det = Determinant;
			if (det == 0)
			{
				throw new InvalidOperationException("Matrix is not invertible");
			}
			return Adjoint() * (1 / det);
		}


		/// <summary>
		/// Inversa per matrici che contengono solo roto traslazioni (molto performante). 
		/// </summary>
		/// <returns>Matrice inversa</returns>
		public RTMatrix InverseRT()
		{
			RTMatrix result = new RTMatrix();
			result._m11 = _m11;
			result._m21 = _m12;
			result._m31 = _m13;
			result._m41 = 0;
			result._m12 = _m21;
			result._m22 = _m22;
			result._m32 = _m23;
			result._m42 = 0;
			result._m13 = _m31;
			result._m23 = _m32;
			result._m33 = _m33;
			result._m43 = 0;
			result._m14 = -(result._m11 * _m14 + result._m12 * _m24 + result._m13 * _m34);
			result._m24 = -(result._m21 * _m14 + result._m22 * _m24 + result._m23 * _m34);
			result._m34 = -(result._m31 * _m14 + result._m32 * _m24 + result._m33 * _m34);
			result._m44 = 1;
			return result;
		}

		/// <summary>
		/// Trasposta
		/// </summary>
		/// <returns>Matrice trasposta</returns>
		public RTMatrix Transpose()
		{
			return new RTMatrix(_m11, _m21, _m31, _m41,
								_m12, _m22, _m32, _m42,
								_m13, _m23, _m33, _m43,
								_m14, _m24, _m34, _m44);
		}
		/// <summary>
		/// Effettua una trasformazione per ruotare attorno a un asse specificato da un punto e un vettore.
		/// </summary>
		/// <param name="origin">Origine</param>
		/// <param name="axisDirection">Direzione asse</param>
		/// <param name="radAngle"></param>
		public RTMatrix Transform(Point3D origin, Vector3D axisDirection, double radAngle)
		{
			RTMatrix result = this;
			RTMatrix trasformInverse = RTMatrix.FromNormal(axisDirection, origin.ToVector());
			RTMatrix trasform = trasformInverse.Inverse();
			RTMatrix rotate = RTMatrix.FromEulerAnglesXYZ(0, 0, radAngle);
			result = trasformInverse * rotate * trasform * this;
			return result;
		}
		/// <summary>
		/// Restituisce i tre angoli di rotazione rispetto ai tre assi cartesiani. 
		/// Nell'ordine X, Y, Z. 
		/// Vengono chiamati anche angoli RPY: roll (x), pitch (y), yaw (z). 
		/// Se simmetricRange è a true yAngle appartiene al range (-PI/2, PI/2) altrimenti 
		/// yAngle appartiene al range (PI/2, 3/2*PI).
		/// Riferimento: Dispense di Robotica Industriale - Bruno Siciliano
		/// </summary>
		/// <param name="xRadAngle"></param>
		/// <param name="yRadAngle"></param>
		/// <param name="zRadAngle"></param>
		public void ToEulerAnglesXYZ(bool simmetricRange, out double xRadAngle, out double yRadAngle, out double zRadAngle)
		{
			if (MathUtils.IsEqual(_m11, 0) && MathUtils.IsEqual(_m21, 0))
			{
				// Caso particolare: singolarità
				if (_m31 < 0)
				{
					xRadAngle = Math.Atan2(_m12, _m22);
					yRadAngle = Math.PI / 2;
					zRadAngle = 0;
				}
				else
				{
					xRadAngle = -Math.Atan2(_m12, _m22);
					yRadAngle = -Math.PI / 2;
					zRadAngle = 0;
				}
			}
			else
			{
				if (simmetricRange == true)
				{
					xRadAngle = Math.Atan2(_m32, _m33);
					double tmp = Math.Sqrt(_m21 * _m21 + _m22 * _m22);
					yRadAngle = Math.Atan2(-_m31, tmp);
					zRadAngle = Math.Atan2(_m21, _m11);
				}
				else
				{
					xRadAngle = Math.Atan2(-_m21, -_m22);
					double tmp = Math.Sqrt(_m21 * _m21 + _m22 * _m22);
					yRadAngle = Math.Atan2(-_m31, -tmp);
					zRadAngle = Math.Atan2(-_m21, -_m11);
				}
			}
		}

		/// <summary>
		/// Restituisce i due angoli di rotazione rispetto agli assi Z X in terna corrente. 
		/// Che poi equivale ai due angoli in X Z in terna fissa. 
		/// N.B. Considera solo la normale Z, cioè la terza colonna della matrice. 
		/// Con firstSolution si può scegliere tra 2 soluzioni valide.
		/// </summary>
		/// <param name="firstSolution">Soluzioni valide</param>
		/// <param name="zRadAngle">Angolo per asse Z</param>
		/// <param name="xRadAngle">Angolo rotazione asse X</param>
		public void ToEulerAnglesZX(bool firstSolution, out double zRadAngle, out double xRadAngle)
		{
			double sZ, cZ, sX, cX;
			Vector3D normal = new Vector3D(_m13, _m23, _m33);
			if (normal.IsEquals(Vector3D.UnitZ))
			{
				if (firstSolution)
				{
					zRadAngle = 0;
					xRadAngle = 0;
				}
				else
				{
					zRadAngle = Math.PI;
					xRadAngle = 0;
				}
			}
			else if (normal.IsEquals(Vector3D.NegativeUnitZ))
			{
				if (firstSolution)
				{
					zRadAngle = 0;
					xRadAngle = Math.PI;
				}
				else
				{
					zRadAngle = Math.PI;
					xRadAngle = Math.PI;
				}
			}
			else
			{
				double tmp = Math.Sqrt(1 - normal.Z * normal.Z);

				if (firstSolution == true)
				{
					sZ = normal.X / tmp;
					cZ = -normal.Y / tmp;
					sX = tmp;
					cX = normal.Z;
				}
				else
				{
					sZ = -normal.X / tmp;
					cZ = normal.Y / tmp;
					sX = -tmp;
					cX = normal.Z;
				}
				zRadAngle = Math.Atan2(sZ, cZ);
				xRadAngle = Math.Atan2(sX, cX);
			}
		}
		#endregion
		#region Private Methods
		/// <summary>
		/// Get del vettore della colonna i-esima
		/// </summary>
		/// <param name="col"></param>
		/// <returns></returns>
		private Vector3D GetVector(int col)
		{
			return new Vector3D(this[0, col], this[1, col], this[2, col]);
		}

		/// <summary>
		/// Set del vettore della colonna i-esima
		/// </summary>
		/// <param name="col"></param>
		/// <param name="vector"></param>
		private void SetVector(int col, Vector3D vector)
		{
			this[0, col] = vector.X;
			this[1, col] = vector.Y;
			this[2, col] = vector.Z;
		}
		#endregion
		#region OPERATORS
		/// <summary>
		/// Moltiplicazione matrice-matrice
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static RTMatrix operator *(RTMatrix left, RTMatrix right)
		{
			RTMatrix result = new RTMatrix();

			result._m11 = left._m11 * right._m11 + left._m12 * right._m21 + left._m13 * right._m31 + left._m14 * right._m41;
			result._m12 = left._m11 * right._m12 + left._m12 * right._m22 + left._m13 * right._m32 + left._m14 * right._m42;
			result._m13 = left._m11 * right._m13 + left._m12 * right._m23 + left._m13 * right._m33 + left._m14 * right._m43;
			result._m14 = left._m11 * right._m14 + left._m12 * right._m24 + left._m13 * right._m34 + left._m14 * right._m44;

			result._m21 = left._m21 * right._m11 + left._m22 * right._m21 + left._m23 * right._m31 + left._m24 * right._m41;
			result._m22 = left._m21 * right._m12 + left._m22 * right._m22 + left._m23 * right._m32 + left._m24 * right._m42;
			result._m23 = left._m21 * right._m13 + left._m22 * right._m23 + left._m23 * right._m33 + left._m24 * right._m43;
			result._m24 = left._m21 * right._m14 + left._m22 * right._m24 + left._m23 * right._m34 + left._m24 * right._m44;

			result._m31 = left._m31 * right._m11 + left._m32 * right._m21 + left._m33 * right._m31 + left._m34 * right._m41;
			result._m32 = left._m31 * right._m12 + left._m32 * right._m22 + left._m33 * right._m32 + left._m34 * right._m42;
			result._m33 = left._m31 * right._m13 + left._m32 * right._m23 + left._m33 * right._m33 + left._m34 * right._m43;
			result._m34 = left._m31 * right._m14 + left._m32 * right._m24 + left._m33 * right._m34 + left._m34 * right._m44;

			result._m41 = left._m41 * right._m11 + left._m42 * right._m21 + left._m43 * right._m31 + left._m44 * right._m41;
			result._m42 = left._m41 * right._m12 + left._m42 * right._m22 + left._m43 * right._m32 + left._m44 * right._m42;
			result._m43 = left._m41 * right._m13 + left._m42 * right._m23 + left._m43 * right._m33 + left._m44 * right._m43;
			result._m44 = left._m41 * right._m14 + left._m42 * right._m24 + left._m43 * right._m34 + left._m44 * right._m44;

			return result;
		}

		/// <summary>
		/// Applicazione della matrice a un vettore 3D
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static Vector3D operator *(RTMatrix matrix, Vector3D vector)
		{
			Vector3D result = new(
			((matrix._m11 * vector.X) + (matrix._m12 * vector.Y) + (matrix._m13 * vector.Z)),
			 ((matrix._m21 * vector.X) + (matrix._m22 * vector.Y) + (matrix._m23 * vector.Z)),
			((matrix._m31 * vector.X) + (matrix._m32 * vector.Y) + (matrix._m33 * vector.Z)));
			return result;
		}

		/// <summary>
		/// Applicazione della matrice a un punto 3D
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public static Point3D operator *(RTMatrix matrix, Point3D point)
		{
			Point3D result = new(
			((matrix._m11 * point.X) + (matrix._m12 * point.Y) + (matrix._m13 * point.Z) + matrix._m14),
			((matrix._m21 * point.X) + (matrix._m22 * point.Y) + (matrix._m23 * point.Z) + matrix._m24),
			((matrix._m31 * point.X) + (matrix._m32 * point.Y) + (matrix._m33 * point.Z) + matrix._m34));
			return result;
		}

		/// <summary>
		/// Moltiplicazione per uno scalare
		/// </summary>
		/// <param name="left"></param>
		/// <param name="scalar"></param>
		/// <returns></returns>
		public static RTMatrix operator *(RTMatrix left, double scalar)
		{
			RTMatrix result = new RTMatrix
			{
				_m11 = left._m11 * scalar,
				_m12 = left._m12 * scalar,
				_m13 = left._m13 * scalar,
				_m14 = left._m14 * scalar,

				_m21 = left._m21 * scalar,
				_m22 = left._m22 * scalar,
				_m23 = left._m23 * scalar,
				_m24 = left._m24 * scalar,

				_m31 = left._m31 * scalar,
				_m32 = left._m32 * scalar,
				_m33 = left._m33 * scalar,
				_m34 = left._m34 * scalar,

				_m41 = left._m41 * scalar,
				_m42 = left._m42 * scalar,
				_m43 = left._m43 * scalar,
				_m44 = left._m44 * scalar
			};

			return result;
		}

		/// <summary>
		/// Somma elemento per elemento
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static RTMatrix operator +(RTMatrix left, RTMatrix right)
		{
			RTMatrix result = new RTMatrix
			{
				_m11 = left._m11 + right._m11,
				_m12 = left._m12 + right._m12,
				_m13 = left._m13 + right._m13,
				_m14 = left._m14 + right._m14,

				_m21 = left._m21 + right._m21,
				_m22 = left._m22 + right._m22,
				_m23 = left._m23 + right._m23,
				_m24 = left._m24 + right._m24,

				_m31 = left._m31 + right._m31,
				_m32 = left._m32 + right._m32,
				_m33 = left._m33 + right._m33,
				_m34 = left._m34 + right._m34,

				_m41 = left._m41 + right._m41,
				_m42 = left._m42 + right._m42,
				_m43 = left._m43 + right._m43,
				_m44 = left._m44 + right._m44
			};

			return result;
		}

		/// <summary>
		/// Sottrazione elemento per elemento
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static RTMatrix operator -(RTMatrix left, RTMatrix right)
		{
			RTMatrix result = new RTMatrix
			{
				_m11 = left._m11 - right._m11,
				_m12 = left._m12 - right._m12,
				_m13 = left._m13 - right._m13,
				_m14 = left._m14 - right._m14,

				_m21 = left._m21 - right._m21,
				_m22 = left._m22 - right._m22,
				_m23 = left._m23 - right._m23,
				_m24 = left._m24 - right._m24,

				_m31 = left._m31 - right._m31,
				_m32 = left._m32 - right._m32,
				_m33 = left._m33 - right._m33,
				_m34 = left._m34 - right._m34,

				_m41 = left._m41 - right._m41,
				_m42 = left._m42 - right._m42,
				_m43 = left._m43 - right._m43,
				_m44 = left._m44 - right._m44
			};

			return result;
		}

		/// <summary>
		/// Negazione elemento per elemento
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public static RTMatrix operator -(RTMatrix matrix)
		{
			RTMatrix result = new RTMatrix
			{
				_m11 = -matrix._m11,
				_m12 = -matrix._m12,
				_m13 = -matrix._m13,
				_m14 = -matrix._m14,
				_m21 = -matrix._m21,
				_m22 = -matrix._m22,
				_m23 = -matrix._m23,
				_m24 = -matrix._m24,
				_m31 = -matrix._m31,
				_m32 = -matrix._m32,
				_m33 = -matrix._m33,
				_m34 = -matrix._m34,
				_m41 = -matrix._m41,
				_m42 = -matrix._m42,
				_m43 = -matrix._m43,
				_m44 = -matrix._m44
			};
			return result;
		}

		/// <summary>
		/// Uguaglianza esatta elemento per elemento
		/// </summary>
		/// <param name="left">Matrice sinistra</param>
		/// <param name="right">Matrice destra</param>
		/// <returns><c>true</c> Uguaglianza esatta</returns>
		public static bool operator ==(RTMatrix left, RTMatrix right)
		{
			if (left._m11 == right._m11 && left._m12 == right._m12 && left._m13 == right._m13 && left._m14 == right._m14 &&
				left._m21 == right._m21 && left._m22 == right._m22 && left._m23 == right._m23 && left._m24 == right._m24 &&
				left._m31 == right._m31 && left._m32 == right._m32 && left._m33 == right._m33 && left._m34 == right._m34 &&
				left._m41 == right._m41 && left._m42 == right._m42 && left._m43 == right._m43 && left._m44 == right._m44)
				return true;

			return false;
		}

		/// <summary>
		/// Disuguaglianza esatta elemento per elemento
		/// </summary>
		/// <param name="left">Matrice sinistra</param>
		/// <param name="right">Matrice destra</param>
		/// <returns><c>true</c> Disuguaglianza esatta</returns>
		public static bool operator !=(RTMatrix left, RTMatrix right)
		{
			return !(left == right);
		}

		#endregion OPERATORS



		#region PRIVATE METHODS
		private RTMatrix Adjoint()
		{
			double val0 = _m22 * (_m33 * _m44 - _m43 * _m34) - _m23 * (_m32 * _m44 - _m42 * _m34) + _m24 * (_m32 * _m43 - _m42 * _m33);
			double val1 = -(_m12 * (_m33 * _m44 - _m43 * _m34) - _m13 * (_m32 * _m44 - _m42 * _m34) + _m14 * (_m32 * _m43 - _m42 * _m33));
			double val2 = _m12 * (_m23 * _m44 - _m43 * _m24) - _m13 * (_m22 * _m44 - _m42 * _m24) + _m14 * (_m22 * _m43 - _m42 * _m23);
			double val3 = -(_m12 * (_m23 * _m34 - _m33 * _m24) - _m13 * (_m22 * _m34 - _m32 * _m24) + _m14 * (_m22 * _m33 - _m32 * _m23));
			double val4 = -(_m21 * (_m33 * _m44 - _m43 * _m34) - _m23 * (_m31 * _m44 - _m41 * _m34) + _m24 * (_m31 * _m43 - _m41 * _m33));
			double val5 = _m11 * (_m33 * _m44 - _m43 * _m34) - _m13 * (_m31 * _m44 - _m41 * _m34) + _m14 * (_m31 * _m43 - _m41 * _m33);
			double val6 = -(_m11 * (_m23 * _m44 - _m43 * _m24) - _m13 * (_m21 * _m44 - _m41 * _m24) + _m14 * (_m21 * _m43 - _m41 * _m23));
			double val7 = _m11 * (_m23 * _m34 - _m33 * _m24) - _m13 * (_m21 * _m34 - _m31 * _m24) + _m14 * (_m21 * _m33 - _m31 * _m23);
			double val8 = _m21 * (_m32 * _m44 - _m42 * _m34) - _m22 * (_m31 * _m44 - _m41 * _m34) + _m24 * (_m31 * _m42 - _m41 * _m32);
			double val9 = -(_m11 * (_m32 * _m44 - _m42 * _m34) - _m12 * (_m31 * _m44 - _m41 * _m34) + _m14 * (_m31 * _m42 - _m41 * _m32));
			double val10 = _m11 * (_m22 * _m44 - _m42 * _m24) - _m12 * (_m21 * _m44 - _m41 * _m24) + _m14 * (_m21 * _m42 - _m41 * _m22);
			double val11 = -(_m11 * (_m22 * _m34 - _m32 * _m24) - _m12 * (_m21 * _m34 - _m31 * _m24) + _m14 * (_m21 * _m32 - _m31 * _m22));
			double val12 = -(_m21 * (_m32 * _m43 - _m42 * _m33) - _m22 * (_m31 * _m43 - _m41 * _m33) + _m23 * (_m31 * _m42 - _m41 * _m32));
			double val13 = _m11 * (_m32 * _m43 - _m42 * _m33) - _m12 * (_m31 * _m43 - _m41 * _m33) + _m13 * (_m31 * _m42 - _m41 * _m32);
			double val14 = -(_m11 * (_m22 * _m43 - _m42 * _m23) - _m12 * (_m21 * _m43 - _m41 * _m23) + _m13 * (_m21 * _m42 - _m41 * _m22));
			double val15 = _m11 * (_m22 * _m33 - _m32 * _m23) - _m12 * (_m21 * _m33 - _m31 * _m23) + _m13 * (_m21 * _m32 - _m31 * _m22);

			return new RTMatrix(val0, val1, val2, val3, val4, val5, val6, val7, val8, val9, val10, val11, val12, val13, val14, val15);
		}
		#endregion PRIVATE METHODS

		#region Overloads di Object

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat(" | {0} {1} {2} {3} |\n", _m11, _m12, _m13, _m14);
			sb.AppendFormat(" | {0} {1} {2} {3} |\n", _m21, _m22, _m23, _m24);
			sb.AppendFormat(" | {0} {1} {2} {3} |\n", _m31, _m32, _m33, _m34);
			sb.AppendFormat(" | {0} {1} {2} {3} |\n", _m41, _m42, _m43, _m44);

			return sb.ToString();
		}
		#endregion
	}
}
