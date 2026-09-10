using System;

namespace Axiom.GeoMath
{
	/// <summary>
	/// Rappresenta un vettore 3D.
	/// </summary>
	public class Vector3D
	{
		#region STATIC
		/// <summary>
		/// Vettore unitario lungo l'asse X.
		/// </summary>
		public static Vector3D UnitX => new Vector3D(1, 0, 0);

		/// <summary>
		/// Vettore unitario lungo l'asse Y.
		/// </summary>
		public static Vector3D UnitY => new Vector3D(0, 1, 0);

		/// <summary>
		/// Vettore unitario lungo l'asse Z.
		/// </summary>
		public static Vector3D UnitZ => new Vector3D(0, 0, 1);

		/// <summary>
		/// Vettore unitario negativo lungo l'asse X.
		/// </summary>
		public static Vector3D NegativeUnitX => new Vector3D(-1, 0, 0);

		/// <summary>
		/// Vettore unitario negativo lungo l'asse Y.
		/// </summary>
		public static Vector3D NegativeUnitY => new Vector3D(0, -1, 0);

		/// <summary>
		/// Vettore unitario negativo lungo l'asse Z.
		/// </summary>
		public static Vector3D NegativeUnitZ => new Vector3D(0, 0, -1);

		/// <summary>
		/// Vettore nullo.
		/// </summary>
		public static Vector3D Zero => new Vector3D(0, 0, 0);

		/// <summary>
		/// Vettore nullo: Vector3D(double.NaN, double.NaN, double.NaN)
		/// </summary>
		public static Vector3D NullVector => new Vector3D(double.NaN, double.NaN, double.NaN);
		#endregion

		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Componente X del vettore.
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// Componente Y del vettore.
		/// </summary>
		public double Y { get; set; }
		/// <summary>
		/// Componente Z del vettore.
		/// </summary>
		public double Z { get; set; }

		/// <summary>
		/// Restituisce la norma (lunghezza) del vettore.
		/// </summary>
		public double Norm => Math.Sqrt(X * X + Y * Y + Z * Z);

		/// <summary>
		/// Restituisce la lunghezza al quadrato
		/// </summary>
		public double LengthSquared => X * X + Y * Y + Z * Z;

		/// <summary>
		/// Lungezza del vettore.
		/// </summary>
		public double Length => Norm;


		#endregion

		#region Constructors
		/// <summary>
		/// Crea un nuovo vettore 3D.
		/// </summary>
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		/// <summary>
		/// Crea un nuovo vettore 3D.
		/// </summary>
		public Vector3D(Vector3D vector3D)
			: this(vector3D.X, vector3D.Y, vector3D.Z)
		{
		}
		#endregion

		#region Methods

		/// <summary>
		/// Accesso tramite indice
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public double this[int index]
		{
			get
			{
				double result;
				switch (index)
				{
					case 0:
						result = X;
						break;
					case 1:
						result = Y;
						break;
					case 2:
						result = Z;
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
						X = value;
						break;
					case 1:
						Y = value;
						break;
					case 2:
						Z = value;
						break;
				}
			}
		}


		/// <summary>
		/// Uguale a un altro vettore, con tolleranza.
		/// </summary>
		/// <param name="other">Vettore di verifica</param>
		/// <returns></returns>
		public bool IsEquals(Vector3D other)
		{
			return X.IsEquals(other.X) &&
				   Y.IsEquals(other.Y) &&
				   Z.IsEquals(other.Z);
		}

		/// <summary>
		/// Ugauale a un altro vettore, con tolleranza.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public bool IsEquals(Vector3D other, double tolerance)
		{
			return X.IsEquals(other.X, tolerance) &&
				   Y.IsEquals(other.Y, tolerance) &&
				   Z.IsEquals(other.Z, tolerance);
		}

		/// <summary>
		/// Confronta i due vettori considerando la tolleranza uguale a MathUtils.Tolerance. 
		/// Il confronto viene effettuato guardando se l'angolo interno (in radianti) tende a zero.
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public bool ApproxEqualsInnerAngle(Vector3D vector) => Angle(vector).IsEquals(0);

		/// <summary>
		/// Confronta i due vettori considerando la tolleranza indicata. 
		/// Il confronto viene effettuato guardando se l'angolo interno (in radianti) tende a zero.
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public bool ApproxEqualsInnerAngle(Vector3D vector, double tolerance) => Angle(vector).IsEquals(0, tolerance);

		/// <summary>
		/// Restituisce il vettore normalizzato (versore).
		/// </summary>
		public Vector3D Normalize()
		{
			double norm = Norm;
			if (norm == 0) throw new InvalidOperationException("Cannot normalize a zero vector.");
			return new Vector3D(X / norm, Y / norm, Z / norm);
		}

		/// <summary>
		/// NOrmalizza il vettore corrente.
		/// </summary>
		public double SetNormalize()
		{
			double length = Norm;
			if (length > double.Epsilon)
			{
				double inverseLength = 1.0 / length;

				X *= inverseLength;
				Y *= inverseLength;
				Z *= inverseLength;
			}
			return length;
		}

