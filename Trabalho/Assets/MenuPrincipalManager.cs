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
        SceneManager.LoadScene("Arena1"); 
    }

    public void IniciarJogo3()
    {
        SceneManager.LoadScene("Arena2"); 
    }

    public void IniciarJogo4()
    {
        SceneManager.LoadScene("Arena3");
    }
}
