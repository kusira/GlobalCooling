using UnityEngine;
using System.Collections;

/// <summary>
/// ネコのトリガーを管理するスクリプト
/// このオブジェクトが-270～90度の角度でGroundタグのオブジェクトに一定時間触れていたらダジャレを成立させる
/// </summary>
public class NekoTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立までの待機時間（秒）")]
    [SerializeField] private float triggerWaitTime = 1.5f;
    
    [Tooltip("元の角度（0度）との差の最小値（度）")]
    [SerializeField] private float minAngleDifference = 80f;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Neko";
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private bool isTouchingGround = false; // Groundタグのオブジェクトに触れているか
    private float timer = 0f; // タイマー
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private SpriteRenderer[] spriteRenderers; // このオブジェクトとその子オブジェクトのSpriteRenderer
    private bool isFadingOut = false; // フェードアウト中かどうか

    private void Awake()
    {
        // このオブジェクトとその子オブジェクトのSpriteRendererを取得
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        
        Debug.Log($"NekoTrigger: 初期化完了 - triggerWaitTime: {triggerWaitTime}秒, minAngleDifference: {minAngleDifference}度, punId: {punId}");
    }

    private void Update()
    {
        // Groundに触れていて、角度が範囲内で、まだトリガーしていない場合
        bool angleInRange = IsAngleInRange();
        
        if (isTouchingGround && !hasTriggered && angleInRange)
        {
            timer += Time.deltaTime;
            
            // 定期的にタイマーの進捗を表示（0.5秒ごと）
            if (Mathf.FloorToInt(timer * 2f) != Mathf.FloorToInt((timer - Time.deltaTime) * 2f))
            {
                Debug.Log($"NekoTrigger: タイマー進行中 - {timer:F2}秒 / {triggerWaitTime:F2}秒");
            }
            
            // 待機時間を超えたらダジャレを発生
            if (timer >= triggerWaitTime)
            {
                Debug.Log($"NekoTrigger: 待機時間達成！ダジャレを発生させます。");
                TriggerPun();
                hasTriggered = true;
            }
        }
        else
        {
            // 条件を満たしていない場合はタイマーをリセット
            if (timer > 0f)
            {
                if (!isTouchingGround)
                {
                    Debug.Log($"NekoTrigger: Groundに触れていないため、タイマーをリセット");
                }
                else if (!angleInRange)
                {
                    Debug.Log($"NekoTrigger: 角度が範囲外のため、タイマーをリセット");
                }
                timer = 0f;
            }
        }
    }

    /// <summary>
    /// 元の角度（0度）との差が80度以上かチェック
    /// </summary>
    private bool IsAngleInRange()
    {
        // Z軸の回転角度を取得（0～360度）
        float currentAngle = transform.rotation.eulerAngles.z;
        
        // 0度との差を計算（0～180度の範囲で）
        float angleDifference = Mathf.Abs(currentAngle - 0f);
        
        // 180度を超える場合は、反対側の角度を計算
        if (angleDifference > 180f)
        {
            angleDifference = 360f - angleDifference;
        }
        
        // 定期的に角度情報を表示（毎フレームは多すぎるので、条件が変わったときのみ）
        bool inRange = angleDifference >= minAngleDifference;
        
        // 差が80度以上かチェック
        return inRange;
    }

    /// <summary>
    /// トリガーに入ったとき
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Debug.Log($"NekoTrigger: Groundタグのオブジェクトに接触開始 - {other.gameObject.name}");
            isTouchingGround = true;
            timer = 0f; // タイマーをリセット
            
            // 現在の角度を表示
            float currentAngle = transform.rotation.eulerAngles.z;
            float angleDifference = Mathf.Abs(currentAngle - 0f);
            if (angleDifference > 180f)
            {
                angleDifference = 360f - angleDifference;
            }
            Debug.Log($"NekoTrigger: 現在の角度: {currentAngle:F2}度, 0度との差: {angleDifference:F2}度, 範囲内: {angleDifference >= minAngleDifference}");
        }
    }

    /// <summary>
    /// トリガー内にいる間
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            isTouchingGround = true;
        }
    }

    /// <summary>
    /// トリガーから出たとき
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Debug.Log($"NekoTrigger: Groundタグのオブジェクトから離脱 - {other.gameObject.name}, タイマー: {timer:F2}秒");
            isTouchingGround = false;
            timer = 0f; // タイマーをリセット
            hasTriggered = false; // リセットして再度トリガー可能にする
        }
    }

    /// <summary>
    /// ダジャレを成立させる
    /// </summary>
    private void TriggerPun()
    {
        if (punDisplayGenerator == null)
        {
            Debug.LogWarning($"NekoTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
            return;
        }

        Debug.Log($"NekoTrigger: ダジャレ発生！ID: \"{punId}\"");
        
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
        
        Debug.Log($"NekoTrigger: インターバル待機開始 ({destroyInterval}秒)");
        
        // インターバル待機
        yield return new WaitForSeconds(destroyInterval);
        
        Debug.Log($"NekoTrigger: インターバル終了。フェードアウトを開始します。");
        
        // フェードアウト
        yield return StartCoroutine(FadeOut());
        
        Debug.Log($"NekoTrigger: フェードアウト完了。オブジェクトをDestroyします。");
        
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
}

