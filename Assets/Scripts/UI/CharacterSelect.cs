using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BaseCharacter[] characterList;
    private int selectedIndex = 0; // Chỉ số nhân vật hiện tại

    [Header("UI References")]
    [SerializeField] private Button backCharButton;
    [SerializeField] private Button nextCharButton;
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterDescription;

    void Start()
    {
        // Gán sự kiện cho nút bấm
        nextCharButton.onClick.AddListener(NextCharacter);
        backCharButton.onClick.AddListener(BackCharacter);

        UpdateCharacterUI();
    }

    public void NextCharacter()
    {
        selectedIndex++;
        if (selectedIndex >= characterList.Length)
        {
            selectedIndex = 0; // Quay lại nhân vật đầu tiên
        }
        UpdateCharacterUI();
    }

    public void BackCharacter()
    {
        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex = characterList.Length - 1; // Nhảy xuống nhân vật cuối cùng
        }
        UpdateCharacterUI();
    }

    private void UpdateCharacterUI()
    {
        if (characterList.Length == 0) return;
            
        BaseCharacter character = characterList[selectedIndex];

        
        characterImage.sprite = character.charImage; // Cập nhật hình ảnh nhân vật
        this.characterDescription.text = "Skill: " + character.characterDescription;
        // Gợi ý: Bạn có thể thêm hiệu ứng Fade hoặc Animation ở đây
    }
}