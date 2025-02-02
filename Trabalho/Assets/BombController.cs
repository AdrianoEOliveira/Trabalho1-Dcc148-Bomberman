using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    public float fuseTime = 3f; // Tempo até a explosão (em segundos)
    private ObjectPool objectPool; // Referência ao ObjectPooler
    [SerializeField] public GameObject explosionPrefab; // Prefab da explosão

    // Referências aos três Tilemaps
    public Tilemap tilemapPiso; // Tilemap para o Piso
    public Tilemap tilemapParedes; // Tilemap para as Paredes
    public Tilemap tilemapDestrutiveis; // Tilemap para tiles Destrutíveis

    public Sprite[] bombSprites; // Array de sprites para animação da bomba (4 sprites)
    public Sprite[] explosionSprites; // Array de sprites para animação da explosão (8 sprites)
    private SpriteRenderer spriteRenderer; // O SpriteRenderer da bomba

    private bool isExploded = false; // Verifica se a bomba já explodiu
    private Vector3Int bombPosition;

    void Start()
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
        // Pega o SpriteRenderer
        objectPool = new ObjectPool(explosionPrefab, 1);
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Pega a posição da bomba no Tilemap
        bombPosition = tilemapDestrutiveis.WorldToCell(transform.position);

        // Inicia a contagem regressiva para a explosão
        StartCoroutine(ExplosionCountdown());

        // Inicia a animação da bomba
        StartCoroutine(AnimateBomb());
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

        // Destrói tiles ao redor da bomba (se forem destrutíveis)
        DestruirTiles();

        // Inicia a animação da explosão
        StartCoroutine(AnimateExplosion());

        GameObject explosion = objectPool.GetFromPool();
        Vector3 worldPosition = transform.position;

        // Converte a posição para a célula do Tilemap
        Vector3Int tilePosition = tilemapPiso.WorldToCell(worldPosition);

        // Calcula a posição no centro do tile
        Vector3 tileCenterPosition = tilemapPiso.CellToWorld(tilePosition) + tilemapPiso.cellSize / 2f;

        // Posiciona a bomba
        explosion.transform.position = tileCenterPosition;
        explosion.SetActive(true);

        // Desativa a bomba após a animação de explosão
        gameObject.SetActive(false);
    }

    // Função para destruir tiles ao redor da bomba
    private void DestruirTiles()
    {
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
            // Verifica se o tile não é uma parede ou piso (apenas destrutíveis)
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
