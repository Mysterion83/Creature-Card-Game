using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Element", menuName = "Project/Element")]
public class Element : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public Color Color;

    public Dictionary<Element, float> ElementModifiers; 
}
