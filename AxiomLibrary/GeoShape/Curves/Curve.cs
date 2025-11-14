using Axiom.GeoMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.GeoShape.Curves2D
{
	public abstract class Curve : ICloneable
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Punto di start. 
		/// Sola lettura.
		/// </summary>
		public abstract Point3D StartPoint { get; }

		/// <summary>
		/// Punto di end. 
		/// Sola lettura.
		/// </summary>
		public abstract Point3D EndPoint { get; }

		/// <summary>
		/// Tangente di start. 
		/// Sola lettura.
		/// </summary>
		public abstract Vector3D StartTangent { get; }

		/// <summary>
		/// Tangente di end. 
		/// Sola lettura.
		/// </summary>
		public abstract Vector3D EndTangent { get; }

		/// <summary>
		/// Lunghezza della curva. 
		/// Sola lettura.
		/// </summary>
		public abstract double Length { get; }
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore
		/// </summary>
		public Curve()
		{
		}
		#endregion

		#region Methods
		/// <summary>
		/// Clona la curva
		/// </summary>
		public abstract Curve Clone();

		/// <summary>
		/// Clona i dati di Curve nel parametro target
		/// </summary>
		protected virtual void CloneTo(Curve target) { }

		/// <summary>
		/// Distanza punto - curva
		/// </summary>
		/// <param name="point">Punto</param>
		public abstract double Dist(Point3D point);

		/// <summary>
		/// Confronta le due curve considerando la tolleranza di default
		/// </summary>
		public abstract bool IsEquals(Curve curve);

		/// <summary>
		/// Confronta le due curve considerando la tolleranza indicata
		/// </summary>
		public abstract bool IsEquals(Curve curve, double tolerance);

		/// <summary>
		/// Evaluate percentuale. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la percentuale da percorrere a partire dal punto di start. 
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e 1 il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <returns></returns>
		public Point3D Evaluate(double offset) => Evaluate(offset, out _);

		/// <summary>
		/// Evaluate percentuale. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la percentuale da percorrere a partire dal punto di start. 
		/// Restituisce inoltre la tangente al punto valutato.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e 1 il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <param name="tangent">Tangente</param>
		/// <returns></returns>
		public abstract Point3D Evaluate(double offset, out Vector3D tangent);

		/// <summary>
		/// Evaluate assoluto. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la lunghezza da percorrere a partire dal punto di start. 
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e Length il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <returns></returns>
		public Point3D EvaluateAbs(double offset) => EvaluateAbs(offset, out _);

		/// <summary>
		/// Evaluate assoluto. 
		/// Restituisce un punto che appartiene alla curva oppure al suo prolungamento 
		/// in base al parametro che indica la lunghezza da percorrere a partire dal punto di start. 
		/// Restituisce inoltre la tangente al punto valutato.
		/// </summary>
		/// <param name="offset">Se viene passato un valore tra 0 e Length il punto apparterrà alla curva altrimenti ai prolungamenti</param>
		/// <param name="tangent">Tangente</param>
		/// <returns></returns>
		public abstract Point3D EvaluateAbs(double offset, out Vector3D tangent);

		/// <summary>
		/// Valuta se il punto passato appartiene alla curva.
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <returns></returns>
		public bool IsOnCurve(Point3D point) => IsOnCurve(point, MathUtils.FineTolerance);

		/// <summary>
		/// Valuta se il punto passato appartiene alla curva.
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="tolerance">Tolleranza</param>
		/// <returns></returns>
		public bool IsOnCurve(Point3D point, double tolerance) => IsOnCurve(point, tolerance, out _);

		/// <summary>
		/// Valuta se il punto passato appartiene alla curva. 
		/// Restituisce inoltre il valore percentuale (0 - 1) che rappresenta la distanza dal punto di start. 
		/// Se non appartiene alla curva restituisce un valore negativo (-1).
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="offset">In uscita: distanza percentuale dal punto di start</param>
		/// <returns></returns>
		public bool IsOnCurve(Point3D point, out double offset) => IsOnCurve(point, MathUtils.FineTolerance, out offset);

		/// <summary>
		/// Valuta se il punto passato appartiene alla curva. 
		/// Restituisce inoltre il valore percentuale (0 - 1) che rappresenta la distanza dal punto di start. 
		/// Se non appartiene alla curva restituisce un valore negativo (-1).
		/// </summary>
		/// <param name="point">Punto da valutare</param>
		/// <param name="tolerance">Tolleranza</param>
		/// <param name="offset">In uscita: distanza percentuale dal punto di start</param>
		/// <returns></returns>
		public abstract bool IsOnCurve(Point3D point, double tolerance, out double offset);

		/// <summary>
		/// Trasla la curva della quantità indicata
		/// </summary>
		/// <param name="traslation">Vettore che indica la traslazione</param>
		public abstract void Move(Vector3D traslation);

		/// <summary>
		/// Ruota e trasla la curva
		/// La componente Z viene ignorata
		/// </summary>
		/// <param name="matrix"></param>
		public abstract void ApplyRT(RTMatrix matrix);

		/// <summary>
		/// Restituisce la curva scalata
		/// </summary>
		/// <param name="factor"></param>
		public abstract Curve Scale(double factor);

		/// <summary>
		/// Restituisce la curva invertita
		/// </summary>
		/// <returns></returns>
		public abstract Curve Inverse();

		/// <summary>
		/// Inverte la curva
		/// </summary>
		/// <returns></returns>
		public abstract void SetInverse();

		/// <summary>
		/// Restituisce una parte della curva. 
		/// Se uno dei 2 parametri è minore di zero o maggiore della lunghezza 
		/// viene restituito null
		/// </summary>
		/// <param name="start">Inizio: distanza dal punto di start</param>
		/// <param name="end">Fine: distanza dal punto di start</param>
		/// <returns></returns>
		public abstract Curve Trim(double start, double end);

		/// <summary>
		/// Restituisce la lunghezza della curva tra i due offset
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public virtual double LengthFromTo(double start, double end) => Trim(start * Length, end * Length).Length;
		
		/// <summary>
		/// Restituisce l'ABBox2D corrispondente
		/// </summary>
		/// <returns></returns>
		public abstract AABBox3D GetABBox();

		/// <summary>
		/// Restituisce una curva clone specchiata sull'asse X
		/// </summary>
		/// <returns></returns>
		public abstract Curve MirrorX();

		/// <summary>
		/// Restituisce una curva clone specchiata sull'asse Y
		/// </summary>
		/// <returns></returns>
		public abstract Curve MirrorY();
		#endregion

		#region ICloneable Members

		object ICloneable.Clone() => Clone();
		#endregion

	}
}
