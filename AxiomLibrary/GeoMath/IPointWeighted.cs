namespace Axiom.GeoMath
{
    /// <summary>
    /// Rappresenta un elemento che ha una posizione spaziale e un valore di influenza (Peso).
    /// </summary>
    public interface IPointWeighted
    {
        /// <summary>
        /// Posizione
        /// </summary>
        Vector3D Position { get; }

        /// <summary>
        /// Il valore che determina quanto questo punto influenzi il centroide del nodo.
        /// In fisica è la Massa, in ottica l'Intensità, nell'AI la Densità.
        /// </summary>
        double Weight { get; }
    }
}
