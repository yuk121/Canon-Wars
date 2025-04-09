using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform _trans;
    private Transform _curTunrPlayerTrans;
    private Transform _curShellTrans;

    private Vector2 _mapBottomLeft = Vector2.zero;
    private Vector2 _mapTopRight = Vector2.zero;

    private float _camSizeWidth;
    private float _camSizeHegiht;
    private float _newPosX = 0f;
    private float _newPosY = 0f;

    private bool _isInit = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Init()
    {
        _trans = transform;
        _camSizeHegiht = Camera.main.orthographicSize;
        _camSizeWidth = _camSizeHegiht * Camera.main.aspect;

        // �� ���� �ҷ�����
        MapSizeCheck();

        _isInit = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(_isInit == true && GameInitializer.Instance != null)
        {
            if (GameInitializer.Instance.CurShellTrans != null) 
            {
                _curShellTrans = GameInitializer.Instance.CurShellTrans;

                // ī�޶� �� ���� ������ �ȳ������� ����
                _newPosX = Mathf.Clamp(_curShellTrans.position.x, _mapBottomLeft.x, _mapTopRight.x);
                _newPosY = Mathf.Clamp(_curShellTrans.position.y, _mapBottomLeft.y, _mapTopRight.y);

                Vector3 newPos = new Vector3(_newPosX, _newPosY, _trans.position.z);
                _trans.position = newPos;
            }
            else if(GameInitializer.Instance.CurTurnPlayer != null && GameInitializer.Instance.IsCurPlayerTurnWait() == false)
            {
                _curTunrPlayerTrans = GameInitializer.Instance.CurTurnPlayer.transform;

                _newPosX = Mathf.Clamp(_curTunrPlayerTrans.position.x, _mapBottomLeft.x, _mapTopRight.x);
                _newPosY = Mathf.Clamp(_curTunrPlayerTrans.position.y, _mapBottomLeft.y, _mapTopRight.y);

                Vector3 newPos = new Vector3(_newPosX, _newPosY, _trans.position.z);

                _trans.position = newPos;
            }
        }
    }

    private void MapSizeCheck()
    {
        Vector2 mapSize = GameInitializer.Instance.GetMapSize();

        // �� ����� �������� �ʴ� ���
        if (mapSize == Vector2.zero)
            return;

        _mapBottomLeft = new Vector2(-mapSize.x / 2f + _camSizeWidth, -mapSize.y / 2f + _camSizeHegiht);
        _mapTopRight = new Vector2(mapSize.x / 2f - _camSizeWidth, mapSize.y / 2f - _camSizeHegiht);
    }
}
