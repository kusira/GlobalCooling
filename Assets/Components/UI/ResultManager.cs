using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// リザルト画面を管理するスクリプト
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("リザルト画面の黒い背景")]
    [SerializeField] private GameObject blackGround;
    
    [Tooltip("リザルトパネル")]
    [SerializeField] private GameObject resultPanel;
    
    [Header("Animation Settings")]
    [Tooltip("フェードイン/アウトの時間（秒）")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    [Tooltip("ResultPanelの下からの移動距離")]
    [SerializeField] private float panelMoveDistance = 100f;
    
    private bool isShowing = false; // リザルト表示中かどうか
    private CanvasGroup blackGroundCanvasGroup; // BlackGroundのCanvasGroup
    private CanvasGroup resultPanelCanvasGroup; // ResultPanelのCanvasGroup
    private RectTransform resultPanelRectTransform; // ResultPanelのRectTransform
    private Vector2 resultPanelOriginalPosition; // ResultPanelの元の位置
    private Tween blackGroundFadeTween; // BlackGroundのフェードTween
    private Tween resultPanelFadeTween; // ResultPanelのフェードTween
    private Tween resultPanelMoveTween; // ResultPanelの移動Tween
    private MoveCamera moveCamera; // MainCameraのMoveCameraコンポーネント
    
    private void Awake()
    {
        // BlackGroundとResultPanelを非アクティブにする
        if (blackGround != null)
        {
            blackGround.SetActive(false);
            // CanvasGroupを取得または追加
            blackGroundCanvasGroup = blackGround.GetComponent<CanvasGroup>();
            if (blackGroundCanvasGroup == null)
            {
                blackGroundCanvasGroup = blackGround.AddComponent<CanvasGroup>();
            }
        }
        
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
            // CanvasGroupを取得または追加
            resultPanelCanvasGroup = resultPanel.GetComponent<CanvasGroup>();
            if (resultPanelCanvasGroup == null)
            {
                resultPanelCanvasGroup = resultPanel.AddComponent<CanvasGroup>();
            }
            
            // RectTransformを取得
            resultPanelRectTransform = resultPanel.GetComponent<RectTransform>();
            if (resultPanelRectTransform != null)
            {
                resultPanelOriginalPosition = resultPanelRectTransform.anchoredPosition;
            }
            
            // ResultPanelのImageコンポーネントのRaycast Targetを無効化（ドラッグ判定を防ぐため）
            Image resultPanelImage = resultPanel.GetComponent<Image>();
            if (resultPanelImage != null)
            {
                resultPanelImage.raycastTarget = false;
            }
            
            // ResultPanel内のすべてのImageコンポーネントのRaycast Targetを有効化（ボタンなどはクリック可能にする）
            Image[] resultPanelImages = resultPanel.GetComponentsInChildren<Image>();
            foreach (Image img in resultPanelImages)
            {
                // ボタンコンポーネントがある場合はRaycast Targetを有効化
                if (img.GetComponent<Button>() != null)
                {
                    img.raycastTarget = true;
                }
            }
        }
        
        // MainCameraのMoveCameraコンポーネントを取得
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            moveCamera = mainCamera.GetComponent<MoveCamera>();
        }
    }
    
    private void OnDestroy()
    {
        // Tweenを停止
        if (blackGroundFadeTween != null && blackGroundFadeTween.IsActive())
        {
            blackGroundFadeTween.Kill();
        }
        
        if (resultPanelFadeTween != null && resultPanelFadeTween.IsActive())
        {
            resultPanelFadeTween.Kill();
        }
        
        if (resultPanelMoveTween != null && resultPanelMoveTween.IsActive())
        {
            resultPanelMoveTween.Kill();
        }
    }
    
    /// <summary>
    /// リザルト画面を表示
    /// </summary>
    public void ShowResult()
    {
        if (isShowing)
        {
            return;
        }
        
        isShowing = true;
        
        // ゲーム時間を止める
        Time.timeScale = 0f;
        
        // MainCameraのMoveCameraを無効化
        if (moveCamera != null)
        {
            moveCamera.enabled = false;
        }
        
        // BlackGroundをアクティブにしてフェードイン
        if (blackGround != null && blackGroundCanvasGroup != null)
        {
            blackGround.SetActive(true);
            blackGroundCanvasGroup.alpha = 0f;
            
            // 既存のTweenを停止
            if (blackGroundFadeTween != null && blackGroundFadeTween.IsActive())
            {
                blackGroundFadeTween.Kill();
            }
            
            blackGroundFadeTween = blackGroundCanvasGroup.DOFade(1f, fadeDuration)
                .SetUpdate(true) // Time.timeScaleに影響されない
                .SetTarget(blackGroundCanvasGroup);
        }
        
        // ResultPanelをアクティブにして下からフェードイン
        if (resultPanel != null && resultPanelCanvasGroup != null && resultPanelRectTransform != null)
        {
            resultPanel.SetActive(true);
            resultPanelCanvasGroup.alpha = 0f;
            
            // 下に移動した位置を設定
            Vector2 startPosition = resultPanelOriginalPosition;
            startPosition.y -= panelMoveDistance;
            resultPanelRectTransform.anchoredPosition = startPosition;
            
            // 既存のTweenを停止
            if (resultPanelFadeTween != null && resultPanelFadeTween.IsActive())
            {
                resultPanelFadeTween.Kill();
            }
            
            if (resultPanelMoveTween != null && resultPanelMoveTween.IsActive())
            {
                resultPanelMoveTween.Kill();
            }
            
            // フェードインと移動を同時に実行
            resultPanelFadeTween = resultPanelCanvasGroup.DOFade(1f, fadeDuration)
                .SetUpdate(true)
                .SetTarget(resultPanelCanvasGroup);
            
            resultPanelMoveTween = resultPanelRectTransform.DOAnchorPos(resultPanelOriginalPosition, fadeDuration)
                .SetUpdate(true)
                .SetEase(Ease.OutCubic)
                .SetTarget(resultPanelRectTransform);
        }
    }
}

