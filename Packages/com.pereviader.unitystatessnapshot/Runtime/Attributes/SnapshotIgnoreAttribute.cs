using System;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Attribute used to ignore fields or properties when extracting state snapshots of Unity components.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class SnapshotIgnoreAttribute : Attribute
    {
    }
}
