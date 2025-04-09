using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Map Data List")]
    [SerializeField] private List<MapData> _mapDataList = new List<MapData>();

    private MapData _curMap = null;
    private Ground _selectedGround = null;
    private int _seletedMapIndex = 0;

    public void SpawnSelectMap(eMapType eSelectMap)
    {
        _curMap = null;

        if (eSelectMap == eMapType.Random)
        {
            // �������� ���� ������ ���
            _seletedMapIndex = Random.Range((int)eMapType.Valley, (int)eMapType.Max);
        }
        else
        {
            _seletedMapIndex = (int)eSelectMap;
        }

        // �� �����͸� ���ؼ� ����
        _curMap = _mapDataList[_seletedMapIndex];

        // �İ� ����
        Instantiate(_curMap.backgroundPrefab);
        // ���� ����
        GameObject goFore = Instantiate(_curMap.foregroundPrefab);

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
        return _curMap.mapSize;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // �߽� ��ǥ (���� ������Ʈ ��ġ ����)
        Vector3 center = transform.position;

        if(_curMap != null)
        {
            // Z���� 0���� ����, X�� Y�� ���
            Vector3 size = new Vector3(_curMap.mapSize.x, _curMap.mapSize.y, 0f);

            Gizmos.DrawWireCube(center, size);
        }
    }
}
