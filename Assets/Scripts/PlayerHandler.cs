using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GridBrushBase;

public class PlayerHandler : Soul
{
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] Camera m_cameraRef;
    [SerializeField] TrailRenderer m_driftTrailLeftRef;
    [SerializeField] TrailRenderer m_driftTrailRightRef;
    [SerializeField] ParticleSystem m_driftParticlesLeftRef;
    [SerializeField] ParticleSystem m_driftParticlesRightRef;
    [SerializeField] TrailRenderer m_loveTrailRef;
    [SerializeField] GameObject m_shootReadinessIndicatorRef;
    [SerializeField] Transform m_vibeSpawnPoint;

    //Minimap
    [SerializeField] MiniMapIcon m_miniMapIconRef;
    [SerializeField] Camera m_miniMapCameraRef;

    GameHandler m_gameHandlerRef;
    BattleHandler m_battleHandlerRef;
    Rigidbody2D m_rigidBodyRef;

    //Movement
    [SerializeField] UIRamp m_speedRampRef;
    internal const float m_startingAcceleration = 1f;
    float m_acceleration;
    internal const float m_startingMass = 25f;
    internal const float m_startingMaxSpeed = 10f;
    float m_maxSpeed;
    internal const float m_startingGrip = 2f;
    float m_grip;
    bool m_brakingEnabled = false;
    internal const float m_startingBrakingStrength = 1f;
    float m_brakingStrength;
    bool m_braking = false;
    internal const float m_startingRotateSpeed = 70f;
    float m_rotateSpeed;
    const float m_rotateDrag = 0.6f;
    float m_velocityAlignmentRotForce = 200f;

    //Drifting
    bool m_drifting = false;
    float m_driftDrag = 0.2f;
    float m_driftAngleNeededForEffects = 25f;
    const float m_driftSoundVolume = 0.4f;
    bool m_aquaplaningEnabled = false;
    bool m_aquaplaning = false;

    //Wake
    [SerializeField] GameObject m_wakeRef;
    bool m_wakeEnabled = false;

    //Love Combat
    int m_meleeLoveStrength = m_maxLove;

    //Shoot
    internal const float m_startingFireRate = 1f;
    Ability m_abilityShoot;
    int m_shootSpread = 1;
    float m_shootSpreadAngle = 5f;
    bool m_autoShootUnlocked = false;
    bool m_autoShootEnabled = true;
    bool m_mouseAiming = false;
    bool m_shootButtonWasDownLastFrame = false;

    //Bullet Time
    Ability m_abilityBulletTime;

    //Beserk Shot
    Ability m_abilityBeserkShot;
    const float m_beserkShotSpeed = 5f;
    [SerializeField] GameObject m_beserkShotPrefab;

    [SerializeField] GameObject m_collisionEffectPrefab;

    //Speed Chime
    [SerializeField] AudioClip m_speedChimeAudioClip;
    float m_speedChimeTimerRepeatTime = 1.2f;
    const float m_speedChimeCutoffSpeed = 10f;
    vTimer m_speedChimeTimer;

    //Vessel Radar
    [SerializeField] GameObject m_vesselRadarRef;
    [SerializeField] SpriteRenderer m_vesselRadarCaretRef;
    Vector3 m_vesselRadarEulers;
    float m_vesselRadarPingDecay = 3f;

    //DriftSpread
    bool m_driftSpreadEnabled = false;
    float m_driftSpread = 1f;

    //Audio
    [SerializeField] AudioSource m_driftSoundAudioSource;
    [SerializeField] AudioClip m_driftSound;
    [SerializeField] AudioClip m_vesselHitSound;
    [SerializeField] AudioClip m_wallHitSound;
    [SerializeField] AudioClip m_fireSound;

    bool m_touchingWall = false;

    internal float GetSpeed() { return m_rigidBodyRef.velocity.magnitude; }
    float GetSpeedPercentage() { return GetSpeed() / GetUpgradeStrength(UpgradeItem.UpgradeId.TopSpeed); }

    float GetMaxTheoreticalSpeed() { return GameHandler._upgradeTree.GetUpgradeMaxLeveledStrength(UpgradeItem.UpgradeId.TopSpeed); }

    static internal float GetUpgradeStrength(UpgradeItem.UpgradeId a_upgrade) { return GameHandler._upgradeTree.GetUpgradeLeveledStrength(a_upgrade); }

    bool GetUpgradeButton(UpgradeItem.UpgradeId a_id) { return Input.GetButton(GameHandler._upgradeTree.GetUpgrade(a_id).m_key); }

    internal Ability GetAbility(UpgradeItem.UpgradeId a_id)
    {
        Ability ability = null;
        switch (a_id)
        {
            case UpgradeItem.UpgradeId.Shooting:
                ability = m_abilityShoot;
                break;
            case UpgradeItem.UpgradeId.Mindfulness:
                ability = m_abilityBulletTime;
                break;
            case UpgradeItem.UpgradeId.BerserkShot:
                ability = m_abilityBeserkShot;
                break;
            default:
                break;
        }
        return ability;
    }

    void Awake()
    {
        m_rigidBodyRef = GetComponent<Rigidbody2D>();
        m_rigidBodyRef.velocity = new Vector2(0f, -1f);
        InitialiseUpgrades();
        m_speedChimeTimer = new vTimer(m_speedChimeTimerRepeatTime);
        InitialiseAudio();
        InitialiseColors();
        m_miniMapIconRef.Init(m_miniMapCameraRef);
    }

    void InitialiseAudio()
    {
        m_driftSoundAudioSource.clip = m_driftSound;
        m_driftSoundAudioSource.pitch = 0.1f;
        m_driftSoundAudioSource.loop = true;
        m_driftSoundAudioSource.Play();
        m_shootReadinessIndicatorRef.SetActive(m_abilityShoot.m_enabled && m_abilityShoot.m_ready);
    }

    void InitialiseColors()
    {
        GameHandler gameHandler = FindObjectOfType<GameHandler>();
        Color playerColor = gameHandler.m_loveColorMax;
        //m_loveTrailRef.startColor = m_spriteRendererRef.color;
        //m_loveTrailRef.endColor = new Color(playerColor.r, playerColor.g, playerColor.b, 0f);
        //m_miniMapIconRef.GetComponent<SpriteRenderer>().color = m_spriteRendererRef.color;
        m_vesselRadarCaretRef.color = gameHandler.m_fearColors[0];
    }

    void InitialiseUpgrades()
    {
        m_acceleration = GetUpgradeStrength(UpgradeItem.UpgradeId.Acceleration);
        m_rigidBodyRef.mass = GetUpgradeStrength(UpgradeItem.UpgradeId.Mass);
        m_brakingEnabled = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Braking);
        m_brakingStrength = GetUpgradeStrength(UpgradeItem.UpgradeId.Braking);
        m_grip = GetUpgradeStrength(UpgradeItem.UpgradeId.Grip);
        m_maxSpeed = GetUpgradeStrength(UpgradeItem.UpgradeId.TopSpeed);
        m_rotateSpeed = GetUpgradeStrength(UpgradeItem.UpgradeId.TurnSpeed);
        m_vesselRadarRef.SetActive(false);// GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Radar));
        m_vesselRadarEulers = Vector3.zero;

        //Shooting
        m_abilityShoot = new Ability(GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Shooting));
        m_abilityShoot.SetUpCooldown(1f / GetUpgradeStrength(UpgradeItem.UpgradeId.FireRate));
        m_shootSpread = 1 + (int)GetUpgradeStrength(UpgradeItem.UpgradeId.ShootSpread);
        m_aquaplaningEnabled = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Aquaplane);
        m_autoShootUnlocked = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.AutoShoot);
        m_mouseAiming = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.MouseAim);

        m_abilityBulletTime = new Ability(GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Mindfulness), GetUpgradeStrength(UpgradeItem.UpgradeId.Mindfulness));
        m_abilityBulletTime.SetUpResource(1f, 0.1f);

        m_abilityBeserkShot = new Ability(GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.BerserkShot));
        m_abilityBeserkShot.SetUpCooldown(5f);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_emotion = 1;
        m_battleHandlerRef = FindObjectOfType<BattleHandler>();
        m_gameHandlerRef =  FindObjectOfType<GameHandler>();
    }

    void ShootVibe(Vector2 a_aimDirection)
    {
        for (int i = 0; i < m_shootSpread; i++)
        {
            float angle = i * (2 * m_shootSpreadAngle) - ((m_shootSpread - 1) * m_shootSpreadAngle);
            Vibe loveVibe = Instantiate(m_loveVibePrefab, m_vibeSpawnPoint.position, Quaternion.identity).GetComponent<Vibe>();
            loveVibe.Init(null, a_aimDirection.normalized.RotateVector2(angle), m_rigidBodyRef.velocity, m_emotion, GetUpgradeStrength(UpgradeItem.UpgradeId.ProjectileSpeed));
        }

        GameHandler._audioManager.PlaySFX(m_fireSound, 0.5f);
    }

    void ShootUpdate()
    {
        if (m_abilityShoot.m_enabled)
        {
            m_abilityShoot.UpdateCooldown();
            Vector2 aimDirection = VLib.EulerAngleToVector2(-m_rigidBodyRef.rotation);

            if (m_mouseAiming)
            {
                Vector3 worldMousePoint = m_cameraRef.ScreenToWorldPoint(Input.mousePosition);
                aimDirection = worldMousePoint - transform.position;
            }
            aimDirection = aimDirection.normalized;

            m_shootReadinessIndicatorRef.SetActive(m_abilityShoot.m_ready);

            bool shootButtonPressed = GetUpgradeButton(UpgradeItem.UpgradeId.Shooting);
            bool shooting = m_autoShootEnabled && m_autoShootUnlocked;

            if (m_autoShootUnlocked && shootButtonPressed && !m_shootButtonWasDownLastFrame)
            {
                m_shootButtonWasDownLastFrame = true;
                m_autoShootEnabled = !m_autoShootEnabled;
                shooting = m_autoShootEnabled;
            }
            else if (GetUpgradeButton(UpgradeItem.UpgradeId.Shooting))
            {
                shooting = true;
            }

            if (shooting && m_abilityShoot.AttemptToActivate())
            {
                ShootVibe(aimDirection);
            }
            m_shootButtonWasDownLastFrame = shootButtonPressed;
        }
    }

    float GetDriftAngle()
    {
        float currentRotation = m_rigidBodyRef.rotation;
        currentRotation = VLib.ClampRotation(currentRotation);
        float velocityAngle = VLib.Vector2ToEulerAngle(m_rigidBodyRef.velocity);

        float driftAngle = currentRotation - velocityAngle;
        driftAngle = VLib.ClampRotation(driftAngle);

        return driftAngle;
    }

    void UpdateDriftingEffects()
    {
        float driftAngle = GetDriftAngle();
        m_drifting = Mathf.Abs(driftAngle) >= m_driftAngleNeededForEffects;
        m_drifting &= !m_aquaplaning;
        if (m_braking)
        {
            if (!m_driftParticlesLeftRef.isPlaying)
            {
                m_driftParticlesLeftRef.Play();
            }
            if (!m_driftParticlesRightRef.isPlaying)
            {
                m_driftParticlesRightRef.Play();
            }
            m_driftTrailLeftRef.emitting = true;
            m_driftTrailRightRef.emitting = true;
        }
        else if (m_drifting) 
        {
            if (driftAngle > 0)
            {
                if (!m_driftParticlesLeftRef.isPlaying)
                {
                    m_driftParticlesLeftRef.Play();
                }
                if (m_driftParticlesRightRef.isPlaying)
                {
                    m_driftParticlesRightRef.Stop();
                }
                m_driftTrailLeftRef.emitting = true;
                m_driftTrailRightRef.emitting = false;
            }
            else if (driftAngle < 0)
            {
                if (!m_driftParticlesRightRef.isPlaying)
                {
                    m_driftParticlesRightRef.Play();
                }
                if (m_driftParticlesLeftRef.isPlaying)
                {
                    m_driftParticlesLeftRef.Stop();
                }
                m_driftTrailRightRef.emitting = true;
                m_driftTrailLeftRef.emitting = false;
            }
            m_driftSoundAudioSource.volume = m_driftSoundVolume;
        }
        else
        {
            if (m_driftParticlesLeftRef.isPlaying)
            {
                m_driftParticlesLeftRef.Stop();
            }
            if (m_driftParticlesRightRef.isPlaying)
            {
                m_driftParticlesRightRef.Stop();
            }
            m_driftTrailLeftRef.emitting = false;
            m_driftTrailRightRef.emitting = false;
            m_driftSoundAudioSource.volume = 0f;
        }
    }

    void ApplyDrift()
    {
        float driftAngle = GetDriftAngle();

        if (!m_touchingWall)
        {
            float windCorrectionForce = driftAngle;
            windCorrectionForce += driftAngle * 10f * Mathf.Clamp(Mathf.Abs(driftAngle) - 90f, 0f, 90f) / 90f;
            m_rigidBodyRef.AddTorque(-windCorrectionForce * Time.deltaTime * m_rigidBodyRef.mass * GetSpeedPercentage());
        }
        float maxRotation = driftAngle < 0 ? 0f : driftAngle;
        float minRotation = driftAngle < 0 ? driftAngle : 0f;
        m_rigidBodyRef.velocity = m_rigidBodyRef.velocity.RotateVector2(Mathf.Clamp(driftAngle * m_grip * Time.deltaTime, minRotation, maxRotation));

        //Angle Drag
        float angleDrag = Mathf.Abs(Mathf.Sin(Mathf.PI * driftAngle / 180f));
        angleDrag *= m_rotateDrag;
        angleDrag *= Time.deltaTime;
        angleDrag *= m_grip;
        angleDrag *= Mathf.Pow(m_rigidBodyRef.velocity.magnitude / 20f, 0.3f);


        float rotationSlowdownEffect = 1f - angleDrag;
        m_rigidBodyRef.velocity *= rotationSlowdownEffect;
    }

    void HandleRotation()
    {
        if (m_aquaplaningEnabled)
        {
            m_aquaplaning = GetUpgradeButton(UpgradeItem.UpgradeId.Aquaplane);
        }

        if (m_rigidBodyRef.velocity.magnitude != 0f)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                m_rigidBodyRef.AddTorque(m_rotateSpeed * Time.deltaTime * m_rigidBodyRef.mass);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                m_rigidBodyRef.AddTorque(-m_rotateSpeed * Time.deltaTime * m_rigidBodyRef.mass);
            }
            if (!m_aquaplaning)
            {
                ApplyDrift();
            }
        }
    }

    void AccelerateForwards()
    {
        Vector2 forwardDirection;
        if (m_rigidBodyRef.velocity.magnitude == 0)
        {
            forwardDirection = VLib.Euler2dAngleToVector3(transform.eulerAngles.z);
        }
        else
        {
            forwardDirection = m_rigidBodyRef.velocity.normalized;
        }
        m_rigidBodyRef.velocity += forwardDirection * m_acceleration * Time.deltaTime;
        m_rigidBodyRef.velocity = m_rigidBodyRef.velocity.normalized * Mathf.Clamp(m_rigidBodyRef.velocity.magnitude, 0f, m_maxSpeed);
    }

    void HandleSpeedChimeSoundEffect()
    {
        float velocity = m_rigidBodyRef.velocity.magnitude;

        if (velocity > m_speedChimeCutoffSpeed)
        {
            if (m_speedChimeTimer.Update())
            {
                float volume = (velocity - m_speedChimeCutoffSpeed) / (20f - m_speedChimeCutoffSpeed);
                volume = Mathf.Clamp(volume, 0f, 1f);
                //volume = Mathf.Pow(volume, 3f);
                GameHandler._audioManager.PlaySFX(m_speedChimeAudioClip, volume);
            }
        }
    }

    void HandleBraking()
    {
        m_braking = m_brakingEnabled && GetUpgradeButton(UpgradeItem.UpgradeId.Braking);
        if (m_braking)
        {
            m_rigidBodyRef.velocity *= 1f - m_brakingStrength*Time.deltaTime;
        }
    }

    void MovementUpdate()
    {
        HandleRotation();

        AccelerateForwards();
        HandleSpeedChimeSoundEffect();
        UpdateDriftingEffects();

        HandleBraking();

        float velocity = m_rigidBodyRef.velocity.magnitude;
        float speedPercentage = velocity/ GetMaxTheoreticalSpeed();
        if (m_wakeEnabled)
        {
            m_wakeRef.transform.eulerAngles = VLib.Vector2ToEulerAngles(m_rigidBodyRef.velocity);
        }
        m_speedRampRef.SetRampPercent(speedPercentage);
        m_speedRampRef.SetColor(Color.Lerp(m_gameHandlerRef.m_fearColors[0], m_gameHandlerRef.m_loveColorMax, speedPercentage));
    }

    void VesselRadarUpdate()
    {
        m_vesselRadarRef.transform.eulerAngles = m_vesselRadarEulers;
        m_vesselRadarCaretRef.color = new Color(m_vesselRadarCaretRef.color.r, m_vesselRadarCaretRef.color.g, m_vesselRadarCaretRef.color.b, m_vesselRadarCaretRef.color.a * 1f - Time.deltaTime * m_vesselRadarPingDecay);
    }

    internal void ReceiveVesselRadarPing(Vector3 a_position)
    {
        m_vesselRadarEulers = VLib.Vector3ToEulerAngles(a_position - transform.position);
        m_vesselRadarCaretRef.color = new Color(m_vesselRadarCaretRef.color.r, m_vesselRadarCaretRef.color.g, m_vesselRadarCaretRef.color.b, 1f);
    }

    void SpreadUpdate()
    {
        if (m_driftSpreadEnabled)
        {
            if (m_drifting)
            {
                m_driftSpread += Time.deltaTime;
            }
            else
            {
                m_driftSpread = Mathf.Pow(m_driftSpread, Time.deltaTime);
            }
            transform.localScale = new Vector3(m_driftSpread, transform.localScale.y, transform.localScale.z);
        }
    }

    void BulletTimeUpdate()
    {
        bool m_bulletTimeActive = m_abilityBulletTime.m_enabled && m_abilityBulletTime.SetResourceBasedActivateInput(GetUpgradeButton(UpgradeItem.UpgradeId.Mindfulness));
        m_abilityBulletTime.ResourceUpdate();

        if (m_bulletTimeActive)
        {
            m_battleHandlerRef.SetBulletTimeFactor(1f / m_abilityBulletTime.m_effectStrength);
        }
        else
        {
            m_battleHandlerRef.SetBulletTimeFactor(1f);
        }
    }

    void BeserkShotUpdate()
    {
        if (m_abilityBeserkShot.m_enabled)
        {
            m_abilityBeserkShot.UpdateCooldown();
            Vector2 aimDirection = VLib.EulerAngleToVector2(-m_rigidBodyRef.rotation);

            //if (m_mouseAiming)
            //{
            //    Vector3 worldMousePoint = m_cameraRef.ScreenToWorldPoint(Input.mousePosition);
            //    aimDirection = worldMousePoint - transform.position;
            //}
            aimDirection = aimDirection.normalized;
            
            if (GetUpgradeButton(UpgradeItem.UpgradeId.BerserkShot) && m_abilityBeserkShot.AttemptToActivate())
            {
                BeserkShot beserkShot = Instantiate(m_beserkShotPrefab, transform.position, Quaternion.identity).GetComponent<BeserkShot>();
                beserkShot.Init(aimDirection * m_beserkShotSpeed + m_rigidBodyRef.velocity);
                GameHandler._audioManager.PlaySFX(m_fireSound, 0.5f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_battleHandlerRef.m_paused)
        {
            ShootUpdate();
            MovementUpdate();
            SpreadUpdate();
            VesselRadarUpdate();
            BulletTimeUpdate();
            BeserkShotUpdate();
        }
    }

    void FixedUpdate()
    {
        m_touchingWall = false;
        m_spriteRendererRef.color = Color.red;
    }

    void SpawnCollisionEffect(Vector2 a_collisionPos, Vector2 a_normal)
    {
        GameObject m_collisionEffect = Instantiate(m_collisionEffectPrefab, a_collisionPos, VLib.Vector2DirectionToQuaternion(VLib.RotateVector2(a_normal, 90f)), transform);
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        if (a_collision.gameObject.tag == "Vessel")
        {
            //float impulseStrength = a_collision.contacts[0].normalImpulse;
            float collisionAngle = Vector2.SignedAngle(a_collision.relativeVelocity.normalized, a_collision.contacts[0].normal);
            float contactStrength = Mathf.Sin(Mathf.PI * collisionAngle / 180f + Mathf.PI / 2f);
            float impulseStrength = Mathf.Pow(a_collision.relativeVelocity.magnitude,2f) * contactStrength;
            Vessel vessel = a_collision.gameObject.GetComponent<Vessel>();

            Vector2 collisionDirection = a_collision.contacts[0].normal;

            SpawnCollisionEffect(a_collision.contacts[0].point, collisionDirection);

            if (vessel.GetEmotion() == (int)Vessel.eEmotionType.Bully || vessel.GetEmotion() == (int) Vessel.eEmotionType.Jaded)
            {
                m_rigidBodyRef.AddForce(collisionDirection * 10000f);
                vessel.TriggerShieldEffect(collisionDirection);
            }
            else
            {
                vessel.CollideWithPlayer(m_meleeLoveStrength, -collisionDirection);
                GameHandler._audioManager.PlaySFX(m_vesselHitSound,1.4f);
            }
        }
        else if (a_collision.gameObject.tag == "LoveVibe" && a_collision.gameObject.GetComponent<Vibe>().GetEmotionalAffect() < 0)
        {
            Destroy(a_collision.gameObject);
        }
        else if (a_collision.gameObject.tag == "Environment")
        {
            m_driftSpread = 1f;
            GameHandler._audioManager.PlaySFX(m_wallHitSound);
            m_touchingWall = true;
            //m_spriteRendererRef.color = Color.green;
        }
    }

    private void OnCollisionStay2D(Collision2D a_collision)
    {
        if (a_collision.gameObject.tag == "Environment")
        {
            m_touchingWall = true;
            //m_spriteRendererRef.color = Color.green;
        }
    }

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        Vessel m_enemyVessel = a_collision.GetComponent<Vessel>();
        m_enemyVessel.SetPlayerAwareness(true);
    }
}
