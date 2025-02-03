using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using System.Linq;
using Unity.VisualScripting;

public class AIController : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Sprite[] UpSprites;
    [SerializeField] private Sprite[] LeftSprites;
    [SerializeField] private Sprite[] RightSprites;
    [SerializeField] private Sprite[] DownSprites;
    [SerializeField] private Sprite[] deathSprites;

    private Tilemap tilemapPiso;
    private Tilemap tilemapParedes;
    private Tilemap tilemapDestrutiveis;
    private Tilemap tilemapItem;
    private SpriteRenderer spriteRenderer;
    private ObjectPool bombPool;

    private Vector3 targetPosition;
    private bool isMoving = false;
    [SerializeField] public bool isDead = false;
    private int powerUp = 1;

    [SerializeField] private float minBombInterval = 2.5f;
    [SerializeField] private float maxBombInterval = 3.5f;
    private float bombPlacementTimer;
    private float currentBombInterval;

    [SerializeField] private float destructibleWeight = 2f;
    [SerializeField] private float playerWeight = 1f;

    [SerializeField] private float enemyWeight = 1.5f;
    [SerializeField] private float minBombScore = 1.5f;

    [SerializeField] private int maxSafeMoves = 100;  // Máximo de movimentos seguros
    private int safeMoveCount = 0;  // Contador de movimentos seguros

    [SerializeField] private GameObject enemy1;
    [SerializeField] private GameObject enemy2;

    private void Awake()
    {
        tilemapPiso = GameObject.FindWithTag("Piso")?.GetComponent<Tilemap>();
        tilemapParedes = GameObject.FindWithTag("Parede")?.GetComponent<Tilemap>();
        tilemapDestrutiveis = GameObject.FindWithTag("Destrutiveis")?.GetComponent<Tilemap>();
        tilemapItem = GameObject.FindWithTag("Item")?.GetComponent<Tilemap>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        bombPool = new ObjectPool(bombPrefab, 3); // Inicializa o pool com 3 bombas

        Vector3Int initialCell = tilemapPiso.WorldToCell(transform.position);
        transform.position = tilemapPiso.GetCellCenterWorld(initialCell);
        SetNewBombInterval();
    }

    private void Update()
    {
        CheckItemTile();
        HandleBombPlacement();
        Death();
        if (isDead || isMoving || safeMoveCount >= maxSafeMoves) return;

        Vector3Int currentCell = tilemapPiso.WorldToCell(transform.position);
        List<Vector3Int> possibleMoves = GetPossibleMoves(currentCell);

        if (possibleMoves.Count > 0)
        {
            Vector3Int safestMove = FindSafestMove(possibleMoves);
            if (safestMove != currentCell)
            {
                targetPosition = tilemapPiso.GetCellCenterWorld(safestMove);
                StartCoroutine(MoveToTarget());
                safeMoveCount++;  // Incrementa o contador de movimentos seguros
            }
        }
    }

    private void CheckItemTile()
    {
        Vector3Int currentCell = tilemapItem.WorldToCell(transform.position);
        if (tilemapItem.HasTile(currentCell))
        {
            tilemapItem.SetTile(currentCell, null);
            powerUp++;
            bombPool.SetPowerUp(powerUp); // Atualiza o powerUp no pool de bombas
        }
    }

    private void HandleBombPlacement()
    {
        if (isDead) return;

        bombPlacementTimer += Time.deltaTime;
        if (bombPlacementTimer >= currentBombInterval)
        {
            Bomb();
            SetNewBombInterval();
        }
    }

    private void SetNewBombInterval()
    {
        currentBombInterval = Random.Range(minBombInterval, maxBombInterval);
    }

    private void Bomb()
    {
        if (!CanEscapeAfterBombPlacement() || !IsGoodBombPosition(transform.position))
        {
            return; // Não coloca a bomba se não puder escapar
        }

        GameObject bomb = bombPool.GetFromPool();
        if (bomb != null)
        {
            Vector3Int tilePosition = tilemapPiso.WorldToCell(transform.position);
            Vector3 bombPosition = tilemapPiso.GetCellCenterWorld(tilePosition);
            bomb.transform.position = bombPosition;
            bomb.SetActive(true);
            bombPlacementTimer = 0f;
        }
    }

    private bool IsGoodBombPosition(Vector3 position)
    {
        float score = CalculateBombScore(position);
        return score >= minBombScore;
    }

    private float CalculateBombScore(Vector3 position)
    {
        float score = 0f;

        // Verifica paredes destrutíveis próximas
        Vector3Int cell = tilemapDestrutiveis.WorldToCell(position);
        score += CountAdjacentDestructibles(cell) * destructibleWeight;

        // Verifica proximidade do jogador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(position, player.transform.position);
            score += playerWeight * (1f / Mathf.Max(distance, 0.1f));
        }
        GameObject[] enemies = { enemy1, enemy2 };
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                score += enemyWeight * (1f / Mathf.Max(distance, 0.1f));
            }
        }

        return score;
    }

    private int CountAdjacentDestructibles(Vector3Int cell)
    {
        int count = 0;
        Vector3Int[] directions = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var dir in directions)
        {
            if (tilemapDestrutiveis.HasTile(cell + dir))
                count++;
        }
        return count;
    }

    private bool CanEscapeAfterBombPlacement()
    {
        Vector3Int currentCell = tilemapPiso.WorldToCell(transform.position);

        // Apenas direções ortogonais (cima, baixo, esquerda, direita)
        List<Vector3Int> orthogonalCells = new List<Vector3Int>
    {
        currentCell + Vector3Int.up,
        currentCell + Vector3Int.down,
        currentCell + Vector3Int.left,
        currentCell + Vector3Int.right
    };

        int escapeRoutes = 0;

        // Conta quantos caminhos ortogonais estão livres
        foreach (Vector3Int cell in orthogonalCells)
        {
            Vector3 worldPos = tilemapPiso.GetCellCenterWorld(cell);
            if (!IsObstacle(worldPos))
            {
                escapeRoutes++;
                if (escapeRoutes >= 1)
                {
                    return true; // Pelo menos 2 caminhos livres, pode colocar a bomba
                }
            }
        }

        return false; // Menos de 2 caminhos livres, não coloca a bomba
    }


    private List<Vector3Int> GetPossibleMoves(Vector3Int currentCell)
    {
        List<Vector3Int> moves = new List<Vector3Int>();

        // 🔹 Depois verifica os movimentos individuais
        AddMoveIfValid(moves, currentCell + Vector3Int.left);  // Left
        AddMoveIfValid(moves, currentCell + Vector3Int.up);    // Up
        AddMoveIfValid(moves, currentCell + Vector3Int.down);  // Down
        AddMoveIfValid(moves, currentCell + Vector3Int.right); // Right

        // 🔹 Verifica as diagonais se o primeiro for disponivel
        if (moves.Contains(currentCell + Vector3Int.left))
        {
            AddMoveIfValid(moves, currentCell + Vector3Int.left * 2);
        }
        if (moves.Contains(currentCell + Vector3Int.up))
        {
            AddMoveIfValid(moves, currentCell + Vector3Int.up * 2);
        }
        if (moves.Contains(currentCell + Vector3Int.right))
        {
            AddMoveIfValid(moves, currentCell + Vector3Int.right * 2);
        }
        if (moves.Contains(currentCell + Vector3Int.down))
        {
            AddMoveIfValid(moves, currentCell + Vector3Int.down * 2);
        }
        return moves;
    }
    private void AddMoveIfValid(List<Vector3Int> moves, Vector3Int cell)
    {
        Vector3 worldPos = tilemapPiso.GetCellCenterWorld(cell);
        if (!IsObstacle(worldPos)) // Só adiciona se não for parede ou bloco destrutível
        {
            moves.Add(cell);
        }
    }

    private void CheckMove(List<Vector3Int> moves, Vector3Int cell)
    {
        Vector3 worldPos = tilemapPiso.GetCellCenterWorld(cell);
        if (!IsObstacle(worldPos)) moves.Add(cell);
    }

    private bool IsObstacle(Vector3 position)
    {
        Vector3Int cell = tilemapParedes.WorldToCell(position);
        if (tilemapParedes.HasTile(cell)) return true;

        cell = tilemapDestrutiveis.WorldToCell(position);
        if (tilemapDestrutiveis.HasTile(cell)) return true;

        // Check for active bombs at the position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("bomb") && collider.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

 private Vector3Int FindSafestMove(List<Vector3Int> possibleMoves)
{
    if (possibleMoves.Count == 0)
        return tilemapPiso.WorldToCell(transform.position);

    Vector3Int currentCell = tilemapPiso.WorldToCell(transform.position);
    possibleMoves.Add(currentCell);

    float maxSafety = float.MinValue;
    List<Vector3Int> equallySafeMoves = new List<Vector3Int>();

    // Verifica a segurança e coleta movimentos igualmente seguros
    foreach (Vector3Int move in possibleMoves)
    {
        Vector3 movePos = tilemapPiso.GetCellCenterWorld(move);
        float safetyScore = CalculateSafety(movePos);

        // Se encontrar um movimento mais seguro, reinicia a lista
        if (safetyScore > maxSafety)
        {
            maxSafety = safetyScore;
            equallySafeMoves.Clear();
            equallySafeMoves.Add(move);
        }
        // Se a segurança for a mesma, adiciona à lista de opções seguras
        else if (safetyScore == maxSafety)
        {
            equallySafeMoves.Add(move);
        }
    }

    // Se algum movimento seguro já for muito próximo (por exemplo, a uma distância pequena), ignora os outros
    if (equallySafeMoves.Count > 0)
    {
        Vector3Int closestSafeMove = equallySafeMoves[0];
        float closestDistance = Vector3.Distance(tilemapPiso.GetCellCenterWorld(closestSafeMove), transform.position);
        
        // Defina um limite de proximidade (ajuste o valor conforme necessário)
        float proximityThreshold = 1.0f;  // Distância limite para considerar um movimento como "muito próximo"

        if (closestDistance <= proximityThreshold)
        {
            return closestSafeMove;
        }

        // Se não houver um movimento suficientemente próximo, ordena os movimentos seguros pela proximidade
        equallySafeMoves.Sort((a, b) => Vector3.Distance(tilemapPiso.GetCellCenterWorld(a), transform.position)
                                            .CompareTo(Vector3.Distance(tilemapPiso.GetCellCenterWorld(b), transform.position)));
        
        return equallySafeMoves[0];  // Retorna o mais seguro e mais próximo
    }

    // Caso não haja movimentos seguros, retornar o mais próximo entre os possíveis
    return possibleMoves.OrderBy(move => Vector3.Distance(tilemapPiso.GetCellCenterWorld(move), transform.position)).First();
}

    private float CalculateSafety(Vector3 position)
    {
        float safety = 0f;
        foreach (BombController bomb in FindObjectsByType<BombController>(FindObjectsSortMode.None))
        {
            if (IsPositionInBombExplosion(position, bomb))
            {
                // Closer bombs with less time remaining are more dangerous
                float distance = Vector3.Distance(position, bomb.transform.position);
                float timeFactor = 1 - (bomb.GetRemainingTime() / bomb.fuseTime);
                safety -= 1 / distance * timeFactor * 100;
            }
            else
            {
                // Add positive safety for distance from bombs
                safety += Vector3.Distance(position, bomb.transform.position);
            }
        }
        return safety;
    }

    private bool IsPositionInBombExplosion(Vector3 position, BombController bomb)
    {
        return VerificarDentroRaio(bomb.transform.position, position, powerUp);
    }
    private bool VerificarDentroRaio(Vector3 explosaoPosicao, Vector3 targetPos, int alcance)
    {
        // Converte as posições para coordenadas de tile
        Vector3Int explosaoTilePos = tilemapPiso.WorldToCell(explosaoPosicao);
        Vector3Int targetTilePos = tilemapPiso.WorldToCell(targetPos);

        // Verifica se o alvo está dentro do alcance em X e Y (não diagonal)
        bool dentroRaio = Mathf.Abs(explosaoTilePos.x - targetTilePos.x) <= alcance &&
                          Mathf.Abs(explosaoTilePos.y - targetTilePos.y) <= alcance;

        if (explosaoPosicao == targetPos)
        {
            return true;
        }

        // Verifica se está na diagonal
        bool naDiagonal = Mathf.Abs(explosaoTilePos.x - targetTilePos.x) == Mathf.Abs(explosaoTilePos.y - targetTilePos.y);

        // Se estiver na diagonal, retorna false para que não morra
        if (naDiagonal)
        {
            return false;
        }

        return dentroRaio;
    }

    private IEnumerator MoveToTarget()
    {
        isMoving = true;
        float distance;

        do
        {
            distance = Vector3.Distance(transform.position, targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            UpdateSpriteAnimation();
            yield return null;
        } while (distance > 0.01f);

        transform.position = targetPosition;
        isMoving = false;
    }

    private void UpdateSpriteAnimation()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Sprite[] sprites = GetSpritesForDirection(direction);

        if (sprites != null && sprites.Length > 0)
        {
            int frame = (int)(Time.time * 10) % sprites.Length;
            spriteRenderer.sprite = sprites[frame];
        }
    }

    private Sprite[] GetSpritesForDirection(Vector3 direction)
    {
        if (direction.y > 0) return UpSprites; // Up
        if (direction.y < 0) return DownSprites; // Down
        if (direction.x < 0) return LeftSprites; // Left
        if (direction.x > 0) return RightSprites; // Right
        return DownSprites;
    }

    public void SetDeath(bool death)
    {
        isDead = death;
        if (death) StartCoroutine(PlayDeathAnimation());
    }

    private IEnumerator PlayDeathAnimation()
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
        if (isDead)
        {
            StartCoroutine(PlayDeathAnimation());
        }

        //gameObject.SetActive(false);
    }
}