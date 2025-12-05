using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D物理演算オブジェクトをドラッグアンドドロップするマネージャー
/// MovePositionを使用してマウス位置にオブジェクトを移動させます
/// </summary>
public class DragAndDropManager : MonoBehaviour
{
    [Header("Throw Settings")]
    [Tooltip("離したときに加える力の倍率")]
    [SerializeField] private float forceMultiplier = 1f;
    
    [Tooltip("離したときに加えるトルクの倍率")]
    [SerializeField] private float torqueMultiplier = 0.25f;
    
    [Tooltip("投げたときの最大速度（0以下で無制限）")]
    [SerializeField] private float maxThrowSpeed = 10f;
    
    private Camera mainCamera;
    private Rigidbody2D draggedObject;
    private Vector3 dragOffset; // ドラッグ開始時のオフセット
    private Vector3 previousMousePosition; // 前フレームのマウス位置
    private bool isDragging = false;

    private void Awake()
    {
        // メインカメラを取得
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("DragAndDropManager: Cameraが見つかりません。");
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (isDragging && draggedObject != null)
        {
            UpdateDragPosition();
        }
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
    }

    /// <summary>
    /// ドラッグを開始
    /// </summary>
    private void StartDrag()
    {
        // 既にドラッグ中の場合は何もしない
        if (isDragging)
        {
            return;
        }

        // マウス位置からRaycastを飛ばしてオブジェクトを検出
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        
        if (hit.collider != null && hit.rigidbody != null && hit.rigidbody.bodyType == RigidbodyType2D.Dynamic)
        {
            draggedObject = hit.rigidbody;
            
            // ドラッグ開始時のオフセットを計算（クリック位置とオブジェクト中心の差）
            dragOffset = draggedObject.transform.position - mouseWorldPos;
            
            // 初期マウス位置を記録
            previousMousePosition = mouseWorldPos;
            
            isDragging = true;
        }
    }

    /// <summary>
    /// ドラッグ中の位置更新
    /// </summary>
    private void UpdateDragPosition()
    {
        if (draggedObject == null)
        {
            return;
        }

        // マウス位置をワールド座標に変換
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // オフセットを考慮した目標位置（カーソルが真ん中に吸われないようにオフセットを保持）
        Vector3 targetPosition = mouseWorldPos + dragOffset;
        
        // MovePositionを使用して物理演算を維持しながら位置を更新
        draggedObject.MovePosition(targetPosition);
        
        // 次のフレーム用にマウス位置を記録
        previousMousePosition = mouseWorldPos;
    }

    /// <summary>
    /// ドラッグを終了
    /// </summary>
    private void EndDrag()
    {
        if (!isDragging || draggedObject == null)
        {
            return;
        }

        // マウス位置をワールド座標に変換
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // マウスの移動速度を計算（ワールド座標での速度）
        Vector3 mouseVelocity = (mouseWorldPos - previousMousePosition) / Time.fixedDeltaTime;
        
        // 最大速度を制限
        if (maxThrowSpeed > 0f)
        {
            float currentSpeed = mouseVelocity.magnitude;
            if (currentSpeed > maxThrowSpeed)
            {
                mouseVelocity = mouseVelocity.normalized * maxThrowSpeed;
            }
        }
        
        // マウス位置を起点として力を加える
        Vector3 forcePoint = mouseWorldPos; // マウスカーソルの位置
        Vector2 force = mouseVelocity * forceMultiplier;
        
        // 力を加える（マウス位置を起点として）
        draggedObject.AddForceAtPosition(force, forcePoint, ForceMode2D.Impulse);
        
        // トルクを加える（回転方向はマウスの移動方向に基づく）
        float torque = CalculateTorque(mouseVelocity, forcePoint);
        draggedObject.AddTorque(torque * torqueMultiplier, ForceMode2D.Impulse);
        
        draggedObject = null;
        isDragging = false;
    }

    /// <summary>
    /// トルクを計算
    /// </summary>
    private float CalculateTorque(Vector3 mouseVelocity, Vector3 forcePoint)
    {
        if (draggedObject == null)
        {
            return 0f;
        }

        // オブジェクトの中心から力の作用点へのベクトル
        Vector2 toForcePoint = forcePoint - draggedObject.transform.position;
        
        // 力の方向と作用点の位置からトルクを計算
        // トルク = 力 × 距離 × sin(角度)
        // 2Dでは、外積のz成分がトルクになる
        float torque = toForcePoint.x * mouseVelocity.y - toForcePoint.y * mouseVelocity.x;
        
        return torque;
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

    /// <summary>
    /// クリーンアップ処理
    /// </summary>
    private void OnDestroy()
    {
        EndDrag();
    }

    /// <summary>
    /// 無効化時にもクリーンアップ
    /// </summary>
    private void OnDisable()
    {
        EndDrag();
    }
}
