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
    
    [Tooltip("オブジェクトをDestroyするかどうか")]
    [SerializeField] private bool shouldDestroy = true;
    
    [Header("Sprite Settings")]
    [Tooltip("クリック数が50%未満のときのスプライト（デフォルト）")]
    [SerializeField] private Sprite defaultSprite;
    
    [Tooltip("クリック数が50%以上100%未満のときのスプライト")]
    [SerializeField] private Sprite spriteAt50Percent;
    
    [Tooltip("クリック数が100%のときのスプライト")]
    [SerializeField] private Sprite spriteAt100Percent;
    
    private int currentClickCount = 0; // 現在のクリック回数
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか
    private Camera mainCamera;
    private Collider2D objectCollider; // このオブジェクトのCollider2D
    private SpriteRenderer spriteRenderer; // スプライトレンダラー

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
        
        // SpriteRendererを取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"AzarasiTrigger: SpriteRendererが見つかりません。GameObject: {gameObject.name}");
        }
        
        // 初期スプライトを設定（デフォルトスプライトが設定されている場合）
        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
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
            
            // スプライトを更新
            UpdateSprite();
            
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
    /// クリック数に応じてスプライトを更新
    /// </summary>
    private void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            return;
        }
        
        // クリック数の割合を計算
        float clickPercentage = (float)currentClickCount / requiredClickCount;
        
        // 100%以上の場合
        if (clickPercentage >= 1.0f)
        {
            if (spriteAt100Percent != null)
            {
                spriteRenderer.sprite = spriteAt100Percent;
            }
        }
        // 50%以上100%未満の場合
        else if (clickPercentage >= 0.5f)
        {
            if (spriteAt50Percent != null)
            {
                spriteRenderer.sprite = spriteAt50Percent;
            }
        }
        // 50%未満の場合（デフォルト）
        else
        {
            if (defaultSprite != null)
            {
                spriteRenderer.sprite = defaultSprite;
            }
        }
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
        punDisplayGenerator.GeneratePun(punId, gameObject);
        
        // インターバル後にフェードアウトしてDestroy（共通処理を使用）
        PunTriggerHelper.StartDestroyAfterFadeOut(
            this,
            gameObject,
            destroyInterval,
            fadeOutDuration,
            shouldDestroy,
            ref isFadingOut);
    }
}

