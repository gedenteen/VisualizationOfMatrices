using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    [SerializeField] private MatricesVisualizer _matricesVisualizer;
    [SerializeField] private Button _buttonFindWithoutVisualization;
    [SerializeField] private Button _buttonFindWithVisualization;

    private void Awake()
    {
        _buttonFindWithoutVisualization.onClick.AddListener(
            CallFindWithoutVisualization);
        _buttonFindWithVisualization.onClick.AddListener(
            CallFindWithVisualization);
    }

    private void OnDestroy()
    {
        _buttonFindWithoutVisualization.onClick.RemoveListener(
            CallFindWithoutVisualization);
        _buttonFindWithVisualization.onClick.RemoveListener(
            CallFindWithVisualization);
    }

    private void CallFindWithoutVisualization()
    {
        _matricesVisualizer.FindPossibleOffsets(false).Forget();
    }

    private void CallFindWithVisualization()
    {
        _matricesVisualizer.FindPossibleOffsets(true).Forget();
    }
}
