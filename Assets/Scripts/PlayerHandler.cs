using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : Soul
{
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] Camera m_cameraRef;
    [SerializeField] GameHandler m_gameHandler;

    BattleHandler m_battleHandlerRef;
    Rigidbody2D m_rigidBodyRef;

    //Movement
    float m_maxSpeed = 10f;
    float m_acceleration = 1f;
    float m_rotateSpeed = 100f;
    float m_rotateDrag = 0.2f;
    float m_velocityAlignmentRotForce = 200f;

    //Love Combat
    float m_meleeLoveStrength = 1f;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        m_emotion = 1f;
        m_battleHandlerRef = FindObjectOfType<BattleHandler>();
        CalculateEmotionColor();
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
            loveVibe.Init(m_battleHandlerRef, null, deltaMousePos.normalized, m_rigidBodyRef.velocity, m_emotion, 10f);
        }
        else if (Input.GetMouseButton(1) && m_readyToShoot)
        {
            m_readyToShoot = false;
            Vibe loveVibe = Instantiate(m_loveVibePrefab, transform.position, Quaternion.identity).GetComponent<Vibe>();
            loveVibe.Init(m_battleHandlerRef, null, deltaMousePos.normalized, m_rigidBodyRef.velocity, 0f, 10f);
        }
        UpdateShootTimer();
    }

    void HandleRotation()
    {
        if (m_rigidBodyRef.velocity.magnitude != 0f)
        {
            float rotation = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                rotation = 1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                rotation = -1f;
            }
            rotation *= m_rotateSpeed * Time.deltaTime;

            m_rigidBodyRef.velocity = VLib.RotateVector3In2D(m_rigidBodyRef.velocity, rotation);
            m_rigidBodyRef.velocity *= rotation == 0 ? 1f : 1f - m_rotateDrag * Time.deltaTime;
            //m_rigidBodyRef.rotation = VLib.Vector3ToEulerAngle(m_rigidBodyRef.velocity);

            float deltaAngle = VLib.Vector3ToEulerAngle(m_rigidBodyRef.velocity) - m_rigidBodyRef.rotation;
            if (deltaAngle > 180f)
            {
                deltaAngle -= 360f;
            }
            else if (deltaAngle < -180f)
            {
                deltaAngle += 360f;
            }
            m_rigidBodyRef.MoveRotation(Mathf.Lerp(m_rigidBodyRef.rotation, m_rigidBodyRef.rotation + deltaAngle, Time.deltaTime * m_velocityAlignmentRotForce));
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

        if (Input.GetKey(KeyCode.S))
        {
            m_rigidBodyRef.velocity = Vector3.zero;
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
            Debug.Log(impulseStrength);
            a_collision.gameObject.GetComponent<Vessel>().AddEmotion(1f, m_meleeLoveStrength * impulseStrength);
        }
    }
}
