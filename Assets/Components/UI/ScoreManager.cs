using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

/// <summary>
/// スコアを管理するスクリプト
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("現在のスコアを表示するTextMeshPro")]
    [SerializeField] private TMP_Text currentScoreText;
    
    [Header("Animation Settings")]
    [Tooltip("スコア更新時の拡縮アニメーション時間（秒）")]
    [SerializeField] private float scaleAnimationDuration = 0.3f;
    
    [Tooltip("スコア更新時の拡縮倍率")]
    [SerializeField] private float scaleMultiplier = 1.2f;
    
    [Header("Result Settings")]
    [Tooltip("リザルト画面を表示するスコア")]
    [SerializeField] private int resultTriggerScore = 10;
    
    [Tooltip("リザルト画面を表示するまでの遅延時間（秒）")]
    [SerializeField] private float resultDelayTime = 3f;
    
    [Tooltip("ResultManagerへの参照")]
    [SerializeField] private ResultManager resultManager;
    
    private bool hasTriggeredResult = false; // リザルト表示をトリガーしたかどうか
    
    /// <summary>
    /// 現在見つけたダジャレ数
    /// </summary>
    public int CurrentScore { get; private set; } = 0;
    
    private void Awake()
    {
        // 初期スコアを表示
        UpdateScoreText();
    }
    
    /// <summary>
    /// スコアをインクリメント
    /// </summary>
    public void IncrementScore()
    {
        CurrentScore++;
        UpdateScoreText();
        PlayScaleAnimation();
        
        // リザルト表示条件をチェック（一度だけ実行）
        if (CurrentScore >= resultTriggerScore && resultManager != null && !hasTriggeredResult)
        {
            hasTriggeredResult = true;
            StartCoroutine(ShowResultDelayed());
        }
    }
    
    /// <summary>
    /// スコアテキストを更新
    /// </summary>
    private void UpdateScoreText()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = CurrentScore.ToString();
        }
    }
    
    /// <summary>
    /// 拡縮アニメーションを再生
    /// </summary>
    private void PlayScaleAnimation()
    {
        if (currentScoreText == null)
        {
            return;
        }
        
        // 既存のアニメーションを停止
        currentScoreText.transform.DOKill();
        
        // 元のスケールを保存
        Vector3 originalScale = Vector3.one;
        
        // 拡大→縮小のアニメーション
        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(
            currentScoreText.transform.DOScale(originalScale * scaleMultiplier, scaleAnimationDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .SetTarget(currentScoreText.transform)
        );
        scaleSequence.Append(
            currentScoreText.transform.DOScale(originalScale, scaleAnimationDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .SetTarget(currentScoreText.transform)
        );
    }
    
    /// <summary>
    /// 遅延後にリザルト画面を表示
    /// </summary>
    private IEnumerator ShowResultDelayed()
    {
        yield return new WaitForSeconds(resultDelayTime);
        
        if (resultManager != null)
        {
            resultManager.ShowResult();
        }
    }
}

