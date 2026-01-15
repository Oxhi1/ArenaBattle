using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;

    public CharacterHealth player;
    public CharacterHealth rival;

    private bool isGameOver = false;

    private void Update()
    {
        if (isGameOver) return;

        if (player.currentHealth <= 0)
        {
            ShowGameOver("KAYBETTİN!");
        }
        else if (rival.currentHealth <= 0)
        {
            ShowGameOver("KAZANDIN!");
        }
    }

    void ShowGameOver(string message)
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        resultText.text = message;
        Time.timeScale = 0f;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Aktif sahneyi yeniden yükle
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Oyunu devam ettir
        SceneManager.LoadScene(0); // MainMenu (Build Settings'te 0. sırada olmalı)
    }
}
