using UnityEngine;

[System.Serializable]
public class Tile : MonoBehaviour
{
    [Header("Tile Definition")]
    public TileData Data;
    
    [Header("Active Info")]
    public int RegionID = -1;
    public Creature Occupant;

    private TileData _prevData;
    private GameObject _model;
    private GameObject _prevModel;
    
    private void OnValidate()
    {
        if (Data == _prevData) return;
        OnDataUpdate();
    }

    public void Init()
    {
        OnDataUpdate();
    }
    
    private void OnDataUpdate()
    {
        if (!Data) return;
        _prevData = Data;
        
        _prevModel = _model;
        _model = Data.RenderObjectPrefab;
        if (!_model)
        {
            Debug.LogWarning($"Tiles using {Data} have no render object prefab, please assign one in {Data}'s Inspector");
            return;
        }
        _model = Instantiate(_model, transform);
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall+=()=>
        {
           if (_prevModel) DestroyImmediate(_prevModel);
        };
        #endif
    }
}