using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryManager : MonoBehaviour
{
    [SerializeField] private Button voltar;

    private void Start()
    {

        voltar.onClick.AddListener(() => VoltarMenu());
    }

    public void VoltarMenu()
    {
        SceneManager.LoadScene("MenuPrincipal"); 
    }
}
