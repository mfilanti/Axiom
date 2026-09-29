# Architettura GeoMath + GeoShape (Vista Logica)

## Scopo
Questo documento descrive l’architettura logica dei progetti `Axiom.GeoMath` e `Axiom.GeoShape`.
Si concentra su ruoli, responsabilità e relazioni tra entità (non sulle singole proprietà).

---

## 1. Architettura a Livello di Progetto

### Axiom.GeoMath
`GeoMath` è il kernel geometrico/matematico. Fornisce:
- Primitive 3D per punti, vettori, matrici e bounding box allineati agli assi.
- Utilità numeriche e confronti con tolleranza.
- Supporto alla partizione spaziale tramite octree.

### Axiom.GeoShape
`GeoShape` è il livello di modellazione geometrica costruito sopra `GeoMath`.
Fornisce:
- Entità geometriche astratte e concrete (curve, solidi, superfici, mesh).
- Composizione scene-graph (`Node3D`) e flusso di aggiornamento parametrico.
- Pipeline di conversione da entità analitiche a mesh triangolate.
- Estensioni di utilità per approssimazione, export, integrazione triangolazione ed estrazione visibilità.

### Direzione delle Dipendenze
- `GeoShape` dipende da `GeoMath`.
- `GeoMath` non dipende da `GeoShape` (eccetto il namespace di `AABBox3D` in `GeoShape.Elements`, ma il tipo è fisicamente nel progetto GeoMath e usato come box di base).

---

## 2. Componenti Logici di GeoMath

## 2.1 Fondazione Numerica
- **MathUtils**: costanti centrali e helper matematici (conversione angoli, tolleranze, swap generico).
- **MathExtensions**: confronti con tolleranza ed estensioni numeriche usate in tutta la logica geometrica.

## 2.2 Primitive Geometriche Core
- **Point3D**: primitiva di posizione con test geometrici e operazioni punto-punto.
- **Vector3D**: primitiva direzione/modulo con algebra vettoriale, logica angolare, interpolazione e rotazione.
- **RTMatrix**: matrice 4x4 di roto-traslazione con creazione da Euler/vettori/normali, composizione, inversione, conversioni Euler e trasformazioni.

Questi tre tipi costituiscono il contratto base usato da tutti gli oggetti di livello superiore in GeoShape.

## 2.3 Bounding e Partizionamento Spaziale
- **AABBox3D**: volume di bounding allineato agli assi per contenimento/intersezione/unione/ingrandimento e verifiche spaziali rapide.
- **IPointWeighted**: astrazione per oggetti con posizione pesata e vettori di orientamento locale.
- **OctreeNode<T>**: nodo octree generico pesato per inserimento, suddivisione, mantenimento del centroide pesato e query di range.

---

## 3. Architettura Logica di GeoShape

## 3.1 Contratti Runtime Trasversali
- **Parameter**: astrazione di parametro geometrico (valore/formula/metadati su unità).
- **Variable**: astrazione di variabile runtime nominata usata nelle formule.
- **Delegates**: punti di integrazione verso servizi esterni:
  - valutatore di espressioni,
  - motore di triangolazione dei profili,
  - callback opzionale per collisione ray-mesh.

Questi contratti disaccoppiano il core geometrico dai motori di valutazione/triangolazione/collisione.

## 3.2 Livello Scene Graph
- **Node3D**: contenitore gerarchico che combina:
  - nodi figli,
  - entità figlie,
  - trasformazione locale,
  - propagazione della trasformazione world ereditata,
  - catena di valutazione variabili/formule,
  - indirizzamento per path e traversal del sottoalbero.

`Node3D` è il nodo di orchestrazione delle assembly parametriche.

## 3.3 Astrazione Base delle Entità
- **Entity3D** (astratta): contratto comune per tutte le entità geometriche.
  - Definisce il comportamento di clone.
  - Definisce l’estrazione AABB.
  - Definisce il flusso di update guidato da formule.
  - Incapsula la semantica di trasformazioni locali/world.

Tutte le entità 3D concrete ereditano da questo tipo.

---

## 4. Sistema Curve (GeoShape.Curves)

## 4.1 Base Curva
- **Curve3D** (astratta): contratto unificato per:
  - valutazione per offset normalizzato/assoluto,
  - tangenti,
  - lunghezza,
  - trim,
  - inversione,
  - trasformazione rigida,
  - bounding box,
  - verifiche punto-su-curva e distanza,
  - punto di ingresso per intersezioni a coppie.

## 4.2 Curve Concrete
- **Line3D**: modello di segmento finito con helper di proiezione e intersezione linea-linea.
- **Arc3D**: modello di arco circolare con ricostruzione geometrica tramite costruttori multipli e logica di intersezione linea/arco.
- **Ellipse3D**: modello di arco ellittico con conversioni angolo circolare/ellittico e lunghezza campionata.
- **Helix3D**: modello di elica con semantica passo/profondità e valutazione parametrica.
- **PolyLine3D**: catena spezzata lineare (aperta/chiusa), mappata su `Figure3D` per la logica di valutazione.
- **Spline3D**: modello spline cubica cardinale/canonica con cache di interpolazione e comportamento guidato dalla tensione.

