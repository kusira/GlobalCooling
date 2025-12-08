using UnityEngine;
using System.Collections;

/// <summary>
/// 石のトリガーを管理するスクリプト
/// Y軸がある値以上でオブジェクトを落としたときにダジャレを成立させる
/// </summary>
public class IsiTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立に必要なドロップから着地までの最小時間（秒）")]
    [SerializeField] private float minDropToLandTime = 0.3f;
    
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
    private float dropTime = -1f; // ドロップ時刻（-1は未ドロップ状態）
    private bool isWaitingForLanding = false; // 着地待ち中かどうか

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
    /// <param name="dropTime">マウスを離した時刻（Time.time）</param>
    public void OnDragReleased(Vector3 releaseVelocity, float dropTime)
    {
        // ドロップ時刻を記録（DragAndDropManagerから渡された時刻を使用）
        this.dropTime = dropTime;
        isWaitingForLanding = true;
    }

    /// <summary>
    /// 衝突検出（着地検出用）
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 着地待ち中で、石オブジェクトが衝突した場合
        if (isWaitingForLanding && stoneObject != null)
        {
            // このスクリプトがアタッチされているオブジェクトが衝突した場合
            if (collision.gameObject == stoneObject || collision.otherCollider.gameObject == stoneObject)
            {
                CheckLandingTime();
            }
        }
    }

    /// <summary>
    /// トリガー検出（着地検出用、トリガーコライダーがある場合）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 着地待ち中で、石オブジェクトがトリガーに入った場合
        if (isWaitingForLanding && stoneObject != null)
        {
            if (other.gameObject == stoneObject)
            {
                CheckLandingTime();
            }
        }
    }

    /// <summary>
    /// 着地時刻をチェックしてダジャレを成立させる
    /// </summary>
    private void CheckLandingTime()
    {
        if (dropTime < 0f)
        {
            return; // ドロップ時刻が記録されていない
        }

        // ドロップから着地までの時間を計算
        float dropToLandTime = Time.time - dropTime;

        // 一定時間以上経過しているかチェック
        if (dropToLandTime >= minDropToLandTime)
        {
            // インターバル後にダジャレを成立させる
            StartCoroutine(TriggerPunDelayed());
        }

        // 着地待ち状態をリセット
        isWaitingForLanding = false;
        dropTime = -1f;
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

