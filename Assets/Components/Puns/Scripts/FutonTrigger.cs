using UnityEngine;

/// <summary>
/// ふとんのトリガーを管理するスクリプト
/// ドラッグを離したときの速度が一定値以上でダジャレを成立させる
/// </summary>
public class FutonTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立に必要な速度の閾値")]
    [SerializeField] private float triggerSpeedThreshold = 50f;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID（デフォルト: \"1\" = ふとんが吹っ飛んだ）")]
    [SerializeField] private string punId = "1";
    
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"FutonTrigger: Rigidbody2Dが見つかりません。GameObject: {gameObject.name}");
        }
    }

    /// <summary>
    /// ドラッグが終了したときに呼び出される（DragAndDropManagerから呼び出される想定）
    /// </summary>
    /// <param name="releaseVelocity">離したときの速度</param>
    public void OnDragReleased(Vector3 releaseVelocity)
    {
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