## 4.3 Aggregazione Curve
- **Figure3D**: collezione ordinata di `Curve3D` usata come contenitore di profili/percorsi/contorni.
  - Supporta verifiche di continuità, estrazione loop, ordinamento automatico, riduzione, suddivisione, trim, approssimazione, rilevamento piano e operazioni di supporto all’estrusione.

## 4.4 Estensioni Curve
- **Figure3DExtensions**:
  - export semplificato in stile DXF,
  - utilità di ordinamento/orientamento loop,
  - utilità di approssimazione delle figure.

---

## 5. Elementi Geometrici (GeoShape.Elements)

Queste sono entità geometriche helper di basso livello usate da curve/mesh/solidi:

- **Plane3D**: modello di piano con logica di intersezione linea/raggio/triangolo/piano e operazioni su sistemi di riferimento.
- **Ray3D**: modello di raggio con helper di trasformazione e confronto.
- **Triangle3D**: modello di triangolo con logica di normale/area/centro, trasformazioni e suddivisione tramite piano di taglio.
- **TriangleNormals**: tripletta di normali per vertice associata a un triangolo.
- **Polygon3D** + **VertexType**: modello di poligono planare con orientamento/classificazione vertici/diagonali e operazioni di conversione.
- **Edge3D**: astrazione di edge mesh che collega vertici e indici di triangoli.

Moduli di estensione elementi:
- **AABox3DExtensions**: conversione AABB -> figura wireframe, conversione OBB e helper di intersezione linea-box.
- **Polygon3DExtensions**: mappatura bilineare tra quadrilateri e costruzione convex hull.

---

## 6. Famiglie di Entità (GeoShape.Entities)

## 6.1 Contenitori Geometrici Diretti
- **FigureEntity3D**: incapsula una `Figure3D` dentro un contenitore `Entity3D`.
- **Mesh3D**: entità triangolata con collezioni opzionali di outline, normali e snap-point; include routine di correttezza e semplificazione mesh.

## 6.2 Solidi / Superfici Analitiche
- **OBBox3D**: entità box orientato con estrazione dei piani faccia e logica di contenimento.
- **Cylinder3D**: entità cilindro analitico.
- **Sphere3D**: entità sfera analitica con helper di intersezione linea.
- **Torus3D**: entità toro analitico.
- **Extrusion3D**: estrusione lineare di profilo con piani di taglio iniziali/finali opzionali.
- **SweepExtrusion3D**: sweep di profilo lungo un percorso curva con tagli opzionali.
- **Revolution3D**: entità di rivoluzione di profilo.
- **PlanarFace3D**: faccia planare definita da una shape 2D e payload texture opzionale.
- **BoxFace**: enum per indirizzamento facce del box orientato.

## 6.3 Estensioni di Conversione e Processing
- **Entity3DExtensions**: hub centrale di conversione da entità analitiche a `Mesh3D`.
  - Contiene pipeline di meshing specializzate per ciascun tipo entità.
  - Applica approssimazione dei profili e triangolazione opzionale per superfici di chiusura.
  - Gestisce tagli, normali, outline e propagazione trasformazioni.

- **Mesh3DExtensions**:
  - import/export STL,
  - builder generico di mesh di rivoluzione,
  - estrazione outline visibile da insiemi di mesh con direzione vista e delegate opzionale di collisione ray-mesh.

---

## 7. Sottosistema Shape2D

- **Shape2D** (astratta): contratto shape 2D basato su `Entity3D`, con comportamento di orientamento/specchiatura e validazione/generazione figura specifica della shape.
- **Shape2DCustom**: implementazione concreta basata su una `Figure3D` custom.

`Shape2D` è usata come sorgente profilo da `Extrusion3D`, `SweepExtrusion3D`, `Revolution3D` e `PlanarFace3D`.

---

## 8. Flusso Logico End-to-End

Flusso tipico nell’architettura:
1. Un albero parametrico `Node3D` contiene entità e formule.
2. Le formule runtime vengono valutate tramite integrazione con il delegate evaluator.
3. Ogni `Entity3D` può esporre un bounding box per ragionamento spaziale.
4. Le entità analitiche possono essere convertite in `Mesh3D` tramite `Entity3DExtensions`.
5. I dati mesh possono essere esportati, post-processati, semplificati, verificati o proiettati in outline.
6. L’accelerazione spaziale può essere applicata con pattern `AABBox3D` e `OctreeNode<T>`.

---

## 9. Sintesi Architetturale

- `GeoMath` è il kernel di geometria computazionale.
- `GeoShape` è il livello di modellazione/orchestrazione.
- `Curve3D` ed `Entity3D` sono i due pilastri principali di astrazione.
- `Figure3D` è l’aggregato centrale di curve usato come contenitore di profili/percorsi/loop.
- `Mesh3D` è la rappresentazione triangolata finale, adatta a interscambio e rendering.
- I delegate forniscono punti di estensibilità strategici per valutazione formule, triangolazione e gestione collisioni.
