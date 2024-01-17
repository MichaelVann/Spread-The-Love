using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayArea : MonoBehaviour
{
    [SerializeField] BattleHandler m_battleHandlerRef;
    [SerializeField] PhysicsMaterial2D m_physicsMaterialRef;
    // Start is called before the first frame update

    private void Awake()
    {
        EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        edgeCollider.sharedMaterial = m_physicsMaterialRef;
        Vector2[] colliderPoints = new Vector2[50];
        for (int i = 0; i < colliderPoints.Length; i++)
        {
            colliderPoints[i] = VLib.EulerAngleToVector2(i * 360f/(colliderPoints.Length-1)) * 1.1f;
        }
        edgeCollider.points = colliderPoints;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D a_collision)
    {
        Vessel vessel = a_collision.gameObject.GetComponent<Vessel>();
        Vibe vibe = a_collision.gameObject.GetComponent<Vibe>();
        if (vessel != null)
        {
            m_battleHandlerRef.DestroyVessel(vessel);
        }
        else if (vibe != null)
        {
            Destroy(vibe.gameObject);
        }
    }
}
