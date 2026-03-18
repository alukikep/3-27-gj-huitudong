using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StatusType
{
    Debuff_Dot,//持续伤害
    Debuff_StatMod,//属性削弱
    Buff_StatMod,//属性增强
}

public class StatusEffectSO : ScriptableObject
{
    [Header("基础配置")]
    public string statusName;
    public float duration;
    public StatusType statusType;
    public bool isStackable;
    [Header("数值成长配置")]
    public float baseValue;
    public float growthPerLevel;
    public float GetValueByLevel(float level)
    {
        return baseValue + growthPerLevel * (level - 1);
    }
    public virtual void OnStart(GameObject target, StatusManager.RuntimeStatus status)
    {

    }

    public virtual void OnTick(GameObject target, StatusManager.RuntimeStatus status) { }
    public virtual void OnEnd(GameObject target, StatusManager.RuntimeStatus status)
    {

    }
    public virtual float GetSpeedMultiplier(StatusManager.RuntimeStatus status)
    {
        return 1f;
    }
    public virtual float GetDamageTakenMultiplier(StatusManager.RuntimeStatus status)
    {
        return 1f;
    }

}
