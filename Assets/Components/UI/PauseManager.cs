using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// ポーズ機能を管理するスクリプト
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ポーズボタン")]
    [SerializeField] private Button pauseButton;
    
    [Tooltip("ポーズ画面の黒い背景")]
    [SerializeField] private GameObject blackGround;
    
    [Tooltip("ポーズパネル")]
    [SerializeField] private GameObject pausePanel;
    
    [Tooltip("閉じるボタン")]
    [SerializeField] private Button closeButton;
    
    [Header("Animation Settings")]
    [Tooltip("フェードイン/アウトの時間（秒）")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    [Tooltip("PausePanelの下からの移動距離")]
    [SerializeField] private float panelMoveDistance = 100f;
    
    private bool isPaused = false; // ポーズ中かどうか
    private CanvasGroup blackGroundCanvasGroup; // BlackGroundのCanvasGroup
    private CanvasGroup pausePanelCanvasGroup; // PausePanelのCanvasGroup
    private RectTransform pausePanelRectTransform; // PausePanelのRectTransform
    private Vector2 pausePanelOriginalPosition; // PausePanelの元の位置
    private Tween blackGroundFadeTween; // BlackGroundのフェードTween
    private Tween pausePanelFadeTween; // PausePanelのフェードTween
    private Tween pausePanelMoveTween; // PausePanelの移動Tween
    private MoveCamera moveCamera; // MainCameraのMoveCameraコンポーネント
    
    private void Awake()
    {
        // BlackGroundとPausePanelを非アクティブにする
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
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            // CanvasGroupを取得または追加
            pausePanelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (pausePanelCanvasGroup == null)
            {
                pausePanelCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }
            
            // RectTransformを取得
            pausePanelRectTransform = pausePanel.GetComponent<RectTransform>();
            if (pausePanelRectTransform != null)
            {
                pausePanelOriginalPosition = pausePanelRectTransform.anchoredPosition;
            }
            
            // PausePanelのImageコンポーネントのRaycast Targetを無効化（ドラッグ判定を防ぐため）
            Image pausePanelImage = pausePanel.GetComponent<Image>();
            if (pausePanelImage != null)
            {
                pausePanelImage.raycastTarget = false;
            }
            
            // PausePanel内のすべてのImageコンポーネントのRaycast Targetを有効化（ボタンなどはクリック可能にする）
            Image[] pausePanelImages = pausePanel.GetComponentsInChildren<Image>();
            foreach (Image img in pausePanelImages)
            {
                // ボタンコンポーネントがある場合はRaycast Targetを有効化
                if (img.GetComponent<Button>() != null)
                {
                    img.raycastTarget = true;
                }
            }
        }
        
        // ボタンのイベントを設定
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // BlackGroundのクリックイベントを設定（Imageコンポーネントが必要）
        if (blackGround != null)
        {
            Image blackGroundImage = blackGround.GetComponent<Image>();
            if (blackGroundImage != null)
            {
                // Buttonコンポーネントを追加してクリック検出
                Button blackGroundButton = blackGround.GetComponent<Button>();
                if (blackGroundButton == null)
                {
                    blackGroundButton = blackGround.AddComponent<Button>();
                }
                // 透明なボタンとして機能させる
                blackGroundButton.transition = Selectable.Transition.None;
                blackGroundButton.onClick.AddListener(OnBlackGroundClicked);
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
        // ボタンのイベントを解除
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
        
        // Tweenを停止
        if (blackGroundFadeTween != null && blackGroundFadeTween.IsActive())
        {
            blackGroundFadeTween.Kill();
        }
        
        if (pausePanelFadeTween != null && pausePanelFadeTween.IsActive())
        {
            pausePanelFadeTween.Kill();
        }
        
        if (pausePanelMoveTween != null && pausePanelMoveTween.IsActive())
        {
            pausePanelMoveTween.Kill();
        }
    }
    
    /// <summary>
    /// ポーズボタンがクリックされたとき
    /// </summary>
    private void OnPauseButtonClicked()
    {
        // EventSystemの選択状態をクリア
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        // ボタンのAudioSourceを再生
        PlayButtonAudioSource(pauseButton);
        
        if (!isPaused)
        {
            OpenPause();
        }
    }
    
    /// <summary>
    /// 閉じるボタンがクリックされたとき
    /// </summary>
    private void OnCloseButtonClicked()
    {
        // EventSystemの選択状態をクリア
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        // ボタンのAudioSourceを再生
        PlayButtonAudioSource(closeButton);
        
        if (isPaused)
        {
            ClosePause();
        }
    }
    
    /// <summary>
    /// BlackGroundがクリックされたとき（PausePanel以外の部分）
    /// </summary>
    private void OnBlackGroundClicked()
    {
        // EventSystemの選択状態をクリア
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        if (isPaused)
        {
            // PausePanelのRectTransformを取得して、クリック位置がPausePanel内かチェック
            if (pausePanelRectTransform != null)
            {
                // 新しいInput Systemからマウス位置を取得
                Vector2 mouseScreenPos = Mouse.current != null 
                    ? Mouse.current.position.ReadValue() 
                    : Vector2.zero;
                
                Vector2 clickPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    pausePanelRectTransform.parent as RectTransform,
                    mouseScreenPos,
                    null,
                    out clickPosition
                );
                
                // PausePanelの範囲外をクリックした場合のみ閉じる
                Rect pausePanelRect = pausePanelRectTransform.rect;
                Vector3 pausePanelWorldPos = pausePanelRectTransform.position;
                RectTransform parentRect = pausePanelRectTransform.parent as RectTransform;
                
                if (parentRect != null)
                {
                    Vector2 pausePanelLocalPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentRect,
                        pausePanelRectTransform.position,
                        null,
                        out pausePanelLocalPos
                    );
                    
                    Rect pausePanelLocalRect = new Rect(
                        pausePanelLocalPos.x + pausePanelRect.x,
                        pausePanelLocalPos.y + pausePanelRect.y,
                        pausePanelRect.width,
                        pausePanelRect.height
                    );
                    
                    if (!pausePanelLocalRect.Contains(clickPosition))
                    {
                        ClosePause();
                    }
                }
            }
            else
            {
                // PausePanelのRectTransformが取得できない場合は常に閉じる
                ClosePause();
            }
        }
    }
    
    /// <summary>
    /// ポーズを開く
    /// </summary>
    private void OpenPause()
    {
        if (isPaused)
        {
            return;
        }
        
        isPaused = true;
        
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
        
        // PausePanelをアクティブにして下からフェードイン
        if (pausePanel != null && pausePanelCanvasGroup != null && pausePanelRectTransform != null)
        {
            pausePanel.SetActive(true);
            pausePanelCanvasGroup.alpha = 0f;
            
            // 下に移動した位置を設定
            Vector2 startPosition = pausePanelOriginalPosition;
            startPosition.y -= panelMoveDistance;
            pausePanelRectTransform.anchoredPosition = startPosition;
            
            // 既存のTweenを停止
            if (pausePanelFadeTween != null && pausePanelFadeTween.IsActive())
            {
                pausePanelFadeTween.Kill();
            }
            
            if (pausePanelMoveTween != null && pausePanelMoveTween.IsActive())
            {
                pausePanelMoveTween.Kill();
            }
            
            // フェードインと移動を同時に実行
            pausePanelFadeTween = pausePanelCanvasGroup.DOFade(1f, fadeDuration)
                .SetUpdate(true)
                .SetTarget(pausePanelCanvasGroup);
            
            pausePanelMoveTween = pausePanelRectTransform.DOAnchorPos(pausePanelOriginalPosition, fadeDuration)
                .SetUpdate(true)
                .SetEase(Ease.OutCubic)
                .SetTarget(pausePanelRectTransform);
        }
    }
    
    /// <summary>
    /// ポーズを閉じる
    /// </summary>
    private void ClosePause()
    {
        if (!isPaused)
        {
            return;
        }
        
        isPaused = false;
        
        // ゲーム時間を再開
        Time.timeScale = 1f;
        
        // MainCameraのMoveCameraを再有効化
        if (moveCamera != null)
        {
            moveCamera.enabled = true;
        }
        
        // BlackGroundをフェードアウト
        if (blackGroundCanvasGroup != null)
        {
            // 既存のTweenを停止
            if (blackGroundFadeTween != null && blackGroundFadeTween.IsActive())
            {
                blackGroundFadeTween.Kill();
            }
            
            blackGroundFadeTween = blackGroundCanvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (blackGround != null)
                    {
                        blackGround.SetActive(false);
                    }
                })
                .SetTarget(blackGroundCanvasGroup);
        }
        
        // PausePanelを下に移動しながらフェードアウト
        if (pausePanelCanvasGroup != null && pausePanelRectTransform != null)
        {
            // 既存のTweenを停止
            if (pausePanelFadeTween != null && pausePanelFadeTween.IsActive())
            {
                pausePanelFadeTween.Kill();
            }
            
            if (pausePanelMoveTween != null && pausePanelMoveTween.IsActive())
            {
                pausePanelMoveTween.Kill();
            }
            
            // フェードアウトと移動を同時に実行
            Vector2 endPosition = pausePanelOriginalPosition;
            endPosition.y -= panelMoveDistance;
            
            pausePanelFadeTween = pausePanelCanvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .SetTarget(pausePanelCanvasGroup);
            
            pausePanelMoveTween = pausePanelRectTransform.DOAnchorPos(endPosition, fadeDuration)
                .SetUpdate(true)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    if (pausePanel != null)
                    {
                        pausePanel.SetActive(false);
                    }
                })
                .SetTarget(pausePanelRectTransform);
        }
    }
    
    /// <summary>
    /// ボタンのAudioSourceを再生（存在する場合のみ）
    /// </summary>
    /// <param name="button">対象のボタン</param>
    private void PlayButtonAudioSource(Button button)
    {
        if (button == null)
        {
            return;
        }
        
        // ボタンのGameObjectからAudioSourceを取得（自身または子オブジェクトから）
        AudioSource audioSource = button.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = button.GetComponentInChildren<AudioSource>();
        }
        
        // AudioSourceが見つかり、AudioClipが設定されている場合のみ再生
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}

