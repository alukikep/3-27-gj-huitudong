using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    [System.Serializable]
    public class RuntimeStatus
    {
        public StatusEffectSO effectData;
        public float timer;
        public float tickTimer;
        public float level;
        public float snapshotValue;

        public RuntimeStatus(StatusEffectSO data, float lvl = 1)
        {
            effectData = data;
            timer = data.duration;
            tickTimer = 0f;
            level = lvl;
            snapshotValue = data.GetValueByLevel(lvl);
        }
    }

    public List<RuntimeStatus> activeStatuses = new List<RuntimeStatus>();
    public float currentSpeedMult { get; private set; } = 1f;
    public float currentDamageTakenMult { get; private set; } = 1f;

    void Update()
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            var status = activeStatuses[i];
            status.timer -= Time.deltaTime;
            status.tickTimer += Time.deltaTime;
            if (status.tickTimer >= 1f)
            {
                status.effectData.OnTick(this.gameObject, status);
                status.tickTimer = 0f;
            }
            if (status.timer <= 0f)
            {
                status.effectData.OnEnd(this.gameObject, status);
                activeStatuses.RemoveAt(i);
                RecalculateStats();
            }
        }
    }

    public void ApplyStatus(StatusEffectSO newEffect, float level = 1)
    {
        var existing = activeStatuses.Find(x => x.effectData == newEffect);

        // 修正点：将 || 改为 &&
        // 逻辑：只有当 "真的找到了(不为空)" 并且 "设定为不可叠加" 时，才去刷新时间
        if (existing != null && !newEffect.isStackable)
        {
            existing.timer = newEffect.duration;
            // 如果你需要刷新等级，也可以在这里加： existing.level = level;
        }
        else
        {
            // 1. 没找到 (existing == null) -> 进这里，创建新的
            // 2. 找到了但可叠加 (!isStackable == false) -> 进这里，创建新的(叠层)
            var newStatus = new RuntimeStatus(newEffect, (int)level); // 注意类型转换
            newEffect.OnStart(this.gameObject, newStatus);
            activeStatuses.Add(newStatus);
            RecalculateStats();
        }
    }
    private void RecalculateStats()
    {
        float speed = 1f;
        float damageTaken = 1f;
        foreach (var status in activeStatuses)
        {
            speed *= status.effectData.GetSpeedMultiplier(status);
            damageTaken *= status.effectData.GetDamageTakenMultiplier(status);
        }
        currentSpeedMult = speed;
        currentDamageTakenMult = damageTaken;
    }

}
