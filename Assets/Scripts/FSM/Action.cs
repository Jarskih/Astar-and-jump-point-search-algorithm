using UnityEngine;

namespace Astar
{
    public abstract class Action : ScriptableObject
    {
            public abstract void Act(StateController controller);
    }
}