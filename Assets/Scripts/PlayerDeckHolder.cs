using UnityEngine;

public class PlayerDeckHolder : MonoBehaviour
{
    public static PlayerDeckHolder Instance { get; private set; }

    public PlayerDeck playerDeck;

    private void Awake()
    {
        // Ensure only one instance of PlayerDeckHolder exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}