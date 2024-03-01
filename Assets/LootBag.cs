using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBag : MonoBehaviour
{
    const int m_scoreAmount = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        if (a_collision.gameObject.tag == "Player")
        {
            GameHandler.ChangeScore(m_scoreAmount);
            Destroy(gameObject);
        }
    }
}
