using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Soul : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer m_spriteRendererRef;
        //static internal Color m_afraidColorRef = new Color(0.1982f, 0.7641f, 0.5605f, 1f);

        protected int m_emotion;
        protected const int m_maxLove = 2;
        protected const int m_minLove = -4;

        internal int GetEmotion() { return m_emotion;}
        internal int GetFear() { return m_maxLove - GetEmotion(); }

        static internal int GetMaxPossibleLove() {  return m_maxLove; }
        static internal int GetMinPossibleLove() {  return m_minLove; }

        static internal float GetEmotionMappedFromMinToMax(float a_emotion) { return a_emotion < 0 ? a_emotion / (-m_minLove) : a_emotion / m_maxLove; }

        // Use this for initialization
        void Start()
        {
            m_emotion = 0;
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal static Color CalculateEmotionColor(float a_emotion)
        {
            Color color = new Color(0f, 0f, 0f, 1f);
            float lerp = GetEmotionMappedFromMinToMax(a_emotion);
            if (a_emotion < 0)
            {
                int index = (int)-(a_emotion + 1);
                color = GameHandler._autoRef.m_fearColors[index];
            }
            else
            {
                switch (a_emotion)
                {
                    case 0:
                        color = GameHandler._autoRef.m_neutralColor;
                        break;
                    case 1:
                        color = GameHandler._autoRef.m_loveColor1;
                        break;
                    case 2:
                        color = GameHandler._autoRef.m_loveColorMax;
                        break;
                    default:
                        break;
                }
            }
            
            return color;
        }

        protected Color CalculateEmotionColor()
        {
            return CalculateEmotionColor(m_emotion);
        }

        protected int AffectEmotion(int a_change)
        {
            int priorEmotion = m_emotion;
            m_emotion = Mathf.Clamp(m_emotion+ a_change, m_minLove, m_maxLove);
            int deltaEmotion = m_emotion - priorEmotion;
            return deltaEmotion;
        }
    }
}