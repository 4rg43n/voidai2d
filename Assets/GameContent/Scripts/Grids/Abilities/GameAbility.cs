using System.Collections;
using UnityEngine;

public enum AreaEffectType
{ 
    AREA=0,
    WALKABLE_AREA,
    PATH,
}

public enum AbilityType
{ 
    MOVE=0,
    ATTACK,
    MAGIC,
    SKILL,
    ITEM,
}

public enum EffectType
{
    ENEMY=0,
    EMPTY_CELL,
    FRIENDLY,
}

public abstract class GameAbility : MonoBehaviour
{
    public delegate void OnGameAbilityStart(GridObject user, TileCell target);
    public delegate void OnGameAbilityComplete();

    public AreaEffectType AreaEffectType = AreaEffectType.AREA;
    public AbilityType AbilityType = AbilityType.ATTACK;
    public EffectType EffectType = EffectType.ENEMY;

    public int Range = 1;

    [HideInInspector]
    public GridObject user;
    [HideInInspector]
    public TileCell targetCell;
    [HideInInspector]
    public GridObject target;

    public event OnGameAbilityStart OnAbilityStartEvent;
    public event OnGameAbilityComplete OnAbilityCompleteEvent;

    public abstract void DoStartAbility(GridObject user, TileCell target);
    public abstract void DoCompleteAbility();

    public void UseAbility(GridObject user, TileCell target)
    {
        this.user = user;
        if (target != null)
            targetCell = target;
        if (this.targetCell != null)
            this.target = targetCell.Contents;


        if (this.target == null)
        {
            Debug.Log(user.name + " attacks but there is no target in the cell!");
            FinishAbility();
            return;
        }

        if (OnAbilityStartEvent != null)
            OnAbilityStartEvent(user, target);

        DoStartAbility(user, target);
    }

    public void FinishAbility()
    {
        DoCompleteAbility();

        if (OnAbilityCompleteEvent != null)
            OnAbilityCompleteEvent();

        GameObject.Destroy(gameObject);
    }

    public void FinishAbilityDelay(float time)
    {
        StartCoroutine(_FinishAbilityDelay(time));
    }

    protected IEnumerator _FinishAbilityDelay(float time)
    {
        yield return new WaitForSeconds(time);

        FinishAbility();
    }
}
