using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    //area1 休息区
    //area2 轻工厂
    //area3 重工厂
    //area4 地狱工厂

    //后续有需求可以用SO填数据
    public float area1Time;//冒险者在每个区域的工作效率，可以设置为获取金币时间间隔或者获取金币倍率，我个人倾向于获取金币时间间隔
    public float area1Damage;
    public float area2Time;
    public float area2Damage;
    public float area3Time;
    public float area3Damage;
    public float area4Time;
    public float area4Damage;
    public float adventurerEfficiency;//冒险者效率，可以设置为每隔一段时间获得的金币数
    public float coinCount;//当前拥有的金币数

    void OnEnable()
    {
        EventManager.AddListener<int>("UnlockTechTree", TechTreeEffect);
    }

    void OnDisable()
    {
        EventManager.RemoveListener<int>("UnlockTechTree", TechTreeEffect);
    }

    public void AdventurerUpgrade(float amount)//可以升级时调用也可以解锁科技树提高工人效率时调用
    {
        adventurerEfficiency += amount;
    }

    public void AreaUpgrade(int areaIndex, float amount)//提高工作区效率的方法，乘算减少时间间隔，看情况实现
    {

    }

    public void ChangeCoins(float amount)
    {
        coinCount += amount;
        //UI更新逻辑
    }


    private void TechTreeEffect(int id)
    {
        switch (id)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
            case 11:
                break;
            case 12:
                break;
            case 13:
                break;
            case 14:
                break;
            case 15:
                break;
            case 16:
                break;
            case 17:
                break;
            case 18:
                break;
            case 20:
                break;
            case 21:
                break;
            case 22:
                break;
            case 23:
                break;
            case 24:
                break;
        }
    }
}
