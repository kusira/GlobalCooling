using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 画面のフェードイン/フェードアウトとシーン遷移を管理するスクリプト
/// </summary>
public class FadeManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("フェード用のImage（フルスクリーン推奨）")]
    [SerializeField] private Image fadeImage;

    [Header("Fade Settings")]
    [Tooltip("フェード時間（秒）")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("フェードカラー")]
    [SerializeField] private Color fadeColor = Color.black;

    [Tooltip("シーン開始時に自動でフェードインするか")]
    [SerializeField] private bool fadeInOnStart = true;

    private Tween fadeTween;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponentInChildren<Image>();
        }

        if (fadeImage != null)
        {
            // 開始時のアルファ設定
            Color startColor = fadeColor;
            startColor.a = fadeInOnStart ? 1f : 0f;
            fadeImage.color = startColor;
            fadeImage.raycastTarget = true; // フェード中は入力をブロック
            fadeImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("FadeManager: fadeImageが設定されていません。");
        }
    }

    private void Start()
    {
        if (fadeInOnStart && fadeImage != null)
        {
            StartCoroutine(FadeToAlpha(0f));
        }
        else if (fadeImage != null)
        {
            // フェードインしない場合は非表示扱いに
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }
    }

    /// <summary>
    /// 外部から呼び出すシーン遷移付きフェード
    /// </summary>
    public void FadeOutAndLoadScene(string sceneName)
    {
        if (isTransitioning || fadeImage == null)
        {
            return;
        }

        StartCoroutine(FadeOutAndSwitchScene(sceneName));
    }

    /// <summary>
    /// フェードアウト→シーンロード→フェードインの流れを実行
    /// </summary>
    private IEnumerator FadeOutAndSwitchScene(string sceneName)
    {
        isTransitioning = true;
        fadeImage.gameObject.SetActive(true);

        yield return FadeToAlpha(1f);

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        yield return FadeToAlpha(0f);

        fadeImage.gameObject.SetActive(false);
        isTransitioning = false;
    }

    /// <summary>
    /// 指定アルファまでフェードさせる共通処理
    /// </summary>
    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeImage.color.a);

        fadeTween = fadeImage.DOFade(targetAlpha, fadeDuration)
            .SetUpdate(true)
            .SetEase(Ease.Linear)
            .SetTarget(fadeImage);

        yield return fadeTween.WaitForCompletion();

        // 完全に透明なら非アクティブにして入力を通す
        if (Mathf.Approximately(targetAlpha, 0f))
        {
            fadeImage.raycastTarget = false;
            fadeImage.gameObject.SetActive(false);
        }
        else
        {
            fadeImage.raycastTarget = true;
        }
    }
}

