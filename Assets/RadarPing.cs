using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarPing : MonoBehaviour
{
    [SerializeField] PlayerHandler m_playerHandler;
    float m_expandSpeed = 15f;
    Vector3 m_startingScale;
    // Start is called before the first frame update
    void Start()
    {
        m_startingScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += Vector3.one * Time.deltaTime * m_expandSpeed;
    }

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        Vessel vessel = a_collision.gameObject.GetComponent<Vessel>();
        if (vessel != null && vessel.GetEmotion() < Soul.GetMaxPossibleLove())
        {
            m_playerHandler.ReceiveVesselRadarPing(a_collision.transform.position);
            transform.localScale = m_startingScale;
        }
    }
}
