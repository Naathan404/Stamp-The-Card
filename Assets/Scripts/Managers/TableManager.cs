using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TableManager : Singleton<TableManager>
{
    [Header("Seat UI")]
    public Transform BottomSeatTransform;
    public Transform TopSeatTransform;
    public TextMeshProUGUI BottomNameUI;
    public TextMeshProUGUI TopNameUI;

    public TextMeshProUGUI BottomHP;
    public TextMeshProUGUI TopHP;

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
            BottomNameUI.text = playerData.DisplayName.ToString();
        else    // cập nhật tên cho đối thủ
            TopNameUI.text = playerData.DisplayName.ToString();
    }

    public void UpdateGameplayUI(bool amIHost)
    {
        if(amIHost)
        {
            BottomHP.text = $"HP: {GameManager.Instance.HostHP}";
            TopHP.text = $"HP: {GameManager.Instance.ClientHP}";
        }
        else
        {
            BottomHP.text = $"HP: {GameManager.Instance.ClientHP}";
            TopHP.text = $"HP: {GameManager.Instance.HostHP}";
        }
    }
}
