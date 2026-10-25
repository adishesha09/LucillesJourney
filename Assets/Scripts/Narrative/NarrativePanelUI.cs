using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LucillesJourney.Interaction;
using LucillesJourney.Core;
using LucillesJourney.Player;

namespace LucillesJourney.Narrative
{
    /// <summary>
    /// Displays a NarrativeSequenceSO one line at a time. Disables player movement and
    /// interaction while visible so narrative always has the player's full attention.
    /// One instance is expected per gameplay scene (assigned to the HUD canvas).
    /// </summary>
    public class NarrativePanelUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text continueButtonLabel;

        public static NarrativePanelUI Instance { get; private set; }

        private DialogueLine[] lines = Array.Empty<DialogueLine>();
        private int lineIndex;
        private Action onComplete;
        private InputAction advanceAction;

        private void Awake()
        {
            Instance = this;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            advanceAction = new InputAction("AdvanceDialogue", InputActionType.Button);
            advanceAction.AddBinding("<Keyboard>/space");
            advanceAction.AddBinding("<Keyboard>/enter");
            advanceAction.AddBinding("<Gamepad>/buttonSouth");

            if (continueButton != null) continueButton.onClick.AddListener(Advance);
            HideImmediate();
        }

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(CanvasGroup group, TMP_Text speaker, TMP_Text body, Button continueBtn, TMP_Text continueLabel)
        {
            canvasGroup = group;
            speakerLabel = speaker;
            bodyLabel = body;
            continueButton = continueBtn;
            continueButtonLabel = continueLabel;
            if (continueButton != null) continueButton.onClick.AddListener(Advance);
            HideImmediate();
        }

        private void OnEnable()
        {
            advanceAction?.Enable();
            if (advanceAction != null) advanceAction.performed += OnAdvancePerformed;
        }

        private void OnDisable()
        {
            if (advanceAction != null) advanceAction.performed -= OnAdvancePerformed;
            advanceAction?.Disable();
        }

        private void OnDestroy() => advanceAction?.Dispose();

        private void OnAdvancePerformed(InputAction.CallbackContext _)
        {
            if (IsShowing && !GameManager.Instance.IsPaused) Advance();
        }

        public bool IsShowing { get; private set; }

        /// <summary>Plays a sequence. If the sequence is null/empty, onComplete fires immediately.</summary>
        public void Play(NarrativeSequenceSO sequence, Action onCompleteCallback = null)
        {
            onComplete = onCompleteCallback;

            if (sequence == null || sequence.lines == null || sequence.lines.Length == 0)
            {
                onComplete?.Invoke();
                onComplete = null;
                return;
            }

            lines = sequence.lines;
            lineIndex = 0;
            Show();
            DisplayCurrentLine();
        }

        private void Show()
        {
            IsShowing = true;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            SetWorldInputEnabled(false);
        }

        private void Advance()
        {
            LucillesJourney.Gamification.UI.UISound.Play("ui_advance", 0.7f);
            lineIndex++;
            if (lineIndex >= lines.Length)
            {
                Hide();
                var callback = onComplete;
                onComplete = null;
                callback?.Invoke();
                return;
            }
            DisplayCurrentLine();
        }

        private void DisplayCurrentLine()
        {
            var line = lines[lineIndex];
            bool hasSpeaker = !string.IsNullOrWhiteSpace(line.speakerName);
            if (speakerLabel != null)
            {
                speakerLabel.gameObject.SetActive(hasSpeaker);
                speakerLabel.text = line.speakerName;
            }
            if (bodyLabel != null) bodyLabel.text = line.text;
            if (continueButtonLabel != null)
                continueButtonLabel.text = (lineIndex == lines.Length - 1) ? "Close" : "Continue";
        }

        private void Hide()
        {
            IsShowing = false;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            SetWorldInputEnabled(true);
        }

        private void HideImmediate()
        {
            IsShowing = false;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void SetWorldInputEnabled(bool enabled)
        {
            var player = FindFirstObjectByType<PlayerController2D>();
            if (player != null) player.SetMovementEnabled(enabled);

            var detector = FindFirstObjectByType<InteractionDetector>();
            if (detector != null) detector.SetInteractionEnabled(enabled);
        }
    }
}
