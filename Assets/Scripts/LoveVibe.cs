using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoveVibe : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_rigidBodyRef;
    [SerializeField] SpriteRenderer m_spriteRendererRef;


    Soul m_originSoul;

    //Absorption
    Vector3 m_originalLocalScale;
    Soul m_targetSoul;
    vTimer m_absorptionTimer;
    Vector3 m_absorptionStartingLocalPosition;
    const float m_absorptionTime = 0.4f;
    bool m_beingAbsorbed = false;

    float m_startingSpeed = 5f;

    internal bool IsOriginSoul(Soul a_soul) { return m_originSoul == a_soul; }

    // Start is called before the first frame update
    void Start()
    {
        m_originalLocalScale = transform.localScale;
        //m_rigidBodyRef.velocity = VLib.Euler2dAngleToVector3(transform.eulerAngles.z).normalized * m_startingSpeed;
    }

    internal void Init(Soul a_originSoul, Vector2 a_travelDirection)
    {
        m_originSoul = a_originSoul;
        m_rigidBodyRef.velocity = a_travelDirection * m_startingSpeed;
        //transform.rotation = VLib.Vector3ToQuaternion(a_travelDirection);
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_spriteRendererRef.isVisible)
        {
            Destroy(gameObject);
        }
        else if (m_beingAbsorbed)
        {
            if (m_absorptionTimer.Update())
            {
                Destroy(gameObject);
            }
            else
            {
                float completionPercentage = m_absorptionTimer.GetCompletionPercentage();
                transform.localPosition = Vector3.Lerp(m_absorptionStartingLocalPosition, new Vector3(), completionPercentage);
                transform.localScale = m_originalLocalScale * (1f - completionPercentage);
                m_spriteRendererRef.color = new Color(m_spriteRendererRef.color.r, m_spriteRendererRef.color.g, m_spriteRendererRef.color.b, 1f - completionPercentage);
            }
        }
    }

    void StartAbsorption(Soul a_absorbingSoul)
    {
        m_beingAbsorbed = true;
        m_rigidBodyRef.isKinematic = true;
        m_targetSoul = a_absorbingSoul;
        GetComponent<Collider2D>().enabled = false;
        transform.parent = a_absorbingSoul.transform;
        m_absorptionStartingLocalPosition = transform.localPosition;
        m_absorptionTimer = new vTimer(m_absorptionTime);
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        Soul collidedSoul = a_collision.gameObject.GetComponent<Soul>();
        if (collidedSoul != null && collidedSoul != m_originSoul)
        {
            StartAbsorption(collidedSoul);
            //Destroy(gameObject);
        }
    }
}
