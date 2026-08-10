using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Core verification and comparison engine for Unity state snapshots. Performs snapshot comparison,
    /// auto-verification, received snapshot persistence, and unified diff reporting.
    /// </summary>
    public static class SnapshotVerifier
    {
        #region Compare Overloads

        public static SnapshotComparisonResult Compare(
            GameObject target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            settings = ResolveSettings(settings);
            var extractor = new SnapshotExtractor(settings);
            GameObjectSnapshotDTO dto = extractor.Extract(target);

            snapshotName = ResolveSnapshotName(snapshotName, settings, memberName, target.name);
            snapshotDirectory = ResolveSnapshotDirectory(snapshotDirectory, settings, sourceFile);

            string json = JsonUtility.ToJson(dto, true);
            return CompareCore(json, snapshotName, snapshotDirectory, settings);
        }

        public static SnapshotComparisonResult Compare(
            Component target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            settings = ResolveSettings(settings);
            var extractor = new SnapshotExtractor(settings);
            ComponentSnapshotDTO compDto = extractor.ExtractComponent(target);

            var dto = new GameObjectSnapshotDTO
            {
                Name = SnapshotScrubber.ScrubName(target.name),
                ActiveSelf = target.gameObject.activeSelf,
                ActiveInHierarchy = target.gameObject.activeInHierarchy,
                Tag = target.tag,
                Layer = LayerMask.LayerToName(target.gameObject.layer),
                Components = new List<ComponentSnapshotDTO> { compDto }
            };

            snapshotName = ResolveSnapshotName(snapshotName, settings, memberName, target.GetType().Name);
            snapshotDirectory = ResolveSnapshotDirectory(snapshotDirectory, settings, sourceFile);

            string json = JsonUtility.ToJson(dto, true);
            return CompareCore(json, snapshotName, snapshotDirectory, settings);
        }

        public static SnapshotComparisonResult Compare(
            Scene scene,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            settings = ResolveSettings(settings);
            var extractor = new SnapshotExtractor(settings);
            List<GameObjectSnapshotDTO> dtos = extractor.Extract(scene);

            snapshotName = ResolveSnapshotName(snapshotName, settings, memberName, string.IsNullOrEmpty(scene.name) ? "Scene" : scene.name);
            snapshotDirectory = ResolveSnapshotDirectory(snapshotDirectory, settings, sourceFile);

            var containerDto = new GameObjectSnapshotDTO
            {
                Name = $"Scene_{scene.name}",
                ActiveSelf = true,
                ActiveInHierarchy = true,
                Tag = "Scene",
                Layer = "Scene",
                Children = dtos
            };

            string json = JsonUtility.ToJson(containerDto, true);
            return CompareCore(json, snapshotName, snapshotDirectory, settings);
        }

        public static SnapshotComparisonResult Compare(
            IEnumerable<GameObject> targets,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            settings = ResolveSettings(settings);
            var extractor = new SnapshotExtractor(settings);
            List<GameObjectSnapshotDTO> dtos = extractor.Extract(targets);

            snapshotName = ResolveSnapshotName(snapshotName, settings, memberName, "GameObjects");
            snapshotDirectory = ResolveSnapshotDirectory(snapshotDirectory, settings, sourceFile);

            var containerDto = new GameObjectSnapshotDTO
            {
                Name = "Collection",
                ActiveSelf = true,
                ActiveInHierarchy = true,
                Tag = "Untagged",
                Layer = "Default",
                Children = dtos
            };

            string json = JsonUtility.ToJson(containerDto, true);
            return CompareCore(json, snapshotName, snapshotDirectory, settings);
        }

        public static SnapshotComparisonResult Compare(
            GameObjectSnapshotDTO dto,
            string snapshotName = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            VerifySettings settings = VerifySettings.Global;
            snapshotName = ResolveSnapshotName(snapshotName, settings, memberName, dto.Name ?? "Snapshot");
            snapshotDirectory = ResolveSnapshotDirectory(snapshotDirectory, settings, sourceFile);

            string json = JsonUtility.ToJson(dto, true);
            return CompareCore(json, snapshotName, snapshotDirectory, settings);
        }

        public static SnapshotComparisonResult Compare(
            object target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (target is GameObject go)
                return Compare(go, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            if (target is Component comp)
                return Compare(comp, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            if (target is Scene scene)
                return Compare(scene, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            if (target is IEnumerable<GameObject> targets)
                return Compare(targets, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            if (target is GameObjectSnapshotDTO dto)
                return Compare(dto, snapshotName, snapshotDirectory, sourceFile, memberName);

            settings = ResolveSettings(settings);
            snapshotName = ResolveSnapshotName(snapshotName, settings, memberName, target.GetType().Name);
            snapshotDirectory = ResolveSnapshotDirectory(snapshotDirectory, settings, sourceFile);

            string json = JsonUtility.ToJson(target, true);
            return CompareCore(json, snapshotName, snapshotDirectory, settings);
        }

        #endregion

        #region Verify Overloads

        public static string Verify(
            GameObject target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            var result = Compare(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            return VerifyCore(result);
        }

        public static string Verify(
            Component target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            var result = Compare(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            return VerifyCore(result);
        }

        public static string Verify(
            Scene scene,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            var result = Compare(scene, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            return VerifyCore(result);
        }

        public static string Verify(
            IEnumerable<GameObject> targets,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            var result = Compare(targets, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            return VerifyCore(result);
        }

        public static string Verify(
            GameObjectSnapshotDTO dto,
            string snapshotName,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            var result = Compare(dto, snapshotName, snapshotDirectory, sourceFile, memberName);
            return VerifyCore(result);
        }

        public static string Verify(
            object target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            var result = Compare(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
            return VerifyCore(result);
        }

        #endregion

        #region Backward Compatibility Overloads for SnapshotOptions

        public static string Verify(
            GameObject target,
            string snapshotName,
            SnapshotOptions options,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            VerifySettings settings = options != null ? VerifySettings.FromSnapshotOptions(options) : VerifySettings.Global;
            return Verify(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static SnapshotComparisonResult Compare(
            GameObject target,
            string snapshotName,
            SnapshotOptions options,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            VerifySettings settings = options != null ? VerifySettings.FromSnapshotOptions(options) : VerifySettings.Global;
            return Compare(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        #endregion

        #region Core Comparison & Verification

        public static SnapshotComparisonResult CompareCore(
            string json,
            string snapshotName,
            string snapshotDirectory,
            VerifySettings settings)
        {
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));

            json = StandardizeJsonFormatting(json);

            if (!Directory.Exists(snapshotDirectory))
            {
                Directory.CreateDirectory(snapshotDirectory);
            }

            string verifiedFilePath = Path.Combine(snapshotDirectory, $"{snapshotName}.verified.json");
            string receivedFilePath = Path.Combine(snapshotDirectory, $"{snapshotName}.received.json");

            if (!File.Exists(verifiedFilePath))
            {
                File.WriteAllText(verifiedFilePath, json);
                Debug.Log($"[SnapshotVerifier] Created new verified snapshot file: {verifiedFilePath}");
                return new SnapshotComparisonResult
                {
                    IsMatch = true,
                    SnapshotName = snapshotName,
                    Expected = json,
                    Received = json,
                    Diff = null,
                    VerifiedFilePath = verifiedFilePath,
                    ReceivedFilePath = receivedFilePath,
                    WasAutoVerified = false
                };
            }

            string expectedJson = File.ReadAllText(verifiedFilePath);
            expectedJson = StandardizeJsonFormatting(expectedJson);

            bool isMatch = string.Equals(json, expectedJson, StringComparison.Ordinal);

            if (!isMatch)
            {
                if (settings != null && settings.IsAutoVerify)
                {
                    File.WriteAllText(verifiedFilePath, json);
                    if (File.Exists(receivedFilePath))
                    {
                        try { File.Delete(receivedFilePath); } catch { }
                    }
                    Debug.Log($"[SnapshotVerifier] Auto-verified and updated snapshot file: {verifiedFilePath}");
                    return new SnapshotComparisonResult
                    {
                        IsMatch = true,
                        SnapshotName = snapshotName,
                        Expected = expectedJson,
                        Received = json,
                        Diff = null,
                        VerifiedFilePath = verifiedFilePath,
                        ReceivedFilePath = receivedFilePath,
                        WasAutoVerified = true
                    };
                }

                File.WriteAllText(receivedFilePath, json);
                string diff = DiffUtility.CreateUnifiedDiff(expectedJson, json, verifiedFilePath, receivedFilePath);

                return new SnapshotComparisonResult
                {
                    IsMatch = false,
                    SnapshotName = snapshotName,
                    Expected = expectedJson,
                    Received = json,
                    Diff = diff,
                    VerifiedFilePath = verifiedFilePath,
                    ReceivedFilePath = receivedFilePath,
                    WasAutoVerified = false
                };
            }

            if (File.Exists(receivedFilePath))
            {
                try
                {
                    File.Delete(receivedFilePath);
                }
                catch
                {
                    // Ignore deletion failures if locked
                }
            }

            return new SnapshotComparisonResult
            {
                IsMatch = true,
                SnapshotName = snapshotName,
                Expected = expectedJson,
                Received = json,
                Diff = null,
                VerifiedFilePath = verifiedFilePath,
                ReceivedFilePath = receivedFilePath,
                WasAutoVerified = false
            };
        }

        public static string VerifyCore(SnapshotComparisonResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (!result.IsMatch && !result.WasAutoVerified)
            {
                throw new SnapshotMismatchException(
                    $"Snapshot mismatch for '{result.SnapshotName}'.\n\n{result.Diff}\nReceived snapshot saved to: {result.ReceivedFilePath}\nExpected snapshot at: {result.VerifiedFilePath}");
            }

            return result.Received;
        }

        #endregion

        #region Formatting & Resolution Helpers

        private static string StandardizeJsonFormatting(string rawJson)
        {
            if (string.IsNullOrEmpty(rawJson)) return string.Empty;
            return rawJson.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        }

        private static VerifySettings ResolveSettings(VerifySettings settings)
        {
            return settings ?? VerifySettings.Global;
        }

        private static string ResolveSnapshotName(string snapshotName, VerifySettings settings, string memberName, string fallback)
        {
            if (!string.IsNullOrEmpty(snapshotName)) return snapshotName;
            if (!string.IsNullOrEmpty(settings?.FileName)) return settings.FileName;
            if (!string.IsNullOrEmpty(memberName)) return memberName;
            return string.IsNullOrEmpty(fallback) ? "Snapshot" : fallback;
        }

        private static string ResolveSnapshotDirectory(string snapshotDirectory, VerifySettings settings, string sourceFile)
        {
            if (!string.IsNullOrEmpty(snapshotDirectory)) return snapshotDirectory;
            if (!string.IsNullOrEmpty(settings?.Directory)) return settings.Directory;

            if (!string.IsNullOrEmpty(sourceFile))
            {
                string sourceDir = Path.GetDirectoryName(sourceFile);
                if (!string.IsNullOrEmpty(sourceDir))
                {
                    string candidate = Path.Combine(sourceDir, "Snapshots~");
                    if (Directory.Exists(candidate)) return candidate;

                    string candidate2 = Path.Combine(sourceDir, "Snapshots");
                    if (Directory.Exists(candidate2)) return candidate2;

                    return candidate;
                }
            }

            return Path.Combine(Application.dataPath, "MockApp", "Tests", "Snapshots~");
        }

        #endregion
    }

    /// <summary>
    /// Exception thrown when a state snapshot does not match the verified baseline.
    /// </summary>
    public class SnapshotMismatchException : Exception
    {
        public SnapshotMismatchException(string message) : base(message) { }
        public SnapshotMismatchException(string message, Exception innerException) : base(message, innerException) { }
    }
}
