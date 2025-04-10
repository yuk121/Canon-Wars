using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float EDGE_THRESHOLD = 5f;
    [SerializeField] private float _camMoveSpeed = 5f;
    private Transform _trans;
    private Transform _curTunrPlayerTrans;
    private Transform _curShellTrans;

    private Vector2 _mapSize = Vector2.zero;
    private Vector2 _mapBottomLeft = Vector2.zero;
    private Vector2 _mapTopRight = Vector2.zero;

    private Vector3 _prevMousePos = Vector2.zero;

    private float _camSizeWidth;
    private float _camSizeHegiht;
    private float _newPosX = 0f;
    private float _newPosY = 0f;

    private bool _isInit = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
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

    public void PlayerFocusing(PlayerController curPlayer)
    {
        if (curPlayer == null)
            return;

        // �÷��̾� ��Ŀ��
        _newPosX = Mathf.Clamp(curPlayer.transform.position.x, _mapBottomLeft.x, _mapTopRight.x);
        _newPosY = Mathf.Clamp(curPlayer.transform.position.y, _mapBottomLeft.y, _mapTopRight.y);

        Vector3 newPos = new Vector3(_newPosX, _newPosY, _trans.position.z);

        _trans.position = newPos;
    }

    // Update is called once per frame
    void Update()
    {
        if(_isInit == true && GameInitializer.Instance != null)
        {
  
            if (GameInitializer.Instance.CurShellTrans != null)
            {     
                // ��ź ��Ŀ��
                _curShellTrans = GameInitializer.Instance.CurShellTrans;

                // ī�޶� �� ���� ������ �ȳ������� ����
                _newPosX = Mathf.Clamp(_curShellTrans.position.x, _mapBottomLeft.x, _mapTopRight.x);
                _newPosY = Mathf.Clamp(_curShellTrans.position.y, _mapBottomLeft.y, _mapTopRight.y);

                Vector3 newPos = new Vector3(_newPosX, _newPosY, _trans.position.z);
                _trans.position = newPos;
            }
            else
            {
                if(IsMouseInsideGameView() ==true)
                {
                    // ��ź�� ��� �������� ȭ�� ������ ����
                    MoveCameraAroundMap();
                }
            }
        }

    }

    // ���콺�� ���Ӿ� ȭ��ۿ� �־ �� �̵��� �������� �ʵ���
    private bool IsMouseInsideGameView()
    {
        //Vector3 mousePos = Input.mousePosition;
        //return mousePos.x >= 0 && mousePos.x <= Screen.width &&
        //       mousePos.y >= 0 && mousePos.y <= Screen.height;
        return true;
    }

    private void MoveCameraAroundMap()
    {
        // ���콺 ��ġ �˾ƿ���
        Vector3 mousePos = Input.mousePosition;

        // ī�޶�� ���콺���� �Ÿ� Ȯ��
        float topDis = Camera.main.pixelHeight - mousePos.y;
        float bottomDis = mousePos.y;
        float rightDis = Camera.main.pixelWidth - mousePos.x;
        float leftDis = mousePos.x;

        Vector3 moveDir = Vector3.zero;

        // �Ѱ�ġ���� ������쿡�� �̵� (ȭ�� ���� ���� �ٴٸ�����)
        if (topDis <= EDGE_THRESHOLD)
            moveDir += Vector3.up;
        if (bottomDis <= EDGE_THRESHOLD)
            moveDir += Vector3.down;
        if (rightDis <= EDGE_THRESHOLD)
            moveDir += Vector3.right;
        if (leftDis <= EDGE_THRESHOLD)
            moveDir += Vector3.left;

        Vector3 movePos = moveDir.normalized * Time.deltaTime * _camMoveSpeed;

        // ī�޶� ��ġ ����
        float newPosX = Mathf.Clamp(transform.position.x + movePos.x, _mapBottomLeft.x, _mapTopRight.x);
        float newPosY = Mathf.Clamp(transform.position.y + movePos.y, _mapBottomLeft.y, _mapTopRight.y);

        Vector3 newPos = new Vector3(newPosX, newPosY, _trans.position.z);
        transform.position = newPos;
    }

    private void MapSizeCheck()
    {
        _mapSize = GameInitializer.Instance.GetMapSize() * 0.5f;

        // �� ����� �������� �ʴ� ���
        if (_mapSize == Vector2.zero)
            return;

        _mapBottomLeft = new Vector2(-_mapSize.x + _camSizeWidth, -_mapSize.y+ _camSizeHegiht);
        _mapTopRight = new Vector2(_mapSize.x - _camSizeWidth, _mapSize.y - _camSizeHegiht);
    }
}
