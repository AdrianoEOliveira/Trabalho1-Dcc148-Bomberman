using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private Button botao1Players;
    [SerializeField] private Button botao2Players;
    [SerializeField] private Button botao3Players;

    private void Start()
    {
        botao1Players.onClick.AddListener(() => IniciarJogo1());
        botao2Players.onClick.AddListener(() => IniciarJogo2());
        botao3Players.onClick.AddListener(() => IniciarJogo3());
    }

    public void IniciarJogo1()
    {
        SceneManager.LoadScene("Arena1"); 
    }

    public void IniciarJogo2()
    {
        SceneManager.LoadScene("Arena2"); 
    }

    public void IniciarJogo3()
    {
        SceneManager.LoadScene("Arena3");
    }
}
