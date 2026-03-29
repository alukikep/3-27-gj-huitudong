using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum adventurerState { ISDRAGGING, DEFEAT, REST, WORKEASY, WORK, WORKHARD, }

[System.Serializable]

public class Adventurer : MonoBehaviour
{
    [SerializeField] protected string adventurerName;
    [Header("组件")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Image healthBar;
    [SerializeField] protected string workAnim;
    [SerializeField] protected string tiredAnim;
    [SerializeField] protected string dieAnim;
    public Dictionary<string, string> stateToAnim = new();

    protected Animator animator;
    public float currentEfficiency;//获取金币的数量
    public float currentTime;//获取金币的时间间隔
    public float currentDamage;//获取伤害用于扣玩家生命值
    public float maxHealth = 100;
    public float currentHealth;//生命值，跌到0会"罢工"一段时间
    [SerializeField] protected adventurerState currentState;
    [SerializeField] protected TMP_Text coinsTextPrefab;
    [SerializeField] protected Sprite draggingSprite;
    protected float workTimer;//计时器
    protected bool isTired;
    protected Sprite originalSprite;//保存原始精灵图片
    protected string lastAnimationName;//拖拽前播放的动画名字
    protected Vector3 originalPosition;//保存原始位置，用于区域满时返回
    public System.Action<float> OnTimerChanged;
    public System.Action<float> OnCoinGenerated;

    // ========== 区域计数相关 ==========
    // 当前所在的区域索引（1=休息区, 2=轻松区, 3=普通区, 4=困难区）
    protected int currentAreaIndex = 1;
    // 拖拽前的区域索引，用于拖拽结束时更新计数
    protected int dragStartAreaIndex = 1;

    // Start is called before the first frame update
    protected virtual void Start()
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
        
        // 初始放置时，增加当前区域计数（休息区）
        if (DataManager.Instance != null)
        {
            DataManager.Instance.IncrementAreaCount(currentAreaIndex);
            Debug.Log($"Adventurer初始放置: 区域{currentAreaIndex}, 初始计数+1");
            
            // 怪物类型计数
            IncrementMonsterTypeCount();
        }
    }

    protected virtual void OnEnable()
    {
        EventManager.AddListener("AreaCheck", CheckAreaCallback);
    }

    protected virtual void OnDisable()
    {
        EventManager.RemoveListener("AreaCheck", CheckAreaCallback);
    }

    protected virtual void OnDestroy()
    {
        if (DataManager.Instance != null)
        {
            // 减少区域计数
            DataManager.Instance.DecrementAreaCount(currentAreaIndex);
            Debug.Log($"Adventurer销毁: 区域{currentAreaIndex}, 计数-1");
            
            // 怪物类型计数减少
            DecrementMonsterTypeCount();
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //拖拽时时间暂停
        if (currentState != adventurerState.ISDRAGGING)
        {
            workTimer -= Time.deltaTime;
            currentHealth -= currentDamage * Time.deltaTime;
        }

        UpdateHealthBar();
        
        //罢工逻辑
        if (currentHealth < 0 && isTired == false)
        {
            currentState = adventurerState.DEFEAT;
            isTired = true;
            animator.Play(stateToAnim["tired"]);
            currentDamage = -10;
            healthBar.color = Color.red;
        }
        else if (currentHealth < 0 && isTired == true)
        {
            StartCoroutine(WorkerDieAnim());
        }
        
        //从罢工中恢复的逻辑
        if (currentHealth >= maxHealth && currentState == adventurerState.DEFEAT)
        {
            currentState = adventurerState.REST;
            DetectCurrentArea();
            UpdateAreaEffect();
            healthBar.color = Color.green;
        }
        
        //获取金币逻辑
        if (workTimer < 0 && currentState != adventurerState.DEFEAT && 
            currentState != adventurerState.REST && currentState != adventurerState.ISDRAGGING)
        {
            GetCoin();
            workTimer = currentTime;
        }
    }

    protected void UpdateHealthBar()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    protected void UpdateCurrentEffeciency()
    {
        // 后续实现
    }

    protected virtual void OnMouseEnter()
    {
        if (currentState != adventurerState.ISDRAGGING)
        {
            gameObject.transform.DOScale(Vector3.one * 1.2f, 0.15f);
        }
    }

    protected virtual void OnMouseDrag()
    {
        if (currentState == adventurerState.DEFEAT) return;
        
        // 首次进入拖拽时保存状态
        if (currentState != adventurerState.ISDRAGGING)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            originalSprite = sr.sprite;
            originalPosition = transform.position;
            dragStartAreaIndex = currentAreaIndex; // 保存拖拽前的区域

            // 获取当前播放的动画名
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                lastAnimationName = animator.GetLayerName(0) + "." + stateInfo.shortNameHash;
            }

            animator.enabled = false;
        }

