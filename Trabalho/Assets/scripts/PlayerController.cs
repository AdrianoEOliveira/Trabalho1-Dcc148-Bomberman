using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 3f; // Velocidade do jogador
    private Vector2 moveInput;

    public Tilemap tilemapPiso;
    public Tilemap tilemapParedes;
    public Tilemap tilemapDestrutiveis;

    [SerializeField] private GameObject bombPrefab;
    private ObjectPool bombPool;

    [SerializeField] private Sprite[] UpSprites;
    [SerializeField] private Sprite[] LeftSprites;
    [SerializeField] private Sprite[] RightSprites;
    [SerializeField] private Sprite[] DownSprites;
    private SpriteRenderer spriteRenderer;

    private int currentSpriteIndex = 0;
    private float spriteChangeRate = 0.2f; // Tempo para trocar o sprite da animação
    private float spriteTimer = 0f;

    private void Awake()
    {
        tilemapPiso = GameObject.FindWithTag("Piso")?.GetComponent<Tilemap>();
        tilemapParedes = GameObject.FindWithTag("Parede")?.GetComponent<Tilemap>();
        tilemapDestrutiveis = GameObject.FindWithTag("Destrutiveis")?.GetComponent<Tilemap>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        bombPool = new ObjectPool(bombPrefab, 3);
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveInput.y = 1;
            AnimateSprite(UpSprites);
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            moveInput.y = -1;
            AnimateSprite(DownSprites);
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput.x = -1;
            AnimateSprite(LeftSprites);
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput.x = 1;
            AnimateSprite(RightSprites);
        }

        moveInput = moveInput.normalized; // Mantém a velocidade constante em diagonais

        MovePlayer(); // Atualiza a posição do jogador manualmente

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Bomb();
        }
    }

    private void MovePlayer()
    {
        // Calcula a nova posição com base no movimento
        Vector3 newPosition = transform.position + (Vector3)moveInput * speed * Time.deltaTime;

        // Checa se a nova posição não vai colidir com obstáculos (paredes ou tiles destrutíveis)
        if (!IsObstacle(newPosition))
        {
            transform.position = newPosition; // Se não houver obstáculos, o jogador pode se mover
        }
    }
    private bool IsObstacle(Vector2 targetPosition)
    {
        // Checa se a posição de destino tem um tile sólido, seja parede ou tile destrutível
        Vector3Int targetTilePosition = tilemapParedes.WorldToCell(targetPosition);
        if (tilemapParedes.HasTile(targetTilePosition))
        {
            return true; // O jogador bateu numa parede
        }

        Vector3Int targetDestrutivelTilePosition = tilemapDestrutiveis.WorldToCell(targetPosition);
        if (tilemapDestrutiveis.HasTile(targetDestrutivelTilePosition))
        {
            return true; // O jogador bateu em um tile destrutível
        }

        return false; // Se não houver nenhum obstáculo
    }

    private void AnimateSprite(Sprite[] animationSprites)
    {
        spriteTimer += Time.deltaTime;

        if (spriteTimer >= spriteChangeRate)
        {
            spriteTimer = 0f;
            currentSpriteIndex = (currentSpriteIndex + 1) % animationSprites.Length;
            spriteRenderer.sprite = animationSprites[currentSpriteIndex];
        }
    }

    void Bomb()
    {
        GameObject bomb = bombPool.GetFromPool();
        if (bomb != null)
        {
            Vector3Int tilePosition = tilemapPiso.WorldToCell(transform.position);
            Vector3 tileCenterPosition = tilemapPiso.CellToWorld(tilePosition) + tilemapPiso.cellSize / 2f;

            bomb.transform.position = tileCenterPosition;
            bomb.SetActive(true);
        }
    }
}
