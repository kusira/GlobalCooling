using UnityEngine;
using DG.Tweening;

/// <summary>
/// かばんが浮かばんのトリガーを管理するスクリプト
/// カバンを水に沈めることで達成
/// </summary>
public class KabanTrigger : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("Bag（カバン）オブジェクト")]
    [SerializeField] private GameObject bagObject;
    
    [Tooltip("Weight（重り）オブジェクト")]
    [SerializeField] private GameObject weightObject;
    
    [Tooltip("Water（水）オブジェクト")]
    [SerializeField] private GameObject waterObject;
    
    [Tooltip("JudgementFloor（判定床）オブジェクト")]
    [SerializeField] private GameObject judgementFloorObject;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Kaban";
    
    [Header("Water Settings")]
    [Tooltip("浮力（上方向の力）")]
    [SerializeField] private float buoyancyForce = 5f;
    
    [Header("Trigger Settings")]
    [Tooltip("JudgementFloorに触れてからダジャレ成立までの待機時間（秒）")]
    [SerializeField] private float triggerWaitTime = 1.5f;
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    [Tooltip("オブジェクトをDestroyするかどうか")]
    [SerializeField] private bool shouldDestroy = true;
    
    private bool isInWater = false; // Water内にいるかどうか
    private bool isTouchingJudgementFloor = false; // JudgementFloorに触れているかどうか
    private float judgementFloorTimer = 0f; // JudgementFloorに触れている時間
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか
    private Rigidbody2D bagRigidbody; // BagのRigidbody2D
    private Collider2D bagCollider; // BagのCollider2D
    private Collider2D waterCollider; // WaterのCollider2D
    private Collider2D judgementFloorCollider; // JudgementFloorのCollider2D
    
    private void Awake()
    {
        // BagのRigidbody2DとCollider2Dを取得
        if (bagObject != null)
        {
            bagRigidbody = bagObject.GetComponent<Rigidbody2D>();
            bagCollider = bagObject.GetComponent<Collider2D>();
        }
        
        // WaterのCollider2Dを取得
        if (waterObject != null)
        {
            waterCollider = waterObject.GetComponent<Collider2D>();
            
            // Waterにヘルパースクリプトを追加（既にある場合は追加しない）
            WaterTriggerHelper waterHelper = waterObject.GetComponent<WaterTriggerHelper>();
            if (waterHelper == null)
            {
                waterHelper = waterObject.AddComponent<WaterTriggerHelper>();
            }
            waterHelper.SetKabanTrigger(this);
        }
        
        // JudgementFloorのCollider2Dを取得
        if (judgementFloorObject != null)
        {
            judgementFloorCollider = judgementFloorObject.GetComponent<Collider2D>();
            
            // JudgementFloorにヘルパースクリプトを追加（既にある場合は追加しない）
            JudgementFloorTriggerHelper floorHelper = judgementFloorObject.GetComponent<JudgementFloorTriggerHelper>();
            if (floorHelper == null)
            {
                floorHelper = judgementFloorObject.AddComponent<JudgementFloorTriggerHelper>();
            }
            floorHelper.SetKabanTrigger(this);
        }
    }
    
    private void FixedUpdate()
    {
        // Water内にいる場合は浮力を加える
        if (isInWater && bagRigidbody != null)
        {
            bagRigidbody.AddForce(Vector2.up * buoyancyForce, ForceMode2D.Force);
        }
    }
    
    private void Update()
    {
        // JudgementFloorに触れていて、まだトリガーしていない場合
        if (isTouchingJudgementFloor && !hasTriggered)
        {
            judgementFloorTimer += Time.deltaTime;
            
            // 待機時間を超えたらダジャレを発生
            if (judgementFloorTimer >= triggerWaitTime)
            {
                TriggerPun();
            }
        }
        else
        {
            // 条件を満たしていない場合はタイマーをリセット
            if (judgementFloorTimer > 0f)
            {
                judgementFloorTimer = 0f;
            }
        }
        
        // BagとWeightの衝突をチェック
        CheckBagWeightCollision();
    }
    
    /// <summary>
    /// BagとWeightの衝突をチェック
    /// </summary>
    private void CheckBagWeightCollision()
    {
        if (bagCollider == null || weightObject == null)
        {
            return;
        }
        
        Collider2D weightCollider = weightObject.GetComponent<Collider2D>();
        if (weightCollider == null)
        {
            return;
        }
        
        // BagとWeightが重なっているかチェック（Boundsを使用）
        bool isOverlapping = bagCollider.bounds.Intersects(weightCollider.bounds);
        
        if (isOverlapping)
        {
            // 重りをカバンに衝突させた処理
            AbsorbWeight();
        }
    }
    
    /// <summary>
    /// 重りをカバンに吸収する（重りを削除し、BagのMassを増やす）
    /// </summary>
    private void AbsorbWeight()
    {
        if (weightObject == null || bagRigidbody == null)
        {
            return;
        }
        
        // 重りのRigidbody2DからMassを取得
        Rigidbody2D weightRigidbody = weightObject.GetComponent<Rigidbody2D>();
        float weightMass = 1f; // デフォルト値
        
        if (weightRigidbody != null)
        {
            weightMass = weightRigidbody.mass;
        }
        
        // BagのMassを増やす
        bagRigidbody.mass += weightMass;
        
        // 重りを削除
        Destroy(weightObject);
        weightObject = null;
    }
    
    /// <summary>
    /// BagがWaterのトリガーに入ったとき（WaterTriggerHelperから呼び出される）
    /// </summary>
    public void OnBagEnterWater(Collider2D other)
    {
        if (other.gameObject == bagObject)
        {
            isInWater = true;
        }
    }
    
    /// <summary>
    /// BagがWaterのトリガーから出たとき（WaterTriggerHelperから呼び出される）
    /// </summary>
    public void OnBagExitWater(Collider2D other)
    {
        if (other.gameObject == bagObject)
        {
            isInWater = false;
        }
    }
    
    /// <summary>
    /// BagがJudgementFloorのトリガーに入ったとき（JudgementFloorTriggerHelperから呼び出される）
    /// </summary>
    public void OnBagEnterJudgementFloor(Collider2D other)
    {
        if (other.gameObject == bagObject)
        {
            isTouchingJudgementFloor = true;
            judgementFloorTimer = 0f;
        }
    }
    
    /// <summary>
    /// BagがJudgementFloorのトリガー内にいる間（JudgementFloorTriggerHelperから呼び出される）
    /// </summary>
    public void OnBagStayJudgementFloor(Collider2D other)
    {
        if (other.gameObject == bagObject)
        {
            isTouchingJudgementFloor = true;
        }
    }
    
    /// <summary>
    /// BagがJudgementFloorのトリガーから出たとき（JudgementFloorTriggerHelperから呼び出される）
    /// </summary>
    public void OnBagExitJudgementFloor(Collider2D other)
    {
        if (other.gameObject == bagObject)
        {
            isTouchingJudgementFloor = false;
            judgementFloorTimer = 0f;
        }
    }
    
    /// <summary>
    /// ダジャレを成立させる
    /// </summary>
    private void TriggerPun()
    {
        if (hasTriggered)
        {
            return;
        }
        
        hasTriggered = true;
        
        if (punDisplayGenerator == null)
        {
            Debug.LogWarning($"KabanTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
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

