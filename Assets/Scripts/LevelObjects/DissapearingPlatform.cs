using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissapearingPlatform : MonoBehaviour
{
    public float dissapearTime;
    public float reappearTime;

    public bool isDissapearing;
    public Collider2D myCollider;
    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        myCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player" && !isDissapearing)
        {
            isDissapearing = true;
            StartCoroutine(Dissapear());
        }
    }

    IEnumerator Dissapear()
    {
        yield return new WaitForSeconds(dissapearTime);
        //do some kinda animation here
        myCollider.enabled = false;
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(reappearTime);
        myCollider.enabled = true;
        spriteRenderer.enabled = true;
        isDissapearing = false;
    }

}
