using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    public Tilemap tilemapPiso;         // Tilemap para o Piso
    public Tilemap tilemapParedes;      // Tilemap para as Paredes
    public Tilemap tilemapDestrutiveis; // Tilemap para tiles Destrutíveis

    [SerializeField] private GameObject bombPrefab;
    private ObjectPool bombPool;
    [SerializeField] private Sprite[] UpSprites;
    [SerializeField] private Sprite[] LeftSprites;
    [SerializeField] private Sprite[] RightSprites;
    [SerializeField] private Sprite[] DownSprites;
    private SpriteRenderer spriteRenderer;

    private Sprite[] currentAnimation;

    private void Awake()
    {
        GameObject tilemapObj = GameObject.FindWithTag("Piso"); // Busca o GameObject com a Tag
        if (tilemapObj != null)
        {
            tilemapPiso = tilemapObj.GetComponent<Tilemap>(); // Obtém o componente Tilemap
        }
        GameObject tilemapObj2 = GameObject.FindWithTag("Parede"); // Busca o GameObject com a Tag
        if (tilemapObj2 != null)
        {
            tilemapParedes = tilemapObj2.GetComponent<Tilemap>(); // Obtém o componente Tilemap
        }
        GameObject tilemapObj3 = GameObject.FindWithTag("Destrutiveis"); // Busca o GameObject com a Tag
        if (tilemapObj3 != null)
        {
            tilemapDestrutiveis = tilemapObj3.GetComponent<Tilemap>(); // Obtém o componente Tilemap
        }

        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Pega o SpriteRenderer do jogador
        currentAnimation = DownSprites;
        bombPool = new ObjectPool(bombPrefab, 3);
    }

    private void FixedUpdate()
    {
        Vector2 position = rb.position;
        Vector2 translation = speed * Time.fixedDeltaTime * rb.linearVelocity;

        rb.MovePosition(position + translation);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Keyboard.current.upArrowKey.isPressed)
        {
            SetAnimation(UpSprites);
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            SetAnimation(DownSprites);
        }
        else if (Keyboard.current.leftArrowKey.isPressed)
        {
            SetAnimation(LeftSprites);
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            SetAnimation(RightSprites);
        }
        else
        {
            SetAnimation(currentAnimation); // Pode ser o sprite de descanso ou vazio
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Bomb();
        }
    }

    private void SetAnimation(Sprite[] newSprites)
    {
        currentAnimation = newSprites;

        // Troca o sprite para o primeiro da lista de animação
        spriteRenderer.sprite = currentAnimation[0]; // Aqui você pode escolher qual sprite usar, exemplo: [0] é o primeiro sprite
    }

    void Bomb()
    {
        // Pega a bomba do pool
        GameObject bomb = bombPool.GetFromPool();
        if (bomb != null)
        {
            // Posição no mundo do jogador
            Vector3 worldPosition = transform.position;

            // Converte a posição para a célula do Tilemap
            Vector3Int tilePosition = tilemapPiso.WorldToCell(worldPosition);

            // Calcula a posição no centro do tile
            Vector3 tileCenterPosition = tilemapPiso.CellToWorld(tilePosition) + tilemapPiso.cellSize / 2f;

            // Posiciona a bomba
            bomb.transform.position = tileCenterPosition;
            bomb.SetActive(true);

            // Aqui você pode fazer a bomba destruir os tiles ao redor (caso tenha uma explosão)
            DestruirTiles(tilePosition);
        }
    }

    // Função para destruir tiles ao redor da bomba
    void DestruirTiles(Vector3Int position)
    {
        // Checa se o tile na posição dada é destrutível
        if (tilemapDestrutiveis.HasTile(position))
        {
            // Se for destrutível, destrua o tile
            tilemapDestrutiveis.SetTile(position, null);
        }

        // Aqui você pode expandir para destruir tiles adjacentes (explosão)
        // Por exemplo, destrua tiles adjacentes se houver um efeito de explosão
        Vector3Int[] adjacenteTiles = new Vector3Int[]
        {
            position + new Vector3Int(1, 0, 0), // Direita
            position + new Vector3Int(-1, 0, 0), // Esquerda
            position + new Vector3Int(0, 1, 0), // Cima
            position + new Vector3Int(0, -1, 0)  // Baixo
        };

        foreach (var adj in adjacenteTiles)
        {
            if (tilemapDestrutiveis.HasTile(adj))
            {
                tilemapDestrutiveis.SetTile(adj, null); // Remove o tile
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            // DeathSequence();
        }
    }

    private void DeathSequence()
    {
        //enabled = false;
        //GetComponent<BombController>().enabled = false;

        //spriteRenderer.sprite = null; // Faz o sprite desaparecer (ou você pode colocar um sprite de morte aqui)

        //Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        //gameObject.SetActive(false);
        //GameManager.Instance.CheckWinState();
    }//
}
