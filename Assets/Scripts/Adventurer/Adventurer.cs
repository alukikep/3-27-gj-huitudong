using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum adventurerState { ISDRAGGING, DEFEAT, REST, WORKEASY, WORK, WORKHARD, }

public class Adventurer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image healthBar;
    public float currentEfficiency;//获取金币的数量
    public float currentTime;//获取金币的时间间隔
    public float currentDamage;//获取伤害用于扣玩家生命值
    public float maxHealth = 100;
    public float currentHealth;//生命值，跌到0会"罢工"一段时间
    public float buildAddEffeciency = 1;//建筑增加的金币数
    public float buildSubtractTime = 1;//建筑减少的时间间隔
    [SerializeField] private adventurerState currentState;
    [SerializeField] private TMP_Text coinsTextPrefab;
    [SerializeField] private Worker workerData;
    private float workTimer;//计时器
    // Start is called before the first frame update
    void Start()
    {
        Setup(workerData);
        currentHealth = maxHealth;
        healthBar.color = Color.green;
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
        if (currentHealth < 0)
        {
            currentState = adventurerState.DEFEAT;
            currentDamage = -2;
            //设置回血速度
            healthBar.color = Color.red;
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





    }

    //初始化工人
    private void Setup(Worker workerData)
    {
        spriteRenderer.sprite = workerData.sprite;
        currentEfficiency = DataManager.Instance.adventurerEfficiency * workerData.initialEffeciency;
        maxHealth = workerData.maxHealth;
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
        // 将鼠标屏幕坐标转换为世界坐标
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z; // 设置Z轴为相机到物体的距离
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // 更新物体位置
        transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        gameObject.transform.DOScale(Vector3.one * 1.2f, 0.15f);

        // 设置状态为拖拽中
        if (currentState != adventurerState.DEFEAT)
        {
            currentState = adventurerState.ISDRAGGING;
        }
    }

    void OnMouseUp()
    {
        if (currentState == adventurerState.ISDRAGGING)
        {
            gameObject.transform.DOScale(Vector3.one, 0.15f);
            currentState = adventurerState.REST;
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



}