		/// <summary>
		/// Indica se il vettore è (circa) nullo, entro <see cref="MathUtils.FineTolerance"/>.
		/// </summary>
		public bool IsZero() => IsZero(MathUtils.FineTolerance);

		/// <summary>
		/// Indica se il vettore è (circa) nullo, entro la tolleranza indicata.
		/// </summary>
		/// <param name="tolerance">Tolleranza sulla norma.</param>
		public bool IsZero(double tolerance) => !(Norm > tolerance);

		/// <summary>
		/// Tenta di normalizzare il vettore SENZA lanciare eccezioni. Alternativa sicura a
		/// <see cref="Normalize"/> per input potenzialmente degeneri.
		/// </summary>
		/// <param name="normalized">Il versore, oppure <see cref="Zero"/> se il vettore è nullo/NaN.</param>
		/// <returns><c>true</c> se la normalizzazione è riuscita; <c>false</c> se il vettore è nullo o NaN.</returns>
		public bool TryNormalize(out Vector3D normalized)
		{
			double norm = Norm;
			if (double.IsNaN(norm) || norm <= double.Epsilon)
			{
				normalized = Vector3D.Zero;
				return false;
			}
			normalized = new Vector3D(X / norm, Y / norm, Z / norm);
			return true;
		}

		/// <summary>
		/// Restituisce il versore, oppure <see cref="Zero"/> se il vettore è nullo/NaN
		/// (nessuna eccezione). Alternativa sicura a <see cref="Normalize"/>.
		/// </summary>
		public Vector3D NormalizeOrZero() => TryNormalize(out Vector3D n) ? n : Vector3D.Zero;

		/// <summary>
		/// Cambia segno a tutte le componenti
		/// </summary>
		/// <returns></returns>
		public void SetNegate()
		{
			X = -X;
			Y = -Y;
			Z = -Z;
		}

		/// <summary>
		/// Prodotto scalare tra questo vettore e un altro.
		/// </summary>
		public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

		/// <summary>
		/// Prodotto vettoriale tra questo vettore e un altro.
		/// </summary>
		public Vector3D Cross(Vector3D other)
		{
			return new Vector3D(
				Y * other.Z - Z * other.Y,
				Z * other.X - X * other.Z,
				X * other.Y - Y * other.X
			);
		}

		/// <summary>
		/// Restituisce una stringa rappresentativa del vettore.
		/// </summary>
		public override string ToString() => $"({X}, {Y}, {Z})";

		/// <summary>
		/// Restituisce un vettore perpendicolare (normalizzato). 
		/// Tende a restituire il vettore lungo X. 
		/// Regola: [(this x UnitX) x this] con eccezioni nel caso this sia UnitX o NegativeUnitX.
		/// </summary>
		/// <returns></returns>
		public Vector3D Perpendicular()
		{
			// Vettore nullo: non esiste un perpendicolare definito -> Zero (nessuna eccezione).
			if (IsZero())
				return Vector3D.Zero;

			if (IsEquals(Vector3D.NegativeUnitX))
				return Vector3D.NegativeUnitY;
			if (IsEquals(Vector3D.UnitX))
				return Vector3D.UnitY;

			Vector3D cross = Cross(Vector3D.UnitX);
			if (cross.IsZero())
				// this è parallelo all'asse X (ma non ±UnitX): un perpendicolare valido è ±UnitY.
				return X >= 0 ? Vector3D.UnitY : Vector3D.NegativeUnitY;

			return cross.Cross(this).NormalizeOrZero();
		}

        /// <summary>
        /// Indica se i due vettori sono paralleli, controlla che la proiezione sia circa 1. 
        /// Con tolleranza pari a MathUtils.Tolerance.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public bool IsParallel(Vector3D vector) => IsParallel(vector, MathUtils.FineTolerance);

        /// <summary>
        /// Indica se i due vettori sono paralleli, controlla che la proiezione sia circa 1. 
        /// Con tolleranza indicata.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public bool IsParallel(Vector3D vector, double tolerance)
		{
			// Un vettore nullo non ha direzione: per definizione non è parallelo ad alcunché.
			if (!TryNormalize(out Vector3D v1) || !vector.TryNormalize(out Vector3D v2))
				return false;
			bool result = MathExtensions.IsEquals(Math.Abs(v1.Dot(v2)), 1, tolerance);
			return result;
		}

