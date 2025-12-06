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
            bool isDragging = false;
            if (dragAndDropManager != null)
            {
                isDragging = dragAndDropManager.IsDragging(beerObject);
                Debug.Log($"KoutyouTrigger: Beerがドラッグ中: {isDragging}");
            }
            
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
            bool isDragging = false;
            if (dragAndDropManager != null)
            {
                isDragging = dragAndDropManager.IsDragging(beerObject);
            }
            
            if (isDragging)
            {
                // ホバー中でない場合、ホバー開始
                if (!isHovering)
                {
                    Debug.Log("KoutyouTrigger: BeerがPrincipalにホバーしました。スケールアップします。");
                    isHovering = true;
                    StartHoverScaleUp();
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
                StartHoverScaleDown();
            }
        }
    }
    
    /// <summary>
    /// ホバー時のスケールアップを開始
    /// </summary>
    private void StartHoverScaleUp()
    {
        if (principalObject == null)
        {
            return;
        }
        
        // 既存のホバーアニメーションを停止
        if (hoverScaleCoroutine != null)
        {
            StopCoroutine(hoverScaleCoroutine);
        }
        
        hoverScaleCoroutine = StartCoroutine(HoverScaleAnimation(principalOriginalScale, principalOriginalScale * hoverScaleMultiplier));
    }
    
    /// <summary>
    /// ホバー時のスケールダウンを開始
    /// </summary>
    private void StartHoverScaleDown()
    {
        if (principalObject == null)
        {
            return;
        }
        
        // 既存のホバーアニメーションを停止
        if (hoverScaleCoroutine != null)
        {
            StopCoroutine(hoverScaleCoroutine);
        }
        
        hoverScaleCoroutine = StartCoroutine(HoverScaleAnimation(principalObject.transform.localScale, principalOriginalScale));
    }
    
    /// <summary>
    /// ホバー時のスケールアニメーション
    /// </summary>
    private IEnumerator HoverScaleAnimation(Vector3 fromScale, Vector3 toScale)
    {
        if (principalObject == null)
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < hoverScaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / hoverScaleDuration);
            principalObject.transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }
        
        // 最終的に目標のスケールに設定
        principalObject.transform.localScale = toScale;
        hoverScaleCoroutine = null;
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
        if (beerObject != null)
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
        StartCoroutine(PrincipalReaction());
        
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
    
    /// <summary>
    /// Principalのリアクション（色変更とScale）
    /// </summary>
    private IEnumerator PrincipalReaction()
    {
        if (principalObject == null || principalSpriteRenderers == null)
        {
            Debug.LogWarning("KoutyouTrigger: PrincipalまたはSpriteRendererが見つかりません。");
            yield break;
        }
        
        Debug.Log($"KoutyouTrigger: Principalの色とScaleを同時に変化させます。");
        
        // 色とScaleの目標値を設定
        Vector3 targetScale = principalOriginalScale * scaleMultiplier;
        Color[] targetColors = new Color[principalSpriteRenderers.Length];
        for (int i = 0; i < principalSpriteRenderers.Length; i++)
        {
            if (principalSpriteRenderers[i] != null)
            {
                Color originalColor = principalOriginalColors[i];
                targetColors[i] = new Color(reactionColor.r, reactionColor.g, reactionColor.b, originalColor.a);
            }
        }
        
        // 色のフェードインとScaleの拡大を同時に実行
        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(colorChangeDuration, scaleDuration);
        
        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // 色のフェードイン（常に更新してスムーズに変化させる）
            float colorT = 0f;
            if (colorChangeDuration > 0f)
            {
                colorT = Mathf.Clamp01(elapsedTime / colorChangeDuration);
            }
            else
            {
                colorT = 1f; // 時間が0の場合は即座に変更
            }
            
            for (int i = 0; i < principalSpriteRenderers.Length; i++)
            {
                if (principalSpriteRenderers[i] != null)
                {
                    Color originalColor = principalOriginalColors[i];
                    principalSpriteRenderers[i].color = Color.Lerp(originalColor, targetColors[i], colorT);
                }
            }
            
            // Scaleの拡大
            float scaleT = 0f;
            if (scaleDuration > 0f)
            {
                scaleT = Mathf.Clamp01(elapsedTime / scaleDuration);
            }
            else
            {
                scaleT = 1f; // 時間が0の場合は即座に変更
            }
            principalObject.transform.localScale = Vector3.Lerp(principalOriginalScale, targetScale, scaleT);
            
            yield return null;
        }
        
        // 最終的に目標値に設定（確実に目標値に到達させる）
        for (int i = 0; i < principalSpriteRenderers.Length; i++)
        {
            if (principalSpriteRenderers[i] != null)
            {
                principalSpriteRenderers[i].color = targetColors[i];
            }
        }
        principalObject.transform.localScale = targetScale;
        
        Debug.Log("KoutyouTrigger: Principalの色とScaleを元に戻します。");
        
        // 色のフェードアウトとScaleの縮小を同時に実行
        elapsedTime = 0f;
        maxDuration = Mathf.Max(colorChangeDuration, scaleDuration);
        
        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // 色のフェードアウト（常に更新してスムーズに変化させる）
            float colorT = 0f;
            if (colorChangeDuration > 0f)
            {
                colorT = Mathf.Clamp01(elapsedTime / colorChangeDuration);
            }
            else
            {
                colorT = 1f; // 時間が0の場合は即座に変更
            }
            
            for (int i = 0; i < principalSpriteRenderers.Length; i++)
            {
                if (principalSpriteRenderers[i] != null)
                {
                    principalSpriteRenderers[i].color = Color.Lerp(targetColors[i], principalOriginalColors[i], colorT);
                }
            }
            
            // Scaleの縮小
            float scaleT = 0f;
            if (scaleDuration > 0f)
            {
                scaleT = Mathf.Clamp01(elapsedTime / scaleDuration);
            }
            else
            {
                scaleT = 1f; // 時間が0の場合は即座に変更
            }
            principalObject.transform.localScale = Vector3.Lerp(targetScale, principalOriginalScale, scaleT);
            
            yield return null;
        }
        
        // 最終的に元のScaleに設定
        principalObject.transform.localScale = principalOriginalScale;
        
        // 最終的に元の色に設定
        for (int i = 0; i < principalSpriteRenderers.Length; i++)
        {
            if (principalSpriteRenderers[i] != null)
            {
                principalSpriteRenderers[i].color = principalOriginalColors[i];
            }
        }
        
        Debug.Log("KoutyouTrigger: Principalのリアクションが完了しました。");
    }
}
