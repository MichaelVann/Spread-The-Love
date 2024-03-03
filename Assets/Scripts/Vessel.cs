using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Vessel : Soul
{
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] Rigidbody2D m_rigidBodyRef;
    [SerializeField] MouthLineHandler m_mouthLineHandlerRef;
    [SerializeField] EyebrowHandler[] m_eyebrowHandlers; 
    [SerializeField] TrailRenderer m_lovedTrailRef;
    [SerializeField] SpriteRenderer m_miniMapSpriteRendererRef;
    [SerializeField] GameObject m_risingTextPrefab;
    [SerializeField] ParticleSystem m_loveExplosionRef;
    [SerializeField] SpriteRenderer m_hatSpriteRendererRef;
    GameHandler m_gameHandlerRef;

    BattleHandler m_battleHandlerRef;
    PlayerHandler m_playerHandlerRef;

    //Shieldhit
    const float _shieldHitLength = 0.4f;
    [SerializeField] SpriteRenderer m_shieldHitRenderer;
    vTimer m_shieldHitFadeTimer;

    //Hats
    [SerializeField] Sprite[] m_hatSprites;
    //Eyes
    [SerializeField] SpriteRenderer m_eyesRef;
    [SerializeField] Sprite[] m_eyeSprites;

    //Movement
    const float m_defaultWanderSpeed = 1.0f;
    float m_loveWanderSpeedMult = 2f;
    float m_wanderSpeed = 1.0f;
    float m_wanderRotationSpeed = 5f;

    //AI
    internal enum eFearType
    {
        Regular = -1,
        Coward = -2,
        Bully = -3,

    }


    //const int _cowardType = -2;
    //internal const int _bullyType = -3;
    float m_playerSeenDistance = 9f;
    float m_neg2RotateRate = 4f;

    //Soul Attraction
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

    bool IsLoved() { return m_emotion > 0; }

    internal void SetEmotion(int a_emotion) { AddEmotion(a_emotion - m_emotion); }

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

    internal void Init(BattleHandler a_battleHandler, PlayerHandler a_playerHandler, int a_emotion, Camera a_minimapCamera)
    {
        m_shieldHitRenderer.color = m_shieldHitRenderer.color.WithAlpha(0f);
        m_battleHandlerRef = a_battleHandler;
        m_playerHandlerRef = a_playerHandler;
        m_emotion = a_emotion;
        m_shieldHitFadeTimer = new vTimer(_shieldHitLength, true, false, false, false, true);

        UpdateWanderSpeed();
        m_gameHandlerRef = m_battleHandlerRef.m_gameHandlerRef;
        m_miniMapSpriteRendererRef.GetComponent<MiniMapIcon>().Init(a_minimapCamera);
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
        m_spriteRendererRef.color = m_miniMapSpriteRendererRef.color = CalculateEmotionColor();
        m_mouthLineHandlerRef.Refresh(GetEmotionMappedFromMinToMax(m_emotion));
        for (int i = 0; i < m_eyebrowHandlers.Length; i++)
        {
            m_eyebrowHandlers[i].SetEybrowRotation(GetEmotionMappedFromMinToMax(m_emotion));
        }
        m_lovedTrailRef.emitting = IsLoved();
        m_lovedTrailRef.startColor = m_spriteRendererRef.color;
        m_lovedTrailRef.endColor = new Color(m_spriteRendererRef.color.r, m_spriteRendererRef.color.g, m_spriteRendererRef.color.b, 0f);

        m_eyesRef.sprite = IsLoved() ? m_eyeSprites[2] : (m_emotion < 0 ? m_eyeSprites[0] : m_eyeSprites[1]);
        bool hatActive = m_emotion <= (int)eFearType.Coward;
        m_hatSpriteRendererRef.gameObject.SetActive(hatActive);
        if (hatActive)
        {
            m_hatSpriteRendererRef.sprite = m_emotion == (int)eFearType.Coward ? m_hatSprites[0] : m_hatSprites[1];
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
        newLoveVibe.Init(this, a_direction, m_rigidBodyRef.velocity, a_emotion);
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

    void UpdateWanderSpeed()
    {
        m_wanderSpeed = m_defaultWanderSpeed * (Mathf.Abs(m_emotion) + 1);
    }

    void MovementUpdate()
    {
        if (m_rigidBodyRef.velocity.magnitude <= m_wanderSpeed)
        {
            m_rigidBodyRef.velocity = VLib.RotateVector3In2D(m_rigidBodyRef.velocity.normalized * m_wanderSpeed, VLib.vRandom(-m_wanderRotationSpeed, m_wanderRotationSpeed) * Time.deltaTime);
        }
    }

    void AIUpdate()
    {
        if (m_emotion <= (int)eFearType.Coward)
        {
            Vector3 deltaPlayerPos = m_playerHandlerRef.transform.position - transform.position;
            float deltaPlayerMag = deltaPlayerPos.magnitude;
            if (deltaPlayerMag < m_playerSeenDistance)
            {
                float deltaAngle = Vector2.SignedAngle(m_rigidBodyRef.velocity, deltaPlayerPos.ToVector2());
                deltaAngle += m_emotion < (int)eFearType.Coward ? 0f: 180f;
                deltaAngle = VLib.ClampRotation(deltaAngle);
                m_rigidBodyRef.velocity = VLib.RotateVector2(m_rigidBodyRef.velocity, deltaAngle * m_neg2RotateRate * Time.deltaTime).normalized * m_wanderSpeed;
            }
            else
            {
                MovementUpdate();
            }
        }
    }

    internal void TriggerShieldEffect(Vector2 a_collisionNormal)
    {
        m_shieldHitFadeTimer.SetActive(true);
        m_shieldHitFadeTimer.Reset();
        m_shieldHitRenderer.transform.eulerAngles = VLib.Vector2ToEulerAngles(a_collisionNormal);
    }

    void ShieldHitUpdate()
    {
        if (m_shieldHitFadeTimer.IsActive())
        {
            if (m_shieldHitFadeTimer.Update())
            {

            }
            Color color = m_shieldHitRenderer.color;
            m_shieldHitRenderer.color = m_shieldHitRenderer.color.WithAlpha(1f-m_shieldHitFadeTimer.GetCompletionPercentage());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_emotion != (int)eFearType.Coward)
        {
            MovementUpdate();
        }
        AIUpdate();
        ShieldHitUpdate();
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
        bool wasLoved = m_emotion > 0;
        int deltaEmotion = AffectEmotion(a_emotion);

        //Rising Fading Text
        Color textColor = Color.white;
        bool spawningText = false;
        if(deltaEmotion > 0)
        {
            spawningText = true;
            textColor = m_gameHandlerRef.m_loveColorMax;
            var main = m_loveExplosionRef.main;
            main.startColor = textColor;
            m_loveExplosionRef.Play();
        }
        else if (deltaEmotion < 0)
        {
            spawningText = true;
            textColor = m_gameHandlerRef.m_fearColor1;
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
        bool isLoved = m_emotion > 0;
        if (!wasLoved && isLoved)
        {
            m_battleHandlerRef.CrementConvertedVessels(1);
        }
        else if (wasLoved && !isLoved)
        {
            m_battleHandlerRef.CrementConvertedVessels(-1);
        }

        UpdateWanderSpeed();

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

    internal void CollideWithPlayer(int a_meleeStrength, Vector2 a_collisionNormal)
    {
        SetEmotion(a_meleeStrength);
        m_rigidBodyRef.velocity += a_collisionNormal;
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        Vibe vibe = a_collision.gameObject.GetComponent<Vibe>();
        if (vibe != null && !vibe.IsOriginSoul(this)) 
        {
            if (m_emotion != (int)eFearType.Coward)
            {
                AbsorbVibe(a_collision.contacts[0].normal, vibe);
                if (m_spriteRendererRef.isVisible)
                {
                    GameHandler._audioManager.PlaySFX(m_vibeHitSound, 0.2f);
                }
            }
            else
            {
                TriggerShieldEffect(-a_collision.contacts[0].normal);
            }
        }
        else if (a_collision.gameObject.tag == "Vessel")
        {
            Vessel opposingVessel = a_collision.gameObject.GetComponent<Vessel>();
            int oppEmotion = opposingVessel.GetEmotion();
            if (oppEmotion != 0 && oppEmotion != m_emotion)
            {
                bool negativeAndAffected = m_emotion < 0 && (oppEmotion < m_emotion || oppEmotion > 0);
                bool positiveAndAffected = m_emotion > 0 && (oppEmotion > m_emotion || oppEmotion < 0);
                if (m_emotion == 0 || negativeAndAffected || positiveAndAffected)
                {
                    AddEmotion(Mathf.Clamp(oppEmotion - m_emotion, -1, 1));
                }
            }
        }
    }
}