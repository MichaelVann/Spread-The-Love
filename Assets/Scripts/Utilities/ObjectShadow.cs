using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class ObjectShadow : MonoBehaviour
{
    protected const float _shadowAngle = 135f;
    protected const float _shadowDistance = 0.1f;
    protected const float _opacity = 0.75f;

    [SerializeField] internal float m_height = 0f;
    public void Awake()
    {

    }

    public virtual void Start()
    {
        if (transform.parent != null)
        {
            SpriteRenderer parentSprite = transform.parent.gameObject.GetComponent<SpriteRenderer>();
            if (parentSprite != null)
            {
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = parentSprite.sprite;
                spriteRenderer.color = new Color(0f,0f,0f,_opacity);
            }
        }
    }

    public virtual void Update()
    {
        float eulerAnglesForShadow = transform.eulerAngles.z + _shadowAngle;
        float x = Mathf.Sin(eulerAnglesForShadow * Mathf.PI / 180f) / transform.parent.transform.localScale.x;
        float y = Mathf.Cos(eulerAnglesForShadow * Mathf.PI / 180f) / transform.parent.transform.localScale.y;
        float z = m_height / transform.parent.transform.localScale.y;
        transform.localPosition = new Vector3(x, y) * _shadowDistance + new Vector3(0f,-z);
    }
}
