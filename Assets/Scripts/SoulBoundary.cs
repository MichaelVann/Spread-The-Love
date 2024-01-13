using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulBoundary : MonoBehaviour
{
    [SerializeField] Soul m_parentSoulRef;
    [SerializeField] Rigidbody2D m_parentRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D a_other)
    {
        if (a_other != null && Soul.m_repulsedByOtherSouls)
        {
            m_parentSoulRef.AttractToSoul(a_other.transform.position);
        }
    }
}
