using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] SpriteRenderer m_spriteRendererRef;
    [SerializeField] Rigidbody2D m_rigidBodyRef;
    [SerializeField] MouthLineHandler m_mouthLineHandlerRef;
    [SerializeField] EyebrowHandler[] m_eyebrowHandlers; 

    BattleHandler m_battleHandlerRef;
    PlayerHandler m_playerHandlerRef;

    //Soul Attraction
    const float m_soulAttractionConstant = 1f;
    const float m_attractionExponent = 1f;
    const float m_soulRepulsionConstant = 1f;
    const float m_repulsionExponent = 4f;

    const float m_soulToSoulForceConstant = 10f;
    internal const bool m_repulsedByOtherSouls = true;

    //Player Attraction
    //const float m_playerAttractionConstant = 1f;
    //const float m_playerRepulsionConstant = 2f;
    //const float m_playerForceMult = 10f;

    const float m_loveVibeSpawnOffset = 0.42f;
    const float m_loveVibeSpawnAngleRange = 90f;
    float m_love = 50f;
    float m_maxLove = 100f;
    int m_loveToSpawn = 2;
    float m_reEmitTime = 0.5f;

    [SerializeField] Color m_sadColorRef;
    [SerializeField] Color m_happyColorRef;

    struct AbsorbedLove
    {
        internal vTimer timer;
        internal Vector2 collisionNormal;
    } 

    List<AbsorbedLove> m_absorbedLoveList;

    float GetLoveRatio() { return m_love/m_maxLove;}
    float GetLoveScale() { return 2 * (GetLoveRatio() - 0.5f); }

    void Awake()
    {
        m_absorbedLoveList = new List<AbsorbedLove>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    internal void Init(BattleHandler a_battleHandler, PlayerHandler a_playerHandler)
    {
        m_battleHandlerRef = a_battleHandler;
        m_playerHandlerRef = a_playerHandler;
        UpdateVisuals();
    }

    void AbsorbedLoveUpdate()
    {
        for (int i = 0; i < m_absorbedLoveList.Count; i++)
        {
            if (m_absorbedLoveList[i].timer.Update())
            {
                EmitLove(m_absorbedLoveList[i].collisionNormal);
                m_absorbedLoveList.RemoveAt(i);
                i--;
            }
        }
    }

    void UpdateVisuals()
    {
        m_spriteRendererRef.color = Color.Lerp(m_sadColorRef, m_happyColorRef, m_love / m_maxLove);
        m_mouthLineHandlerRef.Refresh(GetLoveScale());
        for (int i = 0; i < m_eyebrowHandlers.Length; i++)
        {
            m_eyebrowHandlers[i].SetEybrowRotation(GetLoveScale());
        }
    }

    void AttractToPlayer()
    {
        AttractToSoul(m_playerHandlerRef.transform.position);

        //Vector3 deltaVector = m_playerHandlerRef.transform.position - transform.position;
        //Vector3 directionVector = deltaVector.normalized;
        //float distanceMagnitude = deltaVector.magnitude;
        //
        //float attractiveForce = m_playerAttractionConstant / Mathf.Pow(distanceMagnitude, 1f);
        //float repulsiveForce = m_playerRepulsionConstant / Mathf.Pow(distanceMagnitude, 2f);
        //Vector3 forceVector = directionVector * m_playerForceMult * (attractiveForce - repulsiveForce);
        //
        //m_rigidBodyRef.AddForce(forceVector);
    }

    internal void AttractToSoul(Vector3 a_otherSoulsPosition)
    {
        Vector3 deltaVector = gameObject.transform.position - a_otherSoulsPosition;
        Vector3 directionVector = deltaVector.normalized;
        float distanceMagnitude = deltaVector.magnitude;

        float attractiveForce = m_soulAttractionConstant / Mathf.Pow(distanceMagnitude, m_attractionExponent);
        float repulsiveForce = m_soulRepulsionConstant / Mathf.Pow(distanceMagnitude, m_repulsionExponent);
        float sadnessEffect = 1f-GetLoveRatio();

        repulsiveForce *= 1f + sadnessEffect * 10f;

        //attractiveForce += loveEffect;
        Vector3 forceVector = directionVector * (repulsiveForce-attractiveForce);
        forceVector *= m_soulToSoulForceConstant;
        m_rigidBodyRef.AddForce(forceVector);
    }

    // Update is called once per frame
    void Update()
    {
        AbsorbedLoveUpdate();
        AttractToPlayer();
    }

    void EmitLove(Vector2 a_collisionNormal)
    {
        for (int i = 0; i < m_loveToSpawn; i++)
        {
            float spawnAngle = ((float)(i+1)/(m_loveToSpawn+1)) * m_loveVibeSpawnAngleRange;
            spawnAngle -= m_loveVibeSpawnAngleRange/2f;
            Vector2 collisionDirection = VLib.RotateVector3In2D(a_collisionNormal, spawnAngle);

            Vector3 spawnOffset = collisionDirection * m_loveVibeSpawnOffset;
            LoveVibe newLoveVibe = Instantiate(m_loveVibePrefab, transform.position + spawnOffset, Quaternion.identity).GetComponent<LoveVibe>();
            newLoveVibe.Init(this, collisionDirection);
        }
    }

    void AddLove(float a_love)
    {
        m_love += a_love;
        m_love = Mathf.Clamp(m_love, 0f, m_maxLove);
        UpdateVisuals();
    }

    void AbsorbLove(Vector3 a_collisionNormal)
    {
        AbsorbedLove absorbedLove = new AbsorbedLove();
        absorbedLove.timer = new vTimer(m_reEmitTime);
        absorbedLove.collisionNormal = a_collisionNormal;
        m_absorbedLoveList.Add(absorbedLove);
        AddLove(1f);
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        LoveVibe loveVibe = a_collision.gameObject.GetComponent<LoveVibe>();
        if (loveVibe != null && !loveVibe.IsOriginSoul(this)) 
        {
            AbsorbLove(a_collision.contacts[0].normal);
        }

        
    }
}