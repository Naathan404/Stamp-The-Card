using TMPro;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _bottomHpText;
    [SerializeField] private TextMeshProUGUI _topHpText;
    [SerializeField] private TextMeshProUGUI _bottomNameText;
    [SerializeField] private TextMeshProUGUI _topNameText;
    [SerializeField] private TextMeshPro _bottomStampCount;
    [SerializeField] private TextMeshPro _topStampCount;
    [Header("Seat Transforms")]
    [SerializeField] private Transform _bottomSeatTransform;
    [SerializeField] private Transform _topSeatTransform;

    public static UIManager Instance;
    private void Awake()
    {
        if(Instance != null  && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        _bottomHpText.text = "15";
        _topHpText.text = "15";
        _bottomStampCount.text = "9";
        _topStampCount.text = "9";
    }

    public void UpdateHpTexts(bool amIHost)
    {
        if(amIHost)
        {
            _bottomHpText.text = $"HP: {GameManager.Instance.HostHP}";
            _topHpText.text = $"HP: {GameManager.Instance.ClientHP}";
        }
        else
        {
            _bottomHpText.text = $"HP: {GameManager.Instance.ClientHP}";
            _topHpText.text = $"HP: {GameManager.Instance.HostHP}";
        }
    }

    public void UpdateStampCount(bool amIhost)
    {
        if(amIhost)
        {
            _bottomStampCount.text = GameManager.Instance.NetworkedHostStampCount.ToString();
            _topStampCount.text = GameManager.Instance.NetworkedClientStampCount.ToString();
        }
        else
        {
            _bottomStampCount.text = GameManager.Instance.NetworkedClientStampCount.ToString(); 
            _topStampCount.text = GameManager.Instance.NetworkedHostStampCount.ToString();
        }
    }

    public void SetSeatPosition(PlayerNetworkData playerData, bool isYourself)
    {
        if(isYourself)
        {
            playerData.gameObject.transform.position = _bottomSeatTransform.position;
            playerData.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.deepSkyBlue;
        }
        else
        {
            playerData.gameObject.transform.position = _topSeatTransform.position;
            playerData.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.softRed;

        }
    }

    public void UpdateNameUI(PlayerNetworkData playerData, bool isYourSelf)
    {
        Debug.Log("[UIMANAGER] Cập nhật tên hiển thị");
        // cập nhật tên cho bản thân
        if(isYourSelf)
            _bottomNameText.text = playerData.DisplayName.ToString();
        else    // cập nhật tên cho đối thủ
            _topNameText.text = playerData.DisplayName.ToString();
    }
}
