using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private Texture2D _handCursor;
    [SerializeField] private Texture2D _attackCursor;
    [SerializeField] private Vector2 _hotSpot = Vector2.zero; 
    [SerializeField] private CursorMode _cursorMode = CursorMode.Auto;

    public Texture2D DefaultCursor => _cursorTexture;
    public Texture2D HandCursorTexture => _handCursor;
    public Texture2D AttackCursorTexture => _attackCursor;

    [Header("Pooling Settings")]
    [SerializeField] private GameObject _clickEffectPrefab;
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxSize = 50;
    [SerializeField] private float _effectDuration = 0.5f;

    private ObjectPool<ParticleSystem> _effectPool;

    public static CursorManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (_cursorTexture != null)
        {
            Cursor.SetCursor(_cursorTexture, _hotSpot, _cursorMode);
        }

        _effectPool = new ObjectPool<ParticleSystem>(
            createFunc: CreateEffect,
            actionOnGet: OnTakeEffectFromPool,
            actionOnRelease: OnReturnEffectToPool,
            actionOnDestroy: OnDestroyEffect,
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlayClickEffect();
        }
    }

    private void PlayClickEffect()
    {
        if (_clickEffectPrefab == null) return;

        ParticleSystem effect = _effectPool.Get();

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        mouseWorldPos.z = -5f; 
        
        effect.transform.position = mouseWorldPos;

        StartCoroutine(ReturnToPoolRoutine(effect, _effectDuration));
    }

    private IEnumerator ReturnToPoolRoutine(ParticleSystem effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        _effectPool.Release(effect);
    }

    #region POOL LIFECYCLE HOOKS
    private ParticleSystem CreateEffect() 
    {
        GameObject obj = Instantiate(_clickEffectPrefab, transform);
        return obj.GetComponent<ParticleSystem>();
    }
    private void OnTakeEffectFromPool(ParticleSystem effect)
    {
        effect.gameObject.SetActive(true);
        effect.Play();
    }
    private void OnReturnEffectToPool(ParticleSystem effect)
    {
        effect.Stop();
        effect.Clear();
        effect.gameObject.SetActive(false);
    }
    private void OnDestroyEffect(ParticleSystem effect) => Destroy(effect.gameObject);
    #endregion
}