using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapIcon : MonoBehaviour
{
    const float m_defaultCameraSize = 8.25f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Init(Camera a_miniMapCamera)
    {
        transform.localScale *= a_miniMapCamera.orthographicSize/ m_defaultCameraSize;
    }
}
