using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//there should be ONE respawn manager for EACH level
public class RespawnManager : MonoBehaviour
{
    public Level Level;
    public SpawnPoint currentSpawn;

    // Start is called before the first frame update
    void Start()
    {
        if (Level != GameManager.Instance.CurrentLevel)
            Debug.LogError("Error! Respawn Manager level does not match game manager level");
    }

    public void SetSpawn(SpawnPoint newSpawn)
    {
        currentSpawn = newSpawn;
    }

    public void RespawnPlayer()
    {
        //do some sort of animation or something
        PlayerController player = GameManager.Instance.player;
        player.Revive();
        player.transform.SetPositionAndRotation(currentSpawn.transform.position, currentSpawn.transform.rotation);
    }
}
