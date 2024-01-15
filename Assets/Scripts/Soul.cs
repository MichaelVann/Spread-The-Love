using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Soul : MonoBehaviour
    {

        static internal Color m_afraidColorRef = new Color(0.1982f, 0.7641f, 0.5605f, 1f);
        static internal Color m_sadColorRef   = new Color(0.2512523f, 0.2090156f, 0.7264151f, 1f);
        static internal Color m_angryColorRef = new Color(0.6749428f, 0.7264151f, 0.2227216f, 1f);
        static internal Color m_loveColorRef  = new Color(1f,0f, 0.3422555f, 1f);


        protected static Vector2 m_afraidPosition = new Vector2(0f, 0f);
        protected static Vector2 m_sadPosition = new Vector2(1f, 0f);
        protected static Vector2 m_angryPosition = new Vector2(0f, 1f);
        protected static Vector2 m_lovePosition = new Vector2(1f, 1f);

        protected Vector2 m_emotion;
        protected float m_emotionalInertia = 100f;

        internal Vector2 GetEmotion() { return m_emotion;}

        internal float GetJoy() { return m_emotion.y; }
        internal float GetSadness() { return 1f - GetJoy(); }
        internal float GetPeace() { return m_emotion.x; }
        internal float GetFear() { return 1f - GetPeace(); }
        internal float GetJoyScale() { return 2 * (m_emotion.y - 0.5f); }
        internal float GetPeaceScale() { return 2 * (m_emotion.x - 0.5f); }

        // Use this for initialization
        void Start()
        {
            m_emotion = new Vector2(0.5f, 0.5f);
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal static Color CalculateEmotionColor(Vector2 a_emotion)
        {
            Color color = new Color(0f, 0f, 0f, 1f);
            color += m_afraidColorRef * Mathf.Clamp((1f - Vector2.Distance(m_afraidPosition, a_emotion)), 0f, 1f);
            color += m_sadColorRef * Mathf.Clamp((1f - Vector2.Distance(m_sadPosition, a_emotion)), 0f, 1f);
            color += m_angryColorRef * Mathf.Clamp((1f - Vector2.Distance(m_angryPosition, a_emotion)), 0f, 1f);
            color += m_loveColorRef * Mathf.Clamp((1f - Vector2.Distance(m_lovePosition, a_emotion)), 0f, 1f);
            return color;
        }

        protected void AffectEmotion(Vector2 a_emotion, float a_emotionStrength)
        {
            m_emotion = Vector2.Lerp(m_emotion, a_emotion, a_emotionStrength / m_emotionalInertia);

            for (int i = 0; i < 2; i++)
            {
                m_emotion[i] = Mathf.Clamp(m_emotion[i], 0f, 1f);
            }
        }
    }
}