using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 空の空間をドラッグすることでカメラのX座標とY座標を移動させるスクリプト
/// </summary>
public class MoveCamera : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [Tooltip("カメラのX座標の最小値")]
    [SerializeField] private float minX = -10f;
    
    [Tooltip("カメラのX座標の最大値")]
    [SerializeField] private float maxX = 10f;
    
    [Tooltip("カメラのY座標の最小値")]
    [SerializeField] private float minY = 0f;
    
    [Tooltip("カメラのY座標の最大値")]
    [SerializeField] private float maxY = 10f;
    
    [Tooltip("ドラッグ感度（マウスの移動量に対するカメラの移動量）")]
    [SerializeField] private float dragSensitivity = 1f;
    
    [Header("Edge Scroll Settings")]
    [Tooltip("画面端の検出エリアの幅（ピクセル）")]
    [SerializeField] private float edgeDetectionWidth = 50f;
    
    [Tooltip("画面端での自動スクロール速度")]
    [SerializeField] private float edgeScrollSpeed = 5f;
    
    [Tooltip("画面端スクロールを有効にするか")]
    [SerializeField] private bool enableEdgeScroll = true;
    
    [Header("Zoom Settings")]
    [Tooltip("カメラの最小サイズ（ズームイン時のサイズ）")]
    [SerializeField] private float minOrthographicSize = 5f;
    
    [Tooltip("カメラの最大サイズ（ズームアウト時のサイズ）")]
    [SerializeField] private float maxOrthographicSize = 15f;
    
    [Tooltip("ズーム速度（マウスホイール1回あたりのサイズ変化量）")]
    [SerializeField] private float zoomSpeed = 1f;
    
    [Tooltip("ズームを有効にするか")]
    [SerializeField] private bool enableZoom = true;
    
    [Header("Map Settings")]
    [Tooltip("マップのGameObject")]
    [SerializeField] private GameObject map;
    
    [Tooltip("マップ上の位置を示すアイコン（Sign）")]
    [SerializeField] private GameObject sign;
    
    [Tooltip("マップの基準サイズ（カメラサイズが基準サイズの時のマップサイズ）")]
    [SerializeField] private float mapBaseSize = 5f;
    
    private Camera mainCamera;
    private Vector3 previousMouseWorldPosition; // 前フレームのマウスのワールド座標
    private bool isDragging = false; // ドラッグ中かどうか
    private float initialOrthographicSize; // 初期のorthographicSize

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
        else
        {
            // 初期のorthographicSizeを保存
            initialOrthographicSize = mainCamera.orthographicSize;
        }
    }

    private void Update()
    {
        HandleInput();
        HandleZoom();
        HandleEdgeScroll();
        UpdateSignPosition();
        UpdateMapScale();
    }

    /// <summary>
    /// ズーム処理
    /// </summary>
    private void HandleZoom()
    {
        if (!enableZoom || mainCamera == null || Mouse.current == null)
        {
            return;
        }

        // UI要素上にマウスがある場合はズームしない
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // マウスホイールのスクロール量を取得
        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
        float scrollValue = scrollDelta.y;

        if (Mathf.Abs(scrollValue) > 0.01f)
        {
            // 現在のサイズを取得
            float currentSize = mainCamera.orthographicSize;

            // ズーム量を計算
            float zoomDelta = -scrollValue * zoomSpeed * Time.deltaTime;
            float newSize = Mathf.Clamp(currentSize + zoomDelta, minOrthographicSize, maxOrthographicSize);

            // カメラサイズを更新
            mainCamera.orthographicSize = newSize;
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
        float deltaY = (mouseWorldPos.y - previousMouseWorldPosition.y) * dragSensitivity;
        
        // カメラの新しい座標を計算
        float newX = transform.position.x + deltaX;
        float newY = transform.position.y + deltaY;
        
        // minとmaxで範囲を制限
        newX = Mathf.Clamp(newX, minX, maxX);
        newY = Mathf.Clamp(newY, minY, maxY);
        
        // カメラの位置を更新（X座標とY座標）
        transform.position = new Vector3(newX, newY, transform.position.z);
        
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
    /// 画面端での自動スクロール処理（クリック中のみ）
    /// </summary>
    private void HandleEdgeScroll()
    {
        // 画面端スクロールが無効、またはマウスボタンが押されていない場合は何もしない
        if (!enableEdgeScroll || mainCamera == null || Mouse.current == null)
        {
            return;
        }
        
        // マウスボタンが押されているかチェック（クリック中のみ動作）
        if (!Mouse.current.leftButton.isPressed)
        {
            return;
        }
        
        // UI要素上にマウスがある場合は何もしない
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        // DragAndDropManagerがオブジェクトをドラッグ中かどうかを確認
        DragAndDropManager dragManager = FindFirstObjectByType<DragAndDropManager>();
        if (dragManager != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Collider2D collider = Physics2D.OverlapPoint(mouseWorldPos);
            if (collider != null && collider.attachedRigidbody != null && 
                collider.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
            {
                // オブジェクトをドラッグ中の場合はカメラを移動しない
                return;
            }
        }
        
        // マウス位置を取得
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // 移動量を初期化
        float moveX = 0f;
        float moveY = 0f;
        
        // 左端の検出
        if (mouseScreenPos.x < edgeDetectionWidth)
        {
            float normalizedDistance = mouseScreenPos.x / edgeDetectionWidth;
            moveX = -edgeScrollSpeed * (1f - normalizedDistance) * Time.deltaTime;
        }
        // 右端の検出
        else if (mouseScreenPos.x > screenWidth - edgeDetectionWidth)
        {
            float normalizedDistance = (screenWidth - mouseScreenPos.x) / edgeDetectionWidth;
            moveX = edgeScrollSpeed * (1f - normalizedDistance) * Time.deltaTime;
        }
        
        // 下端の検出
        if (mouseScreenPos.y < edgeDetectionWidth)
        {
            float normalizedDistance = mouseScreenPos.y / edgeDetectionWidth;
            moveY = -edgeScrollSpeed * (1f - normalizedDistance) * Time.deltaTime;
        }
        // 上端の検出
        else if (mouseScreenPos.y > screenHeight - edgeDetectionWidth)
        {
            float normalizedDistance = (screenHeight - mouseScreenPos.y) / edgeDetectionWidth;
            moveY = edgeScrollSpeed * (1f - normalizedDistance) * Time.deltaTime;
        }
        
        // カメラを移動
        if (moveX != 0f || moveY != 0f)
        {
            float newX = Mathf.Clamp(transform.position.x + moveX, minX, maxX);
            float newY = Mathf.Clamp(transform.position.y + moveY, minY, maxY);
            transform.position = new Vector3(newX, newY, transform.position.z);
        }
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
    /// カメラ位置を設定（範囲内に制限）
    /// </summary>
    /// <param name="targetX">目標X座標</param>
    /// <param name="targetY">目標Y座標</param>
    public void SetCameraPosition(float targetX, float targetY)
    {
        float clampedX = Mathf.Clamp(targetX, minX, maxX);
        float clampedY = Mathf.Clamp(targetY, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
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

    /// <summary>
    /// オブジェクトがカメラの視界内に収まるようにカメラ位置を調整（X座標とY座標の両方）
    /// </summary>
    /// <param name="objectX">オブジェクトのX座標</param>
    /// <param name="objectY">オブジェクトのY座標</param>
    public void FollowObject(float objectX, float objectY)
    {
        if (mainCamera == null)
        {
            return;
        }
        
        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float cameraWidth = orthographicSize * aspect * 2f;
        float cameraHeight = orthographicSize * 2f;
        
        Vector3 cameraPos = transform.position;
        float leftBound = cameraPos.x - cameraWidth * 0.5f;
        float rightBound = cameraPos.x + cameraWidth * 0.5f;
        float bottomBound = cameraPos.y - cameraHeight * 0.5f;
        float topBound = cameraPos.y + cameraHeight * 0.5f;
        
        float newCameraX = cameraPos.x;
        float newCameraY = cameraPos.y;
        
        // オブジェクトが左端より外に出そうな場合
        if (objectX < leftBound)
        {
            newCameraX = objectX + cameraWidth * 0.5f;
        }
        // オブジェクトが右端より外に出そうな場合
        else if (objectX > rightBound)
        {
            newCameraX = objectX - cameraWidth * 0.5f;
        }
        
        // オブジェクトが下端より外に出そうな場合
        if (objectY < bottomBound)
        {
            newCameraY = objectY + cameraHeight * 0.5f;
        }
        // オブジェクトが上端より外に出そうな場合
        else if (objectY > topBound)
        {
            newCameraY = objectY - cameraHeight * 0.5f;
        }
        
        // カメラ位置を更新（範囲内に制限）
        SetCameraPosition(newCameraX, newCameraY);
    }
    
    /// <summary>
    /// Signの位置を更新（カメラ位置に対応するMap上の位置）
    /// </summary>
    private void UpdateSignPosition()
    {
        if (map == null || sign == null)
        {
            return;
        }
        
        // MapのRectTransformを取得
        RectTransform mapRectTransform = map.GetComponent<RectTransform>();
        if (mapRectTransform == null)
        {
            return;
        }
        
        // Mapのサイズを取得
        Rect mapRect = mapRectTransform.rect;
        float mapWidth = mapRect.width;
        float mapHeight = mapRect.height;
        
        // カメラの現在位置
        float cameraX = transform.position.x;
        float cameraY = transform.position.y;
        
        // カメラの移動範囲
        float cameraRangeX = maxX - minX;
        float cameraRangeY = maxY - minY;
        
        // カメラ位置を0-1の範囲に正規化
        float normalizedX = cameraRangeX > 0f ? Mathf.InverseLerp(minX, maxX, cameraX) : 0.5f;
        float normalizedY = cameraRangeY > 0f ? Mathf.InverseLerp(minY, maxY, cameraY) : 0.5f;
        
        // Map上の位置を計算（Mapの中心を原点として）
        float signX = (normalizedX - 0.5f) * mapWidth;
        float signY = (normalizedY - 0.5f) * mapHeight;
        
        // Signの位置を更新（Mapのローカル座標系で）
        RectTransform signRectTransform = sign.GetComponent<RectTransform>();
        if (signRectTransform != null)
        {
            // SignをMapの子要素として配置する場合
            if (sign.transform.parent == map.transform)
            {
                signRectTransform.anchoredPosition = new Vector2(signX, signY);
            }
            else
            {
                // SignがMapの子要素でない場合、ワールド座標で計算
                Vector3 mapWorldPos = mapRectTransform.position;
                Vector3 signWorldPos = new Vector3(
                    mapWorldPos.x + signX,
                    mapWorldPos.y + signY,
                    signRectTransform.position.z
                );
                signRectTransform.position = signWorldPos;
            }
        }
        else
        {
            // RectTransformがない場合（通常のTransform）
            if (sign.transform.parent == map.transform)
            {
                sign.transform.localPosition = new Vector3(signX, signY, sign.transform.localPosition.z);
            }
            else
            {
                Vector3 mapWorldPos = map.transform.position;
                sign.transform.position = new Vector3(
                    mapWorldPos.x + signX,
                    mapWorldPos.y + signY,
                    sign.transform.position.z
                );
            }
        }
    }

    /// <summary>
    /// マップのスケールを更新（カメラのズームに応じて）
    /// </summary>
    private void UpdateMapScale()
    {
        if (map == null || mainCamera == null)
        {
            return;
        }

        // カメラサイズに応じてマップのスケールを調整
        float currentSize = mainCamera.orthographicSize;
        float scaleRatio = currentSize / mapBaseSize;

        // マップのスケールを更新
        RectTransform mapRectTransform = map.GetComponent<RectTransform>();
        if (mapRectTransform != null)
        {
            // マップのサイズを調整（基準サイズに対する比率で調整）
            // 初期サイズを基準として、カメラサイズの変化に比例してマップサイズを変更
            float baseScale = initialOrthographicSize / mapBaseSize;
            float newScale = scaleRatio / baseScale;
            
            // 元のサイズを保持するために、初期サイズを基準に計算
            // 実際には、mapBaseSizeが基準となるサイズなので、それに対する比率で調整
            Vector3 currentScale = map.transform.localScale;
            if (currentScale.x != newScale || currentScale.y != newScale)
            {
                map.transform.localScale = new Vector3(newScale, newScale, 1f);
            }
        }
        else
        {
            // RectTransformがない場合、通常のTransformでスケールを調整
            float newScale = scaleRatio;
            map.transform.localScale = new Vector3(newScale, newScale, 1f);
        }
    }
}
