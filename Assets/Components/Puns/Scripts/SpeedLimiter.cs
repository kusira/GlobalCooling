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
    
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"SpeedLimiter: Rigidbody2Dが見つかりません。GameObject: {gameObject.name}");
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
    }
}

