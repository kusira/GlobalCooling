using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
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

    [Header("Buttons")]
    [Tooltip("ホームに戻る（シーン再読み込み）ボタン")]
    [SerializeField] private Button homeButton;
    
    [Header("Animation Settings")]
    [Tooltip("フェードイン/アウトの時間（秒）")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    [Tooltip("ResultPanelの下からの移動距離")]
    [SerializeField] private float panelMoveDistance = 100f;

    [Header("Audio")]
    [Tooltip("クリア時に鳴らすAudioSource（自身に付けたものを想定）")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("BGMの音量制御用AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("BGMのExposed Parameter名（AudioMixerで設定した名前）")]
    [SerializeField] private string bgmParamName = "BGM";

    [Tooltip("BGMの音量を下げる目標値（0-1の範囲）")]
    [SerializeField, Range(0f, 1f)] private float bgmFadedVolume = 0.2f;

    [Tooltip("BGMの音量を下げる時間（秒）")]
    [SerializeField] private float bgmVolumeFadeDownDuration = 0.3f;

    [Tooltip("BGMの音量を小さくしたまま保持する時間（秒）")]
    [SerializeField] private float bgmVolumeHoldDuration = 2f;

    [Tooltip("BGMの音量を元に戻す時間（秒）")]
    [SerializeField] private float bgmVolumeFadeUpDuration = 0.3f;
    
    private bool isShowing = false; // リザルト表示中かどうか
    private CanvasGroup blackGroundCanvasGroup; // BlackGroundのCanvasGroup
    private CanvasGroup resultPanelCanvasGroup; // ResultPanelのCanvasGroup
    private RectTransform resultPanelRectTransform; // ResultPanelのRectTransform
    private Vector2 resultPanelOriginalPosition; // ResultPanelの元の位置
    private Tween blackGroundFadeTween; // BlackGroundのフェードTween
    private Tween resultPanelFadeTween; // ResultPanelのフェードTween
    private Tween resultPanelMoveTween; // ResultPanelの移動Tween
    private Sequence bgmVolumeSequence; // BGMの音量制御シーケンス
    private float originalBgmVolume = 1f; // 元のBGM音量（0-1の範囲）
    private MoveCamera moveCamera; // MainCameraのMoveCameraコンポーネント
    
    private void Awake()
    {
        // 元のBGM音量を取得
        if (audioMixer != null && !string.IsNullOrEmpty(bgmParamName))
        {
            if (audioMixer.GetFloat(bgmParamName, out float currentDb))
            {
                // デシベル値を0-1の範囲に変換
                originalBgmVolume = currentDb <= -80f ? 0f : Mathf.Pow(10f, currentDb / 20f);
            }
        }

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

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(ReloadCurrentScene);
            homeButton.onClick.AddListener(ReloadCurrentScene);
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

        if (bgmVolumeSequence != null && bgmVolumeSequence.IsActive())
        {
            bgmVolumeSequence.Kill();
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
        
        // クリア時のサウンド再生とBGM音量フェードダウン（同時実行）
        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        // SEが鳴ったタイミングと同時にBGMの音量を小さくし、数秒後に元に戻す
        if (audioMixer != null && !string.IsNullOrEmpty(bgmParamName))
        {
            // 既存のシーケンスを停止
            if (bgmVolumeSequence != null && bgmVolumeSequence.IsActive())
            {
                bgmVolumeSequence.Kill();
            }

            // 現在のBGM音量を取得
            if (audioMixer.GetFloat(bgmParamName, out float currentDb))
            {
                float currentVolume = currentDb <= -80f ? 0f : Mathf.Pow(10f, currentDb / 20f);
                // 現在の音量にfadedVolumeを乗算
                float fadedVolume = currentVolume * bgmFadedVolume;
                // 元の音量を保存（フェードアップ時に使用）
                originalBgmVolume = currentVolume;

                // シーケンスを作成：フェードダウン → 保持 → フェードアップ
                bgmVolumeSequence = DOTween.Sequence()
                    .SetUpdate(true) // 時間停止の影響を受けないように
                    .SetTarget(audioMixer);

                // フェードダウン：音量を小さくする
                bgmVolumeSequence.Append(DOTween.To(
                    () => currentVolume,
                    v =>
                    {
                        currentVolume = v;
                        float db = v <= 0 ? -80f : Mathf.Log10(v) * 20f;
                        audioMixer.SetFloat(bgmParamName, db);
                    },
                    fadedVolume,
                    bgmVolumeFadeDownDuration
                ).SetEase(Ease.Linear));

                // 保持：小さくしたまま数秒間維持
                bgmVolumeSequence.AppendInterval(bgmVolumeHoldDuration);

                // フェードアップ：元の音量に戻す（現在の音量から元の音量へ）
                bgmVolumeSequence.Append(DOTween.To(
                    () => currentVolume,
                    v =>
                    {
                        currentVolume = v;
                        float db = v <= 0 ? -80f : Mathf.Log10(v) * 20f;
                        audioMixer.SetFloat(bgmParamName, db);
                    },
                    originalBgmVolume,
                    bgmVolumeFadeUpDuration
                ).SetEase(Ease.Linear));
            }
        }

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

    /// <summary>
    /// 現在のシーンを再読み込み（ホームボタン用）
    /// </summary>
    private void ReloadCurrentScene()
    {
        // 一時停止を解除してからロード
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }
}

