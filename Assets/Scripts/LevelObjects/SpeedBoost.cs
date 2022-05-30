using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public bool right;
    public float boost = 40;

    public bool onCooldown;
    public float cooldown = 1;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player" && !onCooldown)
        {
            GameManager.Instance.player.additionalMomentum += right ? boost : boost * -1;
            onCooldown = true;
            StartCoroutine(Cooldown());
        }
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}
