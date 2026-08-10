using System;
using System.Collections.Generic;

namespace PereViader.UnityStateSnapshot
{
    [Serializable]
    public class GameObjectSnapshotDTO
    {
        public string Name;
        public bool ActiveSelf;
        public bool ActiveInHierarchy;
        public string Tag;
        public string Layer;
        public TransformSnapshotDTO Transform;
        public List<ComponentSnapshotDTO> Components = new List<ComponentSnapshotDTO>();
        public List<GameObjectSnapshotDTO> Children = new List<GameObjectSnapshotDTO>();
    }
}
