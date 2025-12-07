using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

/// <summary>
/// ダジャレ成立時にPunDisplayのPrefabを生成するスクリプト
/// </summary>
public class PunDisplayGenerator : MonoBehaviour
{
    [Header("Prefab Settings")]
    [Tooltip("PunDisplayのPrefab")]
    [SerializeField] private GameObject punDisplayPrefab;
    
    [Header("Database Settings")]
    [Tooltip("ダジャレデータベース")]
    [SerializeField] private PunsDatabase punsDatabase;
    
    [Header("Generation Settings")]
    [Tooltip("生成位置のオフセット")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    
    [Tooltip("投げてからPunDisplayが表示されるまでの遅延時間（秒、デフォルト: 0.3）")]
    [SerializeField] private float displayDelay = 0.3f;
    
    [Header("Score Settings")]
    [Tooltip("スコアマネージャー（nullの場合は自動検索）")]
    [SerializeField] private ScoreManager scoreManager;
    
    [Header("Camera Shake Settings")]
    [Tooltip("カメラ振動の強さ")]
    [SerializeField] private float shakeStrength = 0.2f;
    
    [Tooltip("カメラ振動の時間（秒）")]
    [SerializeField] private float shakeDuration = 1.0f;
    
    [Tooltip("カメラ振動の振動数")]
    [SerializeField] private int shakeVibrato = 10;
    
    private Transform punsParentTransform; // 「Puns」という名前のGameObjectのTransform
    private bool isDisplaying = false; // 現在PunDisplayが表示中かどうか
    private GameObject currentPunDisplay; // 現在表示中のPunDisplay
    private HashSet<GameObject> triggeredGameObjects = new HashSet<GameObject>(); // 既にダジャレを発生させたGameObject
    private Camera mainCamera; // メインカメラ

    private void Awake()
    {
        // 「Puns」という名前のGameObjectを検索
        FindPunsParent();
        
        // ScoreManagerを自動検索（インスペクタで設定されていない場合）
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }
        
        // メインカメラを取得
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    /// <summary>
    /// 「Puns」という名前のGameObjectを検索
    /// </summary>
    private void FindPunsParent()
    {
        GameObject punsObject = GameObject.Find("Puns");
        
        if (punsObject != null)
        {
            punsParentTransform = punsObject.transform;
        }
        else
        {
            Debug.LogWarning("PunDisplayGenerator: 「Puns」という名前のGameObjectが見つかりません。ルートに生成されます。");
            punsParentTransform = null;
        }
    }

    /// <summary>
    /// ダジャレを生成（互換性のため、呼び出し元のGameObjectを自動検出）
    /// </summary>
    /// <param name="punId">ダジャレのID</param>
    public void GeneratePun(string punId)
    {
        // 呼び出し元のGameObjectを検出（スタックトレースから）
        // ただし、これは信頼性が低いため、明示的にGameObjectを渡すバージョンを使用することを推奨
        GeneratePun(punId, null);
    }
    
    /// <summary>
    /// ダジャレを生成
    /// </summary>
    /// <param name="punId">ダジャレのID</param>
    /// <param name="caller">呼び出し元のGameObject（nullの場合はチェックをスキップ）</param>
    public void GeneratePun(string punId, GameObject caller)
    {
        // 呼び出し元のGameObjectが既にダジャレを発生させている場合は生成しない
        if (caller != null && triggeredGameObjects.Contains(caller))
        {
            Debug.Log($"PunDisplayGenerator: GameObject \"{caller.name}\" は既にダジャレを発生済みのため、生成をスキップします。");
            return;
        }
        
        // 既に表示中の場合は生成しない
        if (isDisplaying)
        {
            return;
        }
        
        // 遅延してから実際に生成する
        StartCoroutine(GeneratePunDelayed(punId, caller));
    }

    /// <summary>
    /// 遅延してからダジャレを生成
    /// </summary>
    /// <param name="punId">ダジャレのID</param>
    /// <param name="caller">呼び出し元のGameObject</param>
    private IEnumerator GeneratePunDelayed(string punId, GameObject caller)
    {
        // 遅延時間を待つ
        if (displayDelay > 0f)
        {
            yield return new WaitForSeconds(displayDelay);
        }

        if (punsDatabase == null)
        {
            Debug.LogError("PunDisplayGenerator: PunsDatabaseが設定されていません。");
            yield break;
        }

        if (punDisplayPrefab == null)
        {
            Debug.LogError("PunDisplayGenerator: PunDisplayPrefabが設定されていません。");
            yield break;
        }

        // データベースからダジャレを取得
        PunsDatabase.PunData punData = punsDatabase.GetPunById(punId);
        
        if (punData == null)
        {
            Debug.LogWarning($"PunDisplayGenerator: ID \"{punId}\" のダジャレが見つかりません。");
            
            // デバッグ: データベース内のすべてのIDを表示
            var allPuns = punsDatabase.GetAllPuns();
            Debug.LogWarning($"PunDisplayGenerator: データベース内のID一覧: {string.Join(", ", allPuns.Select(p => $"\"{p.id}\""))}");
            yield break;
        }

