using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Soul : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer m_spriteRendererRef;
        //static internal Color m_afraidColorRef = new Color(0.1982f, 0.7641f, 0.5605f, 1f);

        protected float m_emotion;
        const float m_maxLove = 1f;
        const float m_minLove = -10f;

        internal float GetEmotion() { return m_emotion;}

        internal float GetPeace() { return m_emotion; }
        internal float GetFear() { return 1f - GetPeace(); }

        static internal float GetEmotionMappedFromMinToMax(float a_emotion) { return a_emotion < 0 ? (a_emotion - m_minLove) / (-m_minLove) : a_emotion / m_maxLove; }

        // Use this for initialization
        void Start()
        {
            m_emotion = 0f;
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal static Color CalculateEmotionColor(float a_emotion)
        {
            Color color = new Color(0f, 0f, 0f, 1f);
            float lerp = GetEmotionMappedFromMinToMax(a_emotion);
            if (a_emotion < 0f)
            {
                color = Color.Lerp(GameHandler.m_autoRef.m_fearColor, GameHandler.m_autoRef.m_neutralColor, lerp);
            }
            else
            {
                color = Color.Lerp(GameHandler.m_autoRef.m_neutralColor, GameHandler.m_autoRef.m_loveColor, lerp);
            }
            return color;
        }

        protected void CalculateEmotionColor()
        {
            m_spriteRendererRef.color = CalculateEmotionColor(m_emotion);
        }

        protected float AffectEmotion(float a_change)
        {
            float priorEmotion = m_emotion;
            m_emotion = Mathf.Clamp(m_emotion+ a_change, 0f, m_maxLove);
            float deltaEmotion = m_emotion - priorEmotion;
            return deltaEmotion;
        }
    }
}