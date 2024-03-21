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
    const float m_defaultWanderSpeed = 0.8f;
    float m_loveWanderSpeedMult = 2f;
    float m_wanderSpeed = 1.0f;
    float m_wanderRotationSpeed = 5f;

    //Audio
    [SerializeField] AudioClip m_vesselHitSound;
    const float m_vesselHitSoundVolume = 1.2f;

    //AI
    bool m_awareOfPlayer = false;
    internal enum eEmotionType
    {
        Regular = -1,
        Coward = -2,
        Bully = -3,
        Jaded = -4,
    }

    struct FearTraits
    {
        internal bool absorbingLove;
        internal bool rejectingLove;
        internal bool awareOfPlayer;
    }
    FearTraits m_fearTraits;

    float m_playerSeenDistance = 9f;
    float m_cowardRotateRate = 2f;
    float m_defaultRotateRate = 4f;

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

    //On hit sprite scaling
    Vector3 m_defaultSpriteScale;
    float m_deltaEmotionSpriteScaleEffect = 1f;
    const float m_deltaEmotionSpriteScaleOriginalSpeed = 2f;
    float m_deltaEmotionSpriteScaleSpeed = m_deltaEmotionSpriteScaleOriginalSpeed;
    const float m_deltaEmotionSpriteScaleDecay = 15f;
    int m_deltaEmotionSpriteScaleDirection = 0;
    bool m_deltaEmotionSpriteScaling = false;

    //Absorbed love

    List<vTimer> m_absorbedLoveTimers;
    float m_absorbedLoveReEmitTime = 0.4f;

    //Audio
    [SerializeField] AudioClip m_vibeHitSound;

    //Damage Flash
    Material m_spriteMaterialRef;
    vTimer m_damageFlashTimer;

    const float m_demotionProtectionTime = 0.4f;
    bool m_demotionProtectionActive = false;
    vTimer m_demotionProtectionTimer;

    //Beserking
    [SerializeField] GameObject m_beserkField;
    struct Beserk
    {
        internal bool active;
        internal List<Vessel> targetVessels;
        internal const float speedBoost = 1.7f;
        internal const int maximumEmotionTargeted = 1;
        internal vTimer m_timer;
    }
    Beserk m_beserk;

    //Enlightened
    bool m_enlightened = false;

    bool IsLoved() { return m_emotion > 0; }

    bool IsFearType(eEmotionType a_fearType) { return m_emotion == (int)a_fearType; }

    internal int SetEmotion(int a_emotion) { return AddEmotion(a_emotion - m_emotion); }

    internal void SetPlayerAwareness(bool a_aware) { m_awareOfPlayer = a_aware; }

    void Awake()
    {
        m_absorbedLoveTimers = new List<vTimer>();
        m_expressionTimer = new vTimer(m_expressInterval);
        m_expressionTimer.SetTimer(VLib.vRandom(0f, m_expressInterval));

        m_spriteMaterialRef = Instantiate(m_spriteRendererRef.material);
        m_spriteRendererRef.material = m_spriteMaterialRef;

        m_defaultSpriteScale = m_spriteRendererRef.transform.localScale;
        m_demotionProtectionTimer = new vTimer(m_demotionProtectionTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBodyRef.velocity = VLib.RotateVector3In2D(new Vector2(1f, 0f), VLib.vRandom(0f, 360f)) * m_wanderSpeed;
        m_damageFlashTimer = new vTimer(0.07f, true);
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

        //Fear traits
        m_fearTraits = new FearTraits();
        UpdateFearTraits();
    }

    void AbsorbedEmotionUpdate()
    {
        for (int i = 0; i < m_absorbedLoveTimers.Count; i++)
        {
            if (m_absorbedLoveTimers[i].Update())
            {
                ReEmitEmotion();
                m_absorbedLoveTimers.RemoveAt(i);
                i--;
            }
        }
    }

    void UpdateEyesSprite()
    {
        if (m_enlightened)
        {
            m_eyesRef.sprite = m_eyeSprites[3];
        }
        else if (m_beserk.active)
        {
            m_eyesRef.sprite = m_eyeSprites[4];
        }
        else
        {
            m_eyesRef.sprite = IsLoved() ? m_eyeSprites[2] : (m_emotion < 0 ? m_eyeSprites[0] : m_eyeSprites[1]);
        }
    }

    void UpdateVisuals()
    {
        Color emotionColor;
        if (m_beserk.active)
        {
            emotionColor = GameHandler._autoRef.m_berserkColor;
        }
        else if (m_enlightened)
        {
            emotionColor = GameHandler._autoRef.m_enlightenedColor;
        }
        else
        {
            emotionColor = CalculateEmotionColor();
        }
        m_miniMapSpriteRendererRef.color = emotionColor;
        m_spriteMaterialRef.SetColor("_Color", emotionColor);
        m_mouthLineHandlerRef.Refresh(GetEmotionMappedFromMinToMax(m_emotion));
        for (int i = 0; i < m_eyebrowHandlers.Length; i++)
        {
            m_eyebrowHandlers[i].SetEybrowRotation(GetEmotionMappedFromMinToMax(m_emotion));
        }
        m_lovedTrailRef.emitting = IsLoved();
        m_lovedTrailRef.startColor = emotionColor;
        m_lovedTrailRef.endColor = new Color(emotionColor.r, emotionColor.g, emotionColor.b, 0f);

        UpdateEyesSprite();

        //Hats
        const int hatStartPoint = -2;
        bool hatActive = m_emotion <= hatStartPoint;
        m_hatSpriteRendererRef.gameObject.SetActive(hatActive);
        if (hatActive)
        {
            int hatIndex = (m_emotion - hatStartPoint) * -1;
            if (hatIndex < m_hatSprites.Length)
            {
                m_hatSpriteRendererRef.sprite = m_hatSprites[hatIndex];
            }
        }
    }

    void UpdateDeltaEmotionScale()
    {
        if (m_deltaEmotionSpriteScaling)
        {
            m_deltaEmotionSpriteScaleEffect += m_deltaEmotionSpriteScaleSpeed * m_deltaEmotionSpriteScaleDirection * Time.unscaledDeltaTime;
            m_deltaEmotionSpriteScaleSpeed -= m_deltaEmotionSpriteScaleDecay * m_deltaEmotionSpriteScaleOriginalSpeed * Time.unscaledDeltaTime;

            if ((m_deltaEmotionSpriteScaleDirection > 0 && m_deltaEmotionSpriteScaleEffect <= 1f) || (m_deltaEmotionSpriteScaleDirection < 0 && m_deltaEmotionSpriteScaleEffect >= 1f))
            {
                m_deltaEmotionSpriteScaling = false;
                m_deltaEmotionSpriteScaleEffect = 1f;
            }
            
            m_spriteRendererRef.transform.localScale = m_defaultSpriteScale * m_deltaEmotionSpriteScaleEffect;
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
        m_wanderSpeed *= m_beserk.active ? Beserk.speedBoost : 1f;
    }

    void MovementUpdate()
    {
        if (m_rigidBodyRef.velocity.magnitude <= m_wanderSpeed)
        {
            m_rigidBodyRef.velocity = VLib.RotateVector3In2D(m_rigidBodyRef.velocity.normalized * m_wanderSpeed, VLib.vRandom(-m_wanderRotationSpeed, m_wanderRotationSpeed) * Time.deltaTime);
        }
    }

    void BeserkUpdate()
    {
        float m_closestDistance = 100000f;
        int m_closestTargetId = -1;
        for (int i = 0; i < m_beserk.targetVessels.Count; i++)
        {
            if (m_beserk.targetVessels[i].GetEmotion() < Beserk.maximumEmotionTargeted)
            {
                float deltaMag = (m_beserk.targetVessels[i].transform.position - transform.position).magnitude;
                if (deltaMag < m_closestDistance)
                {
                    m_closestDistance = deltaMag;
                    m_closestTargetId = i;
                }
            }
        }

        if (m_closestTargetId != -1)
        {
            //Vector2 targetDirection = (m_beserk.targetVessels[m_closestTargetId].transform.position - transform.position).normalized;
            //m_rigidBodyRef.AddForce(targetDirection * m_rigidBodyRef.mass * 1000f * Time.deltaTime);

            Vector3 deltaPos = m_beserk.targetVessels[m_closestTargetId].transform.position - transform.position;
            float deltaAngle = Vector2.SignedAngle(m_rigidBodyRef.velocity, deltaPos.ToVector2());
            deltaAngle = VLib.ClampRotation(deltaAngle);
            m_rigidBodyRef.velocity = VLib.RotateVector2(m_rigidBodyRef.velocity, deltaAngle * m_defaultRotateRate * Beserk.speedBoost * Time.deltaTime).normalized * Beserk.speedBoost * m_wanderSpeed;
        }

        if (m_beserk.m_timer.Update())
        {
            EndBeserk();
        }
    }

    void AIUpdate()
    {
        if (m_beserk.active)
        {
            BeserkUpdate();
        }
        else
        {
            if (m_fearTraits.awareOfPlayer)
            {
                if (m_awareOfPlayer)
                {
                    Vector3 deltaPlayerPos = m_playerHandlerRef.transform.position - transform.position;
                    float deltaAngle = Vector2.SignedAngle(m_rigidBodyRef.velocity, deltaPlayerPos.ToVector2());
                    deltaAngle += m_emotion < (int)eEmotionType.Coward ? 0f : 180f;
                    deltaAngle = VLib.ClampRotation(deltaAngle);
                    float rotateRate = IsFearType(eEmotionType.Coward) ? m_cowardRotateRate : m_defaultRotateRate;
                    m_rigidBodyRef.velocity = VLib.RotateVector2(m_rigidBodyRef.velocity, deltaAngle * rotateRate * Time.deltaTime).normalized * m_wanderSpeed;
                }

                //if (IsFearType(eFearType.Coward))
                //{
                //    AbsorbedEmotionUpdate();
                //}
            }
            if (m_fearTraits.absorbingLove)
            {
                //if (m_absorbedLoveTimers.Count < 0)
                //{
                //    m_absorbedLoveTimers.Add(new vTimer(2f));
                //}
                AbsorbedEmotionUpdate();
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
            m_shieldHitFadeTimer.Update();
            Color color = m_shieldHitRenderer.color;
            m_shieldHitRenderer.color = m_shieldHitRenderer.color.WithAlpha(1f-m_shieldHitFadeTimer.GetCompletionPercentage());
        }
    }

    // Update is called once per frame
    void Update()
    {
        MovementUpdate();
        AIUpdate();
        ShieldHitUpdate();
        if (m_damageFlashTimer.Update())
        {
            m_spriteMaterialRef.SetFloat("_WhiteFlashOn", 0f);
        }
        //AbsorbedEmotionUpdate();
        //ExchangeForceWithPlayer();
        //ProcessEmotions();
        UpdateDeltaEmotionScale();

        if (m_demotionProtectionActive && m_demotionProtectionTimer.Update())
        {
            m_demotionProtectionActive = false;
        }
    }

    void ReEmitEmotion()
    {
        Vector2 direction = Vector2.up.RotateVector2(VLib.vRandom(0f, 360f));
        EmitEmotion(direction, m_emotion);
    }

    void UpdateFearTraits()
    {
        m_fearTraits.rejectingLove = false;
        m_fearTraits.absorbingLove = false;
        m_fearTraits.awareOfPlayer = false;

        switch ((eEmotionType)m_emotion)
        {
            case eEmotionType.Regular:
                break;
            case eEmotionType.Coward:
                m_fearTraits.rejectingLove = true;
                m_fearTraits.awareOfPlayer = true;
                break;
            case eEmotionType.Bully:
                break;
            case eEmotionType.Jaded:
                m_fearTraits.absorbingLove = true;
                m_fearTraits.awareOfPlayer = true;
                break;
            default:
                break;
        }
    }

    void FearTypeChanged(int a_deltaEmotion, bool a_wasLoved, bool a_wasFearful)
    {
        m_damageFlashTimer.Reset();
        m_spriteMaterialRef.SetFloat("_WhiteFlashOn", 1f);
        m_deltaEmotionSpriteScaleEffect = 1f + a_deltaEmotion / 10f;
        m_deltaEmotionSpriteScaleDirection = a_deltaEmotion > 0f ? 1 : -1;
        m_deltaEmotionSpriteScaling = true;

        //Change score
        bool isLoved = m_emotion > 0;
        if (!a_wasLoved && isLoved)
        {
            m_battleHandlerRef.CrementLovedVessels(1);
        }
        else if (a_wasLoved && !isLoved)
        {
            m_battleHandlerRef.CrementLovedVessels(-1);
        }

        bool isFearful = m_emotion < 0;
        if (!a_wasFearful && isFearful)
        {
            m_battleHandlerRef.CrementFearfulVessels(1);
        }
        else if (a_wasFearful && !isFearful)
        {
            m_battleHandlerRef.CrementFearfulVessels(-1);
        }

        UpdateFearTraits();

        UpdateWanderSpeed();

        UpdateVisuals();
    }

    internal int AddEmotion(int a_emotion)
    {
        if (m_enlightened || m_beserk.active || (m_demotionProtectionActive && a_emotion < 0))
        {
            return 0;
        }

        bool wasLoved = m_emotion > 0;
        bool wasFearful = m_emotion < 0;
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
            m_demotionProtectionActive = true;
            m_demotionProtectionTimer.Reset();
        }
        else if (deltaEmotion < 0)
        {
            spawningText = true;
            textColor = m_gameHandlerRef.m_fearColors[0];
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

        if (deltaEmotion != 0)
        {
            FearTypeChanged(deltaEmotion, wasLoved, wasFearful);
        }

        return deltaEmotion;
    }

    int AbsorbVibe(Vibe a_vibe)
    {
        int affect = a_vibe.GetEmotionalAffect();
        if (affect < 0)
        {
            affect = Mathf.Clamp(m_emotion + affect, affect, 0) - m_emotion;
        }
        affect = Mathf.Clamp(affect, -1, 1);
        return AddEmotion(affect);
    }

    internal void HitToFullEmotion(Vector2 a_collisionNormal)
    {
        int deltaEmotion = SetEmotion(m_maxLove);
        if (deltaEmotion > 0)
        {
            GameHandler._audioManager.PlaySFX(m_vesselHitSound, m_vesselHitSoundVolume);
        }
        //m_rigidBodyRef.velocity += a_collisionNormal;
    }

    void Enlighten()
    {
        m_enlightened = true;
        UpdateVisuals();
    }

    internal void CollideWithPlayer(Vector2 a_collisionNormal)
    {
        int deltaEmotion = SetEmotion(m_maxLove);
        if (!m_enlightened)
        {
            GameHandler._audioManager.PlaySFX(m_vesselHitSound, m_vesselHitSoundVolume);
        }
        m_rigidBodyRef.velocity += a_collisionNormal;
        Enlighten();
    }

    internal void GoBeserk()
    {
        m_beserk.targetVessels = new List<Vessel>();
        m_beserk.active = true;
        m_beserkField.SetActive(true);
        UpdateVisuals();
        UpdateWanderSpeed();
        m_beserk.m_timer = new vTimer(PlayerHandler.GetUpgradeStrength(UpgradeItem.UpgradeId.BerserkShot));
    }

    internal void EndBeserk()
    {
        m_beserk.targetVessels = null;
        m_beserk.active = false;
        m_beserkField.SetActive(false);
        UpdateWanderSpeed();
        UpdateVisuals();
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        Vibe vibe = a_collision.gameObject.GetComponent<Vibe>();
        if (vibe != null && !vibe.IsOriginSoul(this)) 
        {
            if ((!m_fearTraits.absorbingLove && !m_fearTraits.rejectingLove) || vibe.GetEmotionalAffect() < 0)
            {
                if (AbsorbVibe(vibe) > 0 && m_spriteRendererRef.isVisible)
                {
                    GameHandler._audioManager.PlaySFX(m_vibeHitSound, 0.2f);
                }
            }
            else
            {
                TriggerShieldEffect(-a_collision.contacts[0].normal);
                if (m_fearTraits.absorbingLove)
                {
                    m_absorbedLoveTimers.Add(new vTimer(m_absorbedLoveReEmitTime)); //a_collision.contacts[0].normal
                }
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
        else if (a_collision.gameObject.tag == "Environment")
        {
            //float collisionAngle = VLib.Vector2ToEulerAngle(-a_collision.contacts[0].normal);
            //m_rigidBodyRef.velocity = VLib.RotateVector3In2D(m_rigidBodyRef.velocity.magnitude * Vector3.up, VLib.vRandom(-45f, 45f) + collisionAngle);
        }
    }

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        if (m_beserk.targetVessels != null && a_collision.gameObject.tag == "Vessel")
        {
            Vessel vessel = a_collision.gameObject.GetComponent<Vessel>();
            if (vessel.GetEmotion() < Beserk.maximumEmotionTargeted) 
            {
                m_beserk.targetVessels.Add(vessel);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D a_collision)
    {
        if (m_beserk.targetVessels != null && a_collision.gameObject.tag == "Vessel")
        {
            Vessel vessel = a_collision.gameObject.GetComponent<Vessel>();
            if (vessel.GetEmotion() < Beserk.maximumEmotionTargeted)
            {
                m_beserk.targetVessels.Remove(vessel);
            }
        }
    }
}