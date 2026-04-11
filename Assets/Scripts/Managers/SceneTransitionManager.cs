using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : Singleton<SceneTransitionManager>
{
    [Header("Transition Settings")]
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Transform posA;
    [SerializeField] private Transform posB;
    [SerializeField] private Transform posC;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private CanvasGroup _canvasGroup;

    private bool _isTransitioning = false;

    private void Start()
    {
        _canvasGroup.blocksRaycasts = false;    
    }

    public void LoadSceneAsync(string sceneName)
    {
        _panel.transform.position = posA.position;
        if(_isTransitioning) return;
        StartCoroutine(HandleLoadSceneAsync(sceneName));
    }

    private IEnumerator HandleLoadSceneAsync(string sceneName)
    {
        _isTransitioning = true;
        _canvasGroup.blocksRaycasts = true;
        yield return _panel.transform.DOMoveX(posB.transform.position.x, _duration).SetEase(Ease.InOutQuint).WaitForCompletion();
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while(!loadOp.isDone) yield return null;
        yield return _panel.transform.DOMoveX(posC.transform.position.x, _duration).SetEase(Ease.InOutQuint).WaitForCompletion();
        _canvasGroup.blocksRaycasts = false;
        _isTransitioning = false;
    }    
}
