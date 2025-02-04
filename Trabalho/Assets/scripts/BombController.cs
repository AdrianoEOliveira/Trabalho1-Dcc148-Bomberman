using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    public float fuseTime = 3f; // Tempo até a explosão (em segundos)

    private float startTime;
    [SerializeField] public GameObject explosionPrefab; // Prefab da explosão

    // Referências aos três Tilemaps
    public Tilemap tilemapPiso; // Tilemap para o Piso
    public Tilemap tilemapParedes; // Tilemap para as Paredes
    public Tilemap tilemapDestrutiveis; // Tilemap para tiles Destrutíveis

    [SerializeField] public GameObject player; // Referência ao jogador

    [SerializeField] GameObject A1; // Referência ao AI1
    
    [SerializeField] GameObject A2; // Referência ao AI2

    [SerializeField] GameObject A3; // Referência ao AI3


    public Sprite[] bombSprites; // Array de sprites para animação da bomba (4 sprites)
    public Sprite[] explosionSprites; // Array de sprites para animação da explosão (8 sprites)
    private SpriteRenderer spriteRenderer; // O SpriteRenderer da bomba

    private bool isExploded = false; // Verifica se a bomba já explodiu
    private bool isBombUsed = false; // Controla se a bomba já foi usada

    public int PowerUp = 1; // Verifica se a bomba é um power-up

    private Vector3Int bombPosition;

    void Start()
    {
        A1 = GameObject.FindWithTag("Ai1");
        A2 = GameObject.FindWithTag("Ai2");
        A3 = GameObject.FindWithTag("Ai3");
        player = GameObject.FindWithTag("Player");
        InitializeBomb();
    }

    void OnEnable()
    {
        if (isBombUsed)
        {
            InitializeBomb();
        }
    }

    public void SetPowerUp(int powerUp)
    {
        PowerUp = powerUp;
    }

    private void InitializeBomb()
    {
        isExploded = false; // A bomba ainda não explodiu
        startTime = Time.time; // Registrar o tempo de ativação
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


        spriteRenderer = GetComponent<SpriteRenderer>();

        // Pega a posição da bomba no Tilemap
        bombPosition = tilemapDestrutiveis.WorldToCell(transform.position);
        // Reseta o estado da bomba
        spriteRenderer.sprite = bombSprites[0]; // Coloca o primeiro sprite da animação da bomba
        gameObject.SetActive(true); // Ativa a bomb
        StartCoroutine(ExplosionCountdown()); // Inicia a contagem regressiva
        StartCoroutine(AnimateBomb()); // Inicia a animação da bomba
    }

    public float GetRemainingTime()
    {
        // Calcula o tempo restante baseado no tempo decorrido desde a ativação
        float elapsedTime = Time.time - startTime;
        return Mathf.Max(fuseTime - elapsedTime, 0f);
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

        // Instancia a explosão no local
        Vector3 worldPosition = tilemapPiso.CellToWorld(bombPosition);
        Vector3 tileCenterPosition = worldPosition + tilemapPiso.cellSize / 2f;

        // Instancia a explosão
        GameObject explosion = Instantiate(explosionPrefab, tileCenterPosition, Quaternion.identity);

        // Ajusta o tamanho da explosão de acordo com o powerUp
        float explosionScale = PowerUp * 1f; // Aumenta a escala da explosão conforme o powerUp
        explosion.transform.localScale = new Vector3(explosionScale, explosionScale, 1);

        isBombUsed = true;
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.Stop();
        // Desativa a bomba após a animação de explosão
        ControleDaMorte();
        gameObject.SetActive(false);
    }

    // Função para destruir tiles ao redor da bomba
    // Função para destruir tiles ao redor da bomba com base no powerUp
    private void DestruirTiles()
    {
        // Determina o alcance da explosão com base no powerUp
        int alcance = PowerUp;  // O valor do powerUp aumenta o alcance

        // Verifica os tiles adjacentes no alcance vertical e horizontal
        for (int x = -alcance; x <= alcance; x++)
        {
            // Para a direção X (horizontal), ignora a linha do centro (onde a bomba está)
            if (x != 0)
            {
                Vector3Int tilePosition = bombPosition + new Vector3Int(x, 0, 0);
                // Verifica se o tile é destrutível
                if (tilemapDestrutiveis.HasTile(tilePosition))
                {
                    tilemapDestrutiveis.SetTile(tilePosition, null); // Remove o tile destrutível
                }
            }
        }

        for (int y = -alcance; y <= alcance; y++)
        {
            // Para a direção Y (vertical), ignora a coluna do centro (onde a bomba está)
            if (y != 0)
            {
                Vector3Int tilePosition = bombPosition + new Vector3Int(0, y, 0);
                // Verifica se o tile é destrutível
                if (tilemapDestrutiveis.HasTile(tilePosition))
                {
                    tilemapDestrutiveis.SetTile(tilePosition, null); // Remove o tile destrutível
                }
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


    private void ControleDaMorte()
    {
        // Calcula o alcance da explosão com base no PowerUp
        int alcance = PowerUp;

        // Posição da explosão no mundo
        Vector3 explosaoPosicao = tilemapPiso.CellToWorld(bombPosition) + tilemapPiso.cellSize / 2f;

        // Verifica se o jogador está dentro do raio da explosão (em X e Y)
        if (VerificarDentroRaio(explosaoPosicao, player.transform.position, alcance))
        {
            MatarJogador(player);
        }

        if(A1 != null)
        {
            // Verifica se o AI1 está dentro do raio da explosão (em X e Y)
            if (VerificarDentroRaio(explosaoPosicao, A1.transform.position, alcance))
            {
                MatarAI(A1);
            }
        }
        if (A2 != null)
        {
            // Verifica se o AI2 está dentro do raio da explosão (em X e Y)
            if (VerificarDentroRaio(explosaoPosicao, A2.transform.position, alcance))
            {
                MatarAI(A2);
            }
        }
        if (A3 != null)
        {
            // Verifica se o AI3 está dentro do raio da explosão (em X e Y)
            if (VerificarDentroRaio(explosaoPosicao, A3.transform.position, alcance))
            {
                MatarAI(A3);
            }
        }
    }

    private bool VerificarDentroRaio(Vector3 explosaoPosicao, Vector3 targetPos, int alcance)
    {
        // Converte as posições para coordenadas de tile
        Vector3Int explosaoTilePos = tilemapPiso.WorldToCell(explosaoPosicao);
        Vector3Int targetTilePos = tilemapPiso.WorldToCell(targetPos);

        // Verifica se o alvo está dentro do alcance em X e Y (não diagonal)
        bool dentroRaio = Mathf.Abs(explosaoTilePos.x - targetTilePos.x) <= alcance &&
                          Mathf.Abs(explosaoTilePos.y - targetTilePos.y) <= alcance;

        // Verifica se está na diagonal
        bool naDiagonal = Mathf.Abs(explosaoTilePos.x - targetTilePos.x) == Mathf.Abs(explosaoTilePos.y - targetTilePos.y);

        if(explosaoTilePos == targetTilePos)
        {
            return true;
        }
        // Se estiver na diagonal, retorna false para que não morra
        if (naDiagonal)
        {
            return false;
        }

        return dentroRaio;
    }

    private void MatarJogador(GameObject jogador)
    {
        // Ação para matar o jogador, como desativá-lo ou chamar animação de morte
        Debug.Log("Jogador morreu!");
        player.GetComponent<PlayerController>().SetDeath(true);
        //jogador.SetActive(false); // Desativa o jogador (ou adicione animação de morte)
    }

    private void MatarAI(GameObject ai)
    {
        // Ação para matar o AI, como desativá-lo ou chamar animação de morte
        ai.GetComponent<AIController>().SetDeath(true);
        //ai.SetActive(false); // Desativa o AI (ou adicione animação de morte)
    }
}
