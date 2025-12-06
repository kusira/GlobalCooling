using UnityEngine;
using System.Collections;

/// <summary>
/// 三時に大惨事のトリガーを管理するスクリプト
/// 時計の針（Needle）を3時（-90度～-120度）に回転させて一定時間待つとダジャレを成立させる
/// </summary>
public class SanziTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ダジャレ成立までの待機時間（秒）")]
    [SerializeField] private float triggerWaitTime = 1.5f;
    
    [Tooltip("3時の角度範囲（最小値、度）")]
    [SerializeField] private float minAngle = -120f;
    
    [Tooltip("3時の角度範囲（最大値、度）")]
    [SerializeField] private float maxAngle = -90f;
    
    [Header("Object References")]
    [Tooltip("時計の針オブジェクト（Needle）")]
    [SerializeField] private GameObject needleObject;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Sanzi";
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    [Tooltip("オブジェクトをDestroyするかどうか")]
    [SerializeField] private bool shouldDestroy = false;
    
    private float timer = 0f; // タイマー
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか

    private void Awake()
    {
        // Needleオブジェクトが設定されていない場合、このオブジェクトを使用
        if (needleObject == null)
        {
            needleObject = gameObject;
        }
    }

    private void Update()
    {
        // 既にトリガー済みの場合は何もしない
        if (hasTriggered)
        {
            return;
        }
        
        // 角度が範囲内かチェック
        bool angleInRange = IsAngleInRange();
        
        if (angleInRange)
        {
            timer += Time.deltaTime;
            
            // 待機時間を超えたらダジャレを発生
            if (timer >= triggerWaitTime)
            {
                hasTriggered = true;
                TriggerPun();
            }
        }
        else
        {
            // 条件を満たしていない場合はタイマーをリセット
            if (timer > 0f)
            {
                timer = 0f;
            }
        }
    }

    /// <summary>
    /// 角度が3時の範囲内（-90度～-120度）かチェック
    /// </summary>
    private bool IsAngleInRange()
    {
        // Needleオブジェクトが破棄されていないかチェック
        if (needleObject == null || needleObject.transform == null)
        {
            return false;
        }
        
        // Z軸の回転角度を取得（-180度～180度の範囲に正規化）
        float currentAngle = NormalizeAngle(needleObject.transform.rotation.eulerAngles.z);
        
        // -90度～-120度の範囲内かチェック
        // minAngle = -120度、maxAngle = -90度なので、currentAngleがこの範囲内にあるか
        bool inRange = currentAngle >= minAngle && currentAngle <= maxAngle;
        
        return inRange;
    }
    
    /// <summary>
    /// 角度を-180度～180度の範囲に正規化
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        // 0～360度を-180～180度に変換
        while (angle > 180f)
        {
            angle -= 360f;
        }
        while (angle < -180f)
        {
            angle += 360f;
        }
        return angle;
    }
    
    /// <summary>
    /// ダジャレを成立させる
    /// </summary>
    private void TriggerPun()
    {
        if (punDisplayGenerator == null)
        {
            Debug.LogWarning($"SanziTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
            return;
        }
        
        Debug.Log($"SanziTrigger: 時計の針が3時の位置に{triggerWaitTime}秒間留まりました。ダジャレを成立させます。");
        
        // PunDisplayGeneratorにダジャレ成立を通知
        punDisplayGenerator.GeneratePun(punId, gameObject);
        
        // インターバル後にフェードアウトしてDestroy（共通処理を使用）
        // このオブジェクト（SanziTriggerがアタッチされているオブジェクト）のみをフェードアウト
        // Needleオブジェクトは破棄しない（HingeJoint2Dがあるため）
        PunTriggerHelper.StartDestroyAfterFadeOut(
            this,
            gameObject,
            destroyInterval,
            fadeOutDuration,
            shouldDestroy,
            ref isFadingOut);
    }
}

