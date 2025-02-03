using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] enemies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Verifique a quantidade de inimigos presentes
        foreach (var enemy in enemies)
        {
            if (enemy == null)
            {
                Debug.LogWarning("Enemy missing: " + enemy.name);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Verificar se o jogador está morto
        if (player.GetComponent<PlayerController>().isDead)
        {
            SceneManager.LoadScene("GameOver");
        }

        // Verificar se todos os inimigos estão mortos
        if (AreAllEnemiesDead())
        {
            Debug.Log("Win");
            SceneManager.LoadScene("Win");
        }
    }

    // Verifica se todos os inimigos estão mortos
    private bool AreAllEnemiesDead()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                AIController aiController = enemy.GetComponent<AIController>();
                if (aiController != null && !aiController.isDead)
                {
                    return false; // Se algum inimigo não estiver morto, retorna falso
                }
            }
        }
        return true; // Todos os inimigos estão mortos
    }
}
