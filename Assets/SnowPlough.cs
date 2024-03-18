using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowPlough : MonoBehaviour
{
    [SerializeField] Transform m_playerTransformRef;
    [SerializeField] SpriteRenderer m_spriteRendererRef;
    float m_originalAlpha;

    internal void SetScale(float a_scale) { transform.localScale = new Vector3(a_scale, a_scale, 1f); }

    // Start is called before the first frame update
    void Start()
    {
        m_originalAlpha = m_spriteRendererRef.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        RefreshPosition();
        float alpha = VLib.ModulateRatio(m_originalAlpha, VLib.SinTimeZeroToOne(1f), 0.55f);
        m_spriteRendererRef.color = new Color(m_spriteRendererRef.color.r, m_spriteRendererRef.color.g, m_spriteRendererRef.color.b, alpha);
    }

    void OnEnable()
    {
        RefreshPosition();
    }


    void RefreshPosition()
    {
        transform.position = m_playerTransformRef.position;
        transform.rotation = m_playerTransformRef.rotation;
    }

    private void OnTriggerStay2D(Collider2D a_collider)
    {
        Vessel vessel = a_collider.gameObject.GetComponent<Vessel>();
        if (vessel != null)
        {
            Vector2 collisionVector;
            bool suckingThrough = false;

            if (suckingThrough)
            {
                Vector3 aimPointLeft = m_playerTransformRef.position + VLib.Euler2dAngleToVector3(m_playerTransformRef.eulerAngles.z - 90f).normalized * 1.7f;
                Vector3 aimPointRight = m_playerTransformRef.position + VLib.Euler2dAngleToVector3(m_playerTransformRef.eulerAngles.z + 90f).normalized * 1.7f;

                float deltaLeft = (vessel.transform.position - aimPointLeft).magnitude;
                float deltaRight = (vessel.transform.position - aimPointRight).magnitude;

                Vector3 aimPoint = deltaLeft < deltaRight ? aimPointLeft : aimPointRight;

                collisionVector = (aimPoint - vessel.transform.position).normalized;
            }
            else
            {
                collisionVector = -(m_playerTransformRef.position - vessel.transform.position).normalized;
            }


            //collisionVector = VLib.Euler2dAngleToVector3(transform.parent.transform.eulerAngles.z + 180f);
            vessel.CollideWithPlayer(collisionVector);
        }
    }
}
