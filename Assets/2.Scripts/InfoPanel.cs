using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
<<<<<<< HEAD
using UnityEditor.U2D.Aseprite;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] Text _nicknameText;
    [SerializeField] Text _uidText;
    [SerializeField] Image _selectedTankImage;
    [SerializeField] Transform _tankSlotContainer;
    [SerializeField] GameObject _tankSlotPrefab;

    [SerializeField] Button _applyBtn;
    [SerializeField] Button _confirmBtn;
    [SerializeField] Button[] _closeBtns;
    [SerializeField] Sprite defaultTankSprite;


    string selectedTankId;

=======

public class InfoPanel : MonoBehaviour
{
    [Header("À¯Àú Á¤º¸ ÅØ½ºÆ®")]
    [SerializeField] private Text _nicknameText;
    [SerializeField] private Text _uidText;

    [Header("¹öÆ°")]
    [SerializeField] private Button _applyBtn;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button[] _closeBtns;

    [Header("ÅÊÅ© ½½·Ô °ü·Ã")]
    [SerializeField] private Transform[] tankSlots; // ½½·Ô ÇÏÀ§¿¡ ÀÌ¹ÌÁö°¡ ÀÖ¾î¾ß ÇÔ
    [SerializeField] private Image _equippedTankImage; // ÀåÂø ÅÊÅ© ¹Ì¸®º¸±â ÀÌ¹ÌÁö

    private List<TankDataSO> allTankDataList = new List<TankDataSO>();
    private TankDataSO _selectedTank; // ¼±ÅÃµÈ ÅÊÅ© µ¥ÀÌÅÍ

    private Image _previousSlotImage;
    private Color _defaultColor = Color.white;
    private Color _selectedColor = Color.yellow;
>>>>>>> 7b40d61 (0328 íƒ±í¬ ì„ íƒ ê¸°ëŠ¥ ì¶”ê°€)

    void Start()
    {
        _applyBtn.onClick.AddListener(OnClickApply);
        _confirmBtn.onClick.AddListener(OnClickConfirm);
        foreach (Button btn in _closeBtns)
        {
            btn.onClick.AddListener(OnClickClose);
        }
<<<<<<< HEAD
    }

    void LoadOwnedTankSlots(List<string> tankList)
    {
        // ±âÁ¸ ½½·Ô Á¦°Å
        foreach (Transform child in _tankSlotContainer)
            Destroy(child.gameObject);

        foreach (string tankId in tankList)
        {
            GameObject slot = Instantiate(_tankSlotPrefab, _tankSlotContainer);
            Image image = slot.GetComponent<Image>();

            Button button = slot.GetComponent<Button>();
            button.onClick.AddListener(() => OnSelectTank(tankId, image));
        }
    }

    Image previouslySelectedImage;

    void OnSelectTank(string tankId, Image image)
    {
        // Å×µÎ¸® Ã³¸®
        if (previouslySelectedImage != null)
            previouslySelectedImage.color = Color.white;

        image.color = Color.green;
        previouslySelectedImage = image;

        selectedTankId = tankId;
    }


    void OnClickApply()
    {
        Debug.Log("Àû¿ë ¹öÆ° Å¬¸¯");
    }
=======

        InitTankData();
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

                // ÇÚµé·¯ ÀÚµ¿ ºÎÂø
                TankSlotClickHandler handler = imageTransform.GetComponent<TankSlotClickHandler>();
                if (handler == null)
                {
                    handler = imageTransform.gameObject.AddComponent<TankSlotClickHandler>();
                }

                handler.slotIndex = i;
                handler.infoPanel = this;
            }
            else
            {
                tankSlots[i].gameObject.SetActive(false);
            }
        }
    }


    void OnClickApply()
    {
        if (_selectedTank != null)
        {
            _equippedTankImage.sprite = _selectedTank._tankSprite;
            Debug.Log($" ÅÊÅ© ÀåÂø ¿Ï·á: {_selectedTank._tankName}");
        }
        else
        {
            Debug.LogWarning(" ¼±ÅÃµÈ ÅÊÅ©°¡ ¾ø½À´Ï´Ù.");
        }
    }

>>>>>>> 7b40d61 (0328 íƒ±í¬ ì„ íƒ ê¸°ëŠ¥ ì¶”ê°€)
    void OnClickConfirm()
    {
        Debug.Log("È®ÀÎ ¹öÆ° Å¬¸¯");
    }

    void OnClickClose()
    {
        gameObject.SetActive(false);
    }

<<<<<<< HEAD
=======
    public void OnSelectTankFromSlot(int index)
    {
        _selectedTank = allTankDataList[index];
        Debug.Log($" ¼±ÅÃµÈ ÅÊÅ©: {_selectedTank._tankName}");

        // ÇöÀç ½½·Ô ÀÌ¹ÌÁö
        Image currentSlotImage = tankSlots[index].GetComponent<Image>();

        if (currentSlotImage != null)
        {
            // ÀÌÀü ¼±ÅÃ ÇØÁ¦
            if (_previousSlotImage != null)
                _previousSlotImage.color = _defaultColor;

            // ÇöÀç ¼±ÅÃ °­Á¶
            currentSlotImage.color = _selectedColor;
            _previousSlotImage = currentSlotImage;
        }
    }

>>>>>>> 7b40d61 (0328 íƒ±í¬ ì„ íƒ ê¸°ëŠ¥ ì¶”ê°€)

}
