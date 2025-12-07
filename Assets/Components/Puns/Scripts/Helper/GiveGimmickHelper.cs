using UnityEngine;
using System.Collections;

/// <summary>
/// 何かを何かに与えるギミックの共通処理を提供するヘルパークラス
/// </summary>
public static class GiveGimmickHelper
{
    /// <summary>
    /// ホバー時のスケールアニメーションを開始
    /// </summary>
    /// <param name="monoBehaviour">コルーチンを開始するMonoBehaviour</param>
    /// <param name="targetObject">スケールを変更する対象のGameObject</param>
    /// <param name="originalScale">元のスケール</param>
    /// <param name="hoverScaleMultiplier">ホバー時のスケール倍率</param>
    /// <param name="hoverScaleDuration">ホバー時のスケールアニメーション時間（秒）</param>
    /// <param name="hoverScaleCoroutine">既存のコルーチン参照（停止用）</param>
    /// <returns>開始されたコルーチン</returns>
    public static Coroutine StartHoverScaleUp(
        MonoBehaviour monoBehaviour,
        GameObject targetObject,
        Vector3 originalScale,
        float hoverScaleMultiplier,
        float hoverScaleDuration,
        ref Coroutine hoverScaleCoroutine)
    {
        if (monoBehaviour == null || targetObject == null)
        {
            return null;
        }
        
        // 既存のホバーアニメーションを停止
        if (hoverScaleCoroutine != null)
        {
            monoBehaviour.StopCoroutine(hoverScaleCoroutine);
        }
        
        hoverScaleCoroutine = monoBehaviour.StartCoroutine(
            HoverScaleAnimation(targetObject, originalScale, originalScale * hoverScaleMultiplier, hoverScaleDuration));
        
        return hoverScaleCoroutine;
    }
    
    /// <summary>
    /// ホバー時のスケールダウンを開始
    /// </summary>
    /// <param name="monoBehaviour">コルーチンを開始するMonoBehaviour</param>
    /// <param name="targetObject">スケールを変更する対象のGameObject</param>
    /// <param name="originalScale">元のスケール</param>
    /// <param name="hoverScaleDuration">ホバー時のスケールアニメーション時間（秒）</param>
    /// <param name="hoverScaleCoroutine">既存のコルーチン参照（停止用）</param>
    /// <returns>開始されたコルーチン</returns>
    public static Coroutine StartHoverScaleDown(
        MonoBehaviour monoBehaviour,
        GameObject targetObject,
        Vector3 originalScale,
        float hoverScaleDuration,
        ref Coroutine hoverScaleCoroutine)
    {
        if (monoBehaviour == null || targetObject == null)
        {
            return null;
        }
        
        // 既存のホバーアニメーションを停止
        if (hoverScaleCoroutine != null)
        {
            monoBehaviour.StopCoroutine(hoverScaleCoroutine);
        }
        
        hoverScaleCoroutine = monoBehaviour.StartCoroutine(
            HoverScaleAnimation(targetObject, targetObject.transform.localScale, originalScale, hoverScaleDuration));
        
        return hoverScaleCoroutine;
    }
    
