using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    public float fuseTime = 3f; // Tempo até a explosão (em segundos)
    [SerializeField] public GameObject explosionPrefab; // Prefab da explosão

    // Referências aos três Tilemaps
    public Tilemap tilemapPiso; // Tilemap para o Piso
    public Tilemap tilemapParedes; // Tilemap para as Paredes
    public Tilemap tilemapDestrutiveis; // Tilemap para tiles Destrutíveis

    public Sprite[] bombSprites; // Array de sprites para animação da bomba (4 sprites)
    public Sprite[] explosionSprites; // Array de sprites para animação da explosão (8 sprites)
    private SpriteRenderer spriteRenderer; // O SpriteRenderer da bomba

    private bool isExploded = false; // Verifica se a bomba já explodiu
    private bool isBombUsed = false; // Controla se a bomba já foi usada

    private Vector3 worldPosition; // Posição da bomba no mundo
    private Vector3Int bombPosition;

    void Start()
    {
        // Pega o SpriteRenderer
        InitializeBomb();
    }

    void OnEnable()
    {
        if (isBombUsed)
        {
            InitializeBomb();
        }
    }
    private void InitializeBomb()
    {
        isExploded = false; // A bomba ainda não explodiu
        GameObject tilemapObj = GameObject.FindWithTag("Piso");
        if (tilemapObj != null)
        {
            tilemapPiso = tilemapObj.GetComponent<Tilemap>();
        }
        GameObject tilemapObj2 = GameObject.FindWithTag("Parede");
        if (tilemapObj2 != null)
        {
            tilemapParedes = tilemapObj2.GetComponent<Tilemap>();
        }
        GameObject tilemapObj3 = GameObject.FindWithTag("Destrutiveis");
        if (tilemapObj3 != null)
        {
            tilemapDestrutiveis = tilemapObj3.GetComponent<Tilemap>();
        }

        // Pega o SpriteRenderer
        isExploded = false; // A bomba ainda não explodiu

        spriteRenderer = GetComponent<SpriteRenderer>();

        // Pega a posição da bomba no Tilemap
        bombPosition = tilemapDestrutiveis.WorldToCell(transform.position);
        // Reseta o estado da bomba
        spriteRenderer.sprite = bombSprites[0]; // Coloca o primeiro sprite da animação da bomba
        gameObject.SetActive(true); // Ativa a bomb
        StartCoroutine(ExplosionCountdown()); // Inicia a contagem regressiva
        StartCoroutine(AnimateBomb()); // Inicia a animação da bomba
    }


    // Função de contagem regressiva para a explosão
    private IEnumerator ExplosionCountdown()
    {
        yield return new WaitForSeconds(fuseTime);

        // Quando o tempo acabar, explode a bomba
        Explode();
    }

    // Função para lidar com a explosão
    private void Explode()
    {
        if (isExploded)
            return;

        isExploded = true;

        // Atualiza a posição no Tilemap de destrutíveis para garantir que a bomba está no lugar correto
        bombPosition = tilemapDestrutiveis.WorldToCell(transform.position);

        // Destrói tiles ao redor da bomba (incluindo o tile onde a bomba foi colocada)
        DestruirTiles();

        // Inicia a animação da explosão
        StartCoroutine(AnimateExplosion());

        // Instancia a explosão no local
        Vector3 worldPosition = tilemapPiso.CellToWorld(bombPosition);
        Vector3 tileCenterPosition = worldPosition + tilemapPiso.cellSize / 2f;

        Instantiate(explosionPrefab, tileCenterPosition, Quaternion.identity);

        isBombUsed = true;

        // Desativa a bomba após a animação de explosão
        gameObject.SetActive(false);
    }

    // Função para destruir tiles ao redor da bomba
    private void DestruirTiles()
    {
        // Verifica se o tile onde a bomba foi colocada ainda é destrutível
        if (tilemapDestrutiveis.HasTile(bombPosition))
        {
            tilemapDestrutiveis.SetTile(bombPosition, null); // Remove o tile onde a bomba foi colocada
        }

        // Agora, verifica os tiles adjacentes
        Vector3Int[] adjacenteTiles = new Vector3Int[]
        {
        bombPosition + new Vector3Int(1, 0, 0), // Direita
        bombPosition + new Vector3Int(-1, 0, 0), // Esquerda
        bombPosition + new Vector3Int(0, 1, 0), // Cima
        bombPosition + new Vector3Int(0, -1, 0)  // Baixo
        };

        // Verifica se os tiles adjacentes são destrutíveis e os destrói
        foreach (var adj in adjacenteTiles)
        {
            // Verifica se o tile adjacente é destrutível
            if (tilemapDestrutiveis.HasTile(adj))
            {
                tilemapDestrutiveis.SetTile(adj, null); // Remove o tile destrutível
            }
        }
    }

    // Função para animar a bomba enquanto ela espera
    private IEnumerator AnimateBomb()
    {
        int spriteCount = bombSprites.Length; // Total de sprites
        float spriteDuration = fuseTime / spriteCount; // Tempo de duração para cada sprite (0.75 segundos)

        while (!isExploded)
        {
            for (int i = 0; i < spriteCount; i++)
            {
                spriteRenderer.sprite = bombSprites[i]; // Altera o sprite da bomba
                yield return new WaitForSeconds(spriteDuration); // Aguarda antes de trocar o sprite
            }
        }

    }

    // Função para animar a explosão com 8 sprites
    private IEnumerator AnimateExplosion()
    {
        int spriteCount = explosionSprites.Length; // Total de sprites da explosão
        float spriteDuration = 3f / spriteCount; // Tempo de duração para cada sprite (0.375 segundos)

        for (int i = 0; i < spriteCount; i++)
        {
            spriteRenderer.sprite = explosionSprites[i]; // Altera o sprite da explosão
            yield return new WaitForSeconds(spriteDuration); // Aguarda antes de trocar o sprite
        }
    }
}
