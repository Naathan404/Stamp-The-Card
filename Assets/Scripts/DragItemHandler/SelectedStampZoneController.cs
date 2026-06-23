using UnityEngine;

public class SelectedStampZoneController : MonoBehaviour
{
    public void SaveSelectedStampsToLocal()
    {
        LocalPlayerData.SelectedStamps.Clear();

        foreach (Transform child in transform)
        {
            StampSlotUI stampSlotUI = child.GetComponent<StampSlotUI>();

            if (stampSlotUI != null)
            {
                LocalPlayerData.SelectedStamps.Add(stampSlotUI.stampInstance);
            }
        }

        Debug.Log("[SaveSelectedStampController] Da luu danh sach selected stamp vao local");
    }
}
