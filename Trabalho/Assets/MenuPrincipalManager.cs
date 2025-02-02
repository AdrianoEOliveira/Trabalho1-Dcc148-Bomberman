using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private Button botao2Players;
    [SerializeField] private Button botao3Players;
    [SerializeField] private Button botao4Players;

    private void Start()
    {
        botao2Players.onClick.AddListener(() => IniciarJogo2());
        botao3Players.onClick.AddListener(() => IniciarJogo3());
        botao4Players.onClick.AddListener(() => IniciarJogo4());
    }

    public void IniciarJogo2()
    {
        SceneManager.LoadScene("Arena"); // Substitua pelo nome correto da cena do jogo
    }

    public void IniciarJogo3()
    {
        SceneManager.LoadScene("CenaArena"); // Substitua pelo nome correto da cena do jogo
    }

    public void IniciarJogo4()
    {
        SceneManager.LoadScene("CenaArena"); // Substitua pelo nome correto da cena do jogo
    }
}
