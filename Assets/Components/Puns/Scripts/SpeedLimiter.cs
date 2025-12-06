using UnityEngine;

/// <summary>
/// Rigidbody2Dの速度（移動、回転）を制限するスクリプト
/// </summary>
public class SpeedLimiter : MonoBehaviour
{
    [Header("Speed Limit Settings")]
    [Tooltip("X軸の最大速度（0以下で無制限）")]
    [SerializeField] private float maxXVelocity = 40f;
    
    [Tooltip("Y軸の最大速度（0以下で無制限）")]
    [SerializeField] private float maxYVelocity = 40f;
    
    [Tooltip("全体の最大移動速度（0以下で無制限、X/Y軸個別設定より優先）")]
    [SerializeField] private float maxLinearVelocity = 0f;
    
    [Tooltip("回転スピード（角速度）の最大値（0以下で無制限）")]
    [SerializeField] private float maxAngularVelocity = 360f;
    
    [Header("Camera Boundary Settings")]
    [Tooltip("カメラの境界で跳ね返すかどうか")]
    [SerializeField] private bool enableCameraBoundary = true;
    
    private Camera mainCamera;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"SpeedLimiter: Rigidbody2Dが見つかりません。GameObject: {gameObject.name}");
        }
        
        // カメラの自動検索
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
        }
    }

    private void Update()
    {
        if (rb == null)
        {
            return;
        }
        
        Vector2 velocity = rb.linearVelocity;
        bool velocityChanged = false;
        
        // 全体の最大移動速度を制限
        if (maxLinearVelocity > 0f)
        {
            float currentSpeed = velocity.magnitude;
            if (currentSpeed > maxLinearVelocity)
            {
                velocity = velocity.normalized * maxLinearVelocity;
                velocityChanged = true;
            }
        }
        else
        {
            // X軸の最大速度を制限
            if (maxXVelocity > 0f)
            {
                if (Mathf.Abs(velocity.x) > maxXVelocity)
                {
                    velocity.x = Mathf.Sign(velocity.x) * maxXVelocity;
                    velocityChanged = true;
                }
            }
            
            // Y軸の最大速度を制限
            if (maxYVelocity > 0f)
            {
                if (Mathf.Abs(velocity.y) > maxYVelocity)
                {
                    velocity.y = Mathf.Sign(velocity.y) * maxYVelocity;
                    velocityChanged = true;
                }
            }
        }
        
        // 速度を更新
        if (velocityChanged)
        {
            rb.linearVelocity = velocity;
        }
        
        // 回転スピード（角速度）を制限
        if (maxAngularVelocity > 0f)
        {
            float currentAngularVelocity = rb.angularVelocity;
            if (Mathf.Abs(currentAngularVelocity) > maxAngularVelocity)
            {
                rb.angularVelocity = Mathf.Sign(currentAngularVelocity) * maxAngularVelocity;
            }
        }
        
        // カメラの境界処理
        if (enableCameraBoundary)
        {
            HandleCameraBoundary();
        }
    }
    
    /// <summary>
    /// カメラの境界でオブジェクトを跳ね返す
    /// </summary>
    private void HandleCameraBoundary()
    {
        if (mainCamera == null || rb == null)
        {
            return;
        }
        
        // カメラの視野範囲を計算
        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float cameraWidth = orthographicSize * aspect * 2f;
        float cameraHeight = orthographicSize * 2f;
        
        Vector3 cameraPos = mainCamera.transform.position;
        float leftBound = cameraPos.x - cameraWidth * 0.5f;
        float rightBound = cameraPos.x + cameraWidth * 0.5f;
        float bottomBound = cameraPos.y - cameraHeight * 0.5f;
        float topBound = cameraPos.y + cameraHeight * 0.5f;
        
        Vector2 currentVelocity = rb.linearVelocity;
        Vector3 currentPos = transform.position;
        bool velocityChanged = false;
        
        // X軸の境界チェック
        if (currentPos.x <= leftBound && currentVelocity.x < 0f)
        {
            // 左端に当たった場合、X軸の速度を反転
            currentVelocity.x = -currentVelocity.x;
            velocityChanged = true;
            // 位置を境界内にクランプ
            currentPos.x = leftBound;
            transform.position = currentPos;
        }
        else if (currentPos.x >= rightBound && currentVelocity.x > 0f)
        {
            // 右端に当たった場合、X軸の速度を反転
            currentVelocity.x = -currentVelocity.x;
            velocityChanged = true;
            // 位置を境界内にクランプ
            currentPos.x = rightBound;
            transform.position = currentPos;
        }
        
        // Y軸の境界チェック
        if (currentPos.y <= bottomBound && currentVelocity.y < 0f)
        {
            // 下端に当たった場合、Y軸の速度を反転
            currentVelocity.y = -currentVelocity.y;
            velocityChanged = true;
            // 位置を境界内にクランプ
            currentPos.y = bottomBound;
            transform.position = currentPos;
        }
        else if (currentPos.y >= topBound && currentVelocity.y > 0f)
        {
            // 上端に当たった場合、Y軸の速度を反転
            currentVelocity.y = -currentVelocity.y;
            velocityChanged = true;
            // 位置を境界内にクランプ
            currentPos.y = topBound;
            transform.position = currentPos;
        }
        
        // 速度を更新
        if (velocityChanged)
        {
            rb.linearVelocity = currentVelocity;
        }
    }
}

