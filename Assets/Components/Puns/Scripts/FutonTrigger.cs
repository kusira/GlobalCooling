using UnityEngine;
using System.Collections;

/// <summary>
/// ふとんのトリガーを管理するスクリプト
/// ドラッグを離したときの速度が一定値以上でダジャレを成立させる
/// </summary>
public class FutonTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立に必要な速度の閾値")]
    [SerializeField] private float triggerSpeedThreshold = 50f;
    
    [Tooltip("Y軸の最大速度（0以下で無制限）")]
    [SerializeField] private float maxYVelocity = 20f;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "futon";
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers; // このオブジェクトと子オブジェクトのSpriteRenderer
    private bool isFadingOut = false; // フェードアウト中かどうか

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"FutonTrigger: Rigidbody2Dが見つかりません。GameObject: {gameObject.name}");
        }
        
        // このオブジェクトと子オブジェクトのSpriteRendererを取得
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        // Y軸の最大速度を制限
        if (rb != null && maxYVelocity > 0f)
        {
            Vector2 velocity = rb.linearVelocity;
            
            // Y軸の速度を制限
            if (Mathf.Abs(velocity.y) > maxYVelocity)
            {
                velocity.y = Mathf.Sign(velocity.y) * maxYVelocity;
                rb.linearVelocity = velocity;
            }
        }
    }

    /// <summary>
    /// ドラッグが終了したときに呼び出される（DragAndDropManagerから呼び出される想定）
    /// </summary>
    /// <param name="releaseVelocity">離したときの速度</param>
    public void OnDragReleased(Vector3 releaseVelocity)
    {
        // 投げる力のベクトルのY成分が正（上向き）である必要がある
        if (releaseVelocity.y <= 0f)
        {
            return;
        }
        
        // 速度の大きさを計算
        float speed = releaseVelocity.magnitude;
        
        // 閾値を超えているかチェック
        if (speed >= triggerSpeedThreshold)
        {
            // ダジャレを成立させる
            TriggerPun();
        }
    }

    /// <summary>
    /// ダジャレを成立させる
    /// </summary>
    private void TriggerPun()
    {
        if (punDisplayGenerator == null)
        {
            Debug.LogWarning($"FutonTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
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