        // 设置拖拽图片
        SpriteRenderer sr2 = GetComponent<SpriteRenderer>();
        sr2.sprite = draggingSprite;

        // 将鼠标屏幕坐标转换为世界坐标
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        gameObject.transform.DOScale(Vector3.one * 1.2f, 0.15f);

        // 设置状态为拖拽中
        currentState = adventurerState.ISDRAGGING;
    }

    protected virtual void OnMouseUp()
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

            // 拖拽结束时检查区域并处理移动
            HandlePlacementAfterDrag();
        }
        else
        {
            gameObject.transform.DOScale(Vector3.one, 0.15f);
        }
    }

    protected virtual void OnMouseExit()
    {
        gameObject.transform.DOScale(Vector3.one, 0.15f);
    }

    /// <summary>
    /// 检测当前所在的区域
    /// </summary>
    protected void DetectCurrentArea()
    {
        int layerMask = LayerMask.GetMask("EasyArea", "NormalArea", "HardArea", "RestArea");
        Collider2D collider = Physics2D.OverlapPoint(transform.position, layerMask);

        if (collider != null && currentState != adventurerState.DEFEAT)
        {
            string layerName = LayerMask.LayerToName(collider.gameObject.layer);
            
            switch (layerName)
            {
                case "EasyArea":
                    currentState = adventurerState.WORKEASY;
                    currentAreaIndex = 2;
                    break;
                case "NormalArea":
                    currentState = adventurerState.WORK;
                    currentAreaIndex = 3;
                    break;
                case "HardArea":
                    currentState = adventurerState.WORKHARD;
                    currentAreaIndex = 4;
                    break;
                case "RestArea":
                    currentState = adventurerState.REST;
                    currentAreaIndex = 1;
                    break;
            }
        }
        else if (currentState != adventurerState.DEFEAT)
        {
            currentState = adventurerState.REST;
            currentAreaIndex = 1;
        }
    }

    /// <summary>
    /// 拖拽结束后处理放置逻辑
    /// </summary>
    protected virtual void HandlePlacementAfterDrag()
    {
        int oldArea = dragStartAreaIndex;
        int targetArea = 1; // 默认休息区
        bool movedToRestArea = false;
        
        // 检测放置位置
        int layerMask = LayerMask.GetMask("EasyArea", "NormalArea", "HardArea", "RestArea");
        Collider2D collider = Physics2D.OverlapPoint(transform.position, layerMask);

        if (collider != null)
        {
            string layerName = LayerMask.LayerToName(collider.gameObject.layer);
            currentState = adventurerState.REST;
            
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
                Debug.Log($"区域{targetArea}已满，移动到休息区");
                if (DataManager.Instance.restAreaPosition != null)
                {
                    transform.position = DataManager.Instance.restAreaPosition.position;
                }
                targetArea = 1;
                currentState = adventurerState.REST;
                movedToRestArea = true;
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
            movedToRestArea = true;
        }

        // 更新currentAreaIndex
        currentAreaIndex = targetArea;
        
        // 更新区域计数：先减少原区域，再增加新区域
        if (oldArea != currentAreaIndex)
        {
            DataManager.Instance.DecrementAreaCount(oldArea);
            DataManager.Instance.IncrementAreaCount(currentAreaIndex);
            Debug.Log($"区域计数更新: {oldArea} -> {currentAreaIndex}");
        }
        
        // 更新区域效果
        UpdateAreaEffect();
    }

    /// <summary>
    /// 移动时更新区域计数
    /// </summary>
    protected void UpdateAreaCountOnMove(int fromArea)
    {
        if (DataManager.Instance == null) return;
        
        if (fromArea != currentAreaIndex)
        {
            DataManager.Instance.DecrementAreaCount(fromArea);
            DataManager.Instance.IncrementAreaCount(currentAreaIndex);
            Debug.Log($"区域计数更新: {fromArea} -> {currentAreaIndex}");
        }
    }

    /// <summary>
    /// 更新区域效果
    /// </summary>
    protected void UpdateAreaEffect()
    {
        if (DataManager.Instance == null) return;
        
        if (currentState == adventurerState.REST)
        {
            currentTime = DataManager.Instance.area1Time;
            currentDamage = DataManager.Instance.area1Damage;
        }
        else if (currentState == adventurerState.WORKEASY)
        {
            currentTime = DataManager.Instance.area2Time;
            currentDamage = DataManager.Instance.area2Damage;
        }
        else if (currentState == adventurerState.WORK)
        {
            currentTime = DataManager.Instance.area3Time;
            currentDamage = DataManager.Instance.area3Damage;
        }
        else if (currentState == adventurerState.WORKHARD)
        {
            currentTime = DataManager.Instance.area4Time;
            currentDamage = DataManager.Instance.area4Damage;
        }
        
        currentEfficiency = DataManager.Instance.adventurerEfficiency;
        workTimer = currentTime;
    }

    /// <summary>
    /// EventManager的回调，用于外部触发区域检查
    /// </summary>
    protected void CheckAreaCallback()
    {
        DetectCurrentArea();
        UpdateAreaEffect();
    }

    /// <summary>
    /// 增加怪物类型计数
    /// </summary>
    protected void IncrementMonsterTypeCount()
    {
        if (DataManager.Instance == null) return;
        
        if (adventurerName == "goblin")
        {
            DataManager.Instance.goblinCurrentCount++;
        }
        else if (adventurerName == "slime")
        {
            DataManager.Instance.slimeCurrentCount++;
        }
        else if (adventurerName == "skeleton")
        {
            DataManager.Instance.skeletonCurrentCount++;
        }
    }

    /// <summary>
    /// 减少怪物类型计数
    /// </summary>
    protected void DecrementMonsterTypeCount()
    {
        if (DataManager.Instance == null) return;
        
        if (adventurerName == "goblin")
        {
            DataManager.Instance.goblinCurrentCount--;
        }
        else if (adventurerName == "slime")
        {
            DataManager.Instance.slimeCurrentCount--;
        }
        else if (adventurerName == "skeleton")
        {
            DataManager.Instance.skeletonCurrentCount--;
        }
    }

    protected void GetCoin()
    {
        float coins = currentEfficiency;
        DataManager.Instance.ChangeCoins(coins);
        ShowCoinText(coins);

        transform.DOKill();
        transform.DOScale(Vector3.one * 1.3f, 0.1f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InQuad);
            });
        OnCoinGenerated?.Invoke(coins);
    }

    protected void ShowCoinText(float coins)
    {
        if (coinsTextPrefab == null || SimplePool.Instance == null) return;

        GameObject coinTextObj = SimplePool.Instance.Spawn(coinsTextPrefab.gameObject, transform.position, Quaternion.identity);
        TMP_Text coinText = coinTextObj.GetComponent<TMP_Text>();

        if (coinText != null)
        {
            coinText.text = $"+{coins:F0}";
            Color textColor = coinText.color;
            textColor.a = 1f;
            coinText.color = textColor;

            coinTextObj.transform.DOKill();
            coinTextObj.transform.position = transform.position;
            coinTextObj.transform.localScale = Vector3.one * 0.5f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(coinTextObj.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack));
            sequence.Append(coinTextObj.transform.DOMoveY(transform.position.y + 1.5f, 0.8f).SetEase(Ease.OutQuad));
            sequence.Join(coinText.DOFade(0f, 0.8f).SetEase(Ease.OutQuad));
            sequence.OnComplete(() => { SimplePool.Instance.Despawn(coinTextObj); });
            sequence.Play();
        }
    }

    protected IEnumerator WorkerDieAnim()
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