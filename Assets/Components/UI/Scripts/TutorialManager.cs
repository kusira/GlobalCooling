using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// チュートリアル画面を管理するスクリプト
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("チュートリアル画面の黒い背景")]
    [SerializeField] private GameObject blackGround;
    
    [Tooltip("チュートリアルパネル")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Buttons")]
    [Tooltip("チュートリアルを閉じるボタン")]
    [SerializeField] private Button closeButton;
    
    [Header("Animation Settings")]
    [Tooltip("フェードイン/アウトの時間（秒）")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    private CanvasGroup blackGroundCanvasGroup; // BlackGroundのCanvasGroup
    private CanvasGroup tutorialPanelCanvasGroup; // TutorialPanelのCanvasGroup
    private Tween blackGroundFadeTween; // BlackGroundのフェードTween
    private Tween tutorialPanelFadeTween; // TutorialPanelのフェードTween
    private MoveCamera moveCamera; // MainCameraのMoveCameraコンポーネント
    
    private void Awake()
    {
        // BlackGroundとTutorialPanelを非アクティブにする（Startで表示）
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
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            // CanvasGroupを取得または追加
            tutorialPanelCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (tutorialPanelCanvasGroup == null)
            {
                tutorialPanelCanvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
            }
            
            // TutorialPanelのImageコンポーネントのRaycast Targetを無効化（ドラッグ判定を防ぐため）
            Image tutorialPanelImage = tutorialPanel.GetComponent<Image>();
            if (tutorialPanelImage != null)
            {
                tutorialPanelImage.raycastTarget = false;
            }
            
            // TutorialPanel内のすべてのImageコンポーネントのRaycast Targetを有効化（ボタンなどはクリック可能にする）
            Image[] tutorialPanelImages = tutorialPanel.GetComponentsInChildren<Image>();
            foreach (Image img in tutorialPanelImages)
            {
                // ボタンコンポーネントがある場合はRaycast Targetを有効化
                if (img.GetComponent<Button>() != null)
                {
                    img.raycastTarget = true;
                }
            }
        }

        // CloseButtonのリスナーを設定
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTutorial);
            closeButton.onClick.AddListener(CloseTutorial);
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

    private void Start()
    {
        // ゲーム開始時にチュートリアルを表示
        ShowTutorial();
    }
    
    private void OnDestroy()
    {
        // Tweenを停止
        if (blackGroundFadeTween != null && blackGroundFadeTween.IsActive())
        {
            blackGroundFadeTween.Kill();
        }
        
        if (tutorialPanelFadeTween != null && tutorialPanelFadeTween.IsActive())
        {
            tutorialPanelFadeTween.Kill();
        }
    }
    
    /// <summary>
    /// チュートリアル画面を表示
    /// </summary>
    public void ShowTutorial()
    {
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
        
        // TutorialPanelをアクティブにしてフェードイン
        if (tutorialPanel != null && tutorialPanelCanvasGroup != null)
        {
            tutorialPanel.SetActive(true);
            tutorialPanelCanvasGroup.alpha = 0f;
            
            // 既存のTweenを停止
            if (tutorialPanelFadeTween != null && tutorialPanelFadeTween.IsActive())
            {
                tutorialPanelFadeTween.Kill();
            }
            
            tutorialPanelFadeTween = tutorialPanelCanvasGroup.DOFade(1f, fadeDuration)
                .SetUpdate(true)
                .SetTarget(tutorialPanelCanvasGroup);
        }
    }

    /// <summary>
    /// チュートリアル画面を閉じる（CloseButton用）
    /// </summary>
    private void CloseTutorial()
    {
        // ゲーム時間を再開
        Time.timeScale = 1f;
        
        // MainCameraのMoveCameraを再有効化
        if (moveCamera != null)
        {
            moveCamera.enabled = true;
        }
        
        // BlackGroundをフェードアウト
        if (blackGround != null && blackGroundCanvasGroup != null)
        {
            // 既存のTweenを停止
            if (blackGroundFadeTween != null && blackGroundFadeTween.IsActive())
            {
                blackGroundFadeTween.Kill();
            }
            
            blackGroundFadeTween = blackGroundCanvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .SetTarget(blackGroundCanvasGroup)
                .OnComplete(() =>
                {
                    blackGround.SetActive(false);
                });
        }
        
        // TutorialPanelをフェードアウト
        if (tutorialPanel != null && tutorialPanelCanvasGroup != null)
        {
            // 既存のTweenを停止
            if (tutorialPanelFadeTween != null && tutorialPanelFadeTween.IsActive())
            {
                tutorialPanelFadeTween.Kill();
            }
            
            tutorialPanelFadeTween = tutorialPanelCanvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .SetTarget(tutorialPanelCanvasGroup)
                .OnComplete(() =>
                {
                    tutorialPanel.SetActive(false);
                });
        }
    }
}

