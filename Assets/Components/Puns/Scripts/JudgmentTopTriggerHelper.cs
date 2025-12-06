using UnityEngine;

/// <summary>
/// JudementTopのCollider2Dのトリガーイベントを検知して、ArumikanTriggerに転送するヘルパースクリプト
/// </summary>
public class JudgmentTopTriggerHelper : MonoBehaviour
{
    private ArumikanTrigger arumikanTrigger;

    /// <summary>
    /// ArumikanTriggerへの参照を設定
    /// </summary>
    public void SetArumikanTrigger(ArumikanTrigger trigger)
    {
        arumikanTrigger = trigger;
    }

    /// <summary>
    /// トリガーに入ったとき
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (arumikanTrigger != null)
        {
            arumikanTrigger.OnTangerinesEnter(other);
        }
    }

    /// <summary>
    /// トリガー内にいる間
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (arumikanTrigger != null)
        {
            arumikanTrigger.OnTangerinesStay(other);
        }
    }

    /// <summary>
    /// トリガーから出たとき
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (arumikanTrigger != null)
        {
            arumikanTrigger.OnTangerinesExit(other);
        }
    }
}

