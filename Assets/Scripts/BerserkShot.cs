using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeserkShot : MonoBehaviour
{
    const int m_minimumEmotionAffected = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Init(Vector2 a_velocity)
    {
        GetComponent<Rigidbody2D>().velocity = a_velocity;
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        if (a_collision.gameObject.tag == "Vessel") 
        {
            Vessel vessel = a_collision.gameObject.GetComponent<Vessel>();
            if (vessel.GetEmotion() >= m_minimumEmotionAffected)
            {
                vessel.AddEmotion(1000);
                vessel.GoBeserk();
            }
            Destroy(gameObject);
        }
    }
}
