using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D物理演算オブジェクトをドラッグアンドドロップするマネージャー
/// MovePositionを使用してマウス位置にオブジェクトを移動させます
/// </summary>
public class DragAndDropManager : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("ドラッグ中の最大移動速度（0以下で無制限）")]
    [SerializeField] private float maxDragSpeed = 30f;
    
    [Header("Throw Settings")]
    [Tooltip("離したときに加える力の係数")]
    [SerializeField] private float forceMultiplier = 1f;
    
    [Tooltip("離したときに加えるトルクの倍率")]
    [SerializeField] private float torqueMultiplier = 0.5f;
    
    [Tooltip("投げたときの最大速度（0以下で無制限）")]
    [SerializeField] private float maxThrowSpeed = 0.1f;
    
    private Camera mainCamera;
    private Rigidbody2D draggedObject;
    private Vector3 dragOffset; // ドラッグ開始時のオフセット
    private Vector3 previousMousePosition; // 前フレームのマウス位置
    private bool isDragging = false;
    private FutonTrigger currentFutonTrigger; // 現在ドラッグ中のFutonTrigger
    private IsiTrigger currentIsiTrigger; // 現在ドラッグ中のIsiTrigger
    private MoveCamera moveCamera; // カメラ移動制御

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
        
        // MoveCameraコンポーネントを取得
        if (mainCamera != null)
        {
            moveCamera = mainCamera.GetComponent<MoveCamera>();
            if (moveCamera == null)
            {
                moveCamera = FindFirstObjectByType<MoveCamera>();
            }
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
        
        // UI要素をクリックしている場合はドラッグを開始しない
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // マウス位置からRaycastを飛ばしてオブジェクトを検出
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        
        if (hit.collider != null && hit.rigidbody != null && hit.rigidbody.bodyType == RigidbodyType2D.Dynamic)
        {
            draggedObject = hit.rigidbody;
            
            // FutonTriggerコンポーネントを取得
            currentFutonTrigger = draggedObject.GetComponent<FutonTrigger>();
            
            // IsiTriggerコンポーネントを取得
            currentIsiTrigger = draggedObject.GetComponent<IsiTrigger>();
            
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
        
        // 現在の位置から目標位置へのベクトル
        Vector3 currentPosition = draggedObject.transform.position;
        Vector3 direction = targetPosition - currentPosition;
        
        // 最大速度を制限
        if (maxDragSpeed > 0f)
        {
            float distance = direction.magnitude;
            float maxDistancePerFrame = maxDragSpeed * Time.fixedDeltaTime;
            
            if (distance > maxDistancePerFrame)
            {
                // 最大速度を超える場合は、最大速度で移動
                direction = direction.normalized * maxDistancePerFrame;
                targetPosition = currentPosition + direction;
            }
        }
        
        // MovePositionを使用して物理演算を維持しながら位置を更新
        draggedObject.MovePosition(targetPosition);
        
        // オブジェクトがカメラの視界外に出そうな場合、カメラを追従させる
        if (moveCamera != null)
        {
            moveCamera.FollowObject(targetPosition.x);
        }
        
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
        // 投げる力に係数をかける
        Vector2 force = mouseVelocity * forceMultiplier;
        
        // トルクを計算
        float torque = CalculateTorque(mouseVelocity, forcePoint);
        float finalTorque = torque * torqueMultiplier;
        
        // 力を加える（マウス位置を起点として）
        draggedObject.AddForceAtPosition(force, forcePoint, ForceMode2D.Impulse);
        
        // トルクを加える（回転方向はマウスの移動方向に基づく）
        draggedObject.AddTorque(finalTorque, ForceMode2D.Impulse);
        
        // FutonTriggerにドラッグ終了を通知
        if (currentFutonTrigger != null)
        {
            currentFutonTrigger.OnDragReleased(mouseVelocity);
        }
        
        // IsiTriggerにドラッグ終了を通知
        if (currentIsiTrigger != null)
        {
            currentIsiTrigger.OnDragReleased(mouseVelocity);
        }
        
        draggedObject = null;
        currentFutonTrigger = null;
        currentIsiTrigger = null;
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

    /// <summary>
    /// 指定されたRigidbody2Dがドラッグ中かどうかを判定
    /// </summary>
    /// <param name="rb">判定対象のRigidbody2D</param>
    /// <returns>ドラッグ中の場合true</returns>
    public bool IsDragging(Rigidbody2D rb)
    {
        return isDragging && draggedObject == rb;
    }

    /// <summary>
    /// 指定されたGameObjectがドラッグ中かどうかを判定
    /// </summary>
    /// <param name="obj">判定対象のGameObject</param>
    /// <returns>ドラッグ中の場合true</returns>
    public bool IsDragging(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }
        
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            return false;
        }
        
        return IsDragging(rb);
    }
}
