using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 3f; // Velocidade do jogador
    private Vector2 moveInput;

    private Vector3 targetPosition; // Posição de destino do jogador

    public Tilemap tilemapPiso;
    public Tilemap tilemapParedes;
    public Tilemap tilemapDestrutiveis;

    public Tilemap tilemapItem;

    [SerializeField] private GameObject bombPrefab;
    private ObjectPool bombPool;

    private int powerUp = 1;

    [SerializeField] private Sprite[] UpSprites;
    [SerializeField] private Sprite[] LeftSprites;
    [SerializeField] private Sprite[] RightSprites;
    [SerializeField] private Sprite[] DownSprites;
    [SerializeField] private Sprite[] deathSprites;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private TMP_Text textMesh;

    private int currentSpriteIndex = 0;

    private bool isMoving = false;

    public bool isDead = false;

    private void Awake()
    {
        tilemapPiso = GameObject.FindWithTag("Piso")?.GetComponent<Tilemap>();
        tilemapParedes = GameObject.FindWithTag("Parede")?.GetComponent<Tilemap>();
        tilemapDestrutiveis = GameObject.FindWithTag("Destrutiveis")?.GetComponent<Tilemap>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        bombPool = new ObjectPool(bombPrefab, 3);
        // Garante que o jogador começa no centro da célula
        Vector3Int tilePosition = tilemapPiso.WorldToCell(transform.position);
        Vector3 tileCenterPosition = tilemapPiso.CellToWorld(tilePosition) + tilemapPiso.cellSize / 2f;

        transform.position = tileCenterPosition;
    }

    public void SetDeath(bool death)
    {
        isDead = death;
    }

    public void AtualizaHUD()
    {
        textMesh.text = "X " + bombPool.GetActiveCount();
    }



    private void Update()
    {
        CheckItemTile();
        Death();
        AtualizaHUD();
        if (!isMoving)
        {

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                moveInput.y = 1;
                moveInput.x = 0;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                moveInput.y = -1;
                moveInput.x = 0;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput.x = -1;
                moveInput.y = 0;
            }
            else if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput.x = 1;
                moveInput.y = 0;
            }
            if (moveInput != Vector2.zero)
            {
                // Converte a posição atual para a célula do tilemap
                Vector3Int nextCell = tilemapPiso.WorldToCell(transform.position) + GetCellDirection(moveInput);
                Vector3 nextPosition = tilemapPiso.GetCellCenterWorld(nextCell);

                if (!IsObstacle(nextPosition))
                {
                    targetPosition = nextPosition;
                    StartCoroutine(MoveToTarget());
                }
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Bomb();
        }
    }

    private System.Collections.IEnumerator MoveToTarget()
    {
        isMoving = true;

        // Calcular a distância total do movimento
        float totalDistance = (transform.position - targetPosition).magnitude;
        // Calcular o tempo necessário para mover até o target
        float moveDuration = totalDistance / speed;
        // Definir o número de quadros da animação (4 quadros)
        int totalFrames = 4;

        // Calcular o tempo por quadro da animação
        float timePerFrame = moveDuration / totalFrames;

        // Inicializar o contador de quadros da animação
        float frameTimer = 0f;

        while ((transform.position - targetPosition).sqrMagnitude > 0.01f)
        {
            // Atualiza o sprite conforme o eixo de movimento
            if (moveInput.y == 1)
            {
                AnimateSprite(UpSprites);
            }
            else if (moveInput.y == -1)
            {
                AnimateSprite(DownSprites);
            }
            else if (moveInput.x == -1)
            {
                AnimateSprite(LeftSprites);
            }
            else if (moveInput.x == 1)
            {
                AnimateSprite(RightSprites);
            }

            // Atualiza o contador de tempo de animação
            frameTimer += Time.deltaTime;

            // Verifica se é hora de trocar o quadro de animação
            if (frameTimer >= timePerFrame)
            {
                currentSpriteIndex = (currentSpriteIndex + 1) % 4; // Atualiza o índice da animação para o próximo quadro
                frameTimer = 0f; // Reseta o timer para o próximo quadro
            }

            // Move o jogador para a próxima posição
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
        moveInput = Vector2.zero;
    }

    private void AnimateSprite(Sprite[] animationSprites)
    {
        // Atualiza o sprite com base no índice atual
        spriteRenderer.sprite = animationSprites[currentSpriteIndex];
    }

    private Vector3Int GetCellDirection(Vector2 moveDirection)
    {
        return new Vector3Int((int)moveDirection.x, (int)moveDirection.y, 0);
    }

    private bool IsObstacle(Vector2 targetPosition)
    {
        Vector3Int targetTilePosition = tilemapParedes.WorldToCell(targetPosition);
        if (tilemapParedes.HasTile(targetTilePosition))
            return true;

        Vector3Int targetDestrutivelTilePosition = tilemapDestrutiveis.WorldToCell(targetPosition);
        if (tilemapDestrutiveis.HasTile(targetDestrutivelTilePosition))
            return true;

        return false;
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
    private void CheckItemTile()
    {
        Vector3Int playerPosition = tilemapItem.WorldToCell(transform.position);
        if (tilemapItem.HasTile(playerPosition))
        {
            tilemapItem.SetTile(playerPosition, null);
            powerUp++;
            bombPool.SetPowerUp(powerUp);
        }
    }

    private  IEnumerator PlayDeathAnimation()
    {
        int spriteCount = deathSprites.Length;  // Total de sprites para a animação de morte
        float spriteDuration = 0.1f;  // Ajuste o tempo entre os frames da animação

        for (int i = 0; i < spriteCount; i++)
        {
            spriteRenderer.sprite = deathSprites[i];  // Troca o sprite para a animação
            yield return new WaitForSeconds(spriteDuration);  // Aguarda antes de trocar
        }
        gameObject.SetActive(false);
    }
    private void Death()
    {
        if(isDead)
        {
        StartCoroutine(PlayDeathAnimation());
        }

        //gameObject.SetActive(false);
    }
}