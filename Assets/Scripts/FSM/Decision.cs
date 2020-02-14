using UnityEngine;

namespace Astar
{
        public abstract class Decision : ScriptableObject
        {
            public abstract bool Decide(StateController controller);
        }
}