using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantDeath : MonoBehaviour
{
    public AudioSource deathnoise;
    void OnTriggerEnter2D(Collider2D col)
    {
       
        Debug.Log("Collision on DEATH");
        if (col.tag == "Player")
        {
            Debug.Log("PLAYERFOUND");
            //do some animation or what not 
            StartCoroutine(col.GetComponent<PlayerController>().Die());
            deathnoise.Play();
        }
    }
}
