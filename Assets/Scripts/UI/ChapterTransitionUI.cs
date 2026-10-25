using System;
using System.Collections;
using UnityEngine;

namespace LucillesJourney.UI
{
    /// <summary>
    /// A full-screen CanvasGroup used to fade to/from black between chapters so scene loads
    /// don't feel like an abrupt cut. Purely cosmetic - safe to leave unassigned.
    /// </summary>
    public class ChapterTransitionUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.6f;

        private void Awake()
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
                fadeGroup.blocksRaycasts = false;
                fadeGroup.interactable = false;
            }
        }

        /// <summary>Used by the editor scene builder to assign the CanvasGroup created at build time.</summary>
        public void Wire(CanvasGroup group)
        {
            fadeGroup = group;
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
                fadeGroup.blocksRaycasts = false;
                fadeGroup.interactable = false;
            }
        }

        private void Start() => StartCoroutine(Fade(1f, 0f, null));

        public void FadeOut(Action onComplete)
        {
            LucillesJourney.Gamification.UI.UISound.Play("sfx_transition", 0.8f);
            StartCoroutine(Fade(0f, 1f, onComplete));
        }

        private IEnumerator Fade(float from, float to, Action onComplete)
        {
            if (fadeGroup == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            fadeGroup.alpha = to;
            onComplete?.Invoke();
        }
    }
}
