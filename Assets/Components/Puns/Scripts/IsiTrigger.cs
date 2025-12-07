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
    
    [Tooltip("オブジェクトをDestroyするかどうか")]
    [SerializeField] private bool shouldDestroy = true;
    
    private Rigidbody2D rb;
    private bool isFadingOut = false; // フェードアウト中かどうか

    private void Awake()
    {
        // 石オブジェクトからRigidbody2Dを取得
        if (stoneObject != null)
        {
            rb = stoneObject.GetComponent<Rigidbody2D>();
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
        punDisplayGenerator.GeneratePun(punId, gameObject);
        
        // AudioSourceを再生（存在する場合のみ）
        PunTriggerHelper.PlayAudioSource(gameObject);
        
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

