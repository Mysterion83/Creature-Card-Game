using System.Collections.Generic;
using UnityEngine;

public class TeamColours : MonoBehaviour
{
    [Header("Colours")]
    [SerializeField] Color humanColour = new Color(0.2f, 0.5f, 1f);   
    [SerializeField] Color aiColour = new Color(1f, 0.25f, 0.2f);     

    [Header("Marker")]
    [SerializeField] Material markerMaterial;    
    [SerializeField] float markerSize = 0.8f;    
    [SerializeField] float markerHeight = 0.05f;

    readonly HashSet<Creature> marked = new();
    Material humanMaterial, aiMaterial;

    void Start()
    {
        if (markerMaterial == null)
        {            
            enabled = false;
            return;
        }
        humanMaterial = new Material(markerMaterial) { color = humanColour };
        aiMaterial = new Material(markerMaterial) { color = aiColour };
    }

    void Update()
    {        
        foreach (Player player in PlayerRegistry.MasterList)
        {
            if (player == null) continue;

            foreach (Creature creature in player.ActiveCreatures)
            {
                if (creature != null && marked.Add(creature))
                    AddMarker(creature, player.Data.IsAI ? aiMaterial : humanMaterial);
            }
        }

        marked.RemoveWhere(c => c == null);   
    }

    void AddMarker(Creature creature, Material material)
    {
        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "TeamMarker";
        Destroy(disc.GetComponent<Collider>());  

        
        Transform t = disc.transform;
        t.SetParent(creature.transform, true);
        t.position = creature.transform.position + Vector3.up * markerHeight;
        t.rotation = Quaternion.identity;

        
        Vector3 parentScale = creature.transform.lossyScale;
        t.localScale = new Vector3(
            markerSize / parentScale.x,
            0.02f / parentScale.y,
            markerSize / parentScale.z);

        disc.GetComponent<Renderer>().sharedMaterial = material;
    }
}