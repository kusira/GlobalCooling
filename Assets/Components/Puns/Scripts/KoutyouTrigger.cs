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
    private bool hasTriggered = false; // 既にダジャレが発生したか
    private bool isFadingOut = false; // フェードアウト中かどうか
    private bool isHovering = false; // Beerがホバー中かどうか
    private Coroutine hoverScaleCoroutine; // ホバー時のスケールアニメーション用のコルーチン
    private Coroutine reactionCoroutine; // リアクション用のコルーチン
    
    private void Awake()
    {
        Debug.Log($"KoutyouTrigger: Awake() 開始 - GameObject: {gameObject.name}");
        
        // PrincipalのSpriteRendererを取得
        if (principalObject != null)
        {
            principalSpriteRenderers = principalObject.GetComponentsInChildren<SpriteRenderer>();
            principalOriginalScale = principalObject.transform.localScale;
            
            Debug.Log($"KoutyouTrigger: PrincipalのSpriteRenderer数: {(principalSpriteRenderers != null ? principalSpriteRenderers.Length : 0)}");
            
            // 元の色を保存
            if (principalSpriteRenderers != null && principalSpriteRenderers.Length > 0)
            {
                principalOriginalColors = new Color[principalSpriteRenderers.Length];
                for (int i = 0; i < principalSpriteRenderers.Length; i++)
                {
                    if (principalSpriteRenderers[i] != null)
                    {
                        principalOriginalColors[i] = principalSpriteRenderers[i].color;
                    }
                }
            }
            
            // PrincipalのCollider2Dをチェック
            Collider2D principalCollider = principalObject.GetComponent<Collider2D>();
            if (principalCollider != null)
            {
                Debug.Log($"KoutyouTrigger: PrincipalのCollider2D - isTrigger: {principalCollider.isTrigger}, Type: {principalCollider.GetType().Name}");
                
                // Principalにヘルパースクリプトを追加（既にある場合は追加しない）
                PrincipalTriggerHelper helper = principalObject.GetComponent<PrincipalTriggerHelper>();
                if (helper == null)
                {
                    helper = principalObject.AddComponent<PrincipalTriggerHelper>();
                    Debug.Log("KoutyouTrigger: PrincipalTriggerHelperをPrincipalに追加しました。");
                }
                helper.SetKoutyouTrigger(this);
                Debug.Log("KoutyouTrigger: PrincipalTriggerHelperに参照を設定しました。");
            }
            else
            {
                Debug.LogWarning("KoutyouTrigger: PrincipalにCollider2Dが見つかりません。");
            }
        }
        else
        {
            Debug.LogError("KoutyouTrigger: Principalオブジェクトが設定されていません。");
        }
        
        // BeerのCollider2DとRigidbody2Dをチェック
        if (beerObject != null)
        {
            Collider2D beerCollider = beerObject.GetComponent<Collider2D>();
            Rigidbody2D beerRigidbody = beerObject.GetComponent<Rigidbody2D>();
            
            if (beerCollider != null)
            {
                Debug.Log($"KoutyouTrigger: BeerのCollider2D - isTrigger: {beerCollider.isTrigger}, Type: {beerCollider.GetType().Name}");
            }
            else
            {
                Debug.LogWarning("KoutyouTrigger: BeerにCollider2Dが見つかりません。");
            }
            
            if (beerRigidbody != null)
            {
                Debug.Log($"KoutyouTrigger: BeerのRigidbody2D - bodyType: {beerRigidbody.bodyType}");
            }
            else
            {
                Debug.LogWarning("KoutyouTrigger: BeerにRigidbody2Dが見つかりません。");
            }
        }
        else
        {
            Debug.LogError("KoutyouTrigger: Beerオブジェクトが設定されていません。");
        }
        
        // このスクリプトはKoutyou（空のGameObject）にアタッチされている想定
        // Collider2Dは不要（PrincipalのCollider2DのトリガーイベントをPrincipalTriggerHelper経由で受け取る）
        Debug.Log($"KoutyouTrigger: このスクリプトは{gameObject.name}にアタッチされています。PrincipalのトリガーイベントはPrincipalTriggerHelper経由で受け取ります。");
        
        // DragAndDropManagerを検索
        if (dragAndDropManager == null)
        {
            dragAndDropManager = FindFirstObjectByType<DragAndDropManager>();
            if (dragAndDropManager == null)
            {
                Debug.LogWarning("KoutyouTrigger: DragAndDropManagerが見つかりません。");
            }
            else
            {
                Debug.Log("KoutyouTrigger: DragAndDropManagerを検索して見つかりました。");
            }
        }
        else
        {
            Debug.Log("KoutyouTrigger: DragAndDropManagerが設定されています。");
        }
        
        Debug.Log($"KoutyouTrigger: 初期化完了 - Beer: {(beerObject != null ? beerObject.name : "null")}, Principal: {(principalObject != null ? principalObject.name : "null")}");
    }
    
    private void Start()
    {
        Debug.Log($"KoutyouTrigger: Start() - GameObject: {gameObject.name}");
        
        // このスクリプトはKoutyou（空のGameObject）にアタッチされている想定
        if (principalObject != null && gameObject != principalObject)
        {
            Debug.Log($"KoutyouTrigger: このスクリプトは{gameObject.name}にアタッチされています。Principal({principalObject.name})のトリガーイベントはPrincipalTriggerHelper経由で受け取ります。");
        }
        else if (principalObject != null)
        {
            Debug.LogWarning($"KoutyouTrigger: このスクリプトは{gameObject.name}にアタッチされています。Principal({principalObject.name})とは別のGameObjectにアタッチしてください。");
        }
    }
    
    /// <summary>
    /// BeerがPrincipalのトリガーに入ったとき（PrincipalTriggerHelperから呼び出される）
    /// </summary>
    public void OnBeerEnter(Collider2D other)
    {
        Debug.Log($"KoutyouTrigger: OnTriggerEnter2D 呼び出されました！");
        Debug.Log($"KoutyouTrigger: - this GameObject: {gameObject.name}");
        Debug.Log($"KoutyouTrigger: - other GameObject: {other.gameObject.name}");
        Debug.Log($"KoutyouTrigger: - other Collider2D.isTrigger: {other.isTrigger}");
        
        // 既にトリガー済みの場合は何もしない
        if (hasTriggered)
        {
            Debug.Log("KoutyouTrigger: 既にトリガー済みのためスキップ");
            return;
        }
        
        // トリガーに入ったオブジェクトがBeerかチェック
        GameObject enteredObject = other.gameObject;
        bool isBeer = (beerObject != null && enteredObject == beerObject);
        
        Debug.Log($"KoutyouTrigger: isBeer = {isBeer}");
        Debug.Log($"KoutyouTrigger: - beerObject: {(beerObject != null ? beerObject.name : "null")}");
        Debug.Log($"KoutyouTrigger: - enteredObject: {enteredObject.name}");
        
        if (isBeer)
        {
            // Beerがドラッグ中かどうかをチェック
            bool isDragging = GiveGimmickHelper.IsDragging(beerObject, dragAndDropManager);
            Debug.Log($"KoutyouTrigger: Beerがドラッグ中: {isDragging}");
            
            if (isDragging)
            {
                Debug.Log("KoutyouTrigger: Beerがドラッグ中のためスキップ");
                return;
            }
            
            Debug.Log("KoutyouTrigger: BeerがPrincipalのトリガーに入りました。ダジャレを成立させます。");
            hasTriggered = true;
            TriggerPun();
        }
        else
        {
            Debug.Log($"KoutyouTrigger: トリガーに入ったオブジェクト({enteredObject.name})はBeerではありません。");
        }
    }
    
    /// <summary>
    /// BeerがPrincipalのトリガー内にいる間（PrincipalTriggerHelperから呼び出される）
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
                    Debug.Log("KoutyouTrigger: BeerがPrincipalにホバーしました。スケールアップします。");
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
                    Debug.Log("KoutyouTrigger: ホバー中にBeerがドロップされました。Beerを与えた判定にします。");
                    isHovering = false;
                    hasTriggered = true;
                    TriggerPun();
                }
            }
        }
    }
    
    /// <summary>
    /// BeerがPrincipalのトリガーから出たとき（PrincipalTriggerHelperから呼び出される）
    /// </summary>
    public void OnBeerExit(Collider2D other)
    {
        Debug.Log($"KoutyouTrigger: OnTriggerExit2D - other: {other.gameObject.name}");
        
        // トリガーから出たオブジェクトがBeerかチェック
        GameObject exitedObject = other.gameObject;
        bool isBeer = (beerObject != null && exitedObject == beerObject);
        
        if (isBeer)
        {
            // ホバー中の場合、ホバー終了
            if (isHovering)
            {
                Debug.Log("KoutyouTrigger: BeerがPrincipalのトリガーから出ました。スケールを元に戻します。");
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
        Debug.Log($"KoutyouTrigger: TriggerPun() が呼ばれました。punId: {punId}");
        
        if (punDisplayGenerator == null)
        {
            Debug.LogWarning($"KoutyouTrigger: PunDisplayGeneratorが設定されていません。GameObject: {gameObject.name}");
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
            Debug.Log($"KoutyouTrigger: Beer({beerObject.name})を消します。");
            Destroy(beerObject);
            beerObject = null;
        }
        
        // PunDisplayGeneratorにダジャレ成立を通知
        Debug.Log($"KoutyouTrigger: PunDisplayGenerator.GeneratePun({punId}) を呼び出します。");
        punDisplayGenerator.GeneratePun(punId, gameObject);
        
        // Principalのリアクション（色変更とScale）
        Debug.Log("KoutyouTrigger: Principalのリアクションを開始します。");
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
        Debug.Log("KoutyouTrigger: フェードアウト処理を開始します。");
        PunTriggerHelper.StartDestroyAfterFadeOut(
            this,
            gameObject,
            destroyInterval,
            fadeOutDuration,
            shouldDestroy,
            ref isFadingOut);
    }
    
}
