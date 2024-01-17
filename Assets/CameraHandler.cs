using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] GameObject m_playerRef;
    Vector3 m_offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredPosition = Vector3.zero;
        desiredPosition += m_playerRef.GetComponent<Rigidbody2D>().velocity.ToVector3() * 0.1f;
        m_offset = Vector3.Lerp(m_offset, desiredPosition, Time.deltaTime);
        transform.position = m_playerRef.transform.position + new Vector3(0f, 0f, -10f);
        transform.position += m_offset;
    }
}
