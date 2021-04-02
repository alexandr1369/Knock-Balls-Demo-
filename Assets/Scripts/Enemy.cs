using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private void Update()
    {
        if (transform.position.y < 0)
        {
            GameManager.instance.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