		/// <summary>
		/// Restituisce la interpolazione sferica lineare. 
		/// Se i due vettori sono identici restituisce  
		/// Se sono opposti ci sarebbero infiniti piani su cui ruotare, in questo caso considera il piano 
		/// individuato dal parametro normal (che altrimenti non viene considerato). 
		/// Il parametro t deve essere compreso tra 0 e 1.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public Vector3D Slerp(Vector3D destination, double t, Vector3D normal)
		{
			Vector3D result = this;
			Vector3D v0 = new Vector3D(this);
			Vector3D v1 = new Vector3D(destination);
			v0.SetNormalize();
			v1.SetNormalize();

			double d = v0.Dot(v1);
			// Se il dot è 1 allora i due vettori sono uguali
			if (d < 1)
			{
				// Se sono opposti (d ≈ -1) ci sono infiniti piani di rotazione: si usa il piano
				// indicato dal parametro normal. Altrimenti il piano è dato dal prodotto vettoriale.
				// N.B. In precedenza il Cross veniva normalizzato PRIMA di questo controllo, quindi
				// per vettori opposti/paralleli Normalize lanciava eccezione: ora è gestito.
				Vector3D planeNormal = d.IsEquals(-1)
					? normal
					: Cross(destination).NormalizeOrZero();

				if (planeNormal.IsZero() == false)
				{
					double angle = destination.Angle(this, planeNormal);
					result = Rotate(planeNormal, t * angle);
				}
			}

			return result;
		}

		/// <summary>
		/// Determina l'angolo INTERNO tra this e il vettore passato come parametro.
		/// Restituisce un angolo in radianti compreso tra 0 e + PI (N.B. angolo interno)
		/// </summary>
		public double Angle()
		{
			if (!TryNormalize(out Vector3D a))
				return 0;
			Vector3D b = Vector3D.UnitZ;
			return Math.Atan2(a.Cross(b).Length, a.Dot(b));
		}

		/// <summary>
		/// Determina l'angolo INTERNO tra this e il vettore passato come parametro.
		/// Restituisce un angolo in radianti compreso tra 0 e + PI (N.B. angolo interno)
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public double Angle(Vector3D vector)
		{
			if (!TryNormalize(out Vector3D a) || !vector.TryNormalize(out Vector3D b))
				return 0;
			return Math.Atan2(a.Cross(b).Length, a.Dot(b));
		}

		/// <summary>
		/// Determina l'angolo di this rispetto a refX considerando refZ come riferimento Z. 
		/// Restituisce un angolo in radianti compreso tra -PI e + PI (N.B. Può essere o meno l'angolo interno). 
		/// Normal deve essere perpendicolare a this e vector.
		/// </summary>
		/// <param name="refX"></param>
		/// <param name="refZ"></param>
		/// <returns></returns>
		public double Angle(Vector3D refX, Vector3D refZ)
		{
			if (!TryNormalize(out Vector3D a) || !refX.TryNormalize(out Vector3D b))
				return 0;
			Vector3D cross = a.Cross(b);
			double result = Math.Atan2(cross.Length, a.Dot(b));
			if (cross.Dot(refZ) > 0)
				result = -result;

			return result;
		}

		/// <summary>
		/// Ruota attorno al vettore normal di una quantità pari ad angle
		/// </summary>
		/// <param name="normal"></param>
		/// <returns></returns>
		public Vector3D Rotate(Vector3D normal, double radAngle)
		{
			// Vettore nullo o asse di rotazione nullo: nulla da ruotare.
			if (!TryNormalize(out Vector3D thisNorm) || normal.IsZero())
				return new Vector3D(this);

			// Se l'asse è parallelo al vettore, la rotazione lo lascia invariato
			// (e il Cross sarebbe nullo, causando in precedenza un'eccezione in Normalize).
			if (!normal.Cross(thisNorm).TryNormalize(out Vector3D locY))
				return new Vector3D(this);

			RTMatrix matrixA = RTMatrix.FromEulerAnglesXYZ(0, 0, radAngle);
			RTMatrix matrixB = RTMatrix.Identity;
			Vector3D locZ = normal;
			Vector3D locX = locY.Cross(locZ);
			matrixB.SetFromAxes(locX, locY, locZ);
			return (matrixB * matrixA * matrixB.Transpose()) * this;
		}

		/// <summary>
		/// Restituisce un vettore con segno cambiato per tutte le componenti
		/// </summary>
		/// <returns></returns>
		public Vector3D Negate()
		{
			Vector3D result = new(this);
			result.SetNegate();
			return result;
		}
		#endregion

		#region Operators

		/// <summary>
		/// Implicit conversion from Point3D to Vector3D.
		/// </summary>
		/// <param name="point3D"></param>
		public static implicit operator Vector3D(Point3D point3D) => new(point3D.X, point3D.Y, point3D.Z);

		/// <summary>
		/// Somma tra due vettori.
		/// </summary>
		public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		/// <summary>
		/// Differenza tra due vettori.
		/// </summary>
		public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

		/// <summary>
		/// Cambia segno a tutte le componenti
		/// </summary>
		/// <param name="left"></param>
		/// <returns></returns>
		public static Vector3D operator -(Vector3D left) => new(-left.X, -left.Y, -left.Z);
		/// <summary>
		/// Moltiplicazione per scalare.
		/// </summary>
		public static Vector3D operator *(Vector3D v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);
		/// <summary>
		/// Moltiplicazione per scalare (commutativa).
		/// </summary>
		public static Vector3D operator *(double scalar, Vector3D v) => v * scalar;
		/// <summary>
		/// Divisione per scalare.
		/// </summary>
		public static Vector3D operator /(Vector3D v, double scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar);
		#endregion
	}
}
