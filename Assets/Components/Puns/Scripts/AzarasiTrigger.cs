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
    
    private int currentClickCount = 0; // 現在のクリック回数
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか
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

