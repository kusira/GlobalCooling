using UnityEngine;

/// <summary>
/// 受け取るオブジェクトのCollider2Dのトリガーイベントを検知して、各トリガーに転送するヘルパースクリプト
/// </summary>
public class ReceiveTriggerHelper : MonoBehaviour
{
    private ToireTrigger toireTrigger;
    private MinkaTrigger minkaTrigger;

    /// <summary>
    /// ToireTriggerへの参照を設定
    /// </summary>
    public void SetToireTrigger(ToireTrigger trigger)
    {
        toireTrigger = trigger;
    }

    /// <summary>
    /// MinkaTriggerへの参照を設定
    /// </summary>
    public void SetMinkaTrigger(MinkaTrigger trigger)
    {
        minkaTrigger = trigger;
    }

    /// <summary>
    /// トリガーに入ったとき
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (toireTrigger != null)
        {
            toireTrigger.OnGiveObjectEnter(other);
        }
        if (minkaTrigger != null)
        {
            minkaTrigger.OnGiveObjectEnter(other);
        }
    }

    /// <summary>
    /// トリガー内にいる間
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (toireTrigger != null)
        {
            toireTrigger.OnGiveObjectStay(other);
        }
        if (minkaTrigger != null)
        {
            minkaTrigger.OnGiveObjectStay(other);
        }
    }

    /// <summary>
    /// トリガーから出たとき
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (toireTrigger != null)
        {
            toireTrigger.OnGiveObjectExit(other);
        }
        if (minkaTrigger != null)
        {
            minkaTrigger.OnGiveObjectExit(other);
        }
    }
}

