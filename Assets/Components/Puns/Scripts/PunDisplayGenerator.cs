using UnityEngine;
using System.Collections;
using System.Linq;

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
    
    private Transform punsParentTransform; // 「Puns」という名前のGameObjectのTransform
    private bool isDisplaying = false; // 現在PunDisplayが表示中かどうか
    private GameObject currentPunDisplay; // 現在表示中のPunDisplay

    private void Awake()
    {
        // 「Puns」という名前のGameObjectを検索
        FindPunsParent();
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
    /// ダジャレを生成
    /// </summary>
    /// <param name="punId">ダジャレのID</param>
    public void GeneratePun(string punId)
    {
        // 既に表示中の場合は生成しない
        if (isDisplaying)
        {
            return;
        }
        
        // 遅延してから実際に生成する
        StartCoroutine(GeneratePunDelayed(punId));
    }

    /// <summary>
    /// 遅延してからダジャレを生成
    /// </summary>
    /// <param name="punId">ダジャレのID</param>
    private IEnumerator GeneratePunDelayed(string punId)
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
        }
        else
        {
            Debug.LogWarning("PunDisplayGenerator: PunDisplayShowerコンポーネントが見つかりません。");
            // PunDisplayShowerが見つからない場合はフラグをリセット
            isDisplaying = false;
            currentPunDisplay = null;
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
}

