using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform _trans;
    private Transform _curTunrPlayerTrans;
    private Transform _curShellTrans;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _trans = transform;    
    }

    // Update is called once per frame
    void Update()
    {
        
        if(GameInitializer.Instance != null)
        {
            // TODO : ī�޶� �� ���� ������ �ȳ������� ���� �����ϱ�
            Vector2 mapSize = GameInitializer.Instance.GetMapSize();    

            if(GameInitializer.Instance.CurShellTrans != null) 
            {
                _curShellTrans = GameInitializer.Instance.CurShellTrans;
                Vector3 newPos = new Vector3(_curShellTrans.position.x, _curShellTrans.position.y, _trans.position.z);
                _trans.position = newPos;
            }
            else if(GameInitializer.Instance.CurTurnPlayer != null)
            {
                _curTunrPlayerTrans = GameInitializer.Instance.CurTurnPlayer.transform;
                Vector3 newPos = new Vector3(_curTunrPlayerTrans.position.x, _curTunrPlayerTrans.position.y, _trans.position.z);
                _trans.position = newPos;
            }
        }
    }
}
