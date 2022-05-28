using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Level CurrentLevel;
    public PlayerController player;
    public RespawnManager respawns;

    private void Awake()
    {
        Instance = this;
        if(CurrentLevel != Level.MainMenu)
        {
            player = FindObjectOfType<PlayerController>();
            if (player == null)
                Debug.LogError("Error, no player found in scene!");
            respawns = FindObjectOfType<RespawnManager>();
            if (respawns == null)
                Debug.LogError("Error, no respawn manager found in scene!");
        }
    }
}

public enum Level
{
    MainMenu,
    Level1,
    Level2
}
