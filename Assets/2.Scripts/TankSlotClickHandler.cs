using UnityEngine;
using UnityEngine.EventSystems;

public class TankSlotClickHandler : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;
    public InfoPanel infoPanel;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (infoPanel != null)
        {
            infoPanel.OnSelectTankFromSlot(slotIndex);
        }
    }
}
