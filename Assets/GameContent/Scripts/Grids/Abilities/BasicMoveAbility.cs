using UnityEngine;

public class BasicMoveAbility : GameAbility
{
    public override void DoCompleteAbility()
    {
    }

    public override void DoStartAbility(GridObject user, TileCell target)
    {
        FinishAbilityDelay(0.5f);
    }
}
