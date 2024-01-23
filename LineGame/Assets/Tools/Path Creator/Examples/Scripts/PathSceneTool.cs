using System.Collections;
using UnityEngine;


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
        Debug.Log($"{ColouredString.Colorize($"PathSceneTool :: Updated Texture on {this.gameObject.name}", "f7671e")}");
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