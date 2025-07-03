using UnityEngine;

public class MenuInicialController : MonoBehaviour
{
    public GameObject menuUI;              // Painel do menu inicial
    public LevelManager levelManager;      // Arraste no Inspector

    public void Play()
    {
        if (menuUI != null)
            menuUI.SetActive(false);       // Esconde o menu

        if (levelManager != null)
            levelManager.LoadLevel(0);     // Carrega a fase 0 via LevelManager
        else
            Debug.LogError("LevelManager não atribuído no MenuInicialController!");
    }

    public void Quit()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}
