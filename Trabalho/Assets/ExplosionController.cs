using System.Collections;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer; // Componente SpriteRenderer
    public Sprite[] explosionSprites; // Array de Sprites da explosão
    public float duration = 3f; // Duração total da animação

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Pega o SpriteRenderer do jogador
        StartCoroutine(AnimateExplosion());
    }

    private IEnumerator AnimateExplosion()
    {
        int spriteCount = explosionSprites.Length; // Quantidade de sprites
        float frameDuration = duration / spriteCount; // Tempo por sprite

        for (int i = 0; i < spriteCount; i++)
        {
            spriteRenderer.sprite = explosionSprites[i]; // Atualiza o sprite
            yield return new WaitForSeconds(frameDuration); // Espera antes de trocar o sprite
        }

        // Quando a animação termina, desativa o objeto
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}