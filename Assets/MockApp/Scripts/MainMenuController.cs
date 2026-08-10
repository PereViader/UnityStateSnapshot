using UnityEngine;
using UnityEngine.UI;

namespace MockApp
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject MainMenuView;
        public GameObject SettingsView;
        public GameObject GameView;

        public MockButtonUI StartGameButton;
        public MockButtonUI SettingsButton;
        public MockButtonUI ExitButton;
        public MockButtonUI SettingsBackButton;
        public MockButtonUI GameBackButton;

        public Text MainMenuTitle;
        public Text SettingsTitle;
        public Text GameTitle;

        public string ActiveScreen { get; private set; } = "MainMenu";

        private void Start()
        {
            SetupListeners();
            ShowMainMenuView();
        }

        public void SetupListeners()
        {
            if (StartGameButton != null && StartGameButton.Button != null)
            {
                StartGameButton.Button.onClick.RemoveAllListeners();
                StartGameButton.Button.onClick.AddListener(OnStartGameClicked);
            }

            if (SettingsButton != null && SettingsButton.Button != null)
            {
                SettingsButton.Button.onClick.RemoveAllListeners();
                SettingsButton.Button.onClick.AddListener(OnSettingsClicked);
            }

            if (SettingsBackButton != null && SettingsBackButton.Button != null)
            {
                SettingsBackButton.Button.onClick.RemoveAllListeners();
                SettingsBackButton.Button.onClick.AddListener(OnBackClicked);
            }

            if (GameBackButton != null && GameBackButton.Button != null)
            {
                GameBackButton.Button.onClick.RemoveAllListeners();
                GameBackButton.Button.onClick.AddListener(OnBackClicked);
            }
        }

        public void ShowMainMenuView()
        {
            ActiveScreen = "MainMenu";
            if (MainMenuView != null) MainMenuView.SetActive(true);
            if (SettingsView != null) SettingsView.SetActive(false);
            if (GameView != null) GameView.SetActive(false);
        }

        public void ShowSettingsView()
        {
            ActiveScreen = "Settings";
            if (MainMenuView != null) MainMenuView.SetActive(false);
            if (SettingsView != null) SettingsView.SetActive(true);
            if (GameView != null) GameView.SetActive(false);
        }

        public void ShowGameView()
        {
            ActiveScreen = "Game";
            if (MainMenuView != null) MainMenuView.SetActive(false);
            if (SettingsView != null) SettingsView.SetActive(false);
            if (GameView != null) GameView.SetActive(true);
        }

        private void OnStartGameClicked()
        {
            ShowGameView();
        }

        private void OnSettingsClicked()
        {
            ShowSettingsView();
        }

        private void OnBackClicked()
        {
            ShowMainMenuView();
        }
    }
}
