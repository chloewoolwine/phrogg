using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Transform exactPlayerLocation;
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            //do some animation or what not 
            GameManager.Instance.respawns.SetSpawn(this);
        }
    }
}
