using System;
using System.Collections.Generic;
using System.Text;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Utility for computing line-by-line unified diffs between expected and received snapshots.
    /// </summary>
    public static class DiffUtility
    {
        public static string CreateUnifiedDiff(string expected, string received, string expectedLabel = "Expected (Verified)", string receivedLabel = "Received", int contextLines = 3)
        {
            if (expected == null) expected = string.Empty;
            if (received == null) received = string.Empty;

            string[] expectedLines = SplitLines(expected);
            string[] receivedLines = SplitLines(received);

            var sb = new StringBuilder();
            sb.AppendLine($"--- {expectedLabel}");
            sb.AppendLine($"+++ {receivedLabel}");

            int maxLines = Math.Max(expectedLines.Length, receivedLines.Length);
            int firstDiff = -1;
            int lastDiff = -1;

            for (int i = 0; i < maxLines; i++)
            {
                string exp = i < expectedLines.Length ? expectedLines[i] : null;
                string rec = i < receivedLines.Length ? receivedLines[i] : null;

                if (!string.Equals(exp, rec, StringComparison.Ordinal))
                {
                    if (firstDiff == -1) firstDiff = i;
                    lastDiff = i;
                }
            }

            if (firstDiff == -1)
            {
                return "No differences found.";
            }

            int start = Math.Max(0, firstDiff - contextLines);
            int end = Math.Min(maxLines - 1, lastDiff + contextLines);

            sb.AppendLine($"@@ -{start + 1},{end - start + 1} +{start + 1},{end - start + 1} @@");

            for (int i = start; i <= end; i++)
            {
                string exp = i < expectedLines.Length ? expectedLines[i] : null;
                string rec = i < receivedLines.Length ? receivedLines[i] : null;

                if (string.Equals(exp, rec, StringComparison.Ordinal))
                {
                    sb.AppendLine($"  {exp}");
                }
                else
                {
                    if (exp != null)
                    {
                        sb.AppendLine($"- {exp}");
                    }
                    if (rec != null)
                    {
                        sb.AppendLine($"+ {rec}");
                    }
                }
            }

            return sb.ToString();
        }

        private static string[] SplitLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return Array.Empty<string>();
            return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }
    }
}
