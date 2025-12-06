using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// アザラシのトリガーを管理するスクリプト
/// ゲームオブジェクトを10回クリックするとダジャレを成立させる
/// </summary>
public class AzarasiTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立に必要なクリック回数")]
    [SerializeField] private int requiredClickCount = 10;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Azarasi";
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private int currentClickCount = 0; // 現在のクリック回数
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか
    private SpriteRenderer[] spriteRenderers; // このオブジェクトと子オブジェクトのSpriteRenderer
    private Camera mainCamera;
    private Collider2D objectCollider; // このオブジェクトのCollider2D

    private void Awake()
    {
        // メインカメラを取得
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // Collider2Dを取得
        objectCollider = GetComponent<Collider2D>();
        if (objectCollider == null)
        {
            Debug.LogWarning($"AzarasiTrigger: Collider2Dが見つかりません。GameObject: {gameObject.name}");
        }
        
        // このオブジェクトと子オブジェクトのSpriteRendererを取得
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // 既にトリガー済みの場合は何もしない
        if (hasTriggered)
        {
            return;
        }
        
        // マウスが存在しない場合は何もしない
        if (Mouse.current == null)
        {
            return;
        }
        
        // マウスボタンが押された時
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckClick();
        }
    }
    
    /// <summary>
    /// クリックをチェック
    /// </summary>
    private void CheckClick()
    {
        if (objectCollider == null || mainCamera == null)
        {
            return;
        }
        
        // マウス位置からRaycastを飛ばしてオブジェクトを検出
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        
        // このオブジェクトがクリックされたかチェック
        if (hit.collider != null && hit.collider == objectCollider)
        {
            currentClickCount++;
            
            // 必要なクリック回数に達したらダジャレを成立させる
            if (currentClickCount >= requiredClickCount)
            {
                hasTriggered = true;
                TriggerPun();
            }
        }
    }
    
    /// <summary>
    /// マウスのワールド座標を取得
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        // 新しいInput Systemからマウス位置を取得
        Vector2 mouseScreenPos = Mouse.current != null 
            ? Mouse.current.position.ReadValue() 
            : Vector2.zero;
        
        Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f; // 2DなのでZ座標は0
        
        return mouseWorldPos;
    }
    
    /// <summary>
    /// ダジャレを成立させる
    /// </summary>
    private void TriggerPun()
    {
        if (punDisplayGenerator == null)
        {
            Debug.LogWarning($"AzarasiTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
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

