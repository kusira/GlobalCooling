using UnityEngine;
using System.Collections;

/// <summary>
/// アルミ缶の上にあるみかんのトリガーを管理するスクリプト
/// TangerinesをJudementTopの上に置いて一定時間経過でダジャレを成立させる
/// </summary>
public class ArumikanTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立までの待機時間（秒）")]
    [SerializeField] private float triggerWaitTime = 3f;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Arumikan";
    
    [Tooltip("Tangerinesオブジェクト（判定対象）")]
    [SerializeField] private GameObject tangerinesObject;
    
    [Tooltip("JudementTopオブジェクト（判定床）")]
    [SerializeField] private GameObject judgmentTopObject;
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private Collider2D judgmentTopCollider; // JudementTopのCollider2D
    private Rigidbody2D tangerinesRigidbody; // TangerinesのRigidbody2D
    private bool isTangerinesInTrigger = false; // Tangerinesがトリガー内にいるか
    private float timer = 0f; // タイマー
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private SpriteRenderer[] spriteRenderers; // このオブジェクトとその子オブジェクトのSpriteRenderer
    private bool isFadingOut = false; // フェードアウト中かどうか

    private void Awake()
    {
        // JudementTopのCollider2Dを取得
        if (judgmentTopObject != null)
        {
            // 別オブジェクトから取得
            judgmentTopCollider = judgmentTopObject.GetComponent<Collider2D>();
            if (judgmentTopCollider != null)
            {
                // JudementTopにヘルパースクリプトを追加（既にある場合は追加しない）
                JudgmentTopTriggerHelper helper = judgmentTopObject.GetComponent<JudgmentTopTriggerHelper>();
                if (helper == null)
                {
                    helper = judgmentTopObject.AddComponent<JudgmentTopTriggerHelper>();
                }
                helper.SetArumikanTrigger(this);
            }
        }
        
        // TangerinesのRigidbody2Dを取得（参照のみ、使用しない）
        if (tangerinesObject != null)
        {
            tangerinesRigidbody = tangerinesObject.GetComponent<Rigidbody2D>();
        }
        
        // このオブジェクトとその子オブジェクトのSpriteRendererを取得
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // Tangerinesがトリガー内にいる場合、タイマーを進める
        if (isTangerinesInTrigger && !hasTriggered)
        {
            timer += Time.deltaTime;
            
            // 待機時間を超えたらダジャレを発生
            if (timer >= triggerWaitTime)
            {
                TriggerPun();
                hasTriggered = true;
            }
        }
        else
        {
            // 条件を満たしていない場合はタイマーをリセット
            if (timer > 0f)
            {
                timer = 0f;
            }
        }
    }

    /// <summary>
    /// Tangerinesがトリガーに入ったとき（JudgmentTopTriggerHelperから呼ばれる）
    /// </summary>
    public void OnTangerinesEnter(Collider2D other)
    {
        if (tangerinesObject == null)
        {
            return;
        }
        
        if (other.gameObject == tangerinesObject)
        {
            isTangerinesInTrigger = true;
            timer = 0f; // タイマーをリセット
        }
    }

    /// <summary>
    /// Tangerinesがトリガー内にいる間（JudgmentTopTriggerHelperから呼ばれる）
    /// </summary>
    public void OnTangerinesStay(Collider2D other)
    {
        if (other.gameObject == tangerinesObject)
        {
            isTangerinesInTrigger = true;
        }
    }

    /// <summary>
    /// Tangerinesがトリガーから出たとき（JudgmentTopTriggerHelperから呼ばれる）
    /// </summary>
    public void OnTangerinesExit(Collider2D other)
    {
        if (other.gameObject == tangerinesObject)
        {
            isTangerinesInTrigger = false;
            timer = 0f; // タイマーをリセット
            hasTriggered = false; // リセットして再度トリガー可能にする
        }
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
        punDisplayGenerator.GeneratePun(punId);
        
        // インターバル後にフェードアウトしてDestroy
        StartCoroutine(DestroyAfterFadeOut());
    }
    
    /// <summary>
    /// インターバル後にフェードアウトしてDestroy
    /// </summary>
    private IEnumerator DestroyAfterFadeOut()
    {
        // 既にフェードアウト中の場合は何もしない
        if (isFadingOut)
        {
            yield break;
        }
        
        isFadingOut = true;
        
        // インターバル待機
        yield return new WaitForSeconds(destroyInterval);
        
        // フェードアウト
        yield return StartCoroutine(FadeOut());
        
        // Destroy
        Destroy(gameObject);
    }
    
    /// <summary>
    /// フェードアウト処理
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            yield break;
        }
        
        // 各SpriteRendererの初期Alpha値を保存
        float[] initialAlphas = new float[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                initialAlphas[i] = spriteRenderers[i].color.a;
            }
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            
            // 各SpriteRendererのAlphaを更新
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color color = spriteRenderers[i].color;
                    color.a = initialAlphas[i] * alpha;
                    spriteRenderers[i].color = color;
                }
            }
            
            yield return null;
        }
        
        // 最終的にAlphaを0に設定
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color color = spriteRenderers[i].color;
                color.a = 0f;
                spriteRenderers[i].color = color;
            }
        }
    }
}

