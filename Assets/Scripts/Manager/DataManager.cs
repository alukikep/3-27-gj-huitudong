using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public float area1Time;//冒险者在每个区域的工作效率，可以设置为获取金币时间间隔或者获取金币倍率，我个人倾向于获取金币时间间隔
    public float area2Time;
    public float area3Time;
    public float area4Time;
    public float adventurerEfficiency;//冒险者效率，可以设置为每隔一段时间获得的金币数
    public float coinCount;//当前拥有的金币数

    public void AdventurerUpgrade(float amount)//可以升级时调用也可以解锁科技树提高工人效率时调用
    {
        adventurerEfficiency += amount;
    }

    public void AreaUpgrade(int areaIndex, float amount)//提高工作区效率的方法，乘算减少时间间隔，看情况实现
    {

    }

    public void ChangeCoins(float amount)
    {
        coinCount+=amount;
        //UI更新逻辑
    }
}