    /// <summary>
    /// ホバー時のスケールアニメーション
    /// </summary>
    private static IEnumerator HoverScaleAnimation(
        GameObject targetObject,
        Vector3 fromScale,
        Vector3 toScale,
        float hoverScaleDuration)
    {
        if (targetObject == null)
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < hoverScaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / hoverScaleDuration);
            targetObject.transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }
        
        // 最終的に目標のスケールに設定
        targetObject.transform.localScale = toScale;
    }
    
    /// <summary>
    /// 与えた時のリアクション（色変更とスケール）を開始
    /// </summary>
    /// <param name="monoBehaviour">コルーチンを開始するMonoBehaviour</param>
    /// <param name="targetObject">リアクションを適用する対象のGameObject</param>
    /// <param name="spriteRenderers">対象のSpriteRenderer配列</param>
    /// <param name="originalColors">元の色配列</param>
    /// <param name="originalScale">元のスケール</param>
    /// <param name="reactionColor">リアクション時の色</param>
    /// <param name="colorChangeDuration">色変更時間（秒）</param>
    /// <param name="scaleMultiplier">スケール倍率</param>
    /// <param name="scaleDuration">スケールアニメーション時間（秒）</param>
    /// <returns>開始されたコルーチン</returns>
    public static Coroutine StartReaction(
        MonoBehaviour monoBehaviour,
        GameObject targetObject,
        SpriteRenderer[] spriteRenderers,
        Color[] originalColors,
        Vector3 originalScale,
        Color reactionColor,
        float colorChangeDuration,
        float scaleMultiplier,
        float scaleDuration)
    {
        if (monoBehaviour == null || targetObject == null)
        {
            return null;
        }
        
        return monoBehaviour.StartCoroutine(
            ReactionCoroutine(targetObject, spriteRenderers, originalColors, originalScale, reactionColor, colorChangeDuration, scaleMultiplier, scaleDuration));
    }
    
    /// <summary>
    /// リアクションコルーチン（色変更とスケール）
    /// </summary>
    private static IEnumerator ReactionCoroutine(
        GameObject targetObject,
        SpriteRenderer[] spriteRenderers,
        Color[] originalColors,
        Vector3 originalScale,
        Color reactionColor,
        float colorChangeDuration,
        float scaleMultiplier,
        float scaleDuration)
    {
        if (targetObject == null || spriteRenderers == null)
        {
            yield break;
        }
        
        // 色とScaleの目標値を設定
        Vector3 targetScale = originalScale * scaleMultiplier;
        Color[] targetColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color originalColor = originalColors[i];
                targetColors[i] = new Color(reactionColor.r, reactionColor.g, reactionColor.b, originalColor.a);
            }
        }
        
        // 色のフェードインとScaleの拡大を同時に実行
        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(colorChangeDuration, scaleDuration);
        
        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // 色のフェードイン（常に更新してスムーズに変化させる）
            float colorT = 0f;
            if (colorChangeDuration > 0f)
            {
                colorT = Mathf.Clamp01(elapsedTime / colorChangeDuration);
            }
            else
            {
                colorT = 1f; // 時間が0の場合は即座に変更
            }
            
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color originalColor = originalColors[i];
                    spriteRenderers[i].color = Color.Lerp(originalColor, targetColors[i], colorT);
                }
            }
            
            // Scaleの拡大
            float scaleT = 0f;
            if (scaleDuration > 0f)
            {
                scaleT = Mathf.Clamp01(elapsedTime / scaleDuration);
            }
            else
            {
                scaleT = 1f; // 時間が0の場合は即座に変更
            }
            targetObject.transform.localScale = Vector3.Lerp(originalScale, targetScale, scaleT);
            
            yield return null;
        }
        
        // 最終的に目標値に設定（確実に目標値に到達させる）
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = targetColors[i];
            }
        }
        targetObject.transform.localScale = targetScale;
        
        // 色のフェードアウトとScaleの縮小を同時に実行
        elapsedTime = 0f;
        maxDuration = Mathf.Max(colorChangeDuration, scaleDuration);
        
        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // 色のフェードアウト（常に更新してスムーズに変化させる）
            float colorT = 0f;
            if (colorChangeDuration > 0f)
            {
                colorT = Mathf.Clamp01(elapsedTime / colorChangeDuration);
            }
            else
            {
                colorT = 1f; // 時間が0の場合は即座に変更
            }
            
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = Color.Lerp(targetColors[i], originalColors[i], colorT);
                }
            }
            
            // Scaleの縮小
            float scaleT = 0f;
            if (scaleDuration > 0f)
            {
                scaleT = Mathf.Clamp01(elapsedTime / scaleDuration);
            }
            else
            {
                scaleT = 1f; // 時間が0の場合は即座に変更
            }
            targetObject.transform.localScale = Vector3.Lerp(targetScale, originalScale, scaleT);
            
            yield return null;
        }
        
        // 最終的に元のScaleに設定
        targetObject.transform.localScale = originalScale;
        
        // 最終的に元の色に設定
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }
    
    /// <summary>
    /// 指定されたGameObjectがドラッグ中かどうかを判定
    /// </summary>
    /// <param name="obj">判定対象のGameObject</param>
    /// <param name="dragAndDropManager">DragAndDropManagerへの参照（nullの場合は自動検索）</param>
    /// <returns>ドラッグ中の場合true</returns>
    public static bool IsDragging(GameObject obj, DragAndDropManager dragAndDropManager = null)
    {
        if (obj == null)
        {
            return false;
        }
        
        // DragAndDropManagerが指定されていない場合は自動検索
        if (dragAndDropManager == null)
        {
            dragAndDropManager = Object.FindFirstObjectByType<DragAndDropManager>();
        }
        
        if (dragAndDropManager == null)
        {
            return false;
        }
        
        return dragAndDropManager.IsDragging(obj);
    }
    
    /// <summary>
    /// 指定されたGameObjectに付いているDragAndDropManagerを無効化
    /// </summary>
    /// <param name="obj">対象のGameObject</param>
    public static void DisableDragAndDropManager(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        
        DragAndDropManager dragAndDropManager = obj.GetComponent<DragAndDropManager>();
        if (dragAndDropManager != null)
        {
            dragAndDropManager.enabled = false;
        }
    }
}

