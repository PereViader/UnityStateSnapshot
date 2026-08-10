using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using UnityEngine.Video;
using UnityEngine.UIElements;
using TMPro;
using PereViader.UnityStateSnapshot;
using Object = UnityEngine.Object;

namespace MockApp.Tests
{
    [TestFixture]
    public class SnapshotEngineUnitTests
    {
        #region 1. TextMeshProUGUI Extraction

        [Test]
        public void TextMeshProUGUI_Extraction_ExtractsTextPropertiesCorrectly()
        {
            var go = new GameObject("TMP_TestObject", typeof(RectTransform));
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "Hello State Snapshot";
            tmp.fontSize = 28.5f;
            tmp.color = new Color(1f, 0f, 0f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);

                Assert.IsNotNull(dto);
                Assert.AreEqual("TMP_TestObject", dto.Name);

                var compDto = dto.Components.Find(c => c.TypeName == "TextMeshProUGUI");
                Assert.IsNotNull(compDto, "Component DTO for TextMeshProUGUI should exist.");

                var textEntry = compDto.StateEntries.Find(e => e.Key == "Text");
                Assert.IsNotNull(textEntry);
                Assert.AreEqual("Hello State Snapshot", textEntry.Value);

                var colorEntry = compDto.StateEntries.Find(e => e.Key == "Color");
                Assert.IsNotNull(colorEntry);
                Assert.AreEqual("#FF0000FF", colorEntry.Value);

                var fontSizeEntry = compDto.StateEntries.Find(e => e.Key == "FontSize");
                Assert.IsNotNull(fontSizeEntry);
                Assert.AreEqual("28.5", fontSizeEntry.Value);

                var alignEntry = compDto.StateEntries.Find(e => e.Key == "Alignment");
                Assert.IsNotNull(alignEntry);
                Assert.AreEqual(TextAlignmentOptions.Center.ToString(), alignEntry.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 2. Canvas Resolution Invariance

        [Test]
        public void CanvasResolutionInvariance_DifferentDisplayResolutions_ProduceIdenticalSnapshots()
        {
            var canvasGo = new GameObject("TestRootCanvas", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var childGo = new GameObject("Panel", typeof(RectTransform));
            childGo.transform.SetParent(canvasGo.transform, false);
            var childRt = childGo.GetComponent<RectTransform>();
            childRt.anchorMin = Vector2.zero;
            childRt.anchorMax = Vector2.one;
            childRt.sizeDelta = Vector2.zero;

            try
            {
                var rt = canvasGo.GetComponent<RectTransform>();

                // Simulate resolution 1 (e.g. 640x480)
                rt.sizeDelta = new Vector2(640, 480);
                rt.anchoredPosition = new Vector2(320, 240);
                rt.localPosition = new Vector3(320, 240, 0);

                var settings = new VerifySettings().WithFloatPrecision(2);
                var dtoResolution1 = Snapshot.Extract(canvasGo, settings);
                string json1 = JsonUtility.ToJson(dtoResolution1, true);

                // Simulate resolution 2 (e.g. 1920x1080)
                rt.sizeDelta = new Vector2(1920, 1080);
                rt.anchoredPosition = new Vector2(960, 540);
                rt.localPosition = new Vector3(960, 540, 0);

                var dtoResolution2 = Snapshot.Extract(canvasGo, settings);
                string json2 = JsonUtility.ToJson(dtoResolution2, true);

                // Simulate resolution 3 (e.g. 800x600 in batchmode)
                rt.sizeDelta = new Vector2(800, 600);
                rt.anchoredPosition = new Vector2(400, 300);
                rt.localPosition = new Vector3(400, 300, 0);

                var dtoResolution3 = Snapshot.Extract(canvasGo, settings);
                string json3 = JsonUtility.ToJson(dtoResolution3, true);

                Assert.AreEqual(json1, json2, "Snapshot of root ScreenSpaceOverlay canvas should be identical across 640x480 and 1920x1080.");
                Assert.AreEqual(json1, json3, "Snapshot of root ScreenSpaceOverlay canvas should be identical across 640x480 and 800x600.");

                // Root overlay canvas transform should be normalized to zero
                Assert.AreEqual(0f, dtoResolution1.Transform.LocalPosition.X);
                Assert.AreEqual(0f, dtoResolution1.Transform.LocalPosition.Y);
                Assert.AreEqual(0f, dtoResolution1.Transform.RectTransform.AnchoredPosition.X);
                Assert.AreEqual(0f, dtoResolution1.Transform.RectTransform.SizeDelta.X);
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        #endregion

        #region 3. [SerializeField] Private Field Extraction

        private class SampleSerializedBehaviour : MonoBehaviour
        {
            [SerializeField] private int _secretCode = 1337;
            [SerializeField] private string _heroName = "Aria";
            private int _unserializedHidden = 999;
            public float PublicSpeed = 5.5f;
        }

        [Test]
        public void SerializeField_PrivateFields_AreExtractedWhenEnabled()
        {
            var go = new GameObject("SerializedField_Test");
            go.AddComponent<SampleSerializedBehaviour>();

            try
            {
                var settingsWithFields = new VerifySettings().IncludeSerializedFields(true);
                var dtoWith = Snapshot.Extract(go, settingsWithFields);
                var compDtoWith = dtoWith.Components.Find(c => c.TypeName == nameof(SampleSerializedBehaviour));

                Assert.IsNotNull(compDtoWith);
                Assert.IsNotNull(compDtoWith.StateEntries.Find(e => e.Key == "_secretCode" && e.Value == "1337"));
                Assert.IsNotNull(compDtoWith.StateEntries.Find(e => e.Key == "_heroName" && e.Value == "Aria"));
                Assert.IsNotNull(compDtoWith.StateEntries.Find(e => e.Key == "PublicSpeed" && e.Value == "5.5"));
                Assert.IsNull(compDtoWith.StateEntries.Find(e => e.Key == "_unserializedHidden"));

                var settingsWithoutFields = new VerifySettings().IncludeSerializedFields(false);
                var dtoWithout = Snapshot.Extract(go, settingsWithoutFields);
                var compDtoWithout = dtoWithout.Components.Find(c => c.TypeName == nameof(SampleSerializedBehaviour));

                Assert.IsNotNull(compDtoWithout);
                Assert.IsNull(compDtoWithout.StateEntries.Find(e => e.Key == "_secretCode"));
                Assert.IsNull(compDtoWithout.StateEntries.Find(e => e.Key == "_heroName"));
                Assert.IsNotNull(compDtoWithout.StateEntries.Find(e => e.Key == "PublicSpeed"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 4. [SnapshotIgnore] Attribute Skipping

        private class SampleIgnoreBehaviour : MonoBehaviour
        {
            [SnapshotIgnore]
            public string SessionToken = "SECRET_TOKEN_XYZ";

            [SerializeField, SnapshotIgnore]
            private int _runtimeRandomSeed = 98765;

            [SnapshotIgnore]
            public int LiveFps => 120;

            public string VisibleName = "PublicHero";
            [SerializeField] private int _visibleLevel = 50;
        }

        [Test]
        public void SnapshotIgnore_FieldsAndProperties_AreOmittedFromSnapshot()
        {
            var go = new GameObject("Ignore_Test");
            go.AddComponent<SampleIgnoreBehaviour>();

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(SampleIgnoreBehaviour));

                Assert.IsNotNull(compDto);
                Assert.IsNull(compDto.StateEntries.Find(e => e.Key == "SessionToken"));
                Assert.IsNull(compDto.StateEntries.Find(e => e.Key == "_runtimeRandomSeed"));
                Assert.IsNull(compDto.StateEntries.Find(e => e.Key == "LiveFps"));

                Assert.IsNotNull(compDto.StateEntries.Find(e => e.Key == "VisibleName" && e.Value == "PublicHero"));
                Assert.IsNotNull(compDto.StateEntries.Find(e => e.Key == "_visibleLevel" && e.Value == "50"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 5. Custom ISnapshotComponentHandler Registration

        private class SampleCharacterStats : MonoBehaviour
        {
            public int CurrentHP = 40;
            public int MaxHP = 100;
            public int AttackPower = 15;
        }

        private class CustomCharacterStatsHandler : ISnapshotComponentHandler
        {
            public int Priority => 100;

            public bool CanHandle(Type componentType)
            {
                return typeof(SampleCharacterStats).IsAssignableFrom(componentType);
            }

            public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
            {
                if (component is SampleCharacterStats stats)
                {
                    targetState["HealthDisplay"] = $"{stats.CurrentHP}/{stats.MaxHP}";
                    targetState["Status"] = stats.CurrentHP > 0 ? "Alive" : "Dead";
                }
            }
        }

        [Test]
        public void CustomComponentHandler_WhenRegistered_OverridesDefaultExtraction()
        {
            var go = new GameObject("CustomHandler_Test");
            var stats = go.AddComponent<SampleCharacterStats>();
            stats.CurrentHP = 75;
            stats.MaxHP = 100;
            stats.AttackPower = 20;

            try
            {
                var customHandler = new CustomCharacterStatsHandler();
                var settings = new VerifySettings().RegisterComponentHandler(customHandler);

                var dto = Snapshot.Extract(go, settings);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(SampleCharacterStats));

                Assert.IsNotNull(compDto);
                Assert.IsNotNull(compDto.StateEntries.Find(e => e.Key == "HealthDisplay" && e.Value == "75/100"));
                Assert.IsNotNull(compDto.StateEntries.Find(e => e.Key == "Status" && e.Value == "Alive"));
                Assert.IsNull(compDto.StateEntries.Find(e => e.Key == "AttackPower"), "Default fields should not be extracted when custom handler processes it.");

                // Unregistering handler restores default extraction
                settings.UnregisterComponentHandler(customHandler);
                var dtoDefault = Snapshot.Extract(go, settings);
                var compDtoDefault = dtoDefault.Components.Find(c => c.TypeName == nameof(SampleCharacterStats));

                Assert.IsNotNull(compDtoDefault);
                Assert.IsNotNull(compDtoDefault.StateEntries.Find(e => e.Key == "AttackPower" && e.Value == "20"));
                Assert.IsNull(compDtoDefault.StateEntries.Find(e => e.Key == "HealthDisplay"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 6. -0.0f Float Scrubbing to 0.0f

        private class SampleFloatBehaviour : MonoBehaviour
        {
            public float NegZeroFloat = -0.0f;
            public float SmallNegFloat = -0.00001f;
            public double NegZeroDouble = -0.0d;
            public Vector3 NegZeroVec = new Vector3(-0.0f, -0.00001f, 0.0f);
        }

        [Test]
        public void FloatScrubbing_NegativeZero_NormalizesToPositiveZero()
        {
            // 1. Direct scrubber tests
            float negZero = -0.0f;
            float scrubbedFloat = SnapshotScrubber.RoundFloat(negZero, 2);
            Assert.AreEqual(0.0f, scrubbedFloat);
            Assert.AreEqual(0, BitConverter.SingleToInt32Bits(scrubbedFloat), "Scrubbed float bit pattern must be +0.0f, not -0.0f.");

            double negZeroD = -0.0d;
            double scrubbedDouble = SnapshotScrubber.RoundDouble(negZeroD, 2);
            Assert.AreEqual(0.0d, scrubbedDouble);
            Assert.AreEqual(0L, BitConverter.DoubleToInt64Bits(scrubbedDouble), "Scrubbed double bit pattern must be +0.0d, not -0.0d.");

            float smallNeg = -0.0001f;
            float scrubbedSmall = SnapshotScrubber.RoundFloat(smallNeg, 2);
            Assert.AreEqual(0.0f, scrubbedSmall);
            Assert.AreEqual(0, BitConverter.SingleToInt32Bits(scrubbedSmall));

            // 2. Component extraction test
            var go = new GameObject("NegativeZero_Test");
            go.AddComponent<SampleFloatBehaviour>();

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(SampleFloatBehaviour));

                Assert.IsNotNull(compDto);
                var fEntry = compDto.StateEntries.Find(e => e.Key == "NegZeroFloat");
                Assert.IsNotNull(fEntry);
                Assert.AreEqual("0", fEntry.Value);

                var dEntry = compDto.StateEntries.Find(e => e.Key == "NegZeroDouble");
                Assert.IsNotNull(dEntry);
                Assert.AreEqual("0", dEntry.Value);

                var vEntry = compDto.StateEntries.Find(e => e.Key == "NegZeroVec");
                Assert.IsNotNull(vEntry);
                Assert.AreEqual("(0, 0, 0)", vEntry.Value);

                string json = JsonUtility.ToJson(dto, true);
                Assert.IsFalse(json.Contains("-0.0"), "Serialized JSON should not contain -0.0.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 7. AutoVerify Mode

        [Test]
        public void AutoVerify_WhenEnabled_AutomaticallyOverwritesBaselineFile()
        {
            string testDir = Path.Combine(Application.temporaryCachePath, "AutoVerifyTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);

            string snapshotName = "AutoVerify_Snapshot";
            string verifiedPath = Path.Combine(testDir, $"{snapshotName}.verified.json");
            string receivedPath = Path.Combine(testDir, $"{snapshotName}.received.json");

            var go = new GameObject("AutoVerify_Target");

            try
            {
                // 1. Write an initial baseline file with outdated contents
                File.WriteAllText(verifiedPath, "{\n    \"Outdated\": true\n}");
                Assert.IsTrue(File.Exists(verifiedPath));

                // 2. Verify with AutoVerify = true
                var settings = new VerifySettings()
                    .UseDirectory(testDir)
                    .AutoVerify(true);

                string json = Snapshot.Verify(go, snapshotName, settings);

                // 3. Assert baseline was automatically updated to match new JSON
                Assert.IsTrue(File.Exists(verifiedPath));
                string updatedFileContent = File.ReadAllText(verifiedPath);
                Assert.IsTrue(updatedFileContent.Contains("AutoVerify_Target"));
                Assert.IsFalse(updatedFileContent.Contains("Outdated"));
                Assert.IsFalse(File.Exists(receivedPath), "Received file should not remain after auto-verification.");
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, true);
                }
            }
        }

        #endregion

        #region 8. Snapshot Mismatch Unified Diff Exception

        [Test]
        public void SnapshotMismatch_ThrowsSnapshotMismatchException_WithRichUnifiedDiff()
        {
            string testDir = Path.Combine(Application.temporaryCachePath, "MismatchDiffTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);

            string snapshotName = "Diff_Snapshot";
            string verifiedPath = Path.Combine(testDir, $"{snapshotName}.verified.json");
            string receivedPath = Path.Combine(testDir, $"{snapshotName}.received.json");

            var go = new GameObject("ActualObjectName");

            try
            {
                // Create an expected baseline file with a different name
                string fakeExpectedJson = "{\n    \"Name\": \"ExpectedObjectName\"\n}";
                File.WriteAllText(verifiedPath, fakeExpectedJson);

                var settings = new VerifySettings()
                    .UseDirectory(testDir)
                    .AutoVerify(false);

                var ex = Assert.Throws<SnapshotMismatchException>(() =>
                {
                    Snapshot.Verify(go, snapshotName, settings);
                });

                Assert.IsNotNull(ex);
                StringAssert.Contains("Snapshot mismatch for 'Diff_Snapshot'", ex.Message);
                StringAssert.Contains("--- ", ex.Message);
                StringAssert.Contains("+++ ", ex.Message);
                StringAssert.Contains("@@ -", ex.Message);
                StringAssert.Contains("- ", ex.Message);
                StringAssert.Contains("+ ", ex.Message);
                StringAssert.Contains("Received snapshot saved to:", ex.Message);
                StringAssert.Contains("Expected snapshot at:", ex.Message);

                Assert.IsTrue(File.Exists(receivedPath), "Received snapshot file must be created on disk upon mismatch.");
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, true);
                }
            }
        }

        #endregion

        #region 9. Rendering Snapshot Handlers

        [Test]
        public void SpriteRenderer_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("SpriteRenderer_Test");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 1f, 0f, 1f);
            sr.flipX = true;
            sr.flipY = false;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(SpriteRenderer));
                Assert.IsNotNull(compDto);

                Assert.AreEqual("#00FF00FF", compDto.StateEntries.Find(e => e.Key == "Color")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "FlipX")?.Value);
                Assert.AreEqual("false", compDto.StateEntries.Find(e => e.Key == "FlipY")?.Value);
                Assert.AreEqual(SpriteDrawMode.Simple.ToString(), compDto.StateEntries.Find(e => e.Key == "DrawMode")?.Value);
                Assert.AreEqual("Default", compDto.StateEntries.Find(e => e.Key == "SortingLayerName")?.Value);
                Assert.AreEqual("5", compDto.StateEntries.Find(e => e.Key == "SortingOrder")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Camera_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("Camera_Test");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5.0f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.depth = 1.5f;

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(Camera));
                Assert.IsNotNull(compDto);

                Assert.AreEqual(CameraClearFlags.SolidColor.ToString(), compDto.StateEntries.Find(e => e.Key == "ClearFlags")?.Value);
                Assert.AreEqual("#000000FF", compDto.StateEntries.Find(e => e.Key == "BackgroundColor")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "Orthographic")?.Value);
                Assert.AreEqual("5", compDto.StateEntries.Find(e => e.Key == "OrthographicSize")?.Value);
                Assert.AreEqual("0.3", compDto.StateEntries.Find(e => e.Key == "NearClipPlane")?.Value);
                Assert.AreEqual("1000", compDto.StateEntries.Find(e => e.Key == "FarClipPlane")?.Value);
                Assert.AreEqual("1.5", compDto.StateEntries.Find(e => e.Key == "Depth")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Light_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("Light_Test");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 2.5f;
            light.shadows = LightShadows.Soft;

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(Light));
                Assert.IsNotNull(compDto);

                Assert.AreEqual(LightType.Directional.ToString(), compDto.StateEntries.Find(e => e.Key == "Type")?.Value);
                Assert.AreEqual("#FFFFFFFF", compDto.StateEntries.Find(e => e.Key == "Color")?.Value);
                Assert.AreEqual("2.5", compDto.StateEntries.Find(e => e.Key == "Intensity")?.Value);
                Assert.AreEqual(LightShadows.Soft.ToString(), compDto.StateEntries.Find(e => e.Key == "Shadows")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 10. Physics Snapshot Handlers

        [Test]
        public void Rigidbody_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("Rigidbody_Test");
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 12.5f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.05f;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(Rigidbody));
                Assert.IsNotNull(compDto);

                Assert.AreEqual("12.5", compDto.StateEntries.Find(e => e.Key == "Mass")?.Value);
                Assert.AreEqual("0.5", compDto.StateEntries.Find(e => e.Key == "Drag")?.Value);
                Assert.AreEqual("0.05", compDto.StateEntries.Find(e => e.Key == "AngularDrag")?.Value);
                Assert.AreEqual("false", compDto.StateEntries.Find(e => e.Key == "UseGravity")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "IsKinematic")?.Value);
                Assert.AreEqual(RigidbodyInterpolation.Interpolate.ToString(), compDto.StateEntries.Find(e => e.Key == "Interpolation")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Collider_Extraction_ExtractsBoxAndSphereCorrectly()
        {
            var goBox = new GameObject("BoxCollider_Test");
            var box = goBox.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0, 1, 0);
            box.size = new Vector3(2, 4, 6);

            var goSphere = new GameObject("SphereCollider_Test");
            var sphere = goSphere.AddComponent<SphereCollider>();
            sphere.radius = 3.5f;
            sphere.center = new Vector3(1, 2, 3);

            try
            {
                var dtoBox = Snapshot.Extract(goBox);
                var compDtoBox = dtoBox.Components.Find(c => c.TypeName == nameof(BoxCollider));
                Assert.IsNotNull(compDtoBox);
                Assert.AreEqual("true", compDtoBox.StateEntries.Find(e => e.Key == "IsTrigger")?.Value);
                Assert.AreEqual("(0, 1, 0)", compDtoBox.StateEntries.Find(e => e.Key == "Center")?.Value);
                Assert.AreEqual("(2, 4, 6)", compDtoBox.StateEntries.Find(e => e.Key == "Size")?.Value);

                var dtoSphere = Snapshot.Extract(goSphere);
                var compDtoSphere = dtoSphere.Components.Find(c => c.TypeName == nameof(SphereCollider));
                Assert.IsNotNull(compDtoSphere);
                Assert.AreEqual("3.5", compDtoSphere.StateEntries.Find(e => e.Key == "Radius")?.Value);
                Assert.AreEqual("(1, 2, 3)", compDtoSphere.StateEntries.Find(e => e.Key == "Center")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(goBox);
                Object.DestroyImmediate(goSphere);
            }
        }

        [Test]
        public void Rigidbody2D_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("Rigidbody2D_Test");
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 3.0f;
            rb.gravityScale = 1.5f;
            rb.simulated = true;

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(Rigidbody2D));
                Assert.IsNotNull(compDto);

                Assert.AreEqual(RigidbodyType2D.Dynamic.ToString(), compDto.StateEntries.Find(e => e.Key == "BodyType")?.Value);
                Assert.AreEqual("3", compDto.StateEntries.Find(e => e.Key == "Mass")?.Value);
                Assert.AreEqual("1.5", compDto.StateEntries.Find(e => e.Key == "GravityScale")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "Simulated")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 11. Animation & Audio Snapshot Handlers

        [Test]
        public void AudioSource_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("AudioSource_Test");
            var audio = go.AddComponent<AudioSource>();
            audio.volume = 0.75f;
            audio.pitch = 1.2f;
            audio.loop = true;
            audio.playOnAwake = false;
            audio.mute = false;
            audio.spatialBlend = 1.0f;

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(AudioSource));
                Assert.IsNotNull(compDto);

                Assert.AreEqual("0.75", compDto.StateEntries.Find(e => e.Key == "Volume")?.Value);
                Assert.AreEqual("1.2", compDto.StateEntries.Find(e => e.Key == "Pitch")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "Loop")?.Value);
                Assert.AreEqual("false", compDto.StateEntries.Find(e => e.Key == "PlayOnAwake")?.Value);
                Assert.AreEqual("1", compDto.StateEntries.Find(e => e.Key == "SpatialBlend")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ParticleSystem_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("ParticleSystem_Test");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 4.0f;
            main.loop = false;
            main.startLifetime = 2.0f;
            main.startSpeed = 10.0f;

            try
            {
                var dto = Snapshot.Extract(go);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(ParticleSystem));
                Assert.IsNotNull(compDto);

                Assert.AreEqual("4", compDto.StateEntries.Find(e => e.Key == "Duration")?.Value);
                Assert.AreEqual("false", compDto.StateEntries.Find(e => e.Key == "Loop")?.Value);
                Assert.AreEqual("2", compDto.StateEntries.Find(e => e.Key == "StartLifetime")?.Value);
                Assert.AreEqual("10", compDto.StateEntries.Find(e => e.Key == "StartSpeed")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 12. Non-throwing Snapshot.Compare Tests

        [Test]
        public void SnapshotCompare_MatchingAndMismatching_ReturnsExpectedResultWithoutThrowing()
        {
            string tempDir = Path.Combine(Application.temporaryCachePath, "CompareTests_" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("CompareTestObject");
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.red;

            try
            {
                var settings = new VerifySettings().UseDirectory(tempDir);

                // 1. Initial baseline creation
                var initialResult = Snapshot.Compare(go, "CompareTestSnapshot", settings);
                Assert.IsTrue(initialResult.IsMatch);
                Assert.IsFalse(initialResult.WasAutoVerified);
                Assert.IsNull(initialResult.Diff);
                Assert.IsTrue(File.Exists(initialResult.VerifiedFilePath));

                // 2. Compare matching state
                var matchResult = Snapshot.Compare(go, "CompareTestSnapshot", settings);
                Assert.IsTrue(matchResult.IsMatch);
                Assert.IsNull(matchResult.Diff);
                Assert.AreEqual(initialResult.Expected, matchResult.Received);

                // 3. Mutate object state -> compare mismatching state
                img.color = Color.blue;
                var mismatchResult = Snapshot.Compare(go, "CompareTestSnapshot", settings);
                Assert.IsFalse(mismatchResult.IsMatch);
                Assert.IsNotNull(mismatchResult.Diff);
                Assert.IsTrue(mismatchResult.Diff.Contains("---"));
                Assert.IsTrue(mismatchResult.Diff.Contains("+++"));
                Assert.IsTrue(File.Exists(mismatchResult.ReceivedFilePath));
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        #endregion

        #region 13. Tilemap & Grid Snapshot Handlers

        [Test]
        public void TilemapAndGrid_Extraction_ExtractsPropertiesCorrectly()
        {
            var gridGo = new GameObject("Grid_Test");
            var grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(2f, 2f, 0f);
            grid.cellGap = new Vector3(0.1f, 0.2f, 0f);
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            var tilemapGo = new GameObject("Tilemap_Child");
            tilemapGo.transform.SetParent(gridGo.transform);
            var tilemap = tilemapGo.AddComponent<Tilemap>();
            tilemap.color = new Color(0.2f, 0.4f, 0.6f, 1.0f);
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

            var tilemapRenderer = tilemapGo.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingOrder = 3;
            tilemapRenderer.mode = TilemapRenderer.Mode.Chunk;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var gridDto = Snapshot.Extract(gridGo, settings);
                var gridCompDto = gridDto.Components.Find(c => c.TypeName == nameof(Grid));
                Assert.IsNotNull(gridCompDto);

                Assert.AreEqual("(2, 2, 0)", gridCompDto.StateEntries.Find(e => e.Key == "CellSize")?.Value);
                Assert.AreEqual("(0.1, 0.2, 0)", gridCompDto.StateEntries.Find(e => e.Key == "CellGap")?.Value);
                Assert.AreEqual(GridLayout.CellLayout.Rectangle.ToString(), gridCompDto.StateEntries.Find(e => e.Key == "CellLayout")?.Value);
                Assert.AreEqual(GridLayout.CellSwizzle.XYZ.ToString(), gridCompDto.StateEntries.Find(e => e.Key == "CellSwizzle")?.Value);

                var tilemapDto = gridDto.Children.Find(c => c.Name == "Tilemap_Child");
                Assert.IsNotNull(tilemapDto);

                var tilemapCompDto = tilemapDto.Components.Find(c => c.TypeName == nameof(Tilemap));
                Assert.IsNotNull(tilemapCompDto);
                Assert.AreEqual("#336699FF", tilemapCompDto.StateEntries.Find(e => e.Key == "Color")?.Value);
                Assert.AreEqual("(0.5, 0.5, 0)", tilemapCompDto.StateEntries.Find(e => e.Key == "TileAnchor")?.Value);

                var trCompDto = tilemapDto.Components.Find(c => c.TypeName == nameof(TilemapRenderer));
                Assert.IsNotNull(trCompDto);
                Assert.AreEqual("3", trCompDto.StateEntries.Find(e => e.Key == "SortingOrder")?.Value);
                Assert.AreEqual(TilemapRenderer.Mode.Chunk.ToString(), trCompDto.StateEntries.Find(e => e.Key == "Mode")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(gridGo);
            }
        }

        #endregion

        #region 14. Navigation Snapshot Handlers

        [Test]
        public void NavMeshAgent_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("NavMeshAgent_Test");
            var agent = go.AddComponent<NavMeshAgent>();
            agent.speed = 5.5f;
            agent.angularSpeed = 180f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.75f;
            agent.autoBraking = true;
            agent.radius = 0.4f;
            agent.height = 1.8f;
            agent.baseOffset = 0.1f;
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.areaMask = 1;
            agent.avoidancePriority = 25;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(NavMeshAgent));
                Assert.IsNotNull(compDto);

                Assert.AreEqual("5.5", compDto.StateEntries.Find(e => e.Key == "Speed")?.Value);
                Assert.AreEqual("180", compDto.StateEntries.Find(e => e.Key == "AngularSpeed")?.Value);
                Assert.AreEqual("12", compDto.StateEntries.Find(e => e.Key == "Acceleration")?.Value);
                Assert.AreEqual("0.75", compDto.StateEntries.Find(e => e.Key == "StoppingDistance")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "AutoBraking")?.Value);
                Assert.AreEqual("0.4", compDto.StateEntries.Find(e => e.Key == "Radius")?.Value);
                Assert.AreEqual("1.8", compDto.StateEntries.Find(e => e.Key == "Height")?.Value);
                Assert.AreEqual("0.1", compDto.StateEntries.Find(e => e.Key == "BaseOffset")?.Value);
                Assert.AreEqual("false", compDto.StateEntries.Find(e => e.Key == "IsStopped")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "UpdatePosition")?.Value);
                Assert.AreEqual("false", compDto.StateEntries.Find(e => e.Key == "UpdateRotation")?.Value);
                Assert.AreEqual("1", compDto.StateEntries.Find(e => e.Key == "AreaMask")?.Value);
                Assert.AreEqual("25", compDto.StateEntries.Find(e => e.Key == "AvoidancePriority")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

#pragma warning disable CS0618
        [Test]
        public void NavMeshObstacleAndOffMeshLink_Extraction_ExtractsPropertiesCorrectly()
        {
            var obstacleGo = new GameObject("NavObstacle_Test");
            var obstacle = obstacleGo.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = new Vector3(0, 1, 0);
            obstacle.size = new Vector3(2, 2, 2);
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;

            var linkGo = new GameObject("OffMeshLink_Test");
            var link = linkGo.AddComponent<OffMeshLink>();
            link.costOverride = 2.5f;
            link.biDirectional = true;
            link.activated = true;

            var startGo = new GameObject("LinkStart");
            var endGo = new GameObject("LinkEnd");
            link.startTransform = startGo.transform;
            link.endTransform = endGo.transform;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);

                var obstDto = Snapshot.Extract(obstacleGo, settings);
                var obstComp = obstDto.Components.Find(c => c.TypeName == nameof(NavMeshObstacle));
                Assert.IsNotNull(obstComp);
                Assert.AreEqual(NavMeshObstacleShape.Box.ToString(), obstComp.StateEntries.Find(e => e.Key == "Shape")?.Value);
                Assert.AreEqual("(0, 1, 0)", obstComp.StateEntries.Find(e => e.Key == "Center")?.Value);
                Assert.AreEqual("(2, 2, 2)", obstComp.StateEntries.Find(e => e.Key == "Size")?.Value);
                Assert.AreEqual("true", obstComp.StateEntries.Find(e => e.Key == "Carving")?.Value);
                Assert.AreEqual("false", obstComp.StateEntries.Find(e => e.Key == "CarveOnlyStationary")?.Value);

                var linkDto = Snapshot.Extract(linkGo, settings);
                var linkComp = linkDto.Components.Find(c => c.TypeName == nameof(OffMeshLink));
                Assert.IsNotNull(linkComp);
                Assert.AreEqual("2.5", linkComp.StateEntries.Find(e => e.Key == "CostOverride")?.Value);
                Assert.AreEqual("true", linkComp.StateEntries.Find(e => e.Key == "BiDirectional")?.Value);
                Assert.AreEqual("true", linkComp.StateEntries.Find(e => e.Key == "Activated")?.Value);
                Assert.AreEqual("LinkStart", linkComp.StateEntries.Find(e => e.Key == "StartTransform")?.Value);
                Assert.AreEqual("LinkEnd", linkComp.StateEntries.Find(e => e.Key == "EndTransform")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(obstacleGo);
                Object.DestroyImmediate(linkGo);
                Object.DestroyImmediate(startGo);
                Object.DestroyImmediate(endGo);
            }
        }
#pragma warning restore CS0618

        #endregion

        #region 15. Video Snapshot Handlers

        [Test]
        public void VideoPlayer_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("VideoPlayer_Test");
            var vp = go.AddComponent<VideoPlayer>();
            vp.url = "https://media.example.com/stream.mp4";
            vp.isLooping = true;
            vp.playOnAwake = false;
            vp.renderMode = VideoRenderMode.CameraNearPlane;
            vp.playbackSpeed = 1.25f;
            vp.aspectRatio = VideoAspectRatio.FitInside;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(VideoPlayer));
                Assert.IsNotNull(compDto);

                Assert.AreEqual("https://media.example.com/stream.mp4", compDto.StateEntries.Find(e => e.Key == "Url")?.Value);
                Assert.AreEqual("true", compDto.StateEntries.Find(e => e.Key == "IsLooping")?.Value);
                Assert.AreEqual("false", compDto.StateEntries.Find(e => e.Key == "PlayOnAwake")?.Value);
                Assert.AreEqual(VideoRenderMode.CameraNearPlane.ToString(), compDto.StateEntries.Find(e => e.Key == "RenderMode")?.Value);
                Assert.AreEqual("1.25", compDto.StateEntries.Find(e => e.Key == "PlaybackSpeed")?.Value);
                Assert.AreEqual(VideoAspectRatio.FitInside.ToString(), compDto.StateEntries.Find(e => e.Key == "AspectRatio")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 16. UI Toolkit UIDocument Snapshot Handler

        [Test]
        public void UIDocument_Extraction_ExtractsVisualElementHierarchyCorrectly()
        {
            var go = new GameObject("UIDocument_Test");
            var doc = go.AddComponent<UIDocument>();
            doc.sortingOrder = 10f;

            // Build an in-memory VisualElement hierarchy in rootVisualElement
            var root = doc.rootVisualElement;
            if (root != null)
            {
                var label = new Label("Health: 100");
                label.name = "HealthLabel";
                label.AddToClassList("hud-text");
                label.AddToClassList("bold");
                root.Add(label);

                var button = new UnityEngine.UIElements.Button { text = "Attack" };
                button.name = "AttackButton";
                root.Add(button);

                var toggle = new UnityEngine.UIElements.Toggle { value = true };
                toggle.name = "MuteToggle";
                root.Add(toggle);

                var textField = new TextField { value = "PlayerOne" };
                textField.name = "PlayerNameField";
                root.Add(textField);
            }

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);
                var compDto = dto.Components.Find(c => c.TypeName == nameof(UIDocument));
                Assert.IsNotNull(compDto);

                Assert.AreEqual("10", compDto.StateEntries.Find(e => e.Key == "SortingOrder")?.Value);

                var rootEntry = compDto.StateEntries.Find(e => e.Key == "RootVisualElement");
                Assert.IsNotNull(rootEntry);
                Assert.IsNotNull(rootEntry.Value);

                // Verify the tree string contains the children names, texts/values, classes
                Assert.IsTrue(rootEntry.Value.Contains("HealthLabel"));
                Assert.IsTrue(rootEntry.Value.Contains("Health: 100"));
                Assert.IsTrue(rootEntry.Value.Contains("hud-text"));
                Assert.IsTrue(rootEntry.Value.Contains("AttackButton"));
                Assert.IsTrue(rootEntry.Value.Contains("Attack"));
                Assert.IsTrue(rootEntry.Value.Contains("MuteToggle"));
                Assert.IsTrue(rootEntry.Value.Contains("PlayerNameField"));
                Assert.IsTrue(rootEntry.Value.Contains("PlayerOne"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 17. UI Controls (Toggle, Slider, Scrollbar, ScrollRect)

        [Test]
        public void UIControls_Extraction_ExtractsToggleSliderScrollbarScrollRect()
        {
            var toggleGo = new GameObject("Toggle_Test", typeof(RectTransform));
            var toggle = toggleGo.AddComponent<UnityEngine.UI.Toggle>();
            toggle.isOn = true;
            toggle.toggleTransition = UnityEngine.UI.Toggle.ToggleTransition.Fade;

            var sliderGo = new GameObject("Slider_Test", typeof(RectTransform));
            var slider = sliderGo.AddComponent<UnityEngine.UI.Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;

            var scrollbarGo = new GameObject("Scrollbar_Test", typeof(RectTransform));
            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.size = 0.2f;
            scrollbar.numberOfSteps = 5;

            var scrollRectGo = new GameObject("ScrollRect_Test", typeof(RectTransform));
            var scrollRect = scrollRectGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);

                var toggleDto = Snapshot.Extract(toggleGo, settings);
                var toggleComp = toggleDto.Components.Find(c => c.TypeName == nameof(UnityEngine.UI.Toggle));
                Assert.IsNotNull(toggleComp);
                Assert.AreEqual("true", toggleComp.StateEntries.Find(e => e.Key == "IsOn")?.Value);
                Assert.AreEqual(UnityEngine.UI.Toggle.ToggleTransition.Fade.ToString(), toggleComp.StateEntries.Find(e => e.Key == "ToggleTransition")?.Value);

                var sliderDto = Snapshot.Extract(sliderGo, settings);
                var sliderComp = sliderDto.Components.Find(c => c.TypeName == nameof(UnityEngine.UI.Slider));
                Assert.IsNotNull(sliderComp);
                Assert.AreEqual("100", sliderComp.StateEntries.Find(e => e.Key == "MaxValue")?.Value);

                var scrollbarDto = Snapshot.Extract(scrollbarGo, settings);
                var scrollbarComp = scrollbarDto.Components.Find(c => c.TypeName == nameof(Scrollbar));
                Assert.IsNotNull(scrollbarComp);
                Assert.AreEqual("0.2", scrollbarComp.StateEntries.Find(e => e.Key == "Size")?.Value);
                Assert.AreEqual("5", scrollbarComp.StateEntries.Find(e => e.Key == "NumberOfSteps")?.Value);

                var srDto = Snapshot.Extract(scrollRectGo, settings);
                var srComp = srDto.Components.Find(c => c.TypeName == nameof(ScrollRect));
                Assert.IsNotNull(srComp);
                Assert.AreEqual("true", srComp.StateEntries.Find(e => e.Key == "Horizontal")?.Value);
                Assert.AreEqual("false", srComp.StateEntries.Find(e => e.Key == "Vertical")?.Value);
                Assert.AreEqual(ScrollRect.MovementType.Clamped.ToString(), srComp.StateEntries.Find(e => e.Key == "MovementType")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(toggleGo);
                Object.DestroyImmediate(sliderGo);
                Object.DestroyImmediate(scrollbarGo);
                Object.DestroyImmediate(scrollRectGo);
            }
        }

        #endregion

        #region 18. TMP Controls (TMP_InputField, TMP_Dropdown)

        [Test]
        public void TMPControls_Extraction_ExtractsInputFieldAndDropdown()
        {
            var inputGo = new GameObject("TMP_Input_Test", typeof(RectTransform));
            var input = inputGo.AddComponent<TMP_InputField>();
            input.text = "Username123";
            input.characterLimit = 20;

            var dropGo = new GameObject("TMP_Drop_Test", typeof(RectTransform));
            var captionGo = new GameObject("Caption", typeof(RectTransform));
            captionGo.transform.SetParent(dropGo.transform);
            var captionTmp = captionGo.AddComponent<TextMeshProUGUI>();

            var dropdown = dropGo.AddComponent<TMP_Dropdown>();
            dropdown.captionText = captionTmp;
            dropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Easy"),
                new TMP_Dropdown.OptionData("Medium"),
                new TMP_Dropdown.OptionData("Hard")
            };
            dropdown.value = 1;

            try
            {
                var inputDto = Snapshot.Extract(inputGo);
                var inputComp = inputDto.Components.Find(c => c.TypeName == nameof(TMP_InputField));
                Assert.IsNotNull(inputComp);
                Assert.AreEqual("Username123", inputComp.StateEntries.Find(e => e.Key == "Text")?.Value);
                Assert.AreEqual("20", inputComp.StateEntries.Find(e => e.Key == "CharacterLimit")?.Value);

                var dropDto = Snapshot.Extract(dropGo);
                var dropComp = dropDto.Components.Find(c => c.TypeName == nameof(TMP_Dropdown));
                Assert.IsNotNull(dropComp);
                Assert.AreEqual("1", dropComp.StateEntries.Find(e => e.Key == "Value")?.Value);
                Assert.AreEqual("3", dropComp.StateEntries.Find(e => e.Key == "OptionsCount")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(inputGo);
                Object.DestroyImmediate(dropGo);
            }
        }

        #endregion

        #region 19. 3D Mesh Rendering (MeshFilter, MeshRenderer, SkinnedMeshRenderer)

        [Test]
        public void MeshRendering_Extraction_ExtractsMeshFilterAndMeshRenderer()
        {
            var go = new GameObject("Mesh_Test");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
            mr.receiveShadows = false;
            mr.sortingOrder = 2;

            try
            {
                var dto = Snapshot.Extract(go);

                var mfDto = dto.Components.Find(c => c.TypeName == nameof(MeshFilter));
                Assert.IsNotNull(mfDto);
                Assert.IsNotNull(mfDto.StateEntries.Find(e => e.Key == "SharedMesh"));

                var mrDto = dto.Components.Find(c => c.TypeName == nameof(MeshRenderer));
                Assert.IsNotNull(mrDto);
                Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.TwoSided.ToString(), mrDto.StateEntries.Find(e => e.Key == "ShadowCastingMode")?.Value);
                Assert.AreEqual("false", mrDto.StateEntries.Find(e => e.Key == "ReceiveShadows")?.Value);
                Assert.AreEqual("2", mrDto.StateEntries.Find(e => e.Key == "SortingOrder")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 20. Line and Trail Renderers

        [Test]
        public void LineAndTrailRenderers_Extraction_ExtractsPropertiesCorrectly()
        {
            var go = new GameObject("Renderers_Test");
            var lr = go.AddComponent<LineRenderer>();
            lr.startWidth = 0.1f;
            lr.endWidth = 0.5f;
            lr.startColor = Color.cyan;
            lr.endColor = Color.magenta;
            lr.loop = true;

            var tr = go.AddComponent<TrailRenderer>();
            tr.time = 2.5f;
            tr.startWidth = 0.2f;
            tr.endWidth = 0.0f;
            tr.autodestruct = false;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);

                var lrDto = dto.Components.Find(c => c.TypeName == nameof(LineRenderer));
                Assert.IsNotNull(lrDto);
                Assert.AreEqual("0.1", lrDto.StateEntries.Find(e => e.Key == "StartWidth")?.Value);
                Assert.AreEqual("0.5", lrDto.StateEntries.Find(e => e.Key == "EndWidth")?.Value);
                Assert.AreEqual("#00FFFFFF", lrDto.StateEntries.Find(e => e.Key == "StartColor")?.Value);
                Assert.AreEqual("#FF00FFFF", lrDto.StateEntries.Find(e => e.Key == "EndColor")?.Value);
                Assert.AreEqual("true", lrDto.StateEntries.Find(e => e.Key == "Loop")?.Value);

                var trDto = dto.Components.Find(c => c.TypeName == nameof(TrailRenderer));
                Assert.IsNotNull(trDto);
                Assert.AreEqual("2.5", trDto.StateEntries.Find(e => e.Key == "Time")?.Value);
                Assert.AreEqual("0.2", trDto.StateEntries.Find(e => e.Key == "StartWidth")?.Value);
                Assert.AreEqual("0", trDto.StateEntries.Find(e => e.Key == "EndWidth")?.Value);
                Assert.AreEqual("false", trDto.StateEntries.Find(e => e.Key == "Autodestruct")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 21. CharacterController, CapsuleCollider, MeshCollider

        [Test]
        public void CharacterController_And_Colliders_Extraction_ExtractsCorrectly()
        {
            var ccGo = new GameObject("CC_Test");
            var cc = ccGo.AddComponent<CharacterController>();
            cc.radius = 0.6f;
            cc.height = 2.2f;
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;
            cc.skinWidth = 0.08f;

            var capGo = new GameObject("Capsule_Test");
            var cap = capGo.AddComponent<CapsuleCollider>();
            cap.radius = 0.5f;
            cap.height = 1.8f;
            cap.direction = 1; // Y-axis

            var meshColGo = new GameObject("MeshCollider_Test");
            var meshCol = meshColGo.AddComponent<MeshCollider>();
            meshCol.convex = true;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);

                var ccDto = Snapshot.Extract(ccGo, settings);
                var ccComp = ccDto.Components.Find(c => c.TypeName == nameof(CharacterController));
                Assert.IsNotNull(ccComp);
                Assert.AreEqual("0.6", ccComp.StateEntries.Find(e => e.Key == "Radius")?.Value);
                Assert.AreEqual("2.2", ccComp.StateEntries.Find(e => e.Key == "Height")?.Value);
                Assert.AreEqual("45", ccComp.StateEntries.Find(e => e.Key == "SlopeLimit")?.Value);
                Assert.AreEqual("0.3", ccComp.StateEntries.Find(e => e.Key == "StepOffset")?.Value);

                var capDto = Snapshot.Extract(capGo, settings);
                var capComp = capDto.Components.Find(c => c.TypeName == nameof(CapsuleCollider));
                Assert.IsNotNull(capComp);
                Assert.AreEqual("0.5", capComp.StateEntries.Find(e => e.Key == "Radius")?.Value);
                Assert.AreEqual("1.8", capComp.StateEntries.Find(e => e.Key == "Height")?.Value);
                Assert.AreEqual("1", capComp.StateEntries.Find(e => e.Key == "Direction")?.Value);

                var meshDto = Snapshot.Extract(meshColGo, settings);
                var meshComp = meshDto.Components.Find(c => c.TypeName == nameof(MeshCollider));
                Assert.IsNotNull(meshComp);
                Assert.AreEqual("true", meshComp.StateEntries.Find(e => e.Key == "Convex")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(ccGo);
                Object.DestroyImmediate(capGo);
                Object.DestroyImmediate(meshColGo);
            }
        }

        #endregion

        #region 22. 2D Colliders (Box2D, Circle2D, Capsule2D, Polygon2D)

        [Test]
        public void Collider2D_Extraction_ExtractsBoxCircleCapsulePolygon2DCorrectly()
        {
            var go = new GameObject("Collider2D_Test");
            var box2D = go.AddComponent<BoxCollider2D>();
            box2D.size = new Vector2(3f, 4f);
            box2D.offset = new Vector2(0.5f, 0.5f);

            var circle2D = go.AddComponent<CircleCollider2D>();
            circle2D.radius = 1.5f;

            var cap2D = go.AddComponent<CapsuleCollider2D>();
            cap2D.size = new Vector2(1f, 3f);
            cap2D.direction = CapsuleDirection2D.Vertical;

            var poly2D = go.AddComponent<PolygonCollider2D>();
            poly2D.points = new[] { Vector2.zero, Vector2.up, Vector2.right };

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);

                var boxComp = dto.Components.Find(c => c.TypeName == nameof(BoxCollider2D));
                Assert.IsNotNull(boxComp);
                Assert.AreEqual("(3, 4)", boxComp.StateEntries.Find(e => e.Key == "Size")?.Value);
                Assert.AreEqual("(0.5, 0.5)", boxComp.StateEntries.Find(e => e.Key == "Offset")?.Value);

                var circleComp = dto.Components.Find(c => c.TypeName == nameof(CircleCollider2D));
                Assert.IsNotNull(circleComp);
                Assert.AreEqual("1.5", circleComp.StateEntries.Find(e => e.Key == "Radius")?.Value);

                var capComp = dto.Components.Find(c => c.TypeName == nameof(CapsuleCollider2D));
                Assert.IsNotNull(capComp);
                Assert.AreEqual("(1, 3)", capComp.StateEntries.Find(e => e.Key == "Size")?.Value);
                Assert.AreEqual(CapsuleDirection2D.Vertical.ToString(), capComp.StateEntries.Find(e => e.Key == "Direction")?.Value);

                var polyComp = dto.Components.Find(c => c.TypeName == nameof(PolygonCollider2D));
                Assert.IsNotNull(polyComp);
                Assert.AreEqual("3", polyComp.StateEntries.Find(e => e.Key == "PointsCount")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 23. Legacy Animation and AudioListener

        [Test]
        public void AnimationAndAudioListener_Extraction_ExtractsCorrectly()
        {
            var go = new GameObject("AnimAudio_Test");
            var anim = go.AddComponent<Animation>();
            anim.playAutomatically = true;
            anim.wrapMode = WrapMode.Loop;
            anim.animatePhysics = false;

            var listener = go.AddComponent<AudioListener>();
            AudioListener.volume = 0.9f;
            AudioListener.pause = false;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);

                var animDto = dto.Components.Find(c => c.TypeName == nameof(Animation));
                Assert.IsNotNull(animDto);
                Assert.AreEqual("true", animDto.StateEntries.Find(e => e.Key == "PlayAutomatically")?.Value);
                Assert.AreEqual(WrapMode.Loop.ToString(), animDto.StateEntries.Find(e => e.Key == "WrapMode")?.Value);
                Assert.AreEqual("false", animDto.StateEntries.Find(e => e.Key == "AnimatePhysics")?.Value);

                var listenerDto = dto.Components.Find(c => c.TypeName == nameof(AudioListener));
                Assert.IsNotNull(listenerDto);
                Assert.AreEqual("0.9", listenerDto.StateEntries.Find(e => e.Key == "Volume")?.Value);
                Assert.AreEqual("false", listenerDto.StateEntries.Find(e => e.Key == "Pause")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 24. Layout Fitters and RawImage

        [Test]
        public void LayoutFittersAndRawImage_Extraction_ExtractsCorrectly()
        {
            var go = new GameObject("LayoutAndRaw_Test", typeof(RectTransform));
            var rawImg = go.AddComponent<RawImage>();
            rawImg.color = Color.yellow;
            rawImg.uvRect = new Rect(0, 0, 1, 1);

            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var arf = go.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.aspectRatio = 1.777f;

            try
            {
                var settings = new VerifySettings().WithFloatPrecision(2);
                var dto = Snapshot.Extract(go, settings);

                var rawComp = dto.Components.Find(c => c.TypeName == nameof(RawImage));
                Assert.IsNotNull(rawComp);
                Assert.AreEqual(SnapshotScrubber.ScrubColor(Color.yellow), rawComp.StateEntries.Find(e => e.Key == "Color")?.Value);

                var csfDto = dto.Components.Find(c => c.TypeName == nameof(ContentSizeFitter));
                Assert.IsNotNull(csfDto);
                Assert.AreEqual(ContentSizeFitter.FitMode.PreferredSize.ToString(), csfDto.StateEntries.Find(e => e.Key == "HorizontalFit")?.Value);
                Assert.AreEqual(ContentSizeFitter.FitMode.Unconstrained.ToString(), csfDto.StateEntries.Find(e => e.Key == "VerticalFit")?.Value);

                var arfDto = dto.Components.Find(c => c.TypeName == nameof(AspectRatioFitter));
                Assert.IsNotNull(arfDto);
                Assert.AreEqual(AspectRatioFitter.AspectMode.FitInParent.ToString(), arfDto.StateEntries.Find(e => e.Key == "AspectMode")?.Value);
                Assert.AreEqual("1.78", arfDto.StateEntries.Find(e => e.Key == "AspectRatio")?.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region 25. Negative & Mismatch Detection Tests

        [Test]
        public void HierarchyMutation_ChildAddedOrRemoved_TriggersMismatch()
        {
            string tempDir = Path.Combine(Application.temporaryCachePath, "HierarchyMutation_" + Guid.NewGuid().ToString("N"));
            var parentGo = new GameObject("Parent");
            var childA = new GameObject("ChildA");
            childA.transform.SetParent(parentGo.transform);

            try
            {
                var settings = new VerifySettings().UseDirectory(tempDir);

                // 1. Create baseline with Parent containing only ChildA
                var baselineResult = Snapshot.Compare(parentGo, "HierarchySnapshot", settings);
                Assert.IsTrue(baselineResult.IsMatch);

                // 2. Add ChildB -> Verification must detect mismatch
                var childB = new GameObject("ChildB");
                childB.transform.SetParent(parentGo.transform);

                var addResult = Snapshot.Compare(parentGo, "HierarchySnapshot", settings);
                Assert.IsFalse(addResult.IsMatch, "Adding a child GameObject must trigger a snapshot mismatch.");
                Assert.IsNotNull(addResult.Diff);
                StringAssert.Contains("ChildB", addResult.Diff);

                // 3. Remove ChildA -> Verification must detect missing child
                Object.DestroyImmediate(childA);
                var removeResult = Snapshot.Compare(parentGo, "HierarchySnapshot", settings);
                Assert.IsFalse(removeResult.IsMatch, "Removing a child GameObject must trigger a snapshot mismatch.");
                Assert.IsNotNull(removeResult.Diff);
            }
            finally
            {
                Object.DestroyImmediate(parentGo);
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Test]
        public void ComponentMutation_ComponentAddedOrRemoved_TriggersMismatch()
        {
            string tempDir = Path.Combine(Application.temporaryCachePath, "ComponentMutation_" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("Entity");
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.white;

            try
            {
                var settings = new VerifySettings().UseDirectory(tempDir);

                // 1. Create baseline with only Image
                var baseline = Snapshot.Compare(go, "EntitySnapshot", settings);
                Assert.IsTrue(baseline.IsMatch);

                // 2. Add AudioSource -> Must fail
                var audio = go.AddComponent<AudioSource>();
                audio.volume = 0.5f;

                var addCompResult = Snapshot.Compare(go, "EntitySnapshot", settings);
                Assert.IsFalse(addCompResult.IsMatch, "Adding a component must trigger a snapshot mismatch.");
                StringAssert.Contains("AudioSource", addCompResult.Diff);

                // 3. Remove Image -> Must fail
                Object.DestroyImmediate(img);
                var removeCompResult = Snapshot.Compare(go, "EntitySnapshot", settings);
                Assert.IsFalse(removeCompResult.IsMatch, "Removing a component must trigger a snapshot mismatch.");
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Test]
        public void ActiveAndEnabledStateMutation_TriggersMismatch()
        {
            string tempDir = Path.Combine(Application.temporaryCachePath, "ActiveEnabledMutation_" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("ToggleTarget");
            var light = go.AddComponent<Light>();
            light.enabled = true;

            var child = new GameObject("ChildObject");
            child.transform.SetParent(go.transform);
            child.SetActive(true);

            try
            {
                var settings = new VerifySettings().UseDirectory(tempDir);

                // 1. Create baseline
                var baseline = Snapshot.Compare(go, "ActiveEnabledSnapshot", settings);
                Assert.IsTrue(baseline.IsMatch);

                // 2. Disable component -> Must fail
                light.enabled = false;
                var compDisabledResult = Snapshot.Compare(go, "ActiveEnabledSnapshot", settings);
                Assert.IsFalse(compDisabledResult.IsMatch, "Disabling a component must trigger a snapshot mismatch.");
                StringAssert.Contains("Enabled", compDisabledResult.Diff);

                light.enabled = true; // Restore

                // 3. Deactivate child GameObject -> Must fail
                child.SetActive(false);
                var childInactiveResult = Snapshot.Compare(go, "ActiveEnabledSnapshot", settings);
                Assert.IsFalse(childInactiveResult.IsMatch, "Deactivating a child GameObject must trigger a snapshot mismatch.");
                StringAssert.Contains("ActiveSelf", childInactiveResult.Diff);
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Test]
        public void FloatPrecisionBoundary_SubPrecisionTolerated_BeyondPrecisionTriggersMismatch()
        {
            string tempDir = Path.Combine(Application.temporaryCachePath, "PrecisionBoundary_" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("PrecisionTarget");
            var light = go.AddComponent<Light>();
            light.intensity = 10.251f;

            try
            {
                // Precision 2 (e.g. 10.25)
                var settings = new VerifySettings().WithFloatPrecision(2).UseDirectory(tempDir);

                // 1. Create baseline at 10.251f (rounded to 10.25)
                var baseline = Snapshot.Compare(go, "PrecisionSnapshot", settings);
                Assert.IsTrue(baseline.IsMatch);

                // 2. Small sub-precision change: 10.254f still rounds to 10.25 -> Should PASS
                light.intensity = 10.254f;
                var subPrecisionResult = Snapshot.Compare(go, "PrecisionSnapshot", settings);
                Assert.IsTrue(subPrecisionResult.IsMatch, "Sub-precision float differences must be tolerated by the configured precision.");

                // 3. Significant change beyond precision: 10.261f rounds to 10.26 -> Should FAIL
                light.intensity = 10.261f;
                var beyondPrecisionResult = Snapshot.Compare(go, "PrecisionSnapshot", settings);
                Assert.IsFalse(beyondPrecisionResult.IsMatch, "Float changes beyond precision threshold must trigger a mismatch.");
                StringAssert.Contains("Intensity", beyondPrecisionResult.Diff);
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Test]
        public void TransformShift_PositionRotationScaleChanges_TriggersMismatch()
        {
            string tempDir = Path.Combine(Application.temporaryCachePath, "TransformShift_" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("TransformTarget");
            go.transform.localPosition = new Vector3(1f, 2f, 3f);
            go.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
            go.transform.localScale = Vector3.one;

            try
            {
                var settings = new VerifySettings().UseDirectory(tempDir);

                // 1. Create baseline
                var baseline = Snapshot.Compare(go, "TransformSnapshot", settings);
                Assert.IsTrue(baseline.IsMatch);

                // 2. Shift position -> Must fail
                go.transform.localPosition = new Vector3(1f, 5f, 3f);
                var posResult = Snapshot.Compare(go, "TransformSnapshot", settings);
                Assert.IsFalse(posResult.IsMatch, "Position shift must trigger a snapshot mismatch.");
                StringAssert.Contains("LocalPosition", posResult.Diff);

                go.transform.localPosition = new Vector3(1f, 2f, 3f); // Restore

                // 3. Shift rotation -> Must fail
                go.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                var rotResult = Snapshot.Compare(go, "TransformSnapshot", settings);
                Assert.IsFalse(rotResult.IsMatch, "Rotation change must trigger a snapshot mismatch.");
                StringAssert.Contains("LocalEulerAngles", rotResult.Diff);

                go.transform.localEulerAngles = new Vector3(0f, 45f, 0f); // Restore

                // 4. Shift scale -> Must fail
                go.transform.localScale = new Vector3(2f, 2f, 2f);
                var scaleResult = Snapshot.Compare(go, "TransformSnapshot", settings);
                Assert.IsFalse(scaleResult.IsMatch, "Scale change must trigger a snapshot mismatch.");
                StringAssert.Contains("LocalScale", scaleResult.Diff);
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Test]
        public void RectTransformMutation_AnchorsOrPivotChanges_TriggersMismatch()
        {
            string tempDir = Path.Combine(Application.temporaryCachePath, "RectTransformMutation_" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("UIRectTarget", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(100, 50);

            try
            {
                var settings = new VerifySettings().UseDirectory(tempDir);

                // 1. Create baseline
                var baseline = Snapshot.Compare(go, "RectSnapshot", settings);
                Assert.IsTrue(baseline.IsMatch);

                // 2. Change AnchorMax -> Must fail
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                var anchorResult = Snapshot.Compare(go, "RectSnapshot", settings);
                Assert.IsFalse(anchorResult.IsMatch, "Anchor modification must trigger a snapshot mismatch.");
                StringAssert.Contains("AnchorMax", anchorResult.Diff);

                rt.anchorMax = Vector2.one; // Restore

                // 3. Change Pivot -> Must fail
                rt.pivot = Vector2.zero;
                var pivotResult = Snapshot.Compare(go, "RectSnapshot", settings);
                Assert.IsFalse(pivotResult.IsMatch, "Pivot modification must trigger a snapshot mismatch.");
                StringAssert.Contains("Pivot", pivotResult.Diff);

                rt.pivot = new Vector2(0.5f, 0.5f); // Restore

                // 4. Change SizeDelta -> Must fail
                rt.sizeDelta = new Vector2(200, 50);
                var sizeResult = Snapshot.Compare(go, "RectSnapshot", settings);
                Assert.IsFalse(sizeResult.IsMatch, "SizeDelta modification must trigger a snapshot mismatch.");
                StringAssert.Contains("SizeDelta", sizeResult.Diff);
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        #endregion
    }
}

