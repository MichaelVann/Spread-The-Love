using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vessel : Soul
{
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] Rigidbody2D m_rigidBodyRef;
    [SerializeField] MouthLineHandler m_mouthLineHandlerRef;
    [SerializeField] EyebrowHandler[] m_eyebrowHandlers; 
    [SerializeField] TrailRenderer m_lovedTrailRef;
    [SerializeField] SpriteRenderer m_minimapIconRef;
    [SerializeField] GameObject m_risingTextPrefab;
    [SerializeField] ParticleSystem m_loveExplosionRef;
    GameHandler m_gameHandlerRef;

    BattleHandler m_battleHandlerRef;
    PlayerHandler m_playerHandlerRef;

    //Eyes
    [SerializeField] SpriteRenderer m_eyesRef;
    [SerializeField] Sprite[] m_eyeSprites;

    //Soul Attraction
    const float m_defaultWanderSpeed = 1.0f;
    float m_loveWanderSpeedMult = 2f;
    float m_wanderSpeed = 1.0f;
    float m_wanderRotationSpeed = 5f;

    const float m_soulAttractionConstant = 1.3f;
    const float m_attractionExponent = 1f;
    const float m_soulRepulsionConstant = 1f;
    const float m_repulsionExponent = 2f;
    const float m_fearRepulsionConstant = 0f;

    const float m_soulToSoulForceConstant = 10f;
    internal const bool m_repulsedByOtherSouls = true;

    const float m_loveVibeSpawnOffset = 0.52f;
    const float m_loveVibeSpawnAngleRange = 90f;
    //float m_love = 50f;
    //float m_maxLove = 100f;
    int m_loveToSpawn = 2;
    float m_reEmitTime = 0.2f;

    const float m_fearfulToCalmExpressionIncrease = 1f;
    float m_expressInterval = 4f;
    vTimer m_expressionTimer;

    struct AbsorbedLove
    {
        internal vTimer timer;
        internal Vector2 collisionNormal;
        internal int emotion;
    } 

    List<AbsorbedLove> m_absorbedLoveList;

    //Audio
    [SerializeField] AudioClip m_vibeHitSound;

    bool IsMaxLoved() { return m_emotion >= m_maxLove; }

    void Awake()
    {
        m_absorbedLoveList = new List<AbsorbedLove>();
        m_expressionTimer = new vTimer(m_expressInterval);
        m_expressionTimer.SetTimer(VLib.vRandom(0f, m_expressInterval));
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBodyRef.velocity = VLib.RotateVector3In2D(new Vector2(1f, 0f), VLib.vRandom(0f, 360f)) * m_wanderSpeed;
    }

    internal void Init(BattleHandler a_battleHandler, PlayerHandler a_playerHandler, int a_emotion)
    {
        m_battleHandlerRef = a_battleHandler;
        m_playerHandlerRef = a_playerHandler;
        m_emotion = a_emotion;
        m_wanderSpeed = m_defaultWanderSpeed;
        m_gameHandlerRef = m_battleHandlerRef.m_gameHandlerRef;
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
        CalculateEmotionColor();
        m_spriteRendererRef.color = m_minimapIconRef.color = CalculateEmotionColor(m_emotion);
        m_mouthLineHandlerRef.Refresh(GetEmotionMappedFromMinToMax(m_emotion));
        for (int i = 0; i < m_eyebrowHandlers.Length; i++)
        {
            m_eyebrowHandlers[i].SetEybrowRotation(GetEmotionMappedFromMinToMax(m_emotion));
        }
        m_lovedTrailRef.emitting = IsMaxLoved();
        m_lovedTrailRef.startColor = m_gameHandlerRef.m_loveColor;
        m_lovedTrailRef.endColor = new Color(m_gameHandlerRef.m_loveColor.r, m_gameHandlerRef.m_loveColor.g, m_gameHandlerRef.m_loveColor.b, 0f);

        m_eyesRef.sprite = IsMaxLoved() ? m_eyeSprites[2] : (m_emotion < 0 ? m_eyeSprites[0] : m_eyeSprites[1]);
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

            float baseAttractiveForce = m_soulAttractionConstant / Mathf.Pow(distanceMagnitude, m_attractionExponent);
            float baseRepulsiveForce = m_soulRepulsionConstant / Mathf.Pow(distanceMagnitude, m_repulsionExponent);

            baseAttractiveForce *= (GetEmotion() * a_soul.GetEmotion());

            //attractiveForce += loveEffect;
            Vector3 forceVector = directionVector * (baseRepulsiveForce - baseAttractiveForce);
            forceVector *= m_soulToSoulForceConstant;
            m_rigidBodyRef.AddForce(forceVector);
        }
    }

    void EmitEmotion(Vector2 a_direction, int a_emotion)
    {
        Vector3 spawnOffset = a_direction * m_loveVibeSpawnOffset;
        Vibe newLoveVibe = Instantiate(m_loveVibePrefab, transform.position + spawnOffset, Quaternion.identity).GetComponent<Vibe>();
        newLoveVibe.Init(m_battleHandlerRef, this, a_direction, m_rigidBodyRef.velocity, a_emotion);
    }

    void ExpressEmotion()
    {
        float spawnAngle = VLib.vRandom(0f,360f);
        Vector2 collisionDirection = VLib.RotateVector3In2D(Vector2.up, spawnAngle);

        EmitEmotion(collisionDirection, m_emotion);
    }

    void ProcessEmotions()
    {
        m_expressionTimer.SetTimerMax(m_expressInterval / (1f + GetFear() * m_fearfulToCalmExpressionIncrease));
        if (m_expressionTimer.Update())
        {
            ExpressEmotion();
        }
    }

    void MovementUpdate()
    {
        if (m_rigidBodyRef.velocity.magnitude <= m_wanderSpeed)
        {
            m_rigidBodyRef.velocity = VLib.RotateVector3In2D(m_rigidBodyRef.velocity.normalized * m_wanderSpeed, VLib.vRandom(-m_wanderRotationSpeed, m_wanderRotationSpeed) * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        MovementUpdate();
        //AbsorbedEmotionUpdate();
        //ExchangeForceWithPlayer();
        //ProcessEmotions();
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

    internal void AddEmotion(int a_emotion)
    {
        bool wasMaxLove = m_emotion == m_maxLove;
        int deltaEmotion = AffectEmotion(a_emotion);

        //Rising Fading Text
        Color textColor = Color.white;
        bool spawningText = false;
        if(deltaEmotion > 0)
        {
            spawningText = true;
            textColor = m_gameHandlerRef.m_loveColor;
            var main = m_loveExplosionRef.main;
            main.startColor = textColor;
            m_loveExplosionRef.Play();
        }
        else if (deltaEmotion < 0)
        {
            spawningText = true;
            textColor = m_gameHandlerRef.m_fearColor;
            var main = m_loveExplosionRef.main;
            main.startColor = textColor;
            m_loveExplosionRef.Play();
        }
        if (spawningText) 
        {
            RisingFadingText rft = Instantiate(m_risingTextPrefab, transform.position, Quaternion.identity, m_battleHandlerRef.m_worldTextCanvasRef.transform).GetComponent<RisingFadingText>();
            rft.SetUp(deltaEmotion > 0 ? "+" + deltaEmotion : deltaEmotion.ToString(), textColor, textColor);
            rft.SetOriginalScale(0.7f);
        }

        //Change score
        bool isMaxLove = m_emotion == m_maxLove;
        if (!wasMaxLove && isMaxLove)
        {
            m_battleHandlerRef.IncrementConvertedVessels(1);
        }
        else if (wasMaxLove && !isMaxLove)
        {
            m_battleHandlerRef.IncrementConvertedVessels(-1);
        }

        m_wanderSpeed = GetEmotion() == 0 ? m_defaultWanderSpeed : m_defaultWanderSpeed * m_loveWanderSpeedMult ;

        UpdateVisuals();
    }

    void AbsorbVibe(Vector3 a_collisionNormal, Vibe a_vibe)
    {
        AbsorbedLove absorbedLove = new AbsorbedLove();
        absorbedLove.timer = new vTimer(m_reEmitTime);
        absorbedLove.collisionNormal = a_collisionNormal;
        int vibeEmotion = a_vibe.GetEmotionalAffect();
        absorbedLove.emotion = vibeEmotion;

        if (VLib.vRandom(0f,1f) < GetEmotion())
        {
            m_absorbedLoveList.Add(absorbedLove);
        }

        AddEmotion(a_vibe.GetEmotionalAffect());
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        Vibe vibe = a_collision.gameObject.GetComponent<Vibe>();
        if (vibe != null && !vibe.IsOriginSoul(this)) 
        {
            AbsorbVibe(a_collision.contacts[0].normal, vibe);
            if (m_spriteRendererRef.isVisible)
            {
                GameHandler._audioManager.PlayOneShot(m_vibeHitSound, 0.2f);
            }
        }
        else if (a_collision.gameObject.tag == "Vessel")
        {
            Vessel opposingVessel = a_collision.gameObject.GetComponent<Vessel>();
            if (opposingVessel.IsMaxLoved())
            {
                AddEmotion(1);
            }
            else if (opposingVessel.m_emotion < 0)
            {
                AddEmotion(-1);
            }
        }
    }
}