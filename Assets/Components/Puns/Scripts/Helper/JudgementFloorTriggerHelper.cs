using UnityEngine;

/// <summary>
/// JudgementFloorのCollider2Dのトリガーイベントを検知して、KabanTriggerに転送するヘルパースクリプト
/// </summary>
public class JudgementFloorTriggerHelper : MonoBehaviour
{
    private KabanTrigger kabanTrigger;

    /// <summary>
    /// KabanTriggerへの参照を設定
    /// </summary>
    public void SetKabanTrigger(KabanTrigger trigger)
    {
        kabanTrigger = trigger;
    }

    /// <summary>
    /// トリガーに入ったとき
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (kabanTrigger != null)
        {
            kabanTrigger.OnBagEnterJudgementFloor(other);
        }
    }

    /// <summary>
    /// トリガー内にいる間
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (kabanTrigger != null)
        {
            kabanTrigger.OnBagStayJudgementFloor(other);
        }
    }

    /// <summary>
    /// トリガーから出たとき
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (kabanTrigger != null)
        {
            kabanTrigger.OnBagExitJudgementFloor(other);
        }
    }
}

