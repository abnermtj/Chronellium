using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: At any given point, there is only one virus on any pipe, due to the given construct of intersectors
// NOTE: Designer's responsibility to align all the pipes.
public abstract class PipeView : MonoBehaviour
{
    protected PipeView upstream;
    protected PipeView downstream;

    public void CallMoveStream(GameObject content) {
        AbsorbFromUpstream();
        StartCoroutine(MoveStream(content));
    }
    protected abstract IEnumerator MoveStream(GameObject content);
    public abstract float GetStreamSpeed();
    protected abstract void AbsorbFromUpstream();
    public abstract Pipe GetPipe();
    public virtual bool isSplitIntersector() { return false; }
}
