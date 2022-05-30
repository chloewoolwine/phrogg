using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Collider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    public void AttatchToTongue()
    {
    }

    public void Release()
    {
    }

    public void Collect()
    {
        Destroy(gameObject);
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            Collect();
        }
    }
}
