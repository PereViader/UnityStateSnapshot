using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Static entry point for extracting, comparing, and verifying state snapshots of Unity objects.
    /// </summary>
    public static class Snapshot
    {
        #region Extraction

        public static GameObjectSnapshotDTO Extract(GameObject target, VerifySettings settings = null)
        {
            return new SnapshotExtractor(settings).Extract(target);
        }

        public static GameObjectSnapshotDTO Extract(GameObject target, SnapshotOptions options)
        {
            return new SnapshotExtractor(options).Extract(target);
        }

        public static ComponentSnapshotDTO Extract(Component target, VerifySettings settings = null)
        {
            return new SnapshotExtractor(settings).ExtractComponent(target);
        }

        public static List<GameObjectSnapshotDTO> Extract(IEnumerable<GameObject> targets, VerifySettings settings = null)
        {
            return new SnapshotExtractor(settings).Extract(targets);
        }

        public static List<GameObjectSnapshotDTO> Extract(IEnumerable<GameObject> targets, SnapshotOptions options)
        {
            return new SnapshotExtractor(options).Extract(targets);
        }

        public static List<GameObjectSnapshotDTO> Extract(Scene scene, VerifySettings settings = null)
        {
            return new SnapshotExtractor(settings).Extract(scene);
        }

        #endregion

        #region Comparison

        public static SnapshotComparisonResult Compare(
            GameObject target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Compare(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static SnapshotComparisonResult Compare(
            GameObject target,
            string snapshotName,
            SnapshotOptions options,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Compare(target, snapshotName, options, snapshotDirectory, sourceFile, memberName);
        }

        public static SnapshotComparisonResult Compare(
            Component target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Compare(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static SnapshotComparisonResult Compare(
            Scene scene,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Compare(scene, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static SnapshotComparisonResult Compare(
            IEnumerable<GameObject> targets,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Compare(targets, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static SnapshotComparisonResult Compare(
            GameObjectSnapshotDTO dto,
            string snapshotName = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Compare(dto, snapshotName, snapshotDirectory, sourceFile, memberName);
        }

        public static SnapshotComparisonResult Compare(
            object target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Compare(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        #endregion

        #region Verification

        public static string Verify(
            GameObject target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Verify(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static string Verify(
            GameObject target,
            string snapshotName,
            SnapshotOptions options,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Verify(target, snapshotName, options, snapshotDirectory, sourceFile, memberName);
        }

        public static string Verify(
            Component target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Verify(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static string Verify(
            Scene scene,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Verify(scene, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static string Verify(
            IEnumerable<GameObject> targets,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Verify(targets, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        public static string Verify(
            GameObjectSnapshotDTO dto,
            string snapshotName,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Verify(dto, snapshotName, snapshotDirectory, sourceFile, memberName);
        }

        public static string Verify(
            object target,
            string snapshotName = null,
            VerifySettings settings = null,
            string snapshotDirectory = null,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string memberName = "")
        {
            return SnapshotVerifier.Verify(target, snapshotName, settings, snapshotDirectory, sourceFile, memberName);
        }

        #endregion
    }
}
