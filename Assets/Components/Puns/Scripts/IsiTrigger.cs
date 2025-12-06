using UnityEngine;
using System.Collections;

/// <summary>
/// 石のトリガーを管理するスクリプト
/// Y軸がある値以上でオブジェクトを落としたときにダジャレを成立させる
/// </summary>
public class IsiTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立に必要なY軸の最小値（この値以上でトリガー）")]
    [SerializeField] private float minYPosition = 3f;
    
    [Tooltip("ダジャレ発生までのインターバル（秒）")]
    [SerializeField] private float triggerInterval = 0.5f;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Isi";
    
    [Header("Object Reference")]
    [Tooltip("石オブジェクト")]
    [SerializeField] private GameObject stoneObject;
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers; // このオブジェクトとその子オブジェクトのSpriteRenderer
    private bool isFadingOut = false; // フェードアウト中かどうか

    private void Awake()
    {
        // 石オブジェクトからRigidbody2Dを取得
        if (stoneObject != null)
        {
            rb = stoneObject.GetComponent<Rigidbody2D>();
        }
        
        // このオブジェクトとその子オブジェクトのSpriteRendererを取得
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// ドラッグが終了したときに呼び出される（DragAndDropManagerから呼び出される想定）
    /// </summary>
    /// <param name="releaseVelocity">離したときの速度</param>
    public void OnDragReleased(Vector3 releaseVelocity)
    {
        // 石オブジェクトのY座標をチェック
        if (stoneObject != null)
        {
            float yPosition = stoneObject.transform.position.y;
            
            // Y軸が閾値以上かチェック
            if (yPosition >= minYPosition)
            {
                // インターバル後にダジャレを成立させる
                StartCoroutine(TriggerPunDelayed());
            }
        }
    }

    /// <summary>
    /// インターバル後にダジャレを成立させる
    /// </summary>
    private IEnumerator TriggerPunDelayed()
    {
        // インターバル時間を待つ
        yield return new WaitForSeconds(triggerInterval);
        
        // ダジャレを成立させる
        TriggerPun();
    }

    /// <summary>
    /// ダジャレを成立させる
    /// </summary>
    private void TriggerPun()
    {
        if (punDisplayGenerator == null)
        {
            return;
        }

        // PunDisplayGeneratorにダジャレ成立を通知
        punDisplayGenerator.GeneratePun(punId);
        
        // インターバル後にフェードアウトしてDestroy
        StartCoroutine(DestroyAfterFadeOut());
    }
    
    /// <summary>
    /// インターバル後にフェードアウトしてDestroy
    /// </summary>
    private IEnumerator DestroyAfterFadeOut()
    {
        // 既にフェードアウト中の場合は何もしない
        if (isFadingOut)
        {
            yield break;
        }
        
        isFadingOut = true;
        
        // インターバル待機
        yield return new WaitForSeconds(destroyInterval);
        
        // フェードアウト
        yield return StartCoroutine(FadeOut());
        
        // Destroy
        Destroy(gameObject);
    }
    
    /// <summary>
    /// フェードアウト処理
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            yield break;
        }
        
        // 各SpriteRendererの初期Alpha値を保存
        float[] initialAlphas = new float[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                initialAlphas[i] = spriteRenderers[i].color.a;
            }
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            
            // 各SpriteRendererのAlphaを更新
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color color = spriteRenderers[i].color;
                    color.a = initialAlphas[i] * alpha;
                    spriteRenderers[i].color = color;
                }
            }
            
            yield return null;
        }
        
        // 最終的にAlphaを0に設定
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color color = spriteRenderers[i].color;
                color.a = 0f;
                spriteRenderers[i].color = color;
            }
        }
    }

    /// <summary>
    /// 現在の速度を取得（外部から呼び出し可能）
    /// </summary>
    public float GetCurrentSpeed()
    {
        if (rb != null)
        {
            return rb.linearVelocity.magnitude;
        }
        return 0f;
    }
}

