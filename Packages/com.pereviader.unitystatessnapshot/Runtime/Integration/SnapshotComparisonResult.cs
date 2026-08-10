namespace PereViader.UnityStateSnapshot
{
    public class SnapshotComparisonResult
    {
        public bool IsMatch { get; set; }
        public string SnapshotName { get; set; }
        public string Expected { get; set; }
        public string Received { get; set; }
        public string Diff { get; set; }
        public string VerifiedFilePath { get; set; }
        public string ReceivedFilePath { get; set; }
        public bool WasAutoVerified { get; set; }
    }
}
