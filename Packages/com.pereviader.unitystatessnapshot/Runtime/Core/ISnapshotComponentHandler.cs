using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Defines a handler for custom extraction of a Unity Component's state into a target state dictionary.
    /// Handlers with higher Priority values are evaluated first.
    /// </summary>
    public interface ISnapshotComponentHandler
    {
        /// <summary>
        /// Execution priority. Higher values take precedence over lower values.
        /// Built-in handlers typically use priorities between 10 and 100.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Determines whether this handler can process the specified component type.
        /// </summary>
        /// <param name="componentType">The Runtime type of the component.</param>
        /// <returns>True if this handler can process the component; otherwise, false.</returns>
        bool CanHandle(Type componentType);

        /// <summary>
        /// Extracts state from the given component and populates the target state dictionary.
        /// </summary>
        /// <param name="component">The component instance to extract from.</param>
        /// <param name="targetState">The dictionary to receive extracted property and state key-value pairs.</param>
        /// <param name="settings">The active snapshot verification settings.</param>
        void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings);
    }
}
