using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LootBag : MonoBehaviour
{
    BattleHandler m_battleHandlerRef;

    const int m_scoreAmount = 0;
    const int m_vibesDispensed = 6;
    const float m_startingSpeed = 20f;
    [SerializeField] GameObject m_vibePrefab;
    [SerializeField] Rigidbody2D m_rigidBodyRef;
    [SerializeField] AudioClip m_popSoundRef;
    [SerializeField] ParticleSystem m_popParticleSystemRef;
    const float m_acceleration = 200f;

    internal void SetBattleHandlerRef(BattleHandler a_battleHandler) { m_battleHandlerRef = a_battleHandler; }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBodyRef.velocity = new Vector2(VLib.vRandom(-1f,1f), VLib.vRandom(-1,1f));
        m_rigidBodyRef.velocity = 1f * m_rigidBodyRef.velocity.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (m_rigidBodyRef.velocity.magnitude < m_startingSpeed)
        {
            Vector3 acceleratingForce = m_rigidBodyRef.velocity.normalized * Time.fixedDeltaTime * m_acceleration * m_rigidBodyRef.mass;
            acceleratingForce = acceleratingForce.RotateVector3In2D(VLib.vRandom(-90f, 90f));
            m_rigidBodyRef.AddForce(acceleratingForce);
        }
    }

    void Pop()
    {
        m_battleHandlerRef.IncreaseLootBagBonus();
        GameHandler.ChangeScore(m_scoreAmount);
        for (int i = 0; i < m_vibesDispensed; i++) 
        {
            float angle = i * 360f / m_vibesDispensed;
            Vector2 direction = Vector2.up.RotateVector2(angle);
            Vibe loveVibe = Instantiate(m_vibePrefab, transform.position, Quaternion.identity).GetComponent<Vibe>();
            loveVibe.Init(null, direction, m_rigidBodyRef.velocity);
        }
        GameHandler._audioManager.PlaySFX(m_popSoundRef, 4f);
        m_popParticleSystemRef.Play();
        m_popParticleSystemRef.gameObject.transform.parent = null;
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        if (a_collision.gameObject.tag == "Player")
        {
            Pop();
        }
    }
}
