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
    
    private Rigidbody2D rb;

    private void Awake()
    {
        // 石オブジェクトからRigidbody2Dを取得
        if (stoneObject != null)
        {
            rb = stoneObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"IsiTrigger: StoneオブジェクトにRigidbody2Dが見つかりません。GameObject: {stoneObject.name}");
            }
            else
            {
                Debug.Log($"IsiTrigger: 初期化完了 - Stone: {stoneObject.name}, minYPosition: {minYPosition}, triggerInterval: {triggerInterval}秒, punId: {punId}");
            }
        }
        else
        {
            Debug.LogError($"IsiTrigger: Stoneオブジェクトがアサインされていません。GameObject: {gameObject.name}");
        }
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
            
            Debug.Log($"IsiTrigger: ドラッグ終了 - Y座標: {yPosition:F2}, 閾値: {minYPosition:F2}, 速度: {releaseVelocity.magnitude:F2}");
            
            // Y軸が閾値以上かチェック
            if (yPosition >= minYPosition)
            {
                Debug.Log($"IsiTrigger: トリガー条件を満たしました！インターバル({triggerInterval}秒)後にダジャレを発生させます。");
                // インターバル後にダジャレを成立させる
                StartCoroutine(TriggerPunDelayed());
            }
            else
            {
                Debug.Log($"IsiTrigger: Y座標が閾値未満のため、トリガーしません。");
            }
        }
        else
        {
            Debug.LogWarning($"IsiTrigger: Stoneオブジェクトがnullです。");
        }
    }

    /// <summary>
    /// インターバル後にダジャレを成立させる
    /// </summary>
    private IEnumerator TriggerPunDelayed()
    {
        Debug.Log($"IsiTrigger: インターバル待機開始 ({triggerInterval}秒)");
        
        // インターバル時間を待つ
        yield return new WaitForSeconds(triggerInterval);
        
        Debug.Log($"IsiTrigger: インターバル終了。ダジャレを発生させます。");
        
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
            Debug.LogWarning($"IsiTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
            return;
        }

        Debug.Log($"IsiTrigger: ダジャレ発生！ID: \"{punId}\"");
        
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

