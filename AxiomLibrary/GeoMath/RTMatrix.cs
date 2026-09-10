using System;
using System.Runtime.Serialization;
using System.Text;

namespace Axiom.GeoMath
{

    /// <summary>
    /// Matrice di roto-traslazione 4x4 <b>immutabile</b> (readonly struct): gli elementi non cambiano
    /// dopo la costruzione, tutte le operazioni restituiscono una NUOVA istanza. Per "modificare" un
    /// elemento/la traslazione/gli assi/la rotazione si usano i metodi <c>With*</c>.
    /// </summary>
    /// <remarks>
    /// Essendo uno struct non può essere <c>null</c>; <c>default(RTMatrix)</c> è la matrice nulla
    /// (<see cref="Zero"/>), come il vecchio costruttore senza parametri.
    /// </remarks>
    [DataContract]
    public readonly struct RTMatrix : IEquatable<RTMatrix>
    {
        #region Fields
        // Prima riga
        [DataMember] private readonly double _m11, _m12, _m13, _m14;
        // Seconda riga
        [DataMember] private readonly double _m21, _m22, _m23, _m24;
        // Terza riga
        [DataMember] private readonly double _m31, _m32, _m33, _m34;
        // quarta riga
        [DataMember] private readonly double _m41, _m42, _m43, _m44;
        #endregion

        #region Constructors
        /// <summary>
        /// Matrice di rota-traslazione (copia).
        /// </summary>
        public RTMatrix(RTMatrix matrix)
            : this(matrix._m11, matrix._m12, matrix._m13, matrix._m14,
                  matrix._m21, matrix._m22, matrix._m23, matrix._m24,
                  matrix._m31, matrix._m32, matrix._m33, matrix._m34,
                  matrix._m41, matrix._m42, matrix._m43, matrix._m44)
        {
        }

        /// <summary>
        /// Costruttore con i vettori (colonne X, Y, Z + traslazione).
        /// </summary>
        public RTMatrix(Vector3D x, Vector3D y, Vector3D z, Vector3D trasl)
            : this(x.X, y.X, z.X, trasl.X,
                  x.Y, y.Y, z.Y, trasl.Y,
                  x.Z, y.Z, z.Z, trasl.Z,
                  0, 0, 0, 1)
        {
        }

        /// <summary>
        /// Costruttore da array di 16 valori (riga per riga).
        /// </summary>
        public RTMatrix(double[] values)
            : this(values[0], values[1], values[2], values[3],
                  values[4], values[5], values[6], values[7],
                  values[8], values[9], values[10], values[11],
                  values[12], values[13], values[14], values[15])
        {
        }

        /// <summary>
        /// Costruttore con i 16 elementi (riga per riga).
        /// </summary>
        public RTMatrix(double m11, double m12, double m13, double m14,
                        double m21, double m22, double m23, double m24,
                        double m31, double m32, double m33, double m34,
                        double m41, double m42, double m43, double m44)
        {
            _m11 = m11; _m12 = m12; _m13 = m13; _m14 = m14;
            _m21 = m21; _m22 = m22; _m23 = m23; _m24 = m24;
            _m31 = m31; _m32 = m32; _m33 = m33; _m34 = m34;
            _m41 = m41; _m42 = m42; _m43 = m43; _m44 = m44;
        }
        #endregion

        #region STATICS
        /// <summary>
        /// Matrice identità
        /// </summary>
        public static RTMatrix Identity => new RTMatrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

        /// <summary>
        /// Matrice nulla
        /// </summary>
        public static RTMatrix Zero => new RTMatrix(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Dati tre angoli di rotazione rispetto ai tre assi cartesiani.
        /// Composizione rotazione Z * Y * X (convenzionale in robotica).
        /// Vengono chiamati anche angoli RPY: roll (x), pitch (y), yaw (z).
        /// </summary>
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

            return new RTMatrix(m11, m12, m13, 0,
                                m21, m22, m23, 0,
                                m31, m32, m33, 0,
                                0, 0, 0, 1);
        }

        /// <summary>
        /// Dati due angoli di rotazione rispetto agli assi Z X in terna corrente.
        /// Che poi equivale ai due angoli in X Z in terna fissa.
        /// </summary>
        public static RTMatrix FromEulerAnglesZX(double zRadAngle, double xRadAngle)
        {
            double cosZ = Math.Cos(zRadAngle);
            double sinZ = Math.Sin(zRadAngle);
            double cosX = Math.Cos(xRadAngle);
            double sinX = Math.Sin(xRadAngle);

            return new RTMatrix(cosZ, -sinZ * cosX, sinZ * sinX, 0,
                                sinZ, cosZ * cosX, -cosZ * sinX, 0,
                                0, sinX, cosX, 0,
                                0, 0, 0, 1);
        }

        /// <summary>
        /// Dati 3 vettori (colonne X, Y, Z) e la traslazione.
        /// </summary>
        public static RTMatrix FromVectors(Vector3D x, Vector3D y, Vector3D z, Vector3D trasl) => new(x, y, z, trasl);

        /// <summary>
        /// Dati 3 vettori (colonne X, Y, Z), traslazione nulla.
        /// </summary>
        public static RTMatrix FromVectors(Vector3D x, Vector3D y, Vector3D z) => new(x, y, z, Vector3D.Zero);

        /// <summary>
        /// Identità con traslazione.
        /// </summary>
        public static RTMatrix FromTraslation(Vector3D trasl) =>
            new RTMatrix(1, 0, 0, trasl.X,
                         0, 1, 0, trasl.Y,
                         0, 0, 1, trasl.Z,
                         0, 0, 0, 1);

        /// <summary>
        /// Data normale e traslazione. Con vettori x e y ottenuti in maniera arbitraria.
        /// </summary>
        public static RTMatrix FromNormal(Vector3D normal, Vector3D trasl)
        {
            // Normale nulla: nessuna rotazione definita -> solo traslazione (rotazione identità).
            if (!normal.TryNormalize(out Vector3D n))
                return FromTraslation(trasl);

            Vector3D x = n.Perpendicular();
            Vector3D y = n.Cross(x);
            return new RTMatrix(x, y, n, trasl);
        }

        /// <summary>
        /// Data normale (traslazione nulla).
        /// </summary>
        public static RTMatrix FromNormal(Vector3D normal) => FromNormal(normal, Vector3D.Zero);
        #endregion STATICS

        #region Serialized Properties
        /// <summary>
        /// Proprietà compatta (sola lettura) con i 16 valori riga per riga.
        /// </summary>
        public double[] Values => new double[]
        {
            _m11, _m12, _m13, _m14,
            _m21, _m22, _m23, _m24,
            _m31, _m32, _m33, _m34,
            _m41, _m42, _m43, _m44
        };
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
        /// Parte Scale della matrice (diagonale), sola lettura.
        /// </summary>
        public Vector3D Scale => new Vector3D(_m11, _m22, _m33);

        /// <summary>
        /// Parte Translation della matrice (colonna di indice 4), sola lettura.
        /// Per impostarla usare <see cref="WithTranslation"/>.
        /// </summary>
        public Vector3D Translation => new Vector3D(_m14, _m24, _m34);

        /// <summary>
        /// Colonna di indice 1 (asse X), sola lettura.
        /// </summary>
        public Vector3D XVector => GetVector(0);

        /// <summary>
        /// Colonna di indice 2 (asse Y), sola lettura.
        /// </summary>
        public Vector3D YVector => GetVector(1);

        /// <summary>
        /// Colonna di indice 3 (asse Z), sola lettura.
        /// </summary>
        public Vector3D ZVector => GetVector(2);
        #endregion

        #region With (immutabilità)
        /// <summary>
        /// Restituisce una copia con l'elemento (index 0-15, riga per riga) modificato.
        /// Indice fuori range: restituisce la matrice invariata.
        /// </summary>
        public RTMatrix WithElement(int index, double value)
        {
            if (index < 0 || index > 15)
                return this;
            double[] v = Values;
            v[index] = value;
            return new RTMatrix(v);
        }

        /// <summary>
        /// Restituisce una copia con l'elemento (riga, colonna) modificato.
        /// </summary>
        public RTMatrix WithElement(int row, int col, double value) => WithElement(row * 4 + col, value);

        /// <summary>
        /// Restituisce una copia con la traslazione indicata (colonna 4).
        /// </summary>
        public RTMatrix WithTranslation(Vector3D trasl) =>
            new RTMatrix(_m11, _m12, _m13, trasl.X,
                         _m21, _m22, _m23, trasl.Y,
                         _m31, _m32, _m33, trasl.Z,
                         _m41, _m42, _m43, _m44);

        /// <summary>
        /// Restituisce una copia con la sottomatrice 3x3 impostata dalle colonne (assi X, Y, Z),
        /// mantenendo la traslazione e la quarta riga.
        /// </summary>
        public RTMatrix WithAxes(Vector3D xAxis, Vector3D yAxis, Vector3D zAxis) =>
            new RTMatrix(xAxis.X, yAxis.X, zAxis.X, _m14,
                         xAxis.Y, yAxis.Y, zAxis.Y, _m24,
                         xAxis.Z, yAxis.Z, zAxis.Z, _m34,
                         _m41, _m42, _m43, _m44);

        /// <summary>
        /// Restituisce una copia con la rotazione (angoli di Eulero X Y Z) indicata, mantenendo la
        /// traslazione corrente.
        /// </summary>
        public RTMatrix WithRotation(double xRadAngle, double yRadAngle, double zRadAngle) =>
            FromEulerAnglesXYZ(xRadAngle, yRadAngle, zRadAngle).WithTranslation(Translation);

        /// <summary>
        /// Restituisce una copia con la parte Scale (diagonale m11/m22/m33) indicata.
        /// </summary>
        public RTMatrix WithScale(Vector3D scale) =>
            new RTMatrix(scale.X, _m12, _m13, _m14,
                         _m21, scale.Y, _m23, _m24,
                         _m31, _m32, scale.Z, _m34,
                         _m41, _m42, _m43, _m44);

        /// <summary>
        /// Restituisce una copia con la colonna col (righe 0-2) impostata al vettore indicato.
        /// </summary>
        public RTMatrix WithVector(int col, Vector3D vector) =>
            WithElement(0, col, vector.X).WithElement(1, col, vector.Y).WithElement(2, col, vector.Z);
        #endregion

        #region Indexers
        /// <summary>
        /// Accesso in sola lettura tramite 2 indici (riga, colonna 0-base). Per modificare usare <see cref="WithElement(int,int,double)"/>.
        /// </summary>
        public double this[int row, int col] => this[row * 4 + col];

        /// <summary>
        /// Accesso in sola lettura tramite un indice (0-base). Per modificare usare <see cref="WithElement(int,double)"/>.
        /// </summary>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _m11;
                    case 1: return _m12;
                    case 2: return _m13;
                    case 3: return _m14;
                    case 4: return _m21;
                    case 5: return _m22;
                    case 6: return _m23;
                    case 7: return _m24;
                    case 8: return _m31;
                    case 9: return _m32;
                    case 10: return _m33;
                    case 11: return _m34;
                    case 12: return _m41;
                    case 13: return _m42;
                    case 14: return _m43;
                    case 15: return _m44;
                    default: return 0;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Aggiunge una traslazione x,y,z (restituisce una nuova matrice).
        /// </summary>
        public RTMatrix Traslate(double x, double y, double z) =>
            new RTMatrix(_m11, _m12, _m13, _m14 + x,
                         _m21, _m22, _m23, _m24 + y,
                         _m31, _m32, _m33, _m34 + z,
                         _m41, _m42, _m43, _m44);

        /// <summary>
        /// Indica se la matrice ha qualche valore NaN.
        /// </summary>
        public bool IsNaN() =>
                double.IsNaN(_m11) || double.IsNaN(_m12) || double.IsNaN(_m13) || double.IsNaN(_m14) ||
                double.IsNaN(_m21) || double.IsNaN(_m22) || double.IsNaN(_m23) || double.IsNaN(_m24) ||
                double.IsNaN(_m31) || double.IsNaN(_m32) || double.IsNaN(_m33) || double.IsNaN(_m34) ||
                double.IsNaN(_m41) || double.IsNaN(_m42) || double.IsNaN(_m43) || double.IsNaN(_m44);

        /// <summary>
        /// Esegue una copia della matrice.
        /// </summary>
        public RTMatrix Clone() => new RTMatrix(this);

        /// <summary>
        /// Controlla se due matrici sono uguali con una certa tolleranza.
        /// </summary>
        public bool IsEquals(object obj)
        {
            if (obj is RTMatrix other)
            {
                bool row1 = _m11.IsEquals(other._m11) && _m12.IsEquals(other._m12) && _m13.IsEquals(other._m13) && _m14.IsEquals(other._m14);
                bool row2 = _m21.IsEquals(other._m21) && _m22.IsEquals(other._m22) && _m23.IsEquals(other._m23) && _m24.IsEquals(other._m24);
                bool row3 = _m31.IsEquals(other._m31) && _m32.IsEquals(other._m32) && _m33.IsEquals(other._m33) && _m34.IsEquals(other._m34);
                bool row4 = _m41.IsEquals(other._m41) && _m42.IsEquals(other._m42) && _m43.IsEquals(other._m43) && _m44.IsEquals(other._m44);
                return row1 && row2 && row3 && row4;
            }
            return false;
        }

        /// <summary>
        /// Uguaglianza esatta (value type).
        /// </summary>
        public bool Equals(RTMatrix other) => this == other;

        /// <summary>
        /// Uguaglianza esatta.
        /// </summary>
        public override bool Equals(object obj) => obj is RTMatrix other && this == other;

        /// <summary>
        /// HashCode (coerente con l'uguaglianza esatta ==).
        /// </summary>
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
        public RTMatrix Add(RTMatrix matrix) => this + matrix;

        /// <summary>
        /// Sottrazione elemento per elemento
        /// </summary>
        public RTMatrix Subtract(RTMatrix matrix) => this - matrix;

        /// <summary>
        /// Negazione elemento per elemento
        /// </summary>
        public RTMatrix Negate() => -this;
        #endregion

        #region Multiply
        /// <summary>
        /// Moltiplicazione matrice-matrice
        /// </summary>
        public RTMatrix Multiply(RTMatrix matrix) => this * matrix;

        /// <summary>
        /// Applicazione della matrice a un punto 3D
        /// </summary>
        public Point3D Multiply(Point3D point) => this * point;

        /// <summary>
        /// Applicazione della matrice a un vettore 3D
        /// </summary>
        public Vector3D Multiply(Vector3D vector) => this * vector;

        /// <summary>
        /// Moltiplicazione per uno scalare
        /// </summary>
        public RTMatrix Multiply(double scalar) => this * scalar;
        #endregion

        #region Public Methods
        /// <summary>
        /// Inversa
        /// </summary>
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
        /// Inversa per matrici che contengono solo roto traslazioni.
        /// </summary>
        public RTMatrix InverseRT()
        {
            // La sottomatrice 3x3 inversa è la trasposta.
            double r11 = _m11, r12 = _m21, r13 = _m31;
            double r21 = _m12, r22 = _m22, r23 = _m32;
            double r31 = _m13, r32 = _m23, r33 = _m33;
            double r14 = -(r11 * _m14 + r12 * _m24 + r13 * _m34);
            double r24 = -(r21 * _m14 + r22 * _m24 + r23 * _m34);
            double r34 = -(r31 * _m14 + r32 * _m24 + r33 * _m34);

            return new RTMatrix(r11, r12, r13, r14,
                                r21, r22, r23, r24,
                                r31, r32, r33, r34,
                                0, 0, 0, 1);
        }

        /// <summary>
        /// Trasposta
        /// </summary>
        public RTMatrix Transpose() =>
            new RTMatrix(_m11, _m21, _m31, _m41,
                         _m12, _m22, _m32, _m42,
                         _m13, _m23, _m33, _m43,
                         _m14, _m24, _m34, _m44);

        /// <summary>
        /// Effettua una trasformazione per ruotare attorno a un asse specificato da un punto e un vettore.
        /// </summary>
        public RTMatrix Transform(Point3D origin, Vector3D axisDirection, double radAngle)
        {
            RTMatrix trasformInverse = RTMatrix.FromNormal(axisDirection, origin.ToVector());
            RTMatrix trasform = trasformInverse.Inverse();
            RTMatrix rotate = RTMatrix.FromEulerAnglesXYZ(0, 0, radAngle);
            return trasformInverse * rotate * trasform * this;
        }

        /// <summary>
        /// Restituisce i tre angoli di rotazione rispetto ai tre assi cartesiani, nell'ordine X, Y, Z.
        /// </summary>
        public void ToEulerAnglesXYZ(bool simmetricRange, out double xRadAngle, out double yRadAngle, out double zRadAngle)
        {
            if (_m11.IsEquals(0) && _m21.IsEquals(0))
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
        /// N.B. Considera solo la normale Z, cioè la terza colonna della matrice.
        /// </summary>
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

        /// <summary>
        /// Get del vettore della colonna i-esima.
        /// </summary>
        public Vector3D GetVector(int col) => new Vector3D(this[0, col], this[1, col], this[2, col]);
        #endregion

        #region OPERATORS
        /// <summary>
        /// Moltiplicazione matrice-matrice
        /// </summary>
        public static RTMatrix operator *(RTMatrix left, RTMatrix right)
        {
            return new RTMatrix(
                left._m11 * right._m11 + left._m12 * right._m21 + left._m13 * right._m31 + left._m14 * right._m41,
                left._m11 * right._m12 + left._m12 * right._m22 + left._m13 * right._m32 + left._m14 * right._m42,
                left._m11 * right._m13 + left._m12 * right._m23 + left._m13 * right._m33 + left._m14 * right._m43,
                left._m11 * right._m14 + left._m12 * right._m24 + left._m13 * right._m34 + left._m14 * right._m44,

                left._m21 * right._m11 + left._m22 * right._m21 + left._m23 * right._m31 + left._m24 * right._m41,
                left._m21 * right._m12 + left._m22 * right._m22 + left._m23 * right._m32 + left._m24 * right._m42,
                left._m21 * right._m13 + left._m22 * right._m23 + left._m23 * right._m33 + left._m24 * right._m43,
                left._m21 * right._m14 + left._m22 * right._m24 + left._m23 * right._m34 + left._m24 * right._m44,

                left._m31 * right._m11 + left._m32 * right._m21 + left._m33 * right._m31 + left._m34 * right._m41,
                left._m31 * right._m12 + left._m32 * right._m22 + left._m33 * right._m32 + left._m34 * right._m42,
                left._m31 * right._m13 + left._m32 * right._m23 + left._m33 * right._m33 + left._m34 * right._m43,
                left._m31 * right._m14 + left._m32 * right._m24 + left._m33 * right._m34 + left._m34 * right._m44,

                left._m41 * right._m11 + left._m42 * right._m21 + left._m43 * right._m31 + left._m44 * right._m41,
                left._m41 * right._m12 + left._m42 * right._m22 + left._m43 * right._m32 + left._m44 * right._m42,
                left._m41 * right._m13 + left._m42 * right._m23 + left._m43 * right._m33 + left._m44 * right._m43,
                left._m41 * right._m14 + left._m42 * right._m24 + left._m43 * right._m34 + left._m44 * right._m44);
        }

        /// <summary>
        /// Applicazione della matrice a un vettore 3D (solo rotazione, senza traslazione).
        /// </summary>
        public static Vector3D operator *(RTMatrix matrix, Vector3D vector) => new Vector3D(
            (matrix._m11 * vector.X) + (matrix._m12 * vector.Y) + (matrix._m13 * vector.Z),
            (matrix._m21 * vector.X) + (matrix._m22 * vector.Y) + (matrix._m23 * vector.Z),
            (matrix._m31 * vector.X) + (matrix._m32 * vector.Y) + (matrix._m33 * vector.Z));

        /// <summary>
        /// Applicazione della matrice a un punto 3D (rotazione + traslazione).
        /// </summary>
        public static Point3D operator *(RTMatrix matrix, Point3D point) => new Point3D(
            (matrix._m11 * point.X) + (matrix._m12 * point.Y) + (matrix._m13 * point.Z) + matrix._m14,
            (matrix._m21 * point.X) + (matrix._m22 * point.Y) + (matrix._m23 * point.Z) + matrix._m24,
            (matrix._m31 * point.X) + (matrix._m32 * point.Y) + (matrix._m33 * point.Z) + matrix._m34);

        /// <summary>
        /// Moltiplicazione per uno scalare
        /// </summary>
        public static RTMatrix operator *(RTMatrix left, double scalar) =>
            new RTMatrix(left._m11 * scalar, left._m12 * scalar, left._m13 * scalar, left._m14 * scalar,
                         left._m21 * scalar, left._m22 * scalar, left._m23 * scalar, left._m24 * scalar,
                         left._m31 * scalar, left._m32 * scalar, left._m33 * scalar, left._m34 * scalar,
                         left._m41 * scalar, left._m42 * scalar, left._m43 * scalar, left._m44 * scalar);

        /// <summary>
        /// Somma elemento per elemento
        /// </summary>
        public static RTMatrix operator +(RTMatrix left, RTMatrix right) =>
            new RTMatrix(left._m11 + right._m11, left._m12 + right._m12, left._m13 + right._m13, left._m14 + right._m14,
                         left._m21 + right._m21, left._m22 + right._m22, left._m23 + right._m23, left._m24 + right._m24,
                         left._m31 + right._m31, left._m32 + right._m32, left._m33 + right._m33, left._m34 + right._m34,
                         left._m41 + right._m41, left._m42 + right._m42, left._m43 + right._m43, left._m44 + right._m44);

        /// <summary>
        /// Sottrazione elemento per elemento
        /// </summary>
        public static RTMatrix operator -(RTMatrix left, RTMatrix right) =>
            new RTMatrix(left._m11 - right._m11, left._m12 - right._m12, left._m13 - right._m13, left._m14 - right._m14,
                         left._m21 - right._m21, left._m22 - right._m22, left._m23 - right._m23, left._m24 - right._m24,
                         left._m31 - right._m31, left._m32 - right._m32, left._m33 - right._m33, left._m34 - right._m34,
                         left._m41 - right._m41, left._m42 - right._m42, left._m43 - right._m43, left._m44 - right._m44);

        /// <summary>
        /// Negazione elemento per elemento
        /// </summary>
        public static RTMatrix operator -(RTMatrix matrix) =>
            new RTMatrix(-matrix._m11, -matrix._m12, -matrix._m13, -matrix._m14,
                         -matrix._m21, -matrix._m22, -matrix._m23, -matrix._m24,
                         -matrix._m31, -matrix._m32, -matrix._m33, -matrix._m34,
                         -matrix._m41, -matrix._m42, -matrix._m43, -matrix._m44);

        /// <summary>
        /// Uguaglianza esatta elemento per elemento
        /// </summary>
        public static bool operator ==(RTMatrix left, RTMatrix right) =>
            left._m11 == right._m11 && left._m12 == right._m12 && left._m13 == right._m13 && left._m14 == right._m14 &&
            left._m21 == right._m21 && left._m22 == right._m22 && left._m23 == right._m23 && left._m24 == right._m24 &&
            left._m31 == right._m31 && left._m32 == right._m32 && left._m33 == right._m33 && left._m34 == right._m34 &&
            left._m41 == right._m41 && left._m42 == right._m42 && left._m43 == right._m43 && left._m44 == right._m44;

        /// <summary>
        /// Disuguaglianza esatta elemento per elemento
        /// </summary>
        public static bool operator !=(RTMatrix left, RTMatrix right) => !(left == right);
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
        #endregion

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
