using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum eMapType
{
    Random = -1,
    Valley,
    Forest,
    City,
    DesertTemple,
    Max
}
public class MapSpawner : MonoBehaviour
{
    [Header("Map Setting")]
    [SerializeField] private float _mapSizeWidth = 0f;
    [SerializeField] private float _mapSizeHeight = 0f;

    [Header("Map Prefab")]
    [SerializeField] private List<GameObject> _mapBackgroundList = new List<GameObject>();
    [SerializeField] private List<GameObject> _mapForegroundList = new List<GameObject>();

    private Ground _selectedGround = null;
    private int _seletedMapIndex = 0;

    public void SpawnSelectMap(eMapType eSelectMap)
    {
        if (eSelectMap == eMapType.Random)
        {
            // ·£´ýÀ¸·Î ¸ÊÀ» ¼±ÅÃÇÑ °æ¿ì
            _seletedMapIndex = Random.Range((int)eMapType.Valley, (int)eMapType.Max);
        }
        else
        {
            _seletedMapIndex = (int)eSelectMap;
        }
           
        // ·£´ýÀ¸·Î ¸Ê ¼±ÅÃ
        Instantiate(_mapBackgroundList[_seletedMapIndex]);
        GameObject goFore = Instantiate(_mapForegroundList[_seletedMapIndex]);

        _selectedGround = goFore.GetComponent<Ground>();
        _selectedGround.Init();
    }

    public List<Vector3> GetSpawnPosPList()
    {
        List<Vector3> posList = new List<Vector3>();

        foreach (Transform trans in _selectedGround.SpawnTransList)
        {
            posList.Add(trans.position);
        }

        return posList;
    }

    public Vector2 GetMapSize()
    {
        return new Vector2(_mapSizeWidth, _mapSizeHeight);
    }
}
