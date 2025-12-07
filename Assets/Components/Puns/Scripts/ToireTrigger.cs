using UnityEngine;
using System.Collections;

/// <summary>
/// トイレのトリガーを管理するスクリプト
/// 何かを何かに与えたときにダジャレを成立させる
/// </summary>
public class ToireTrigger : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("与えるオブジェクト")]
    [SerializeField] private GameObject humanObject;
    
    [Tooltip("受け取るオブジェクト")]
    [SerializeField] private GameObject toiletObject;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("DragAndDropManagerへの参照（未設定の場合は自動検索）")]
    [SerializeField] private DragAndDropManager dragAndDropManager;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Toire";
    
    [Header("Reaction Settings")]
    [Tooltip("受け取った後のスプライト（受け取る前のスプライトは自動で保存されます）")]
    [SerializeField] private Sprite receivedSprite;
    
    [Tooltip("受け取るオブジェクトの色を変更する色（インスペクタで指定）")]
    [SerializeField] private Color reactionColor = Color.green;
    
    [Tooltip("受け取るオブジェクトの色を変更する時間（秒）")]
    [SerializeField] private float colorChangeDuration = 0.1f;
    
    [Tooltip("与えた時の受け取るオブジェクトのScale倍率")]
    [SerializeField] private float scaleMultiplier = 1.1f;
    
    [Tooltip("受け取るオブジェクトのScaleを大きくする時間（秒）")]
    [SerializeField] private float scaleDuration = 0.3f;
    
    [Tooltip("ホバー時のスケールアップ倍率")]
    [SerializeField] private float hoverScaleMultiplier = 1.05f;
    
    [Tooltip("ホバー時のスケールアップ/ダウンの時間（秒）")]
    [SerializeField] private float hoverScaleDuration = 0.2f;
    
    [Header("Give Settings")]
    [Tooltip("与えた後に与えたオブジェクトをDestroyするかどうか")]
    [SerializeField] private bool shouldDestroyGivenObject = true;
    
    [Header("Fade Out Settings")]
    [Tooltip("ダジャレ発生後のインターバル（秒）")]
    [SerializeField] private float destroyInterval = 1f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    [Tooltip("オブジェクトをDestroyするかどうか")]
    [SerializeField] private bool shouldDestroy = true;
    
    private SpriteRenderer[] receiveSpriteRenderers; // 受け取るオブジェクトとその子オブジェクトのSpriteRenderer
    private Vector3 receiveOriginalScale; // 受け取るオブジェクトの元のScale
    private Color[] receiveOriginalColors; // 受け取るオブジェクトの元の色
    private Sprite[] receiveOriginalSprites; // 受け取るオブジェクトの元のスプライト
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか
    private bool isHovering = false; // 与えるオブジェクトがホバー中かどうか
    private Coroutine hoverScaleCoroutine; // ホバー時のスケールアニメーション用のコルーチン
    private Coroutine reactionCoroutine; // リアクション用のコルーチン
    
    private void Awake()
    {
        // 受け取るオブジェクトのSpriteRendererを取得
        if (toiletObject != null)
        {
            receiveSpriteRenderers = toiletObject.GetComponentsInChildren<SpriteRenderer>();
            receiveOriginalScale = toiletObject.transform.localScale;
            
            // 元の色とスプライトを保存
            if (receiveSpriteRenderers != null && receiveSpriteRenderers.Length > 0)
            {
                receiveOriginalColors = new Color[receiveSpriteRenderers.Length];
                receiveOriginalSprites = new Sprite[receiveSpriteRenderers.Length];
                for (int i = 0; i < receiveSpriteRenderers.Length; i++)
                {
                    if (receiveSpriteRenderers[i] != null)
                    {
                        receiveOriginalColors[i] = receiveSpriteRenderers[i].color;
                        receiveOriginalSprites[i] = receiveSpriteRenderers[i].sprite;
                    }
                }
            }
            
            // 受け取るオブジェクトのCollider2Dをチェック
            Collider2D receiveCollider = toiletObject.GetComponent<Collider2D>();
            if (receiveCollider != null)
            {
                // 受け取るオブジェクトにヘルパースクリプトを追加（既にある場合は追加しない）
                ReceiveTriggerHelper helper = toiletObject.GetComponent<ReceiveTriggerHelper>();
                if (helper == null)
                {
                    helper = toiletObject.AddComponent<ReceiveTriggerHelper>();
                }
                helper.SetToireTrigger(this);
            }
        }
        
        // DragAndDropManagerを検索
        if (dragAndDropManager == null)
        {
            dragAndDropManager = FindFirstObjectByType<DragAndDropManager>();
        }
    }
    
    /// <summary>
    /// 与えるオブジェクトが受け取るオブジェクトのトリガーに入ったとき（ReceiveTriggerHelperから呼び出される）
    /// </summary>
    public void OnGiveObjectEnter(Collider2D other)
    {
        // 既にトリガー済みの場合は何もしない
        if (hasTriggered)
        {
            return;
        }
        
        // トリガーに入ったオブジェクトが与えるオブジェクトかチェック
        GameObject enteredObject = other.gameObject;
        bool isHumanObject = (humanObject != null && enteredObject == humanObject);
        
        if (isHumanObject)
        {
            // 与えるオブジェクトがドラッグ中かどうかをチェック
            bool isDragging = GiveGimmickHelper.IsDragging(humanObject, dragAndDropManager);
            
            if (isDragging)
            {
                return;
            }
            
            hasTriggered = true;
            TriggerPun();
        }
    }
    
    /// <summary>
    /// 与えるオブジェクトが受け取るオブジェクトのトリガー内にいる間（ReceiveTriggerHelperから呼び出される）
    /// </summary>
    public void OnGiveObjectStay(Collider2D other)
    {
        // 既にトリガー済みの場合は何もしない
        if (hasTriggered)
        {
            return;
        }
        
        // トリガー内にいるオブジェクトが与えるオブジェクトかチェック
        GameObject stayingObject = other.gameObject;
        bool isHumanObject = (humanObject != null && stayingObject == humanObject);
        
        if (isHumanObject)
        {
            // 与えるオブジェクトがドラッグ中かどうかをチェック
            bool isDragging = GiveGimmickHelper.IsDragging(humanObject, dragAndDropManager);
            
            if (isDragging)
            {
                // ホバー中でない場合、ホバー開始
                if (!isHovering)
                {
                    isHovering = true;
                    GiveGimmickHelper.StartHoverScaleUp(
                        this,
                        toiletObject,
                        receiveOriginalScale,
                        hoverScaleMultiplier,
                        hoverScaleDuration,
                        ref hoverScaleCoroutine);
                }
            }
            else
            {
                // ドラッグ中でない場合（ホバー中にドロップした場合）、与えた判定にする
                if (isHovering)
                {
                    isHovering = false;
                    hasTriggered = true;
                    TriggerPun();
                }
            }
        }
    }
    
    /// <summary>
    /// 与えるオブジェクトが受け取るオブジェクトのトリガーから出たとき（ReceiveTriggerHelperから呼び出される）
    /// </summary>
    public void OnGiveObjectExit(Collider2D other)
    {
        // トリガーから出たオブジェクトが与えるオブジェクトかチェック
        GameObject exitedObject = other.gameObject;
        bool isHumanObject = (humanObject != null && exitedObject == humanObject);
        
        if (isHumanObject)
        {
            // ホバー中の場合、ホバー終了
            if (isHovering)
            {
                isHovering = false;
                GiveGimmickHelper.StartHoverScaleDown(
                    this,
                    toiletObject,
                    receiveOriginalScale,
                    hoverScaleDuration,
                    ref hoverScaleCoroutine);
            }
        }
    }
    
    /// <summary>
    /// ダジャレを成立させる
    /// </summary>
    private void TriggerPun()
    {
        if (punDisplayGenerator == null)
        {
            Debug.LogWarning($"ToireTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
            return;
        }
        
        // ホバー中のスケールアニメーションを停止
        if (isHovering)
        {
            isHovering = false;
            if (hoverScaleCoroutine != null)
            {
                StopCoroutine(hoverScaleCoroutine);
                hoverScaleCoroutine = null;
            }
        }
        
        // 与えるオブジェクトを消す
        if (shouldDestroyGivenObject && humanObject != null)
        {
            Destroy(humanObject);
            humanObject = null;
        }
        else if (!shouldDestroyGivenObject && humanObject != null)
        {
            // 破壊しない場合はDragAndDropManagerを無効化
            GiveGimmickHelper.DisableDragAndDropManager(humanObject);
        }
        
        // PunDisplayGeneratorにダジャレ成立を通知
        punDisplayGenerator.GeneratePun(punId, gameObject);
        
        // 受け取った後のスプライトに変更
        if (receivedSprite != null && receiveSpriteRenderers != null)
        {
            for (int i = 0; i < receiveSpriteRenderers.Length; i++)
            {
                if (receiveSpriteRenderers[i] != null)
                {
                    receiveSpriteRenderers[i].sprite = receivedSprite;
                }
            }
        }
        
        // 受け取るオブジェクトのリアクション（色変更とScale）
        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
        }
        reactionCoroutine = GiveGimmickHelper.StartReaction(
            this,
            toiletObject,
            receiveSpriteRenderers,
            receiveOriginalColors,
            receiveOriginalScale,
            reactionColor,
            colorChangeDuration,
            scaleMultiplier,
            scaleDuration);
        
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

