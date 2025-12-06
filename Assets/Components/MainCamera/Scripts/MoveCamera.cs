using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 空の空間をドラッグすることでカメラのX座標を移動させるスクリプト
/// </summary>
public class MoveCamera : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [Tooltip("カメラのX座標の最小値")]
    [SerializeField] private float minX = -10f;
    
    [Tooltip("カメラのX座標の最大値")]
    [SerializeField] private float maxX = 10f;
    
    [Tooltip("ドラッグ感度（マウスの移動量に対するカメラの移動量）")]
    [SerializeField] private float dragSensitivity = 1f;
    
    private Camera mainCamera;
    private Vector3 previousMouseWorldPosition; // 前フレームのマウスのワールド座標
    private bool isDragging = false; // ドラッグ中かどうか

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("MoveCamera: Cameraが見つかりません。");
        }
    }

    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 入力処理
    /// </summary>
    private void HandleInput()
    {
        // マウスが存在しない場合は何もしない
        if (Mouse.current == null)
        {
            return;
        }

        // マウスボタンを押した時
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDrag();
        }
        // マウスボタンを離した時
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
        // ドラッグ中
        else if (isDragging)
        {
            UpdateCameraPosition();
        }
    }

    /// <summary>
    /// ドラッグを開始
    /// </summary>
    private void StartDrag()
    {
        // マウス位置からRaycastを飛ばしてオブジェクトを検出
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        
        // 何も当たらない場合（空の空間）のみカメラをドラッグ
        if (hit.collider == null)
        {
            // DragAndDropManagerがオブジェクトをドラッグ中でないことを確認
            DragAndDropManager dragManager = FindFirstObjectByType<DragAndDropManager>();
            if (dragManager != null)
            {
                // DragAndDropManagerがドラッグ中かどうかを確認
                // 簡易的な方法：マウス位置にRigidbody2Dがあるかチェック
                Collider2D collider = Physics2D.OverlapPoint(mouseWorldPos);
                if (collider != null && collider.attachedRigidbody != null && 
                    collider.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
                {
                    // オブジェクトがある場合はカメラドラッグを開始しない
                    return;
                }
            }
            
            isDragging = true;
            previousMouseWorldPosition = mouseWorldPos;
        }
        // オブジェクトに当たった場合はカメラドラッグを開始しない（何もしない）
    }

    /// <summary>
    /// カメラの位置を更新
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (mainCamera == null)
        {
            return;
        }

        // マウス位置をワールド座標に変換
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // DragAndDropManagerがオブジェクトをドラッグ中かどうかを確認
        DragAndDropManager dragManager = FindFirstObjectByType<DragAndDropManager>();
        if (dragManager != null)
        {
            // 簡易的な方法：マウス位置にRigidbody2Dがあるかチェック
            Collider2D collider = Physics2D.OverlapPoint(mouseWorldPos);
            if (collider != null && collider.attachedRigidbody != null && 
                collider.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
            {
                // オブジェクトをドラッグ中の場合はカメラを移動しない
                return;
            }
        }
        
        // マウスの移動量を計算
        float deltaX = (mouseWorldPos.x - previousMouseWorldPosition.x) * dragSensitivity;
        
        // カメラの新しいX座標を計算
        float newX = transform.position.x + deltaX;
        
        // minとmaxで範囲を制限
        newX = Mathf.Clamp(newX, minX, maxX);
        
        // カメラの位置を更新（X座標のみ）
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        
        // 次のフレーム用にマウス位置を記録
        previousMouseWorldPosition = mouseWorldPos;
    }

    /// <summary>
    /// ドラッグを終了
    /// </summary>
    private void EndDrag()
    {
        isDragging = false;
    }

    /// <summary>
    /// マウス位置をワールド座標に変換
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            return Vector3.zero;
        }
        
        // 新しいInput Systemからマウス位置を取得
        Vector2 mouseScreenPos = Mouse.current != null 
            ? Mouse.current.position.ReadValue() 
            : Vector2.zero;
        
        Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f; // 2DなのでZ座標は0
        
        return mouseWorldPos;
    }
}
