using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: At any given point, there is only one virus on any pipe, due to the given construct of intersectors
// NOTE: Designer's responsibility to align all the pipes.
public abstract class PipeView : MonoBehaviour
{
    protected PipeView upstream;
    [SerializeField] protected PipeView downstream;
    // Every pipeview (including subclasses should have an underlying logic pipe)
    
    public void CallMoveStream(GameObject content, PipeView providedUpstream) {
        // Debug.Log($"Moving {content.name} in main stream of {name}");
        AbsorbFromUpstream(providedUpstream);
        StartCoroutine(MoveStream(content));
    }
    protected abstract IEnumerator MoveStream(GameObject content);
    public abstract float GetStreamSpeed();
    protected virtual void AbsorbFromUpstream(PipeView providedUpstream) { 
        Debug.Log("Base class absorb upstream called");
        upstream = providedUpstream; 
    }
    public abstract Pipe GetPipe();
    public virtual bool isSplitIntersector() { return false; }
}
