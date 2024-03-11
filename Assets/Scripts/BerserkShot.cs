using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeserkShot : MonoBehaviour
{
    
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
            if (vessel.GetEmotion() > 0)
            {
                vessel.GoBeserk();
            }
            Destroy(gameObject);
        }
    }
}
