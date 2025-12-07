using UnityEngine;
using System.Collections;

/// <summary>
/// 校長絶好調のトリガーを管理するスクリプト
/// PrincipalとBeerが衝突したときにダジャレを成立させる
/// </summary>
public class KoutyouTrigger : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("Beerオブジェクト")]
    [SerializeField] private GameObject beerObject;
    
    [Tooltip("Principalオブジェクト")]
    [SerializeField] private GameObject principalObject;
    
    [Header("References")]
    [Tooltip("PunDisplayGeneratorへの参照")]
    [SerializeField] private PunDisplayGenerator punDisplayGenerator;
    
    [Tooltip("DragAndDropManagerへの参照（未設定の場合は自動検索）")]
    [SerializeField] private DragAndDropManager dragAndDropManager;
    
    [Tooltip("ダジャレのID")]
    [SerializeField] private string punId = "Koutyou";
    
    [Header("Reaction Settings")]
    [Tooltip("受け取った後のスプライト（受け取る前のスプライトは自動で保存されます）")]
    [SerializeField] private Sprite receivedSprite;
    
    [Tooltip("Principalの色を変更する色（インスペクタで指定）")]
    [SerializeField] private Color reactionColor = Color.green;
    
    [Tooltip("Principalの色を変更する時間（秒）")]
    [SerializeField] private float colorChangeDuration = 0.1f;
    
    [Tooltip("Beerを与えた時のPrincipalのScale倍率")]
    [SerializeField] private float scaleMultiplier = 1.1f;
    
    [Tooltip("PrincipalのScaleを大きくする時間（秒）")]
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
    
    private SpriteRenderer[] principalSpriteRenderers; // Principalとその子オブジェクトのSpriteRenderer
    private Vector3 principalOriginalScale; // Principalの元のScale
    private Color[] principalOriginalColors; // Principalの元の色
    private Sprite[] principalOriginalSprites; // Principalの元のスプライト
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか
    private bool isHovering = false; // Beerがホバー中かどうか
    private Coroutine hoverScaleCoroutine; // ホバー時のスケールアニメーション用のコルーチン
    private Coroutine reactionCoroutine; // リアクション用のコルーチン
    
    private void Awake()
    {
        // PrincipalのSpriteRendererを取得
        if (principalObject != null)
        {
            principalSpriteRenderers = principalObject.GetComponentsInChildren<SpriteRenderer>();
            principalOriginalScale = principalObject.transform.localScale;
            
            // 元の色とスプライトを保存
            if (principalSpriteRenderers != null && principalSpriteRenderers.Length > 0)
            {
                principalOriginalColors = new Color[principalSpriteRenderers.Length];
                principalOriginalSprites = new Sprite[principalSpriteRenderers.Length];
                for (int i = 0; i < principalSpriteRenderers.Length; i++)
                {
                    if (principalSpriteRenderers[i] != null)
                    {
                        principalOriginalColors[i] = principalSpriteRenderers[i].color;
                        principalOriginalSprites[i] = principalSpriteRenderers[i].sprite;
                    }
                }
            }
            
            // PrincipalのCollider2Dをチェック
            Collider2D principalCollider = principalObject.GetComponent<Collider2D>();
            if (principalCollider != null)
            {
                // Principalにヘルパースクリプトを追加（既にある場合は追加しない）
                KoutyouTriggerHelper helper = principalObject.GetComponent<KoutyouTriggerHelper>();
                if (helper == null)
                {
                    helper = principalObject.AddComponent<KoutyouTriggerHelper>();
                }
                helper.SetKoutyouTrigger(this);
            }
        }
        
        // DragAndDropManagerを検索
        if (dragAndDropManager == null)
        {
            dragAndDropManager = FindFirstObjectByType<DragAndDropManager>();
        }
    }
    
    
    /// <summary>
    /// BeerがPrincipalのトリガーに入ったとき（KoutyouTriggerHelperから呼び出される）
    /// </summary>
    public void OnBeerEnter(Collider2D other)
    {
        // 既にトリガー済みの場合は何もしない
        if (hasTriggered)
        {
            return;
        }
        
        // トリガーに入ったオブジェクトがBeerかチェック
        GameObject enteredObject = other.gameObject;
        bool isBeer = (beerObject != null && enteredObject == beerObject);
        
        if (isBeer)
        {
            // Beerがドラッグ中かどうかをチェック
            bool isDragging = GiveGimmickHelper.IsDragging(beerObject, dragAndDropManager);
            
            if (isDragging)
            {
                return;
            }
            
            hasTriggered = true;
            TriggerPun();
        }
    }
    
    /// <summary>
    /// BeerがPrincipalのトリガー内にいる間（KoutyouTriggerHelperから呼び出される）
    /// </summary>
    public void OnBeerStay(Collider2D other)
    {
        // 既にトリガー済みの場合は何もしない
        if (hasTriggered)
        {
            return;
        }
        
        // トリガー内にいるオブジェクトがBeerかチェック
        GameObject stayingObject = other.gameObject;
        bool isBeer = (beerObject != null && stayingObject == beerObject);
        
        if (isBeer)
        {
            // Beerがドラッグ中かどうかをチェック
            bool isDragging = GiveGimmickHelper.IsDragging(beerObject, dragAndDropManager);
            
            if (isDragging)
            {
                // ホバー中でない場合、ホバー開始
                if (!isHovering)
                {
                    isHovering = true;
                    GiveGimmickHelper.StartHoverScaleUp(
                        this,
                        principalObject,
                        principalOriginalScale,
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
    /// BeerがPrincipalのトリガーから出たとき（KoutyouTriggerHelperから呼び出される）
    /// </summary>
    public void OnBeerExit(Collider2D other)
    {
        // トリガーから出たオブジェクトがBeerかチェック
        GameObject exitedObject = other.gameObject;
        bool isBeer = (beerObject != null && exitedObject == beerObject);
        
        if (isBeer)
        {
            // ホバー中の場合、ホバー終了
            if (isHovering)
            {
                isHovering = false;
                GiveGimmickHelper.StartHoverScaleDown(
                    this,
                    principalObject,
                    principalOriginalScale,
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
        
        // Beerを消す
        if (shouldDestroyGivenObject && beerObject != null)
        {
            Destroy(beerObject);
            beerObject = null;
        }
        else if (!shouldDestroyGivenObject && beerObject != null)
        {
            // 破壊しない場合はDragAndDropManagerを無効化
            GiveGimmickHelper.DisableDragAndDropManager(beerObject);
        }
        
        // PunDisplayGeneratorにダジャレ成立を通知
        punDisplayGenerator.GeneratePun(punId, gameObject);
        
        // 受け取った後のスプライトに変更
        if (receivedSprite != null && principalSpriteRenderers != null)
        {
            for (int i = 0; i < principalSpriteRenderers.Length; i++)
            {
                if (principalSpriteRenderers[i] != null)
                {
                    principalSpriteRenderers[i].sprite = receivedSprite;
                }
            }
        }
        
        // Principalのリアクション（色変更とScale）
        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
        }
        reactionCoroutine = GiveGimmickHelper.StartReaction(
            this,
            principalObject,
            principalSpriteRenderers,
            principalOriginalColors,
            principalOriginalScale,
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
