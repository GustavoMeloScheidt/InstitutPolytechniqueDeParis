using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board References")]
    [Tooltip("Transform de referência do tabuleiro. Ideal: um objeto 'BoardRoot' (empty) com o FBX como filho.")]
    public Transform boardTransform;

    [Tooltip("Onde as peças instanciadas vão ficar na Hierarchy.")]
    public Transform piecesParent;

    [Header("Board Dimensions")]
    [Tooltip("Tamanho total do tabuleiro (lado) em unidades do Unity (metros, se 1 unit = 1m).")]
    public float boardSize = 0.40f;

    [Tooltip("Se verdadeiro, calcula boardSize automaticamente a partir do mesh (Renderer.bounds). Recomendo para FBX.")]
    public bool autoComputeBoardSizeFromMesh = true;

    [Tooltip("Altura (Y) acima do tabuleiro para colocar as peças (evita z-fighting).")]
    public float pieceYOffset = 0.002f;

    [Header("Piece Prefabs (White)")]
    public GameObject pawnWhite;
    public GameObject rookWhite;
    public GameObject knightWhite;
    public GameObject bishopWhite;
    public GameObject queenWhite;
    public GameObject kingWhite;

    [Header("Piece Prefabs (Black)")]
    public GameObject pawnBlack;
    public GameObject rookBlack;
    public GameObject knightBlack;
    public GameObject bishopBlack;
    public GameObject queenBlack;
    public GameObject kingBlack;

    private const int N = 8;

    public float SquareSize => boardSize / N;

    private void Start()
    {
        Debug.Log("[BoardManager] Start() called.");

        if (boardTransform == null)
        {
            Debug.LogError("[BoardManager] boardTransform is null. Assign it in the Inspector.");
            return;
        }

        if (piecesParent == null)
        {
            Debug.LogError("[BoardManager] piecesParent is null. Assign it in the Inspector.");
            return;
        }

        if (autoComputeBoardSizeFromMesh)
        {
            AutoComputeBoardSizeFromMesh();
        }

        ClearPieces();
        SpawnInitialPosition();
    }

    private void AutoComputeBoardSizeFromMesh()
    {
        // Procura um Renderer no boardTransform ou nos filhos (para FBX é comum estar no filho)
        Renderer r = boardTransform.GetComponentInChildren<Renderer>();
        if (r == null)
        {
            Debug.LogWarning("[BoardManager] No Renderer found under boardTransform. Using manual boardSize.");
            return;
        }

        // Bounds em world-space
        Bounds b = r.bounds;

        float sizeX = b.size.x;
        float sizeZ = b.size.z;

        // Assume tabuleiro aproximadamente quadrado:
        boardSize = Mathf.Min(sizeX, sizeZ);

        Debug.Log($"[BoardManager] Auto boardSize computed: {boardSize:F3} (bounds x={sizeX:F3}, z={sizeZ:F3})");
    }

    public void ClearPieces()
    {
        for (int i = piecesParent.childCount - 1; i >= 0; i--)
        {
            Destroy(piecesParent.GetChild(i).gameObject);
        }
    }

    public void SpawnInitialPosition()
    {
        // Pawns
        for (int file = 0; file < N; file++)
        {
            SpawnPiece(pawnWhite, file, 1); // rank 2
            SpawnPiece(pawnBlack, file, 6); // rank 7
        }

        // White back rank (rank 1 => index 0)
        SpawnPiece(rookWhite, 0, 0);
        SpawnPiece(knightWhite, 1, 0);
        SpawnPiece(bishopWhite, 2, 0);
        SpawnPiece(queenWhite, 3, 0);
        SpawnPiece(kingWhite, 4, 0);
        SpawnPiece(bishopWhite, 5, 0);
        SpawnPiece(knightWhite, 6, 0);
        SpawnPiece(rookWhite, 7, 0);

        // Black back rank (rank 8 => index 7)
        SpawnPiece(rookBlack, 0, 7);
        SpawnPiece(knightBlack, 1, 7);
        SpawnPiece(bishopBlack, 2, 7);
        SpawnPiece(queenBlack, 3, 7);
        SpawnPiece(kingBlack, 4, 7);
        SpawnPiece(bishopBlack, 5, 7);
        SpawnPiece(knightBlack, 6, 7);
        SpawnPiece(rookBlack, 7, 7);
    }

    private void SpawnPiece(GameObject prefab, int file, int rank)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[BoardManager] Missing prefab for piece at {FileToChar(file)}{rank + 1}");
            return;
        }

        Vector3 pos = GetSquareCenterWorld(file, rank);
        Quaternion rot = boardTransform.rotation;

        GameObject piece = Instantiate(prefab, pos, rot, piecesParent);
        piece.name = $"{prefab.name}_{FileToChar(file)}{rank + 1}";
    }

    /// <summary>
    /// Centro da casa (file, rank) em coordenadas world.
    /// file: 0..7 (A..H)
    /// rank: 0..7 (1..8)
    /// </summary>
    public Vector3 GetSquareCenterWorld(int file, int rank)
    {
        float squareSize = boardSize / N;

        // Offset local a partir do centro:
        float x = (-boardSize / 2f) + (squareSize / 2f) + file * squareSize;
        float z = (-boardSize / 2f) + (squareSize / 2f) + rank * squareSize;

        Vector3 local = new Vector3(x, 0f, z);
        Vector3 world = boardTransform.TransformPoint(local);

        // Levanta um pouco acima do tabuleiro
        world += boardTransform.up * pieceYOffset;

        return world;
    }

    /// <summary>
    /// Dada uma posição world (tipicamente no plano do tabuleiro), retorna a casa (file,rank) se estiver dentro do tabuleiro.
    /// </summary>
    public bool TryGetNearestSquareFromWorld(Vector3 worldPos, out int file, out int rank)
    {
        file = -1;
        rank = -1;

        Vector3 local = boardTransform.InverseTransformPoint(worldPos);

        float half = boardSize / 2f;

        float x01 = (local.x + half) / boardSize; // 0..1
        float z01 = (local.z + half) / boardSize; // 0..1

        if (x01 < 0f || x01 > 1f || z01 < 0f || z01 > 1f)
            return false;

        file = Mathf.Clamp(Mathf.FloorToInt(x01 * N), 0, N - 1);
        rank = Mathf.Clamp(Mathf.FloorToInt(z01 * N), 0, N - 1);
        return true;
    }

    /// <summary>
    /// Interseção do ray com o plano do tabuleiro.
    /// </summary>
    public Vector3 GetBoardPlanePointFromRay(Ray ray, float yOffset = 0f)
    {
        Plane plane = new Plane(boardTransform.up, boardTransform.position);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            return hit + boardTransform.up * yOffset;
        }

        return Vector3.zero;
    }

    private char FileToChar(int file) => (char)('A' + file);

    private void OnDrawGizmosSelected()
    {
        if (boardTransform == null) return;

        Gizmos.color = Color.yellow;
        for (int file = 0; file < N; file++)
        {
            for (int rank = 0; rank < N; rank++)
            {
                Vector3 p = GetSquareCenterWorld(file, rank);
                Gizmos.DrawSphere(p, 0.003f);
            }
        }
    }
}
