using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    
    [Header("UI Settings")]
    [Tooltip("カメラのX位置を表示するScrollbar")]
    [SerializeField] private Scrollbar positionScrollbar;
    
    private Camera mainCamera;
    private Vector3 previousMouseWorldPosition; // 前フレームのマウスのワールド座標
    private bool isDragging = false; // ドラッグ中かどうか
    private CanvasGroup scrollbarCanvasGroup; // ScrollbarのCanvasGroup（opacity制御用）

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
        
        // ScrollbarのCanvasGroupを取得（親またはScrollbar自体）
        if (positionScrollbar != null)
        {
            scrollbarCanvasGroup = positionScrollbar.GetComponent<CanvasGroup>();
            if (scrollbarCanvasGroup == null)
            {
                // Scrollbar自体にCanvasGroupがない場合は親を探す
                scrollbarCanvasGroup = positionScrollbar.GetComponentInParent<CanvasGroup>();
            }
            
            // それでも見つからない場合はScrollbarのGameObjectに追加
            if (scrollbarCanvasGroup == null)
            {
                scrollbarCanvasGroup = positionScrollbar.gameObject.AddComponent<CanvasGroup>();
            }
            
            // 初期状態は常に表示
            scrollbarCanvasGroup.alpha = 1f;
            scrollbarCanvasGroup.interactable = false;
            scrollbarCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateScrollbar();
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
        // UI要素をクリックしている場合はカメラドラッグを開始しない
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        // マウス位置からRaycastを飛ばしてオブジェクトを検出
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        
        // 何も当たらない場合、またはBackGroundタグを持つオブジェクトに当たった場合にカメラをドラッグ
        bool canDragCamera = hit.collider == null || (hit.collider != null && hit.collider.CompareTag("Background"));
        
        if (canDragCamera)
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
        // それ以外のオブジェクトに当たった場合はカメラドラッグを開始しない（何もしない）
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
    
    /// <summary>
    /// Scrollbarを更新（カメラ位置の表示）
    /// </summary>
    private void UpdateScrollbar()
    {
        if (positionScrollbar == null || scrollbarCanvasGroup == null)
        {
            return;
        }
        
        // カメラの現在のX座標
        float currentCameraX = transform.position.x;
        
        // ScrollbarのValueを更新（0-1の範囲に正規化）
        float normalizedValue = Mathf.InverseLerp(minX, maxX, currentCameraX);
        positionScrollbar.value = normalizedValue;
        
        // opacityは固定（常に1.0）
        scrollbarCanvasGroup.alpha = 1f;
        scrollbarCanvasGroup.interactable = false;
        scrollbarCanvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// カメラの視界範囲を取得
    /// </summary>
    /// <returns>左端と右端のX座標</returns>
    public (float left, float right) GetCameraBounds()
    {
        if (mainCamera == null)
        {
            return (0f, 0f);
        }
        
        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float cameraWidth = orthographicSize * aspect * 2f;
        
        Vector3 cameraPos = transform.position;
        float leftBound = cameraPos.x - cameraWidth * 0.5f;
        float rightBound = cameraPos.x + cameraWidth * 0.5f;
        
        return (leftBound, rightBound);
    }
    
    /// <summary>
    /// カメラ位置を設定（範囲内に制限）
    /// </summary>
    /// <param name="targetX">目標X座標</param>
    public void SetCameraPosition(float targetX)
    {
        float clampedX = Mathf.Clamp(targetX, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
    
    /// <summary>
    /// オブジェクトがカメラの視界内に収まるようにカメラ位置を調整
    /// </summary>
    /// <param name="objectX">オブジェクトのX座標</param>
    public void FollowObject(float objectX)
    {
        if (mainCamera == null)
        {
            return;
        }
        
        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float cameraWidth = orthographicSize * aspect * 2f;
        
        Vector3 cameraPos = transform.position;
        float leftBound = cameraPos.x - cameraWidth * 0.5f;
        float rightBound = cameraPos.x + cameraWidth * 0.5f;
        
        // オブジェクトが左端より外に出そうな場合
        if (objectX < leftBound)
        {
            float newCameraX = objectX + cameraWidth * 0.5f;
            SetCameraPosition(newCameraX);
        }
        // オブジェクトが右端より外に出そうな場合
        else if (objectX > rightBound)
        {
            float newCameraX = objectX - cameraWidth * 0.5f;
            SetCameraPosition(newCameraX);
        }
    }
}
