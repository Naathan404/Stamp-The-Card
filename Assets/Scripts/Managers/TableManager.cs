using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TableManager : Singleton<TableManager>
{
    [Header("Seat UI")]
    public Transform BottomSeatTransform;
    public Transform TopSeatTransform;
    public TextMeshProUGUI bottomNameUI;
    public TextMeshProUGUI topNameUI;

    public void Start()
    {

    }

    public void SetSeatPosition(PlayerNetworkData playerData, bool isYourself)
    {
        if(isYourself)
        {
            playerData.gameObject.transform.position = BottomSeatTransform.position;
            playerData.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.deepSkyBlue;
        }
        else
        {
            playerData.gameObject.transform.position = TopSeatTransform.position;
            playerData.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.softRed;

        }
    }

    public void UpdateNameUI(PlayerNetworkData playerData, bool isYourSelf)
    {
        // cập nhật tên cho bản thân
        if(isYourSelf)
            bottomNameUI.text = playerData.Username.ToString();
        else    // cập nhật tên cho đối thủ
            topNameUI.text = playerData.Username.ToString();
    }
}
