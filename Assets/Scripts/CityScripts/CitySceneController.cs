using UnityEngine;

using UnitySceneManager =
    UnityEngine.SceneManagement.SceneManager;

public sealed class CitySceneController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField]
    private string mapSceneName = "MapScene";

    private bool isLoadingScene;

    /// <summary>
    /// Закрывает экран города и возвращает игрока на карту.
    /// Назначается кнопке «Покинуть город».
    /// </summary>
    public void LeaveCity()
    {
        if (isLoadingScene)
            return;

        if (string.IsNullOrWhiteSpace(mapSceneName))
        {
            Debug.LogError(
                "CitySceneController: имя сцены карты не указано.");

            return;
        }

        isLoadingScene = true;

        UnitySceneManager.LoadScene(mapSceneName);
    }
}