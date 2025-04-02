using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InfoPanel : MonoBehaviour
{
    [Header("���� ���� �ؽ�Ʈ")]
    [SerializeField] private Text _nicknameText;
    [SerializeField] private Text _uidText;

    [Header("��ư")]
    [SerializeField] private Button _applyBtn;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button[] _closeBtns;

    [Header("��ũ ���� ����")]
    [SerializeField] private Transform[] tankSlots;
    [SerializeField] private Image _equippedTankImage;

    private List<TankDataSO> allTankDataList = new List<TankDataSO>();
    private TankDataSO _selectedTank;

    private Image _previousSlotImage;
    private Color _defaultColor = Color.white;
    private Color _selectedColor = Color.yellow;

    FirebaseManager _fm;

    void Start()
    {
        _fm = FirebaseManager._instance;

        _applyBtn.onClick.AddListener(OnClickApply);
        _confirmBtn.onClick.AddListener(OnClickConfirm);
        foreach (Button btn in _closeBtns)
        {
            btn.onClick.AddListener(OnClickClose);
        }

        InitUserData();
        InitTankData();
        InitEquippedTankImage();
    }

    void InitUserData()
    {
        _nicknameText.text = _fm.userVO.NickName;
        _uidText.text = _fm.userVO.UID;
    }

    void InitTankData()
    {
        allTankDataList.Clear();
        TankDataSO[] tankArray = Resources.LoadAll<TankDataSO>("Tank");
        allTankDataList.AddRange(tankArray);

        for (int i = 0; i < tankSlots.Length; i++)
        {
            if (i < allTankDataList.Count)
            {
                Transform imageTransform = tankSlots[i].GetChild(0);
                Image image = imageTransform.GetComponent<Image>();
                image.sprite = allTankDataList[i]._tankSprite;
                image.gameObject.SetActive(true);

                TankSlotClickHandler handler = imageTransform.GetComponent<TankSlotClickHandler>();
                if (handler == null)
                    handler = imageTransform.gameObject.AddComponent<TankSlotClickHandler>();

                handler.slotIndex = i;
                handler.infoPanel = this;
            }
            else
            {
                tankSlots[i].gameObject.SetActive(false);
            }
        }
    }

    void InitEquippedTankImage()
    {
        Sprite equippedSprite = TankUtil.GetTankSprite(_fm.userVO.NowTank);
        if (equippedSprite != null)
        {
            _equippedTankImage.sprite = equippedSprite;
        }
    }

    public void OnSelectTankFromSlot(int index)
    {
        _selectedTank = allTankDataList[index];
        Debug.Log($"���õ� ��ũ: {_selectedTank._tankName}");

        Image currentSlotImage = tankSlots[index].GetComponent<Image>();
        if (currentSlotImage != null)
        {
            if (_previousSlotImage != null)
                _previousSlotImage.color = _defaultColor;

            currentSlotImage.color = _selectedColor;
            _previousSlotImage = currentSlotImage;
        }
    }

    void OnClickApply()
    {
        if (_selectedTank != null)
        {
            _equippedTankImage.sprite = TankUtil.GetTankSprite(_selectedTank._tankName);
            Debug.Log($"��ũ ���� �Ϸ�: {_selectedTank._tankName}");

            _fm.userVO.NowTank = _selectedTank._tankName;
        }
        else
        {
            Debug.LogWarning("���õ� ��ũ�� �����ϴ�.");
        }
    }

    void OnClickConfirm()
    {
        OnClickApply();
        gameObject.SetActive(false);
        Debug.Log("Ȯ�� ��ư Ŭ��");
    }

    void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}