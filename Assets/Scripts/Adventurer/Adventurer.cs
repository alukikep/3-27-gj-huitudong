using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public enum adventurerState { ISDRAGGING, REST, WORKEAST, WORK, WORKHARD, }

public class Adventurer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float currentEfficiency;//获取金币的数量
    public float currentTime;//获取金币的时间间隔
    public float maxHealth;
    public float currentHealth;//生命值，跌到0会"罢工"一段时间
    public float buildAddEffeciency;//建筑增加的金币数
    public float buildSubtractTime;//建筑减少的时间间隔
    [SerializeField] private adventurerState currentState;
    [SerializeField] private TMP_Text coinsTextPrefab;
    private float workTimer;//计时器
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        workTimer -= Time.deltaTime;
        if (workTimer < 0 && currentState != adventurerState.REST && currentState != adventurerState.ISDRAGGING)
        {
            GetCoin();
            workTimer = 0.2f;//测试
        }

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

        // 设置状态为拖拽中
        currentState = adventurerState.ISDRAGGING;
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
    }

    void OnMouseExit()
    {
        gameObject.transform.DOScale(Vector3.one, 0.15f);
    }

    private void CheckArea()
    {
        int layerMask = LayerMask.GetMask("EasyArea", "NormalArea", "HardArea");
        // 使用 Physics2D.OverlapPoint 检测当前位置有什么碰撞体
        Collider2D collider = Physics2D.OverlapPoint(transform.position, layerMask);

        if (collider != null)
        {
            // 获取碰撞体所在图层
            string layerName = LayerMask.LayerToName(collider.gameObject.layer);

            // 根据图层名称设置冒险者状态
            switch (layerName)
            {
                case "EasyArea":
                    currentState = adventurerState.WORKEAST;
                    break;
                case "NormalArea":
                    currentState = adventurerState.WORK;
                    break;
                case "HardArea":
                    currentState = adventurerState.WORKHARD;
                    break;
            }

            Debug.Log($"冒险者进入区域: {layerName}, 状态: {currentState}, 效率: {currentEfficiency}, 间隔: {currentTime}");
        }
        else
        {
            // 没有检测到碰撞体，设置为休息状态
            currentState = adventurerState.REST;
            currentEfficiency = 0f;
            currentTime = 0f;
            Debug.Log("冒险者不在任何区域，算作休息区，休息中");
        }
    }

    private void SetEfficiencyBaseOnArea()
    {
        switch (currentState)//根据区域调整效率
        {
            case adventurerState.REST:
                break;
            case adventurerState.WORKEAST:
                break;
            case adventurerState.WORK:
                break;
            case adventurerState.WORKHARD:
                break;
            case adventurerState.ISDRAGGING:
                break;

        }
    }

    private void GetCoin()
    {
        //计算逻辑后续还需要统筹修改
        float coins = currentEfficiency * buildAddEffeciency;
        DataManager.Instance.ChangeCoins(coins);

        // 生成金币特效文本
        ShowCoinText(coins);
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



}
