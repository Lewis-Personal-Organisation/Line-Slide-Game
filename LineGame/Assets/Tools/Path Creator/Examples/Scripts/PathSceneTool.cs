using System.Collections;
using UnityEngine;

namespace PathCreation.Examples
{
    [ExecuteInEditMode]
    public abstract class PathSceneTool : MonoBehaviour
    {
        public event System.Action onDestroyed;
        public PathCreator pathCreator;
        public bool autoUpdate = true;

        protected VertexPath path 
        {
            get 
            {
                return pathCreator.path;
            }
        }

        public void TriggerUpdate() 
        {
            StartCoroutine(PathUpdated());
        }


        protected virtual void OnDestroy() 
        {
            if (onDestroyed != null) 
            {
                onDestroyed();
            }
        }

        protected abstract IEnumerator PathUpdated();
    }
}
