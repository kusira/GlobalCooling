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
    
    [Tooltip("オブジェクトをDestroyするかどうか")]
    [SerializeField] private bool shouldDestroy = true;
    
    private Rigidbody2D rb;
    private bool isFadingOut = false; // フェードアウト中かどうか

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"FutonTrigger: Rigidbody2Dが見つかりません。GameObject: {gameObject.name}");
        }
        
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
        punDisplayGenerator.GeneratePun(punId, gameObject);
        
        // インターバル後にフェードアウトしてDestroy（共通処理を使用）
        PunTriggerHelper.StartDestroyAfterFadeOut(
            this,
            gameObject,
            destroyInterval,
            fadeOutDuration,
            shouldDestroy,
            ref isFadingOut);
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
