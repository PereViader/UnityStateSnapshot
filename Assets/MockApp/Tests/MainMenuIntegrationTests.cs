using System.IO;
using NUnit.Framework;
using UnityEngine;
using MockApp;
using PereViader.UnityStateSnapshot;

namespace MockApp.Tests
{
    [TestFixture]
    public class MainMenuIntegrationTests
    {
        private MainMenuController _controller;
        private GameObject _canvasGo;
        private string _snapshotDir;

        [SetUp]
        public void SetUp()
        {
            _controller = MockAppUIBuilder.CreateMockAppHierarchy();
            _canvasGo = _controller.transform.root.gameObject;
            _snapshotDir = Path.Combine(Application.dataPath, "MockApp", "Tests", "Snapshots~");
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null)
            {
                Object.DestroyImmediate(_canvasGo);
            }
            var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                Object.DestroyImmediate(eventSystem.gameObject);
            }
        }

        [Test]
        public void T01_Initial_MainMenuView_State_MatchesSnapshot()
        {
            Assert.IsTrue(_controller.MainMenuView.activeSelf, "MainMenuView should be active initially.");
            Assert.IsFalse(_controller.SettingsView.activeSelf, "SettingsView should be inactive initially.");
            Assert.IsFalse(_controller.GameView.activeSelf, "GameView should be inactive initially.");

            var settings = new VerifySettings()
                .WithFloatPrecision(2)
                .IncludeDisabledObjects(true)
                .UseDirectory(_snapshotDir);

            string json = Snapshot.Verify(_controller.MainMenuView, "MainMenu_InitialState", settings);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("MainMenuView"));
        }

        [Test]
        public void T02_Selecting_StartGameButton_HighlightsIt_AndMatchesSnapshot()
        {
            Assert.IsNotNull(_controller.StartGameButton, "StartGameButton should not be null");
            _controller.StartGameButton.SelectButton();

            Assert.IsTrue(_controller.StartGameButton.IsSelected, "StartGameButton should be selected.");
            Assert.IsTrue(_controller.StartGameButton.IsHighlighted, "StartGameButton should be highlighted.");

            var settings = new VerifySettings()
                .WithFloatPrecision(2)
                .UseDirectory(_snapshotDir);

            string json = Snapshot.Verify(_controller.StartGameButton.gameObject, "StartGameButton_HighlightedState", settings);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("IsSelected"));
            Assert.IsTrue(json.Contains("true"));
        }

        [Test]
        public void T03_Clicking_SettingsButton_OpensSettingsScreen_AndMatchesSnapshot()
        {
            Assert.IsNotNull(_controller.SettingsButton, "SettingsButton should not be null");
            Assert.IsNotNull(_controller.SettingsButton.Button, "SettingsButton.Button should not be null");

            _controller.SettingsButton.Button.onClick.Invoke();

            Assert.IsFalse(_controller.MainMenuView.activeSelf, "MainMenuView should be inactive after opening Settings.");
            Assert.IsTrue(_controller.SettingsView.activeSelf, "SettingsView should be active after opening Settings.");
            Assert.AreEqual("Settings", _controller.ActiveScreen);

            var settings = new VerifySettings()
                .WithFloatPrecision(2)
                .IncludeDisabledObjects(true)
                .UseDirectory(_snapshotDir);

            string json = Snapshot.Verify(_canvasGo, "SettingsScreen_OpenedState", settings);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("SettingsView"));
        }

        [Test]
        public void T04_Clicking_BackButton_ReturnsToMainMenu_AndMatchesSnapshot()
        {
            Assert.IsNotNull(_controller.SettingsButton, "SettingsButton should not be null");
            Assert.IsNotNull(_controller.SettingsButton.Button, "SettingsButton.Button should not be null");
            Assert.IsNotNull(_controller.SettingsBackButton, "SettingsBackButton should not be null");
            Assert.IsNotNull(_controller.SettingsBackButton.Button, "SettingsBackButton.Button should not be null");

            // Open Settings
            _controller.SettingsButton.Button.onClick.Invoke();
            Assert.IsTrue(_controller.SettingsView.activeSelf);

            // Press Back
            _controller.SettingsBackButton.Button.onClick.Invoke();

            Assert.IsTrue(_controller.MainMenuView.activeSelf, "MainMenuView should be active after pressing Back.");
            Assert.IsFalse(_controller.SettingsView.activeSelf, "SettingsView should be inactive after pressing Back.");
            Assert.AreEqual("MainMenu", _controller.ActiveScreen);

            var settings = new VerifySettings()
                .WithFloatPrecision(2)
                .IncludeDisabledObjects(true)
                .UseDirectory(_snapshotDir);

            string json = Snapshot.Verify(_canvasGo, "MainMenu_NavigatedBackState", settings);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("MainMenuView"));
        }
    }
}
