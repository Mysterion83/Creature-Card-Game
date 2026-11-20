using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Element", menuName = "Project/Element")]
public class Element : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public Color Color;

    /// <summary>
    /// Elemental Advantages and disadvantages between other elements
    /// </summary>
    [SerializeField] public Dictionary<Element, float> ElementModifiers;
}
