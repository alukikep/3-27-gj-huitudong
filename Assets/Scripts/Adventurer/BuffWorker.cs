using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class BuffWorker : Adventurer
{
    [Header("Buff效果")]
    [SerializeField] private float DamageBuffAmount; // 增加金币数的百分比
    [SerializeField] private float TimeBuffAmount;   // 减少时间间隔的百分比

    // 当前是否在应用buff状态
    private bool isBuffActive = false;

    protected override void Start()
    {
        currentHealth = maxHealth;
        healthBar.color = Color.green;
        animator = this.GetComponent<Animator>();
        stateToAnim["work"] = workAnim;
        stateToAnim["tired"] = tiredAnim;
        stateToAnim["die"] = dieAnim;

        // 检查当前所在区域
        DetectCurrentArea();
        UpdateAreaEffect();

        // 初始放置时，增加当前区域计数
        if (DataManager.Instance != null)
        {
            DataManager.Instance.IncrementAreaCount(currentAreaIndex);
            Debug.Log($"BuffWorker初始放置: 区域{currentAreaIndex}, 初始计数+1");

            // 怪物类型计数（BuffWorker特有的）
            IncrementBuffMonsterTypeCount();

            // 如果在非休息区，应用初始buff
            ApplyBuffForCurrentArea();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        // 移除当前区域的buff
        RemoveBuffForCurrentArea();
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        if (DataManager.Instance != null)
        {
            // 移除buff
            RemoveBuffForCurrentArea();

            // 减少区域计数
            DataManager.Instance.DecrementAreaCount(currentAreaIndex);
            Debug.Log($"BuffWorker销毁: 区域{currentAreaIndex}, 计数-1");

            // 怪物类型计数减少
            DecrementBuffMonsterTypeCount();
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
    }

    protected override void OnMouseExit()
    {
        gameObject.transform.DOScale(Vector3.one, 0.15f);
    }

    protected override void OnMouseDrag()
    {
        base.OnMouseDrag();
    }

    protected override void OnMouseUp()
    {
        if (currentState == adventurerState.ISDRAGGING)
        {
            gameObject.transform.DOScale(Vector3.one, 0.15f);

            // 恢复动画器
            animator.enabled = true;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sprite = originalSprite;
            if (!string.IsNullOrEmpty(lastAnimationName))
            {
                animator.Play(lastAnimationName);
            }

            // 拖拽结束时处理放置
            HandleBuffPlacementAfterDrag();
        }
        else
        {
            gameObject.transform.DOScale(Vector3.one, 0.15f);
        }
    }

    /// <summary>
    /// BuffWorker特有的放置处理
    /// </summary>
    protected override void HandlePlacementAfterDrag()
    {
        // 基类处理基本逻辑
        base.HandlePlacementAfterDrag();

        // BuffWorker额外处理buff
        ApplyBuffForCurrentArea();
    }

    /// <summary>
    /// BuffWorker专用的放置处理
    /// </summary>
    protected void HandleBuffPlacementAfterDrag()
    {
        int oldArea = dragStartAreaIndex;
        int targetArea = 1; // 默认休息区
        
        // 先移除旧区域的buff
        RemoveBuffForArea(oldArea);

        // 检测放置位置
        int layerMask = LayerMask.GetMask("EasyArea", "NormalArea", "HardArea", "RestArea");
        Collider2D collider = Physics2D.OverlapPoint(transform.position, layerMask);
        currentState = adventurerState.REST;

        if (collider != null)
        {
            string layerName = LayerMask.LayerToName(collider.gameObject.layer);

            switch (layerName)
            {
                case "EasyArea":
                    targetArea = 2;
                    currentState = adventurerState.WORKEASY;
                    break;
                case "NormalArea":
                    targetArea = 3;
                    currentState = adventurerState.WORK;
                    break;
                case "HardArea":
                    targetArea = 4;
                    currentState = adventurerState.WORKHARD;
                    break;
                case "RestArea":
                    targetArea = 1;
                    currentState = adventurerState.REST;
                    break;
            }
            
            // 检查目标区域是否已满（休息区永远可以放置）
            if (targetArea != 1 && DataManager.Instance != null && 
                !DataManager.Instance.CanPlaceInArea(targetArea))
            {
                // 区域已满，移动到休息区
                Debug.Log($"BuffWorker区域{targetArea}已满，移动到休息区");
                if (DataManager.Instance.restAreaPosition != null)
                {
                    transform.position = DataManager.Instance.restAreaPosition.position;
                }
                targetArea = 1;
                currentState = adventurerState.REST;
            }
        }
        else
        {
            // 不在任何区域，移动到休息区
            if (DataManager.Instance != null && DataManager.Instance.restAreaPosition != null)
            {
                transform.position = DataManager.Instance.restAreaPosition.position;
            }
            targetArea = 1;
            currentState = adventurerState.REST;
        }

        // 更新currentAreaIndex
        currentAreaIndex = targetArea;
        
        // 更新区域计数：先减少原区域，再增加新区域
        if (oldArea != currentAreaIndex)
        {
            DataManager.Instance.DecrementAreaCount(oldArea);
            DataManager.Instance.IncrementAreaCount(currentAreaIndex);
            Debug.Log($"BuffWorker区域计数更新: {oldArea} -> {currentAreaIndex}");
        }

        // 更新区域效果
        UpdateAreaEffect();

        // 应用新区域的buff
        ApplyBuffForCurrentArea();
    }

    /// <summary>
    /// 根据当前区域应用buff
    /// </summary>
    private void ApplyBuffForCurrentArea()
    {
        if (DataManager.Instance == null) return;

        switch (currentAreaIndex)
        {
            case 1: // 休息区 - 正面buff
                DataManager.Instance.area1Damage *= (1 + DamageBuffAmount);
                DataManager.Instance.area1Time *= (1 - TimeBuffAmount);
                isBuffActive = true;
                Debug.Log("BuffWorker: 应用休息区buff");
                break;
            case 2: // 轻松区 - 负面buff
                DataManager.Instance.area2Damage *= (1 - DamageBuffAmount);
                DataManager.Instance.area2Time *= (1 - TimeBuffAmount);
                isBuffActive = true;
                Debug.Log("BuffWorker: 应用轻松区debuff");
                break;
            case 3: // 普通区 - 负面buff
                DataManager.Instance.area3Damage *= (1 - DamageBuffAmount);
                DataManager.Instance.area3Time *= (1 - TimeBuffAmount);
                isBuffActive = true;
                Debug.Log("BuffWorker: 应用普通区debuff");
                break;
            case 4: // 困难区 - 负面buff
                DataManager.Instance.area4Damage *= (1 - DamageBuffAmount);
                DataManager.Instance.area4Time *= (1 - TimeBuffAmount);
                isBuffActive = true;
                Debug.Log("BuffWorker: 应用困难区debuff");
                break;
        }
    }

    /// <summary>
    /// 移除指定区域的buff效果
    /// </summary>
    private void RemoveBuffForArea(int areaIndex)
    {
        if (DataManager.Instance == null) return;

        switch (areaIndex)
        {
            case 1: // 休息区
                DataManager.Instance.area1Damage /= (1 + DamageBuffAmount);
                DataManager.Instance.area1Time /= (1 - TimeBuffAmount);
                Debug.Log("BuffWorker: 移除休息区buff");
                break;
            case 2: // 轻松区
                DataManager.Instance.area2Damage /= (1 - DamageBuffAmount);
                DataManager.Instance.area2Time /= (1 - TimeBuffAmount);
                Debug.Log("BuffWorker: 移除轻松区debuff");
                break;
            case 3: // 普通区
                DataManager.Instance.area3Damage /= (1 - DamageBuffAmount);
                DataManager.Instance.area3Time /= (1 - TimeBuffAmount);
                Debug.Log("BuffWorker: 移除普通区debuff");
                break;
            case 4: // 困难区
                DataManager.Instance.area4Damage /= (1 - DamageBuffAmount);
                DataManager.Instance.area4Time /= (1 - TimeBuffAmount);
                Debug.Log("BuffWorker: 移除困难区debuff");
                break;
        }

        isBuffActive = false;
    }

    /// <summary>
    /// 移除当前区域的buff
    /// </summary>
    private void RemoveBuffForCurrentArea()
    {
        RemoveBuffForArea(currentAreaIndex);
    }

    /// <summary>
    /// 增加BuffWorker怪物类型计数
    /// </summary>
    private void IncrementBuffMonsterTypeCount()
    {
        if (DataManager.Instance == null) return;

        if (adventurerName == "troll")
        {
            DataManager.Instance.trollCurrentCount++;
        }
        else if (adventurerName == "succubus")
        {
            DataManager.Instance.succubusCurrentCount++;
        }
    }

    /// <summary>
    /// 减少BuffWorker怪物类型计数
    /// </summary>
    private void DecrementBuffMonsterTypeCount()
    {
        if (DataManager.Instance == null) return;

        if (adventurerName == "troll")
        {
            DataManager.Instance.trollCurrentCount--;
        }
        else if (adventurerName == "succubus")
        {
            DataManager.Instance.succubusCurrentCount--;
        }
    }

    private IEnumerator WorkerDieAnim()
    {
        if (animator != null)
        {
            animator.enabled = true;
        }

        if (animator != null && stateToAnim.ContainsKey("die"))
        {
            animator.Play(stateToAnim["die"]);
        }

        float animDuration = 1f;
        yield return new WaitForSeconds(animDuration);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float fadeDuration = 0.5f;
            float elapsed = 0f;
            Color originalColor = sr.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}