        // 「Puns」親オブジェクトを再検索（念のため）
        if (punsParentTransform == null)
        {
            FindPunsParent();
        }

        // 呼び出し元のGameObjectが既にダジャレを発生させている場合は生成しない（コルーチン中に他のトリガーが発動した場合の対策）
        if (caller != null && triggeredGameObjects.Contains(caller))
        {
            yield break;
        }
        
        // 既に表示中の場合は生成しない（コルーチン中に他のトリガーが発動した場合の対策）
        if (isDisplaying)
        {
            yield break;
        }
        
        // 表示中フラグを立てる
        isDisplaying = true;
        
        // Prefabを生成（位置は(0,0,0)に固定、「Puns」の子として）
        GameObject instance = Instantiate(punDisplayPrefab, Vector3.zero, Quaternion.identity, punsParentTransform);
        currentPunDisplay = instance;
        
        // PunDisplayShowerコンポーネントを取得してテキストを設定
        PunDisplayShower punDisplayShower = instance.GetComponent<PunDisplayShower>();
        if (punDisplayShower != null)
        {
            // テキストを設定
            punDisplayShower.SetText(punData.text);
            
            // フォントサイズを設定（デフォルト30）
            punDisplayShower.SetFontSize(punData.fontSize);
            
            // テキストオブジェクトのZ軸回転を-10度から10度までランダムに設定
            GameObject textObject = punDisplayShower.GetTextObject();
            if (textObject != null)
            {
                float randomRotationZ = Random.Range(-10f, 10f);
                Vector3 currentRotation = textObject.transform.localEulerAngles;
                textObject.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, randomRotationZ);
            }
            
            // ConcentrateLineのMaterialのDelayを0~10でランダムに設定
            SetupConcentrateLineMaterial(punDisplayShower);
            
            // PunDisplayShowerが破棄されたときに通知を受け取る
            punDisplayShower.SetPunDisplayGenerator(this);
            
            // 正常に生成されたので、呼び出し元のGameObjectを記録
            if (caller != null)
            {
                triggeredGameObjects.Add(caller);
            }
            
            // スコアをインクリメント
            if (scoreManager != null)
            {
                scoreManager.IncrementScore();
            }
            
            // カメラを振動させる
            ShakeCamera();
        }
        else
        {
            Debug.LogWarning("PunDisplayGenerator: PunDisplayShowerコンポーネントが見つかりません。");
            // PunDisplayShowerが見つからない場合はフラグをリセット
            isDisplaying = false;
            currentPunDisplay = null;
            // 生成に失敗したので、生成されたインスタンスを破棄
            Destroy(instance);
        }
    }
    
    /// <summary>
    /// PunDisplayが破棄されたときに呼び出される（PunDisplayShowerから呼び出される）
    /// </summary>
    public void OnPunDisplayDestroyed()
    {
        isDisplaying = false;
        currentPunDisplay = null;
    }

    /// <summary>
    /// ConcentrateLineのMaterialのDelayをランダムに設定
    /// </summary>
    private void SetupConcentrateLineMaterial(PunDisplayShower punDisplayShower)
    {
        if (punDisplayShower == null) return;
        
        // PunDisplayShowerからConcentrateLineオブジェクトを取得
        GameObject concentrationLine = punDisplayShower.GetConcentrationLineObject();
        if (concentrationLine == null)
        {
            Debug.LogWarning("PunDisplayGenerator: ConcentrateLineオブジェクトが見つかりません。");
            return;
        }
        
        // MeshRendererまたはRendererコンポーネントを取得
        Renderer renderer = concentrationLine.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = concentrationLine.GetComponent<MeshRenderer>();
        }
        
        if (renderer != null && renderer.sharedMaterial != null)
        {
            // Materialのインスタンスを作成（元のMaterialに影響を与えないように）
            Material materialInstance = new Material(renderer.sharedMaterial);
            renderer.material = materialInstance;
            
            // Delayプロパティを0~10でランダムに設定
            float randomDelay = Random.Range(0f, 10f);
            materialInstance.SetFloat("_Delay", randomDelay);
        }
        else
        {
            Debug.LogWarning("PunDisplayGenerator: ConcentrateLineのRendererまたはMaterialが見つかりません。");
        }
    }
    
    /// <summary>
    /// カメラを振動させる
    /// </summary>
    private void ShakeCamera()
    {
        if (mainCamera == null)
        {
            return;
        }
        
        // 既存の振動アニメーションを停止
        mainCamera.transform.DOKill();
        
        // カメラを振動させる（位置を振動）
        mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 90f, false, true, ShakeRandomnessMode.Full)
            .SetTarget(mainCamera.transform);
    }
}

