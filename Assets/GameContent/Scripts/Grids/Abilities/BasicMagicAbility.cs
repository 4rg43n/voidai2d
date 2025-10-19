using UnityEngine;

public class BasicMagicAbility : GameAbility
{
    public int damage = 1;
    public float lifetime = 2;

    public override void DoCompleteAbility()
    {
    }

    public override void DoStartAbility(GridObject user, TileCell targetCell)
    {
        GameManager.Singleton.ShowFloatingText("" + damage, target.transform.position, Color.red);
        FinishAbilityDelay(lifetime);
    }
}


