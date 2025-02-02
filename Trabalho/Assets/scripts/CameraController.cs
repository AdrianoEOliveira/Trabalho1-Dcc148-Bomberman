using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 5f; // Velocidade de suavização
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    [SerializeField] private Transform upEdge;
    [SerializeField] private Transform downEdge;

    private Transform player;
    private float halfWidth;
    private float halfHeight;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player não encontrado! Verifique se o jogador tem a tag correta.");
            enabled = false;
            return;
        }

        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;
    }

    void LateUpdate()
    {
        MoveCamera();
    }

    void MoveCamera()
    {
        if (player == null) return;

        // Calcula a nova posição da câmera com restrições
        float x = Mathf.Clamp(player.position.x, leftEdge.position.x + halfWidth, rightEdge.position.x - halfWidth);
        float y = Mathf.Clamp(player.position.y, downEdge.position.y + halfHeight, upEdge.position.y - halfHeight);
        float z = -10f; // Mantém a câmera no plano 2D

        Vector3 targetPosition = new Vector3(x, y, z);
        
        // Suaviza o movimento da câmera para evitar movimentos bruscos
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
