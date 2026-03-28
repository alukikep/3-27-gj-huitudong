using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Worker")]
public class Worker : ScriptableObject
{
    public Sprite sprite;
    public float initialEffeciency;
    public float maxHealth;
}
