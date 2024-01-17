using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Soul : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer m_spriteRendererRef;
        //static internal Color m_afraidColorRef = new Color(0.1982f, 0.7641f, 0.5605f, 1f);

        protected float m_emotion;
        protected float m_emotionalInertia = 100f;

        internal float GetEmotion() { return m_emotion;}

        internal float GetPeace() { return m_emotion; }
        internal float GetFear() { return 1f - GetPeace(); }

        // Use this for initialization
        void Start()
        {
            m_emotion = 0.5f;
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal static Color CalculateEmotionColor(float a_emotion)
        {
            Color color = new Color(0f, 0f, 0f, 1f);
            if (a_emotion < 0.5f)
            {
                color = Color.Lerp(GameHandler.m_autoRef.m_fearColor, GameHandler.m_autoRef.m_neutralColor, a_emotion*2f);
            }
            else
            {
                color = Color.Lerp(GameHandler.m_autoRef.m_neutralColor, GameHandler.m_autoRef.m_loveColor, (a_emotion-0.5f) * 2f);
            }
            return color;
        }

        protected void CalculateEmotionColor()
        {
            m_spriteRendererRef.color = CalculateEmotionColor(m_emotion);
        }

        protected float AffectEmotion(float a_emotion, float a_emotionStrength)
        {
            float priorEmotion = m_emotion;
            m_emotion = Mathf.Lerp(m_emotion, a_emotion, a_emotionStrength / m_emotionalInertia);
            m_emotion = Mathf.Clamp(m_emotion, 0f, 1f);
            float deltaEmotion = m_emotion - priorEmotion;
            return deltaEmotion;
        }
    }
}