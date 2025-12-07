using UnityEngine;
using DG.Tweening;

/// <summary>
/// ダジャレトリガーの共通処理を提供するヘルパークラス
/// </summary>
public static class PunTriggerHelper
{
    /// <summary>
    /// インターバル後にフェードアウトしてDestroyする共通処理
    /// </summary>
    /// <param name="monoBehaviour">コルーチンを開始するMonoBehaviour</param>
    /// <param name="targetObject">フェードアウトとDestroyの対象となるGameObject</param>
    /// <param name="destroyInterval">ダジャレ発生後のインターバル（秒）</param>
    /// <param name="fadeOutDuration">フェードアウト時間（秒）</param>
    /// <param name="shouldDestroy">オブジェクトをDestroyするかどうか</param>
    /// <param name="isFadingOut">既にフェードアウト中かどうかのフラグ（参照渡し）</param>
    /// <returns>開始されたTweenの参照</returns>
    public static Tween StartDestroyAfterFadeOut(
        MonoBehaviour monoBehaviour,
        GameObject targetObject,
        float destroyInterval,
        float fadeOutDuration,
        bool shouldDestroy,
        ref bool isFadingOut)
    {
        if (monoBehaviour == null || targetObject == null)
        {
            return null;
        }
        
        // 既にフェードアウト中の場合は何もしない
        if (isFadingOut)
        {
            return null;
        }
        
        isFadingOut = true;
        
        // 破壊しない場合はフェードアウトも不要
        if (!shouldDestroy)
        {
            // インターバル待機のみ（実際には何もしない）
            return null;
        }
        
        // DOTweenシーケンスを作成
        Sequence sequence = DOTween.Sequence();
        
        // インターバル待機
        if (destroyInterval > 0f)
        {
            sequence.AppendInterval(destroyInterval);
        }
        
        // フェードアウト処理
        if (fadeOutDuration > 0f)
        {
            sequence.AppendCallback(() => StartFadeOut(targetObject, fadeOutDuration));
            sequence.AppendInterval(fadeOutDuration);
        }
        
        // Destroy
        sequence.AppendCallback(() =>
        {
            if (targetObject != null)
            {
                Object.Destroy(targetObject);
            }
        });
        
        return sequence;
    }
    
    /// <summary>
    /// フェードアウト処理を開始
    /// </summary>
    private static void StartFadeOut(GameObject targetObject, float fadeOutDuration)
    {
        if (targetObject == null)
        {
            return;
        }
        
        SpriteRenderer[] spriteRenderers = targetObject.GetComponentsInChildren<SpriteRenderer>();
        
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            return;
        }
        
        // 各SpriteRendererのAlphaをDOTweenでアニメーション
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null && !renderer.Equals(null))
            {
                // ローカル変数に保存（クロージャー用）
                SpriteRenderer localRenderer = renderer;
                
                // Alphaを0にフェードアウト（nullチェック付きで手動制御）
                DOTween.To(
                    () => localRenderer != null && !localRenderer.Equals(null) ? localRenderer.color.a : 0f,
                    alpha =>
                    {
                        if (localRenderer != null && !localRenderer.Equals(null))
                        {
                            Color color = localRenderer.color;
                            color.a = alpha;
                            localRenderer.color = color;
                        }
                    },
                    0f,
                    fadeOutDuration
                )
                .SetEase(DG.Tweening.Ease.Linear)
                .SetTarget(localRenderer);
            }
        }
    }
    
    /// <summary>
    /// 指定されたGameObjectに付いているAudioSourceを再生（存在する場合のみ）
    /// </summary>
    /// <param name="targetObject">AudioSourceを検索する対象のGameObject</param>
    public static void PlayAudioSource(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }
        
        // AudioSourceを取得（自身または子オブジェクトから）
        AudioSource audioSource = targetObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = targetObject.GetComponentInChildren<AudioSource>();
        }
        
        // AudioSourceが見つかり、AudioClipが設定されている場合のみ再生
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}

