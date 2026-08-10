using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MockApp
{
    public static class MockAppUIBuilder
    {
        public static MainMenuController CreateMockAppHierarchy()
        {
            // EventSystem
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            // Root Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Controller Root
            var appGo = new GameObject("MainMenuApp");
            appGo.transform.SetParent(canvasGo.transform, false);
            var controller = appGo.AddComponent<MainMenuController>();

            // MainMenuView Panel
            var mainMenuView = CreatePanel(canvasGo.transform, "MainMenuView");
            controller.MainMenuView = mainMenuView;
            controller.MainMenuTitle = CreateText(mainMenuView.transform, "TitleText", "Main Menu");
            controller.StartGameButton = CreateButton(mainMenuView.transform, "StartGameButton", "Start Game");
            controller.SettingsButton = CreateButton(mainMenuView.transform, "SettingsButton", "Settings");
            controller.ExitButton = CreateButton(mainMenuView.transform, "ExitButton", "Exit");

            // SettingsView Panel
            var settingsView = CreatePanel(canvasGo.transform, "SettingsView");
            controller.SettingsView = settingsView;
            controller.SettingsTitle = CreateText(settingsView.transform, "SettingsTitleText", "Settings");
            controller.SettingsBackButton = CreateButton(settingsView.transform, "SettingsBackButton", "Back");

            // GameView Panel
            var gameView = CreatePanel(canvasGo.transform, "GameView");
            controller.GameView = gameView;
            controller.GameTitle = CreateText(gameView.transform, "GameTitleText", "Game Screen");
            controller.GameBackButton = CreateButton(gameView.transform, "GameBackButton", "Back");

            controller.SetupListeners();
            controller.ShowMainMenuView();

            return controller;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var panelGo = new GameObject(name, typeof(RectTransform));
            panelGo.transform.SetParent(parent, false);
            var rt = panelGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            var img = panelGo.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            return panelGo;
        }

        private static Text CreateText(Transform parent, string name, string content)
        {
            var textGo = new GameObject(name, typeof(RectTransform));
            textGo.transform.SetParent(parent, false);
            var text = textGo.AddComponent<Text>();
            text.text = content;
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        private static MockButtonUI CreateButton(Transform parent, string name, string labelText)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform));
            buttonGo.transform.SetParent(parent, false);
            var img = buttonGo.AddComponent<Image>();
            img.color = Color.gray;
            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = img;

            var mockButton = buttonGo.AddComponent<MockButtonUI>();
            CreateText(buttonGo.transform, "Text", labelText);

            return mockButton;
        }
    }
}
