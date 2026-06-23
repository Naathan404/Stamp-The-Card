using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class WavyTextTMP : MonoBehaviour
{
    [Header("Cấu hình Sóng (Wave)")]
    public float waveSpeed = 5f;       // Tốc độ sóng cuộn
    public float waveHeight = 5f;      // Độ cao của sóng (chữ nảy lên bao nhiêu)
    public float waveFrequency = 1f;   // Độ giãn của sóng giữa các chữ cái

    private TMP_Text _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        // Yêu cầu TextMeshPro tính toán lại hình học trước khi mình bẻ cong nó
        _textComponent.ForceMeshUpdate();
        var textInfo = _textComponent.textInfo;

        // Duyệt qua từng chữ cái một trong câu
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            // Bỏ qua khoảng trắng 
            if (!charInfo.isVisible) continue;

            // Lấy 4 điểm (Vertex) tạo nên hình vuông chứa chữ cái đó
            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            
            // Công thức hàm sine: Tính độ lệch Y dựa trên Thời gian và Vị trí của chữ cái
            float offsetY = Mathf.Sin(Time.time * waveSpeed + i * waveFrequency) * waveHeight;

            // Bẻ cong cả 4 góc của chữ cái lên/xuống theo biến offsetY
            for (int j = 0; j < 4; j++)
            {
                verts[charInfo.vertexIndex + j].y += offsetY;
            }
        }

        // Cập nhật lại khung lưới (Mesh) để hiển thị ra màn hình
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            _textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}