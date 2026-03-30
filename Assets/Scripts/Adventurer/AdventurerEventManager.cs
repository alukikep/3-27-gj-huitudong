using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冒险者事件管理器 - 统一管理所有单位的鼠标事件
/// 解决多个单位重叠时鼠标检测失灵的问题
/// </summary>
public class AdventurerEventManager : MonoBehaviour
{
    public static AdventurerEventManager Instance { get; private set; }

    [Header("检测设置")]
    [SerializeField] private LayerMask adventurerLayerMask;
    [SerializeField] private Camera mainCamera;

    private Adventurer currentDraggingAdventurer;
    private bool isDragging = false;
    private Adventurer hoveredAdventurer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        HandleMouseEvents();
    }

    private void HandleMouseEvents()
    {
        Vector3 mouseScreenPos = Input.mousePosition;

        // 检测鼠标下的单位
        Adventurer[] allAdventurers = FindObjectsOfType<Adventurer>();
        Adventurer topAdventurer = GetTopAdventurerAtMouse(allAdventurers, mouseScreenPos);

        // 处理拖拽
        if (isDragging && currentDraggingAdventurer != null)
        {
            HandleDragging(mouseScreenPos);
            
            // 检测鼠标释放
            if (Input.GetMouseButtonUp(0))
            {
                HandleMouseUp(currentDraggingAdventurer);
                currentDraggingAdventurer = null;
                isDragging = false;
            }
        }
        else
        {
            // 处理悬停
            HandleHover(topAdventurer);

            // 检测鼠标按下
            if (Input.GetMouseButtonDown(0) && topAdventurer != null)
            {
                HandleMouseDown(topAdventurer, mouseScreenPos);
            }
        }
    }

    /// <summary>
    /// 获取鼠标位置最上层的单位
    /// </summary>
    private Adventurer GetTopAdventurerAtMouse(Adventurer[] adventurers, Vector3 mouseScreenPos)
    {
        Adventurer topAdventurer = null;
        float smallestZ = float.MaxValue;

        foreach (Adventurer adv in adventurers)
        {
            if (adv == null) continue;

            // 将鼠标位置转换为世界坐标
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z));

            // 使用 2D 射线检测
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, adventurerLayerMask);

            if (hit.collider != null && hit.collider.gameObject == adv.gameObject)
            {
                // Z 值越小越靠上
                if (adv.transform.position.z < smallestZ)
                {
                    smallestZ = adv.transform.position.z;
                    topAdventurer = adv;
                }
            }
        }

        return topAdventurer;
    }

    private void HandleMouseDown(Adventurer adventurer, Vector3 mouseScreenPos)
    {
        if (adventurer == null) return;

        currentDraggingAdventurer = adventurer;
        isDragging = true;

        // 调用单位的拖拽开始方法
        adventurer.OnEventMouseDown();
    }

    private void HandleDragging(Vector3 mouseScreenPos)
    {
        if (currentDraggingAdventurer == null) return;

        // 将鼠标屏幕坐标转换为世界坐标
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(
            mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z));

        // 调用单位的拖拽中方法
        currentDraggingAdventurer.OnEventMouseDrag(mouseWorldPos);
    }

    private void HandleMouseUp(Adventurer adventurer)
    {
        if (adventurer == null) return;

        // 调用单位的拖拽结束方法
        adventurer.OnEventMouseUp();
    }

    private void HandleHover(Adventurer adventurer)
    {
        if (adventurer != hoveredAdventurer)
        {
            // 之前的悬停单位退出
            if (hoveredAdventurer != null)
            {
                hoveredAdventurer.OnEventMouseExit();
            }

            // 新的悬停单位进入
            if (adventurer != null)
            {
                adventurer.OnEventMouseEnter();
            }

            hoveredAdventurer = adventurer;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}