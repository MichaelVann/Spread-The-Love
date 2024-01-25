using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
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

    //Minimap
    [SerializeField] MiniMapIcon m_miniMapIconRef;
    [SerializeField] Camera m_miniMapCameraRef;

    GameHandler m_gameHandlerRef;
    BattleHandler m_battleHandlerRef;
    Rigidbody2D m_rigidBodyRef;

    //Movement
    [SerializeField] UIRamp m_speedRampRef;
    internal const float m_startingMass = 25f;
    internal const float m_startingMaxSpeed = 10f;
    float m_maxSpeed;
    internal const float m_startingAcceleration = 1f;
    float m_acceleration;
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

    //Love Combat
    int m_meleeLoveStrength = 2;

    //Shoot
    bool m_shootingEnabled = false;
    bool m_readyToShoot = true;
    internal const float m_startingFireRate = 1f;
    float m_fireRate;
    vTimer m_shootTimer;
    int m_shootSpread = 1;
    float m_shootSpreadAngle = 5f;
    bool m_autoShootEnabled = false;
    bool m_mouseAiming = false;

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
    AudioSource m_driftSoundAudioSource;
    [SerializeField] AudioClip m_driftSound;
    [SerializeField] AudioClip m_vesselHitSound;
    [SerializeField] AudioClip m_wallHitSound;
    [SerializeField] AudioClip m_fireSound;

    internal float GetSpeed() { return m_rigidBodyRef.velocity.magnitude; }
    float GetSpeedPercentage() { return GetSpeed() / GetMaxSpeed(); }

    static internal float GetMass() { return m_startingMass * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.Mass)); }

    static internal float GetBraking() { return m_startingBrakingStrength * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.Braking)); }
    static internal float GetAcceleration() { return m_startingAcceleration * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.Acceleration)); }

    static internal float GetMaxSpeed() { return m_startingMaxSpeed + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.TopSpeed); }
    static internal float GetMaxTheorheticalSpeed() { return m_startingMaxSpeed + GameHandler._upgradeTree.GetUpgradeMaxLeveledStrength(UpgradeItem.UpgradeId.TopSpeed); }

    static internal float GetRotateSpeed() { return m_startingRotateSpeed * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.TurnSpeed)); }

    static internal float GetFireRate() { return m_startingFireRate * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.FireRate)); }


    void Awake()
    {
        m_rigidBodyRef = GetComponent<Rigidbody2D>();
        m_rigidBodyRef.velocity = new Vector2(0f, -1f);
        InitialiseUpgrades();
        m_shootTimer = new vTimer(1f/m_fireRate);
        m_speedChimeTimer = new vTimer(m_speedChimeTimerRepeatTime);
        InitialiseAudio();
        InitialiseColors();
        m_miniMapIconRef.Init(m_miniMapCameraRef);
    }

    void InitialiseAudio()
    {
        m_driftSoundAudioSource = gameObject.AddComponent<AudioSource>();
        m_driftSoundAudioSource.clip = m_driftSound;
        m_driftSoundAudioSource.pitch = 0.1f;
        m_driftSoundAudioSource.loop = true;
        m_driftSoundAudioSource.Play();
    }

    void InitialiseColors()
    {
        GameHandler gameHandler = FindObjectOfType<GameHandler>();
        m_spriteRendererRef.color = gameHandler.m_loveColor;
        m_loveTrailRef.startColor = m_spriteRendererRef.color;
        m_loveTrailRef.endColor = new Color(gameHandler.m_loveColor.r, gameHandler.m_loveColor.g, gameHandler.m_loveColor.b, 0f);
        m_miniMapIconRef.GetComponent<SpriteRenderer>().color = m_spriteRendererRef.color;
        m_vesselRadarCaretRef.color = gameHandler.m_fearColor;
    }

    void InitialiseUpgrades()
    {
        m_rigidBodyRef.mass = GetMass();
        m_brakingEnabled = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Braking);
        m_brakingStrength = GetBraking();
        m_acceleration = GetAcceleration();
        m_maxSpeed = GetMaxSpeed();
        m_rotateSpeed = GetRotateSpeed();
        m_fireRate = GetFireRate();
        m_vesselRadarRef.SetActive(false);// GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Radar));
        m_vesselRadarEulers = Vector3.zero;
        m_driftSpreadEnabled = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.DriftSpread);
        m_shootingEnabled = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Shooting);
        m_shootSpread = 1 + (int)GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.ShootSpread);
        m_aquaplaningEnabled = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Aquaplane);
        m_autoShootEnabled = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.AutoShoot);
        m_mouseAiming = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.MouseAim);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_emotion = 1;
        m_battleHandlerRef = FindObjectOfType<BattleHandler>();
        m_gameHandlerRef =  FindObjectOfType<GameHandler>();
    }

    void UpdateShootTimer()
    {
        if (!m_readyToShoot && m_shootTimer.Update())
        {
            m_readyToShoot = true;
        }
    }

    void ShootUpdate()
    {
        if (m_shootingEnabled)
        {
            Vector2 aimDirection = VLib.EulerAngleToVector2(-m_rigidBodyRef.rotation);

            if (m_mouseAiming)
            {
                Vector3 worldMousePoint = m_cameraRef.ScreenToWorldPoint(Input.mousePosition);
                aimDirection = worldMousePoint - transform.position;
            }
            aimDirection = aimDirection.normalized;

            //deltaMousePos = new Vector2(1f,0f);
            if ((m_autoShootEnabled || Input.GetKey(KeyCode.W)) && m_readyToShoot)
            {
                m_readyToShoot = false;
                for (int i = 0; i < m_shootSpread; i++)
                {
                    float angle = i * (2 * m_shootSpreadAngle) - ((m_shootSpread - 1) * m_shootSpreadAngle);
                    Vibe loveVibe = Instantiate(m_loveVibePrefab, transform.position, Quaternion.identity).GetComponent<Vibe>();
                    loveVibe.Init(m_battleHandlerRef, null, aimDirection.normalized.RotateVector2(angle), m_rigidBodyRef.velocity, m_emotion);
                }

                GameHandler._audioManager.PlayOneShot(m_fireSound, 0.5f);
            }
            UpdateShootTimer();
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

        float windCorrectionForce = driftAngle;
        windCorrectionForce += driftAngle * 10f * Mathf.Clamp(Mathf.Abs(driftAngle) - 90f, 0f, 90f) / 90f;
        m_rigidBodyRef.AddTorque(-windCorrectionForce * Time.deltaTime * m_rigidBodyRef.mass * GetSpeedPercentage());
        float alignSpeed = 2f;
        m_rigidBodyRef.velocity = m_rigidBodyRef.velocity.RotateVector2(driftAngle * alignSpeed * Time.deltaTime);
        float angleDrag = Mathf.Abs(Mathf.Sin(Mathf.PI * driftAngle / 180f));
        angleDrag *= m_rotateDrag;
        angleDrag *= Time.deltaTime;
        angleDrag *= Mathf.Pow(m_rigidBodyRef.velocity.magnitude / 20f, 0.3f);
        float rotationSlowdownEffect = 1f - angleDrag;
        m_rigidBodyRef.velocity *= rotationSlowdownEffect;
    }

    void HandleRotation()
    {
        if (m_aquaplaningEnabled)
        {
            m_aquaplaning = Input.GetKey(KeyCode.Space);
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
        Vector2 forwardDirection = Vector3.zero;
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
                Debug.Log(volume);
                volume = Mathf.Clamp(volume, 0f, 1f);
                //volume = Mathf.Pow(volume, 3f);
                GameHandler._audioManager.PlayOneShot(m_speedChimeAudioClip, volume);
            }
        }
    }

    void HandleBraking()
    {
        m_braking = m_brakingEnabled && Input.GetKey(KeyCode.S);
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
        float speedPercentage = velocity/ GetMaxTheorheticalSpeed();
        m_speedRampRef.SetRampPercent(speedPercentage);
        m_speedRampRef.SetColor(Color.Lerp(m_gameHandlerRef.m_fearColor, m_gameHandlerRef.m_loveColor, speedPercentage));
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

    // Update is called once per frame
    void Update()
    {
        if (!m_battleHandlerRef.m_paused)
        {
            ShootUpdate();
            MovementUpdate();
            SpreadUpdate();
            VesselRadarUpdate();
        }
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

            vessel.AddEmotion(m_meleeLoveStrength);
            vessel.GetComponent<Rigidbody2D>().velocity -= a_collision.contacts[0].normal * 1f;
            GameHandler._audioManager.PlayOneShot(m_vesselHitSound);
        }
        else if (a_collision.gameObject.tag == "Environment")
        {
            m_driftSpread = 1f;
            GameHandler._audioManager.PlayOneShot(m_wallHitSound);
            //m_rotation = VLib.Vector2ToEulerAngle(m_rigidBodyRef.velocity);
            //m_rigidBodyRef.rotation = m_rotation;
            m_rigidBodyRef.velocity += a_collision.contacts[0].normal * 1f;

        }
    }
}
