using UnityEngine;
using UnityEngine.UI;

namespace LucillesJourney.UI
{
    /// <summary>
    /// The embedded interaction guide required by the brief (not a separate document). A simple
    /// panel explaining controls, opened from the main menu and/or pause menu.
    /// </summary>
    public class InteractionGuideUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            WireListeners();
            Hide();
        }

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(GameObject root, Button close)
        {
            panelRoot = root;
            closeButton = close;
            WireListeners();
            Hide();
        }

        private void WireListeners()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (panelRoot == null) return;
            panelRoot.SetActive(!panelRoot.activeSelf);
        }
    }
}
