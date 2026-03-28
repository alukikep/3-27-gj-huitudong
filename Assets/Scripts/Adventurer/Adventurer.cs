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

    [Header("组件")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image healthBar;
    [SerializeField] private string workAnim;
    [SerializeField] private string tiredAnim;
    [SerializeField] private string dieAnim;
    public Dictionary<string, string> stateToAnim = new();

    private Animator animator;
    public float currentEfficiency;//获取金币的数量
    public float currentTime;//获取金币的时间间隔
    public float currentDamage;//获取伤害用于扣玩家生命值
    public float maxHealth = 100;
    public float currentHealth;//生命值，跌到0会"罢工"一段时间
    public float buildAddEffeciency = 1;//建筑增加的金币数
    public float buildSubtractTime = 1;//建筑减少的时间间隔
    [SerializeField] private adventurerState currentState;
    [SerializeField] private TMP_Text coinsTextPrefab;
    [SerializeField] private Sprite draggingSprite;
    private float workTimer;//计时器
    private bool isTired;
    private Sprite originalSprite;//保存原始精灵图片
    private string lastAnimationName;//拖拽前播放的动画名字
    public System.Action<float> OnTimerChanged;
    public System.Action<float> OnCoinGenerated;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.color = Color.green;
        animator = this.GetComponent<Animator>();
        stateToAnim["work"] = workAnim;
        stateToAnim["tired"] = tiredAnim;
        stateToAnim["die"] = dieAnim;
    }



    // Update is called once per frame
    void Update()
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
            //设置回血速度
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
            CheckArea();
            healthBar.color = Color.green;
        }
        //获取金币逻辑
        if (workTimer < 0 && currentState != adventurerState.DEFEAT && currentState != adventurerState.REST && currentState != adventurerState.ISDRAGGING)
        {
            GetCoin();
            workTimer = currentTime;
        }
        OnTimerChanged?.Invoke(workTimer);
    }


    private void UpdateHealthBar()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    private void UpdateCurrentEffeciency()
    {

    }

    void OnMouseEnter()
    {
        if (currentState != adventurerState.ISDRAGGING)
        {
            gameObject.transform.DOScale(Vector3.one * 1.2f, 0.15f);
        }
    }

    void OnMouseDrag()
    {
        if (currentState == adventurerState.DEFEAT) return;
        // 首次进入拖拽时保存状态和动画名
        if (currentState != adventurerState.ISDRAGGING)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            originalSprite = sr.sprite; // 保存原始精灵

            // 获取当前播放的动画名
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                lastAnimationName = animator.GetLayerName(0) + "." + stateInfo.shortNameHash;
            }

            animator.enabled = false; // 禁用Animator
        }

        // 设置拖拽图片
        SpriteRenderer sr2 = GetComponent<SpriteRenderer>();
        sr2.sprite = draggingSprite;

        // 将鼠标屏幕坐标转换为世界坐标
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z; // 设置Z轴为相机到物体的距离
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // 更新物体位置
        transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        gameObject.transform.DOScale(Vector3.one * 1.2f, 0.15f);

        // 设置状态为拖拽中
        currentState = adventurerState.ISDRAGGING;
    }

    void OnMouseUp()
    {
        if (currentState == adventurerState.ISDRAGGING)
        {
            gameObject.transform.DOScale(Vector3.one, 0.15f);
            currentState = adventurerState.REST;

            // 恢复动画器
            animator.enabled = true;

            // 恢复原始精灵图片
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sprite = originalSprite;

            // 用拖拽前存储的动画名播放动画
            if (!string.IsNullOrEmpty(lastAnimationName))
            {
                animator.Play(lastAnimationName);
            }

            // 拖拽结束时检查区域
            CheckArea();
        }
        else
        {
            gameObject.transform.DOScale(Vector3.one, 0.15f);
        }
    }

    void OnMouseExit()
    {
        gameObject.transform.DOScale(Vector3.one, 0.15f);
    }

    private void CheckArea()
    {
        int layerMask = LayerMask.GetMask("EasyArea", "NormalArea", "HardArea", "RestArea");
        // 使用 Physics2D.OverlapPoint 检测当前位置有什么碰撞体
        Collider2D collider = Physics2D.OverlapPoint(transform.position, layerMask);

        if (collider != null && currentState != adventurerState.DEFEAT)
        {
            // 获取碰撞体所在图层
            string layerName = LayerMask.LayerToName(collider.gameObject.layer);

            // 根据图层名称设置冒险者状态
            switch (layerName)
            {
                case "EasyArea":
                    currentState = adventurerState.WORKEASY;
                    break;
                case "NormalArea":
                    currentState = adventurerState.WORK;
                    break;
                case "HardArea":
                    currentState = adventurerState.WORKHARD;
                    break;
                case "RestArea":
                    currentState = adventurerState.REST;
                    break;
            }
            SetAreaEffect();
            Debug.Log($"冒险者进入区域: {layerName}, 状态: {currentState}, 效率: {currentEfficiency}, 间隔: {currentTime}");
        }
        else
        {
            // 没有检测到碰撞体，设置为休息状态
            currentState = adventurerState.REST;
            SetAreaEffect();
            Debug.Log("冒险者不在任何区域，算作休息区，休息中");
        }
    }


    private void GetCoin()
    {
        //计算逻辑后续还需要统筹修改
        float coins = currentEfficiency * buildAddEffeciency;
        DataManager.Instance.ChangeCoins(coins);

        // 生成金币特效文本
        ShowCoinText(coins);

        // 获得金币时的膨大缩小动画效果
        transform.DOKill();
        transform.DOScale(Vector3.one * 1.3f, 0.1f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InQuad);
            });
        OnCoinGenerated?.Invoke(coins);
    }

    private void ShowCoinText(float coins)
    {
        if (coinsTextPrefab == null || SimplePool.Instance == null) return;

        // 从对象池生成金币文本
        GameObject coinTextObj = SimplePool.Instance.Spawn(coinsTextPrefab.gameObject, transform.position, Quaternion.identity);
        TMP_Text coinText = coinTextObj.GetComponent<TMP_Text>();

        if (coinText != null)
        {
            coinText.text = $"+{coins:F0}";

            // 【关键】重置透明度！避免从对象池取出时是透明的
            Color textColor = coinText.color;
            textColor.a = 1f;
            coinText.color = textColor;

            // 使用 DOTween 实现跳动上升动画
            coinTextObj.transform.DOKill();

            // 设置初始位置和缩放
            coinTextObj.transform.position = transform.position;
            coinTextObj.transform.localScale = Vector3.one * 0.5f;

            // 创建跳动+上升+淡出动画序列
            Sequence sequence = DOTween.Sequence();
            sequence.Append(coinTextObj.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack)); // 弹跳放大
            sequence.Append(coinTextObj.transform.DOMoveY(transform.position.y + 1.5f, 0.8f).SetEase(Ease.OutQuad)); // 上升
            sequence.Join(coinText.DOFade(0f, 0.8f).SetEase(Ease.OutQuad)); // 淡出
            sequence.OnComplete(() =>
            {
                // 动画完成后回收对象
                SimplePool.Instance.Despawn(coinTextObj);
            });

            sequence.Play();
        }
    }

    private void SetAreaEffect()
    {
        //后续检查是否需要添加处理
        if (currentState == adventurerState.REST)
        {
            //时间间隔，伤害;
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

        //设置对应区域的数值
    }
    

    private IEnumerator WorkerDieAnim()
    {
        // 确保Animator启用
        if (animator != null)
        {
            animator.enabled = true;
        }
        
        // 1. 播放死亡动画
        if (animator != null && stateToAnim.ContainsKey("die"))
        {
            animator.Play(stateToAnim["die"]);
        }

        // 2. 等待动画播放完成（假设死亡动画时长1秒，可根据实际情况调整）
        float animDuration = 1f;
        yield return new WaitForSeconds(animDuration);

        // 3. 逐渐透明淡出
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

        // 4. 销毁游戏对象
        Destroy(gameObject);
    }


}
