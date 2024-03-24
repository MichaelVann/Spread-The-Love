using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeserkShot : MonoBehaviour
{
    [SerializeField] GameObject m_spriteObjectRef;
    Rigidbody2D m_rigidBodyRef;
    const int m_minimumEmotionAffected = 0;
    // Start is called before the first frame update
    void Start()
    {
        m_rigidBodyRef = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        m_spriteObjectRef.transform.eulerAngles = VLib.Vector2ToEulerAngles(m_rigidBodyRef.velocity);
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
                vessel.SetEmotion(Soul.GetMaxPossibleLove());
                vessel.GoBeserk();
            }
            Destroy(gameObject);
        }
    }
}
