using System.Collections;
using System.Collections.Generic;
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
    protected TileCell tempTargetCell;
    [HideInInspector]
    protected GridObject tempTarget;

    public event OnGameAbilityStart OnAbilityStartEvent;
    public event OnGameAbilityComplete OnAbilityCompleteEvent;

    public abstract void DoStartAbility(GridObject user, TileCell target);
    public abstract void DoCompleteAbility();

    int clickCount = 0;

    public virtual bool UpdateClick(GameManager gameMgr, TileCell dst)
    {
        if (gameMgr.MapManager.PreviousSelectedCell != null && 
            gameMgr.MapManager.PreviousSelectedCell.Position != dst.Position && 
            gameMgr.MapManager.IsClickedSelectedArea(dst))
        {
            if (gameMgr.MapManager.IsClickedSelectedPath(dst))
            {
                List<TileCell> selPath = gameMgr.MapManager.GetSelectedPathTo(dst);
                gameMgr.MoveObject(user.Location, dst, selPath);
                gameMgr.MapManager.SelectPath(selPath);
            }
            else
                gameMgr.DrawPath(user.Location, dst);
            return true;
        }


        if (clickCount == 0)
        {
            clickCount = 1 - clickCount;
            return false;
        }
        else
        {
            if (dst.Position == user.Location.Position)
            {
                if (AreaEffectType == AreaEffectType.AREA)
                {
                    gameMgr.DrawArea(user.Location, Range, false);
                }
                else if (AreaEffectType == AreaEffectType.WALKABLE_AREA)
                {
                    gameMgr.DrawArea(user.Location, Range, true);
                }
                else if (AreaEffectType == AreaEffectType.PATH)
                {
                    gameMgr.DrawPath(user.Location, dst);
                }
                else
                {
                    Debug.LogError("Unknown AreaEffectType: " + AreaEffectType.ToString());
                }
                clickCount = 1 - clickCount;
            }
        }


        return true;
    }

    public void UseAbility(GridObject user, TileCell target)
    {
        this.user = user;
        if (target != null)
            tempTargetCell = target;
        if (this.tempTargetCell != null)
            this.tempTarget = tempTargetCell.Contents;


        if (this.tempTarget == null)
        {
            Debug.Log(user.name + " attacks but there is no target in the cell!");
            FinishAbility();
            return;
        }

        if (OnAbilityStartEvent != null)
            OnAbilityStartEvent(user, target);

        clickCount = 0;

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
    public virtual void OnSelect()
    {
        clickCount = 0;
    }

    public virtual void OnDeselect()
    {
        clickCount = 0;
    }
}
