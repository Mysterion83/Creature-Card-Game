using UnityEngine;

[System.Serializable]
public class TileDecoration
{
    [Header("Prefab to Spawn")]
    public GameObject Prefab;
    [Header("Spawning Settings")]
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    public Vector3 Scaling;

    public void Spawn(Transform parentTile)
    {
        if (!Prefab)
        {
            Debug.LogWarning($"[TileDecoration] Prefab not set within {this}, so it could not be spawned");
            return;
        }

        Vector3 targetPos = parentTile.position + PositionOffset;
        Quaternion targetRot = parentTile.rotation * Quaternion.Euler(RotationOffset);

        GameObject spawnedProp = Object.Instantiate(Prefab, targetPos, targetRot, parentTile);
        
        if (Scaling == Vector3.zero)
        {
            spawnedProp.transform.localScale = Vector3.one;
            Debug.LogWarning($"[TileDecoration] Prefab '{Prefab.name}' spawned with a scale modifier of (0,0,0) on tile {parentTile.name}. Resetting to (1,1,1)");
        }
        else spawnedProp.transform.localScale = Scaling;
    }
}