using System.Collections.Generic;

namespace Axiom.GeoMath
{
    /// <summary>
    /// Classe per rappresentare un nodo di un Octree.
    /// </summary>
    public class OctreeNode<T> where T : class, IPointWeighted
    {
        #region Fields
        /// <summary>
        /// Numero massimo di voci prima della suddivisione
        /// </summary>
        private const int MaxEntries = 4;

        /// <summary>
        /// Profondità massima dell'albero. Oltre questo livello un nodo resta foglia e accumula più
        /// di <see cref="MaxEntries"/> voci, così da evitare la ricorsione infinita quando più di
        /// <see cref="MaxEntries"/> voci hanno la stessa posizione (o cadono in un box che collassa).
        /// </summary>
        private const int MaxDepth = 24;

        /// <summary>
        /// Lista delle voci contenute in questo nodo
        /// </summary>
        private List<T> _entries = new List<T>();

        /// <summary>
        /// Figli del nodo (8 ottanti)
        /// </summary>
        private OctreeNode<T>[] _children;

        /// <summary>
        /// Livello di profondità del nodo nell'albero (0 = radice).
        /// </summary>
        private readonly int _depth;

        #endregion

        #region Properties
        public AABBox3D Boundary { get; private set; }

        /// <summary>
        /// Peso totale delle voci contenute in questo nodo
        /// </summary>
        public double TotalWeight { get; private set; }

        /// <summary>
        /// Posizione del baricentro pesato delle voci contenute in questo nodo
        /// </summary>
        public Vector3D WeightedCenter { get; private set; }

        /// <summary>
        /// Foglia (senza figli)
        /// </summary>
        public bool IsLeaf => _children == null;

        /// <summary>
        /// Entries contenute in questo nodo
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetEntries() => _entries;

        /// <summary>
        /// Children di questo nodo
        /// </summary>
        /// <returns></returns>
        public OctreeNode<T>[] GetChildren() => _children;
        #endregion

        #region Constructors
        public OctreeNode(AABBox3D boundary) : this(boundary, 0)
        {
        }

        private OctreeNode(AABBox3D boundary, int depth)
        {
            Boundary = boundary;
            WeightedCenter = Vector3D.Zero;
            TotalWeight = 0;
            _depth = depth;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Trova tutte le entità che si trovano entro una determinata distanza da un centro.
        /// </summary>
        public void GetEntitiesInRange(Point3D center, double radius, List<T> results)
        {
            // 1. Se il box del nodo non interseca la sfera di ricerca, salta tutto il ramo
            if (!Boundary.IntersectsSphere(center, radius))
                return;

            // 2. Se è una foglia, controlla le singole entità
            if (IsLeaf)
            {
                foreach (var entry in _entries)
                {
                    double dist = (entry.Position - (Vector3D)center).Length;
                    if (dist <= radius)
                    {
                        results.Add(entry);
                    }
                }
            }
            else
            {
                // 3. Altrimenti scendi ricorsivamente nei figli
                foreach (var child in _children)
                {
                    child.GetEntitiesInRange(center, radius, results);
                }
            }
        }

        public void Insert(T entry)
        {
            if (!Boundary.Contains(entry.Position)) return;

            // Aggiornamento del baricentro pesato
            UpdateWeights(entry);

            // Foglia con spazio disponibile, oppure profondità massima raggiunta:
            // accumula la voce qui (evita la ricorsione infinita su voci coincidenti).
            if (IsLeaf && (_entries.Count < MaxEntries || _depth >= MaxDepth))
            {
                _entries.Add(entry);
                return;
            }

            if (IsLeaf) Subdivide();

            // La voce viene inserita in UN SOLO figlio: il primo il cui box la contiene.
            // Questo evita il doppio inserimento delle voci che giacciono sui piani di
            // suddivisione (dove più ottanti adiacenti risultano "contenerle").
            InsertIntoChild(entry);
        }

        /// <summary>
        /// Inserisce la voce nel primo figlio il cui box la contiene (assegnazione univoca).
        /// </summary>
        private void InsertIntoChild(T entry)
        {
            foreach (var child in _children)
            {
                if (child.Boundary.Contains(entry.Position))
                {
                    child.Insert(entry);
                    return;
                }
            }
        }

        private void UpdateWeights(T entry)
        {
            double newTotalWeight = TotalWeight + entry.Weight;
            if (newTotalWeight > 0)
            {
                // Media pesata: (Posizione * Peso + VecchioBaricentro * VecchioPesoTotale) / NuovoPesoTotale
                WeightedCenter = (WeightedCenter * TotalWeight + (Vector3D)entry.Position * entry.Weight) / newTotalWeight;
            }
            TotalWeight = newTotalWeight;
        }

        private void Subdivide()
        {
            _children = new OctreeNode<T>[8];
            int childDepth = _depth + 1;
            // Utilizziamo le proprietà LX, LY, LZ e Center del tuo AABBox3D per dividere
            Point3D min = Boundary.MinPoint;
            Point3D max = Boundary.MaxPoint;
            Point3D mid = Boundary.Center;

            // Costruzione degli 8 figli (Ottanti)
            // Sfruttiamo i punti medi per creare i nuovi AABBox3D
            _children[0] = new OctreeNode<T>(new AABBox3D(min, mid), childDepth);
            _children[1] = new OctreeNode<T>(new AABBox3D(new Point3D(mid.X, min.Y, min.Z), new Point3D(max.X, mid.Y, mid.Z)), childDepth);
            _children[2] = new OctreeNode<T>(new AABBox3D(new Point3D(min.X, mid.Y, min.Z), new Point3D(mid.X, max.Y, mid.Z)), childDepth);
            _children[3] = new OctreeNode<T>(new AABBox3D(new Point3D(mid.X, mid.Y, min.Z), new Point3D(max.X, max.Y, mid.Z)), childDepth);
            _children[4] = new OctreeNode<T>(new AABBox3D(new Point3D(min.X, min.Y, mid.Z), new Point3D(mid.X, mid.Y, max.Z)), childDepth);
            _children[5] = new OctreeNode<T>(new AABBox3D(new Point3D(mid.X, min.Y, mid.Z), new Point3D(max.X, mid.Y, max.Z)), childDepth);
            _children[6] = new OctreeNode<T>(new AABBox3D(new Point3D(min.X, mid.Y, mid.Z), new Point3D(mid.X, max.Y, max.Z)), childDepth);
            _children[7] = new OctreeNode<T>(new AABBox3D(mid, max), childDepth);

            // Ridistribuzione delle voci: ciascuna in UN SOLO figlio (assegnazione univoca).
            foreach (var e in _entries)
                InsertIntoChild(e);

            _entries.Clear();
        }
        #endregion
    }
}
