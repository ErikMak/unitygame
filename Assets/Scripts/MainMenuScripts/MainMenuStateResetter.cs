using UnityEngine;

public sealed class MainMenuStateResetter : MonoBehaviour
{
    private void Awake()
    {
        GameState.Instance?.StartNewGame();
    }
}
