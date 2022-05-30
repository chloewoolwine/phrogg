using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform from;
    public Transform to;
    public Transform platform;
    public float speed = 1f;

    public float counter;
    public Vector2 currentPos;
    bool direction;

    void FixedUpdate()
    {
        if (direction)
        {
            counter += .1f * speed;
            currentPos = Vector2.Lerp(from.position, to.position, counter);
            platform.position = currentPos;
            if (counter > 1f)
                direction = !direction;
        } else
        {
            counter -= .1f * speed;
            currentPos = Vector2.Lerp(from.position, to.position, counter);
            platform.position = currentPos;
            if (counter < 0f)
                direction = !direction;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            collision.collider.transform.parent = transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            collision.collider.transform.parent = null;
        }
    }
}
