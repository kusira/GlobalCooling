using UnityEngine;

/// <summary>
/// PrincipalのCollider2Dのトリガーイベントを検知して、KoutyouTriggerに転送するヘルパースクリプト
/// </summary>
public class KoutyouTriggerHelper : MonoBehaviour
{
    private KoutyouTrigger koutyouTrigger;

    /// <summary>
    /// KoutyouTriggerへの参照を設定
    /// </summary>
    public void SetKoutyouTrigger(KoutyouTrigger trigger)
    {
        koutyouTrigger = trigger;
    }

    /// <summary>
    /// トリガーに入ったとき
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (koutyouTrigger != null)
        {
            koutyouTrigger.OnBeerEnter(other);
        }
    }

    /// <summary>
    /// トリガー内にいる間
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (koutyouTrigger != null)
        {
            koutyouTrigger.OnBeerStay(other);
        }
    }

    /// <summary>
    /// トリガーから出たとき
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (koutyouTrigger != null)
        {
            koutyouTrigger.OnBeerExit(other);
        }
    }
}

