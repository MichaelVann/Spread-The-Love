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
    float m_maxSpeed = 10f;
    float m_acceleration = 1f;
    float m_rotateSpeed = 10f;
    float m_rotateDrag = 0.12f;
    float m_velocityAlignmentRotForce = 200f;
    bool m_drifting = false;
    float m_driftDrag = 0.2f;
    float m_driftRotationMult = 2f;

    //Love Combat
    int m_meleeLoveStrength = 1;

    //Shoot
    bool m_readyToShoot = false;
    float m_fireRate = 0.25f;
    vTimer m_shootTimer;

    internal float GetSpeed() { return m_rigidBodyRef.velocity.magnitude; }

    void Awake()
    {
        m_shootTimer = new vTimer(m_fireRate);
        m_rigidBodyRef = GetComponent<Rigidbody2D>();
        m_rigidBodyRef.velocity = new Vector2(0f, -1f);
        InitialiseUpgrades();
    }

    void InitialiseUpgrades()
    {
        m_maxSpeed += GameHandler._upgradeTree.GetUpgradeLevel(UpgradeItem.UpgradeId.TopSpeed);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_emotion = 1;
        m_battleHandlerRef = FindObjectOfType<BattleHandler>();
        //CalculateEmotionColor();
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
