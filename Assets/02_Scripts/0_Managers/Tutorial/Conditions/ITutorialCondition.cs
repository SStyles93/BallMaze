using UnityEngine;

public interface ITutorialCondition
{
    bool IsSatisfied();
}

public interface IContextBoundCondition
{
    void BindContext(TutorialContext context, string anchorId, Canvas canvas);
}
