using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
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


    BattleHandler m_battleHandlerRef;
    Rigidbody2D m_rigidBodyRef;

    //Movement
    internal const float m_startingMass = 25f;
    internal const float m_startingMaxSpeed = 10f;
    float m_maxSpeed;
    internal const float m_startingAcceleration = 1f;
    float m_acceleration;
    internal const float m_startingRotateSpeed = 10f;
    float m_rotateSpeed;
    const float m_rotateDrag = 0.12f;
    float m_velocityAlignmentRotForce = 200f;
    bool m_drifting = false;
    float m_driftDrag = 0.2f;
    float m_driftRotationMult = 2f;

    //Love Combat
    int m_meleeLoveStrength = 1;

    //Shoot
    bool m_readyToShoot = true;
    internal const float m_startingFireRate = 1f;
    float m_fireRate;
    vTimer m_shootTimer;

    //Speed Chime
    [SerializeField] AudioClip m_speedChimeAudioClip;
    float m_speedChimeTimerRepeatTime = 1.2f;
    const float m_speedChimeCutoffSpeed = 10f;
    vTimer m_speedChimeTimer;

    internal float GetSpeed() { return m_rigidBodyRef.velocity.magnitude; }

    static internal float GetMass() { return m_startingMass * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.Mass)); }

    static internal float GetAcceleration() { return m_startingAcceleration * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.Acceleration)); }

    static internal float GetMaxSpeed() { return m_startingMaxSpeed + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.TopSpeed); }

    static internal float GetRotateSpeed() { return m_startingRotateSpeed * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.TurnSpeed)); }

    static internal float GetFireRate() { return m_startingFireRate * (1f + GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.FireRate)); }

    void Awake()
    {
        m_rigidBodyRef = GetComponent<Rigidbody2D>();
        m_rigidBodyRef.velocity = new Vector2(0f, -1f);
        InitialiseUpgrades();
        m_shootTimer = new vTimer(1f/m_fireRate);
        m_speedChimeTimer = new vTimer(m_speedChimeTimerRepeatTime);
    }

    void InitialiseUpgrades()
    {
        m_rigidBodyRef.mass = GetMass();
        m_acceleration = GetAcceleration();
        m_maxSpeed = GetMaxSpeed();
        m_rotateSpeed = GetRotateSpeed();
        m_fireRate = GetFireRate();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_emotion = 1;
        m_battleHandlerRef = FindObjectOfType<BattleHandler>();
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
        Vector3 worldMousePoint = m_cameraRef.ScreenToWorldPoint(Input.mousePosition);

        Vector2 deltaMousePos = worldMousePoint - transform.position;
        //deltaMousePos = new Vector2(1f,0f);
        if (Input.GetMouseButton(0) && m_readyToShoot)
        {
            m_readyToShoot = false;
            Vibe loveVibe = Instantiate(m_loveVibePrefab, transform.position, Quaternion.identity).GetComponent<Vibe>();
            loveVibe.Init(m_battleHandlerRef, null, deltaMousePos.normalized, m_rigidBodyRef.velocity, m_emotion);
        }
        UpdateShootTimer();
    }

    void HandleDrifting()
    {
        if (m_drifting)
        {
            if (Input.GetKey(KeyCode.A))
            {
                if (!m_driftParticlesLeftRef.isPlaying)
                {
                    m_driftParticlesLeftRef.Play();
                }
                m_rigidBodyRef.velocity *= 1f - m_driftDrag * Time.deltaTime;
            }
            else
            {
                m_driftParticlesLeftRef.Stop();
            }

            if (Input.GetKey(KeyCode.D))
            {
                if (!m_driftParticlesRightRef.isPlaying)
                {
                    m_driftParticlesRightRef.Play();
                }
                m_rigidBodyRef.velocity *= 1f - m_driftDrag * Time.deltaTime;
            }
            else
            {
                m_driftParticlesRightRef.Stop();
            }
            m_driftTrailLeftRef.emitting = Input.GetKey(KeyCode.A);
            m_driftTrailRightRef.emitting = Input.GetKey(KeyCode.D);
        }
        else
        {
            m_driftTrailLeftRef.emitting = false;
            m_driftTrailRightRef.emitting = false;
            m_driftParticlesLeftRef.Stop();
            m_driftParticlesRightRef.Stop();
        }
    }

    void HandleRotation()
    {
        if (m_rigidBodyRef.velocity.magnitude != 0f)
        {
            float rotationDirection = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                rotationDirection += 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                rotationDirection -= 1f;
            }

            if (rotationDirection != 0)
            {
                Vector2 turnVector = VLib.RotateVector3In2D(m_rigidBodyRef.velocity.normalized, rotationDirection * 90f).ToVector2();
                turnVector *= m_rotateSpeed;
                turnVector *= Time.deltaTime;
                turnVector *= m_drifting ? m_driftRotationMult : 1f;

                m_rigidBodyRef.velocity += turnVector;
                m_rigidBodyRef.velocity *= 1f - m_rotateDrag * Time.deltaTime;
            }

            float desiredAngle = VLib.Vector3ToEulerAngle(m_rigidBodyRef.velocity);

            if (m_drifting)
            {
                desiredAngle += 30f * rotationDirection;
            }
            HandleDrifting();

            float deltaAngle = desiredAngle - m_rigidBodyRef.rotation;
            if (deltaAngle > 180f)
            {
                deltaAngle -= 360f;
            }
            else if (deltaAngle < -180f)
            {
                deltaAngle += 360f;
            }
            m_rigidBodyRef.MoveRotation(Mathf.Lerp(m_rigidBodyRef.rotation, m_rigidBodyRef.rotation + deltaAngle, Time.deltaTime * m_velocityAlignmentRotForce * m_rigidBodyRef.velocity.magnitude/10f));
            if (m_rigidBodyRef.rotation > 180f)
            {
                m_rigidBodyRef.rotation -= 360f;
            }
            if (m_rigidBodyRef.rotation < -180f)
            {
                m_rigidBodyRef.rotation += 360f;
            }
        }
    }

    void MovementUpdate()
    {
        m_drifting = Input.GetKey(KeyCode.S) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D));

        HandleRotation();
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

        if (m_rigidBodyRef.velocity.magnitude > 7f)
        {
            if (m_speedChimeTimer.Update())
            {
                float speed = m_rigidBodyRef.velocity.magnitude;
                float volume = (speed - m_speedChimeCutoffSpeed)/ (20f-m_speedChimeCutoffSpeed);
                volume = Mathf.Clamp(volume, 0f, 1f);
                //volume = Mathf.Pow(volume, 3f);
                GameHandler._audioManager.PlayOneShot(m_speedChimeAudioClip, volume);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ShootUpdate();
        MovementUpdate();
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        if (a_collision.gameObject.tag == "Vessel")
        {
            //float impulseStrength = a_collision.contacts[0].normalImpulse;
            float collisionAngle = Vector2.SignedAngle(a_collision.relativeVelocity.normalized, a_collision.contacts[0].normal);
            float contactStrength = Mathf.Sin(Mathf.PI * collisionAngle / 180f + Mathf.PI / 2f);
            float impulseStrength = Mathf.Pow(a_collision.relativeVelocity.magnitude,2f) * contactStrength;
            a_collision.gameObject.GetComponent<Vessel>().AddEmotion(m_meleeLoveStrength);
        }
    }
}
