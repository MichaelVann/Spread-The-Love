using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vessel : Soul
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
    const float m_fearRepulsionConstant = 2f;

    const float m_soulToSoulForceConstant = 10f;
    internal const bool m_repulsedByOtherSouls = true;

    //Player Attraction
    //const float m_playerAttractionConstant = 1f;
    //const float m_playerRepulsionConstant = 2f;
    //const float m_playerForceMult = 10f;

    const float m_loveVibeSpawnOffset = 0.42f;
    const float m_loveVibeSpawnAngleRange = 90f;
    //float m_love = 50f;
    //float m_maxLove = 100f;
    int m_loveToSpawn = 2;
    float m_reEmitTime = 0.5f;

    const float m_fearfulToCalmExpressionIncrease = 1f;
    float m_expressInterval = 4f;
    vTimer m_expressionTimer;

    struct AbsorbedLove
    {
        internal vTimer timer;
        internal Vector2 collisionNormal;
        internal Vector2 emotion;
    } 

    List<AbsorbedLove> m_absorbedLoveList;


    void Awake()
    {
        m_absorbedLoveList = new List<AbsorbedLove>();
        m_expressionTimer = new vTimer(m_expressInterval);
        m_expressionTimer.SetTimer(VLib.vRandom(0f, m_expressInterval));
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    internal void Init(BattleHandler a_battleHandler, PlayerHandler a_playerHandler, Vector2 a_emotion)
    {
        m_battleHandlerRef = a_battleHandler;
        m_playerHandlerRef = a_playerHandler;
        m_emotion = a_emotion;
        UpdateVisuals();
    }

    void AbsorbedEmotionUpdate()
    {
        for (int i = 0; i < m_absorbedLoveList.Count; i++)
        {
            if (m_absorbedLoveList[i].timer.Update())
            {
                ReEmitEmotion(m_absorbedLoveList[i]);
                m_absorbedLoveList.RemoveAt(i);
                i--;
            }
        }
    }


    void UpdateVisuals()
    {
        m_spriteRendererRef.color = CalculateEmotionColor(m_emotion);
        m_mouthLineHandlerRef.Refresh(GetJoyScale());
        for (int i = 0; i < m_eyebrowHandlers.Length; i++)
        {
            m_eyebrowHandlers[i].SetEybrowRotation(GetPeaceScale());
        }
    }

    void ExchangeForceWithPlayer()
    {
        ExchangeForceWithSoul(m_playerHandlerRef);
    } 

    internal void ExchangeForceWithSoul(Soul a_soul)
    {
        if (a_soul != null)
        {
            Vector3 deltaVector = gameObject.transform.position - a_soul.transform.position;
            Vector3 directionVector = deltaVector.normalized;
            float distanceMagnitude = deltaVector.magnitude;

            float attractiveForce = m_soulAttractionConstant / Mathf.Pow(distanceMagnitude, m_attractionExponent);
            float repulsiveForce = m_soulRepulsionConstant / Mathf.Pow(distanceMagnitude, m_repulsionExponent);
            
            float soulAngle = VLib.Vector2ToEulerAngle(a_soul.GetEmotion() - m_emotion);
            float centreAngle = VLib.Vector2ToEulerAngle(new Vector2(0.5f, 0.5f) - m_emotion);
            float deltaAngle = soulAngle - centreAngle;
            float soulDifference = Mathf.Abs(Mathf.Sin(Mathf.PI * deltaAngle / 180f));
            repulsiveForce *= 1f + soulDifference;
            repulsiveForce += (1f - a_soul.GetEmotion().x) * m_fearRepulsionConstant;

            //float sadnessEffect = 1f-GetLoveRatio();
            //repulsiveForce *= 1f + sadnessEffect * 10f;

            //attractiveForce += loveEffect;
            Vector3 forceVector = directionVector * (repulsiveForce - attractiveForce);
            forceVector *= m_soulToSoulForceConstant;
            m_rigidBodyRef.AddForce(forceVector);
        }
    }

    void EmitEmotion(Vector2 a_direction, Vector2 a_emotion)
    {
        Vector3 spawnOffset = a_direction * m_loveVibeSpawnOffset;
        Vibe newLoveVibe = Instantiate(m_loveVibePrefab, transform.position + spawnOffset, Quaternion.identity).GetComponent<Vibe>();
        newLoveVibe.Init(this, a_direction, a_emotion);
    }

    void ExpressEmotion()
    {
        float spawnAngle = VLib.vRandom(0f,360f);
        Vector2 collisionDirection = VLib.RotateVector3In2D(Vector2.up, spawnAngle);

        EmitEmotion(collisionDirection, m_emotion);
    }

    void ProcessEmotions()
    {
        m_expressionTimer.SetTimerMax(m_expressInterval / (1f + GetFear()* m_fearfulToCalmExpressionIncrease));
        if (m_expressionTimer.Update())
        {
            ExpressEmotion();
        }
    }

    // Update is called once per frame
    void Update()
    {
        AbsorbedEmotionUpdate();
        ExchangeForceWithPlayer();
        ProcessEmotions();
    }

    void ReEmitEmotion(AbsorbedLove a_absorbedLove)
    {
        for (int i = 0; i < m_loveToSpawn; i++)
        {
            float spawnAngle = ((float)(i+1)/(m_loveToSpawn+1)) * m_loveVibeSpawnAngleRange;
            spawnAngle -= m_loveVibeSpawnAngleRange/2f;
            Vector2 collisionDirection = VLib.RotateVector3In2D(a_absorbedLove.collisionNormal, spawnAngle);

            EmitEmotion(collisionDirection, a_absorbedLove.emotion);
        }
    }


    void AddEmotion(Vector2 a_emotion, float a_emotionStrength)
    {
        AffectEmotion(a_emotion, a_emotionStrength);
        UpdateVisuals();
    }

    void AbsorbVibe(Vector3 a_collisionNormal, Vibe a_vibe)
    {
        AbsorbedLove absorbedLove = new AbsorbedLove();
        absorbedLove.timer = new vTimer(m_reEmitTime);
        absorbedLove.collisionNormal = a_collisionNormal;
        Vector2 vibeEmotion = a_vibe.GetEmotionValue();
        absorbedLove.emotion = vibeEmotion;


        if (VLib.vRandom(0f,1f) > GetPeace())
        {
            m_absorbedLoveList.Add(absorbedLove);
        }


        AddEmotion(vibeEmotion, a_vibe.GetEmotionalAffect());
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        Vibe vibe = a_collision.gameObject.GetComponent<Vibe>();
        if (vibe != null && !vibe.IsOriginSoul(this)) 
        {
            AbsorbVibe(a_collision.contacts[0].normal, vibe);
        }
    }
}