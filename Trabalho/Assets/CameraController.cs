using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    private Transform player; // Referência para o jogador (Bomberman)
    public float followSpeed = 5f; // Velocidade de movimentação da câmera
    public float cameraHeight = 5f; // Distância vertical da câmera
    public float cameraMargin = 1f; // Margem de movimento da câmera para evitar que ela se mova a cada pequeno movimento do jogador

    private Tilemap tilemapPiso; // Tilemap do piso
    private float minX, maxX, minY, maxY; // Limites do cenário

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform; // Encontra o jogador na cena
        GameObject tilemapPisoGO = GameObject.FindWithTag("Piso"); // Encontra o Tilemap do piso
        tilemapPiso = tilemapPisoGO.GetComponent<Tilemap>(); // Pega o componente Tilemap
        // Definir os limites com base no tamanho do Tilemap
        SetCameraLimits();
    }

    void Update()
    {
        // Movimentação da câmera
        MoveCamera();
    }

    // Função que move a câmera conforme a posição do jogador
    void MoveCamera()
    {
        Vector3 targetPosition = player.position;
        targetPosition.z = transform.position.z; // Mantém a posição da câmera no eixo Z

        // Limita a posição da câmera nas coordenadas X e Y
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Lerp para suavizar o movimento da câmera
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    // Função que define os limites da câmera, com base no Tilemap
    void SetCameraLimits()
    {
        // Supondo que o Tilemap está configurado corretamente
        Bounds bounds = tilemapPiso.localBounds; // Pega os limites do Tilemap do piso

        // Limites do cenário
        minX = bounds.min.x + cameraMargin;
        maxX = bounds.max.x - cameraMargin;
        minY = bounds.min.y + cameraMargin;
        maxY = bounds.max.y - cameraMargin;
    }
}
