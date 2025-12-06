using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// ダジャレが成立したときに表示するゲームオブジェクトを管理するスクリプト
/// 親オブジェクトにアタッチし、子に集中線オブジェクトとテキストオブジェクトを持つ構造
/// </summary>
public class PunDisplayShower : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("表示時間（秒）")]
    [SerializeField] private float displayDuration = 3f;
    
    [Header("Object References")]
    private Transform mainCameraTransform; // MainCameraのTransform（自動検索）
    private PunDisplayGenerator punDisplayGenerator; // PunDisplayGeneratorへの参照
    
    [Tooltip("集中線オブジェクト")]
    [SerializeField] private GameObject concentrationLineObject;
    
    [Tooltip("テキストオブジェクト")]
    [SerializeField] private GameObject punText;
    
    [Header("Text Animation Settings")]
    [Tooltip("テキストの初期スケール")]
    [SerializeField] private float initialTextScale = 5f;
    
    [Tooltip("テキストスケールアニメーションの時間（秒）")]
    [SerializeField] private float textScaleDuration = 0.3f;
    
    [Tooltip("テキストスケールアニメーションのイージング")]
    [SerializeField] private Ease textScaleEase = Ease.OutBack;
    
    private TMPro.TextMeshPro textMeshPro;
    private Sequence displaySequence;
    private Renderer concentrationLineRenderer; // 集中線のRenderer
    private Material concentrationLineMaterial; // 集中線のMaterialインスタンス
    private float currentRadius = 0f; // 現在のRadius値（アニメーション用）
    private float originalRadius = 0f; // 元のRadius値（クリーンアップ用）
    
    [Header("Fade Out Settings")]
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Tooltip("スケールアニメーション終了後のスケール縮小の最終倍率（デフォルト0.95）")]
    [SerializeField] private float finalScale = 0.95f;

    /// <summary>
    /// テキストを設定（外部から呼び出し可能）
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    public void SetText(string text)
    {
        if (textMeshPro == null && punText != null)
        {
            textMeshPro = punText.GetComponent<TMPro.TextMeshPro>();
        }

        if (textMeshPro != null)
        {
            textMeshPro.text = text;
        }
        else
        {
            Debug.LogWarning($"PunDisplayShower.SetText: TextMeshProが見つかりません。punText: {(punText != null ? punText.name : "null")}");
        }
    }

    /// <summary>
    /// フォントサイズを設定（外部から呼び出し可能）
    /// </summary>
    /// <param name="size">フォントサイズ</param>
    public void SetFontSize(float size)
    {
        if (textMeshPro == null && punText != null)
        {
            textMeshPro = punText.GetComponent<TMPro.TextMeshPro>();
        }

        if (textMeshPro != null)
        {
            textMeshPro.fontSize = size;
        }
    }

    /// <summary>
    /// MainCameraのTransformを設定（外部から呼び出し可能）
    /// </summary>
    /// <param name="cameraTransform">MainCameraのTransform</param>
    public void SetMainCameraTransform(Transform cameraTransform)
    {
        mainCameraTransform = cameraTransform;
    }
    
    /// <summary>
    /// PunDisplayGeneratorを設定（外部から呼び出し可能）
    /// </summary>
    /// <param name="generator">PunDisplayGenerator</param>
    public void SetPunDisplayGenerator(PunDisplayGenerator generator)
    {
        punDisplayGenerator = generator;
    }
    
    /// <summary>
    /// テキストオブジェクトを取得（外部から呼び出し可能）
    /// </summary>
    /// <returns>テキストオブジェクト</returns>
    public GameObject GetTextObject()
    {
        return punText;
    }

    private void Start()
    {
        // MainCameraのTransformを自動検索
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraTransform = mainCamera.transform;
        }
        else
        {
            Debug.LogWarning("PunDisplayShower: MainCameraが見つかりません。");
        }
        
        // TextMeshProコンポーネントを取得（3Dオブジェクト用）
        if (punText != null)
        {
            textMeshPro = punText.GetComponent<TMPro.TextMeshPro>();
        }
        
        // 集中線のRendererを取得し、Materialのインスタンスを作成
        if (concentrationLineObject != null)
        {
            concentrationLineRenderer = concentrationLineObject.GetComponent<Renderer>();
            if (concentrationLineRenderer == null)
            {
                concentrationLineRenderer = concentrationLineObject.GetComponent<MeshRenderer>();
            }
            
            // Materialのインスタンスを作成（元のMaterialに影響を与えないように）
            if (concentrationLineRenderer != null && concentrationLineRenderer.sharedMaterial != null)
            {
                concentrationLineMaterial = new Material(concentrationLineRenderer.sharedMaterial);
                concentrationLineRenderer.material = concentrationLineMaterial;
                
                // 現在のRadius値を取得
                if (concentrationLineMaterial.HasProperty("_Radius"))
                {
                    currentRadius = concentrationLineMaterial.GetFloat("_Radius");
                    originalRadius = currentRadius;
                }
            }
        }

        // アニメーションを開始
        StartDisplayAnimation();
    }

    private void Update()
    {
        // MainCameraの位置にオブジェクトの中心を合わせる（XとYのみ、Zは変更しない）
        if (mainCameraTransform != null)
        {
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(
                mainCameraTransform.position.x,
                mainCameraTransform.position.y,
                currentPos.z
            );
        }
    }


    /// <summary>
    /// 表示アニメーションを開始（エディターからも呼び出し可能）
    /// </summary>
    public void StartDisplayAnimation()
    {
        // 既存のアニメーションを停止
        if (displaySequence != null && displaySequence.IsActive())
        {
            displaySequence.Kill();
        }

        // シーケンスを作成
        displaySequence = DOTween.Sequence();

        // テキストの初期状態を設定
        if (punText != null)
        {
            punText.transform.localScale = Vector3.one * initialTextScale;
        }

        // テキストスケールアニメーション（勢いを大事に：速く縮小）
        if (punText != null)
        {
            displaySequence.Append(
                punText.transform.DOScale(Vector3.one, textScaleDuration)
                    .SetEase(textScaleEase)
            );
        }

        // 表示時間を待つ
        float waitDuration = displayDuration - textScaleDuration - fadeOutDuration;
        displaySequence.AppendInterval(waitDuration);
        
        // スケールアニメーション終了後から消滅までの間、スケールを線形で縮小
        if (punText != null && waitDuration + fadeOutDuration > 0f)
        {
            float scaleShrinkDuration = waitDuration + fadeOutDuration; // 待機時間 + フェードアウト時間
            displaySequence.Join(
                punText.transform.DOScale(Vector3.one * finalScale, scaleShrinkDuration)
                    .SetEase(Ease.Linear)
            );
        }

        // フェードアウト処理（任意秒でテキストの透明度を0にする、集中線のRadiusを1.4にアニメーション）
        Tween textFadeTween = null;
        Tween radiusTween = null;
        
        if (textMeshPro != null)
        {
            // テキストの透明度を0にアニメーション
            textFadeTween = DOTween.To(
                () => textMeshPro.color.a,
                alpha =>
                {
                    Color color = textMeshPro.color;
                    color.a = alpha;
                    textMeshPro.color = color;
                },
                0f,
                fadeOutDuration
            );
        }
        
        // 集中線のマテリアルのRadiusを1.25にアニメーション（上限1.25）
        if (concentrationLineMaterial != null)
        {
            radiusTween = DOTween.To(
                () => currentRadius,
                radius =>
                {
                    // 上限1.25を超えないように制限
                    radius = Mathf.Min(radius, 1.25f);
                    currentRadius = radius;
                    if (concentrationLineMaterial != null)
                    {
                        concentrationLineMaterial.SetFloat("_Radius", radius);
                    }
                },
                1.25f,
                fadeOutDuration
            );
        }
        
        // テキストと集中線のアニメーションを並行実行
        if (textFadeTween != null && radiusTween != null)
        {
            displaySequence.Append(textFadeTween);
            displaySequence.Join(radiusTween);
        }
        else if (textFadeTween != null)
        {
            displaySequence.Append(textFadeTween);
        }
        else if (radiusTween != null)
        {
            displaySequence.Append(radiusTween);
        }
        else
        {
            displaySequence.AppendInterval(fadeOutDuration);
        }

        // 終了時に非表示
        displaySequence.OnComplete(() =>
        {
            StopDisplay();
        });
    }

    /// <summary>
    /// 集中線のマテリアルのRadiusを設定（上限1.25）
    /// </summary>
    private void SetConcentrateLineRadius(float radius)
    {
        if (concentrationLineMaterial != null)
        {
            // 上限1.25を超えないように制限
            radius = Mathf.Min(radius, 1.25f);
            concentrationLineMaterial.SetFloat("_Radius", radius);
        }
    }

    /// <summary>
    /// ConcentrateLineオブジェクトを取得（外部から呼び出し可能）
    /// </summary>
    public GameObject GetConcentrationLineObject()
    {
        return concentrationLineObject;
    }

    /// <summary>
    /// 表示を停止
    /// </summary>
    private void StopDisplay()
    {
        // アニメーションを停止
        if (displaySequence != null && displaySequence.IsActive())
        {
            displaySequence.Kill();
            displaySequence = null;
        }
        
        // Rendererを無効化してレンダリングを停止
        if (concentrationLineRenderer != null)
        {
            concentrationLineRenderer.enabled = false;
        }
        
        // テキストのRendererも無効化
        if (textMeshPro != null)
        {
            Renderer textRenderer = textMeshPro.GetComponent<Renderer>();
            if (textRenderer != null)
            {
                textRenderer.enabled = false;
            }
        }
        
        // Materialのクリーンアップ
        CleanupMaterial();
        
        // 次のフレームで破棄（レンダリングが完了してから）
        StartCoroutine(DestroyNextFrame());
    }
    
    /// <summary>
    /// 次のフレームでオブジェクトを破棄
    /// </summary>
    private System.Collections.IEnumerator DestroyNextFrame()
    {
        yield return null; // 1フレーム待つ
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Materialのクリーンアップ
    /// </summary>
    private void CleanupMaterial()
    {
        // Materialインスタンスを破棄
        if (concentrationLineMaterial != null)
        {
            Destroy(concentrationLineMaterial);
            concentrationLineMaterial = null;
        }
    }

    private void OnDestroy()
    {
        // クリーンアップ
        if (displaySequence != null && displaySequence.IsActive())
        {
            displaySequence.Kill();
        }
        
        // Materialのクリーンアップ
        CleanupMaterial();
        
        // PunDisplayGeneratorに破棄を通知
        if (punDisplayGenerator != null)
        {
            punDisplayGenerator.OnPunDisplayDestroyed();
        }
    }

    private void OnDisable()
    {
        // 無効化時にもクリーンアップ
        if (displaySequence != null && displaySequence.IsActive())
        {
            displaySequence.Kill();
        }
    }
}
