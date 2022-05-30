using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public Animator animator;
    public Transform center;
    public float bouncePower;

    public bool debounce = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Player")
        {
            Rigidbody2D player = collision.collider.GetComponent<Rigidbody2D>();
            Vector2 force = new Vector2(0, bouncePower);
        //    player.transform.SetPositionAndRotation(center.position, Quaternion.identity);
        //         ^^ last attempt at centering the player onto the trampoline
            player.AddForce(force, ForceMode2D.Impulse);
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle2") || animator.GetCurrentAnimatorStateInfo(0).IsName("shoot"))
            {
                animator.Play("squish");
            }
            
        }
    }
}
