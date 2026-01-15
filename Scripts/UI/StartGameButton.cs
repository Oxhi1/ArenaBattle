using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    // Buraya GameScene’in ismini yazıyoruz
    public string sceneName = "GameScene";

    public void StartGame()
    {
        Debug.Log("StartGameButton basıldı, sahne yükleniyor: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
