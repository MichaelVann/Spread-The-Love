using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoveVibe : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_rigidBodyRef;
    [SerializeField] SpriteRenderer m_spriteRendererRef;

    Soul m_originSoul;

    float m_startingSpeed = 5f;

    internal bool IsOriginSoul(Soul a_soul) { return m_originSoul == a_soul; }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBodyRef.velocity = VLib.Euler2dAngleToVector3(transform.eulerAngles.z).normalized * m_startingSpeed;
    }

    internal void Init(Soul a_originSoul)
    {
        m_originSoul = a_originSoul;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_spriteRendererRef.isVisible)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        Soul originSoul = a_collision.gameObject.GetComponent<Soul>();
        if (originSoul != null && originSoul != m_originSoul)
        {
            Destroy(gameObject);
        }
    }
}
