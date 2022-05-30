using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantDeath : MonoBehaviour
{
    public AudioSource deathnoise;
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            //do some animation or what not 
            StartCoroutine(col.GetComponent<PlayerController>().Die());
            deathnoise.Play();
        }
    }
}
