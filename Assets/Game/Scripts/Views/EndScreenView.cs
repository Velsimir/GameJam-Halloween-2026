using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Views
{
    public class EndScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        public event Action RestartClicked;
        public event Action QuitClicked;

        public float Alpha
        {
            get => _group.alpha;
            set => _group.alpha = Mathf.Clamp01(value);
        }

        public void EnableInput()
        {
            _group.blocksRaycasts = true;
            _group.interactable = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        private void Awake()
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
        }
    }
}
