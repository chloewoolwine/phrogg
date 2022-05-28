using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public PlayerController player;

    public float smoothTimeX;
    public float smoothTimeY;

    public float xoffset;
    public float yoffset = 2;

    private Vector2 velocity;
    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.Instance.player;
    }

    // Update is called once per frame
    void Update()
    {
        float x = Mathf.SmoothDamp(transform.position.x-xoffset, player.transform.position.x, ref velocity.x, smoothTimeX);
        float y = Mathf.SmoothDamp(transform.position.y-yoffset, player.transform.position.y, ref velocity.y, smoothTimeY);

        transform.position = new Vector3(x + xoffset, y + yoffset, -10);
    }
}
