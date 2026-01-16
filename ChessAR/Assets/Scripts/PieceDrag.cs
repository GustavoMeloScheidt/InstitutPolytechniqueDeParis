using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PieceDrag : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Se vazio, o script tenta achar automaticamente na cena.")]
    public BoardManager boardManager;

    [Header("Drag Settings")]
    [Tooltip("Levanta a peça durante o drag (em metros/unidades).")]
    public float liftDuringDrag = 0.01f;

    [Tooltip("Se true, ao soltar fora do tabuleiro volta para a posição original.")]
    public bool forbidDropOutsideBoard = true;

    [Tooltip("Se true, faz snap pela casa MAIS PRÓXIMA por distância (mais natural). Se false, usa o quadrante (mais simples).")]
    public bool snapByNearestDistance = true;

    private Camera cam;
    private bool dragging;

    private Vector3 originalPos;
    private int originalFile = -1;
    private int originalRank = -1;

    private void Start()
    {
        cam = Camera.main;
        if (boardManager == null)
            boardManager = FindFirstObjectByType<BoardManager>();

        if (boardManager == null)
            Debug.LogError("[PieceDrag] BoardManager not found. Assign it in the Inspector.");

        CacheOriginalSquare();
    }

    private void OnMouseDown()
    {
        if (cam == null || boardManager == null) return;

        dragging = true;
        originalPos = transform.position;
        CacheOriginalSquare();
    }

    private void OnMouseDrag()
    {
        if (!dragging || cam == null || boardManager == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // ponto no plano do tabuleiro
        Vector3 hit = boardManager.GetBoardPlanePointFromRay(ray, boardManager.pieceYOffset);

        // coloca no plano e levanta um pouco para ficar mais visível enquanto arrasta
        transform.position = hit + boardManager.boardTransform.up * liftDuringDrag;
    }

    private void OnMouseUp()
    {
        if (!dragging || boardManager == null) return;

        dragging = false;

        // remove o "lift" para considerar a posição real no plano
        Vector3 worldOnPlane = transform.position - boardManager.boardTransform.up * liftDuringDrag;

        bool inside = boardManager.TryGetNearestSquareFromWorld(worldOnPlane, out int file, out int rank);

        if (!inside)
        {
            if (forbidDropOutsideBoard)
                transform.position = originalPos;

            return;
        }

        // Snap:
        if (!snapByNearestDistance)
        {
            // Snap simples: casa do quadrante
            transform.position = boardManager.GetSquareCenterWorld(file, rank);
            originalFile = file;
            originalRank = rank;
            return;
        }

        // Snap por distância: procura a casa mais próxima (robusto para bordas)
        float bestDist = float.PositiveInfinity;
        int bestFile = file;
        int bestRank = rank;

        for (int f = 0; f < 8; f++)
        {
            for (int r = 0; r < 8; r++)
            {
                Vector3 center = boardManager.GetSquareCenterWorld(f, r);

                // mede distância no plano do tabuleiro (ignora variação em Y)
                Vector3 a = worldOnPlane;
                Vector3 b = center;
                a = Vector3.ProjectOnPlane(a - boardManager.boardTransform.position, boardManager.boardTransform.up);
                b = Vector3.ProjectOnPlane(b - boardManager.boardTransform.position, boardManager.boardTransform.up);

                float d = (a - b).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    bestFile = f;
                    bestRank = r;
                }
            }
        }

        transform.position = boardManager.GetSquareCenterWorld(bestFile, bestRank);
        originalFile = bestFile;
        originalRank = bestRank;
    }

    private void CacheOriginalSquare()
    {
        if (boardManager == null) return;

        if (boardManager.TryGetNearestSquareFromWorld(transform.position, out int f, out int r))
        {
            originalFile = f;
            originalRank = r;
        }
    }
}
