using UnityEngine;
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
    
    private Transform punsParentTransform; // 「Puns」という名前のGameObjectのTransform

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
        if (punsDatabase == null)
        {
            Debug.LogError("PunDisplayGenerator: PunsDatabaseが設定されていません。");
            return;
        }

        if (punDisplayPrefab == null)
        {
            Debug.LogError("PunDisplayGenerator: PunDisplayPrefabが設定されていません。");
            return;
        }

        // データベースからダジャレを取得
        PunsDatabase.PunData punData = punsDatabase.GetPunById(punId);
        
        if (punData == null)
        {
            Debug.LogWarning($"PunDisplayGenerator: ID \"{punId}\" のダジャレが見つかりません。");
            
            // デバッグ: データベース内のすべてのIDを表示
            var allPuns = punsDatabase.GetAllPuns();
            Debug.LogWarning($"PunDisplayGenerator: データベース内のID一覧: {string.Join(", ", allPuns.Select(p => $"\"{p.id}\""))}");
            return;
        }

        // 「Puns」親オブジェクトを再検索（念のため）
        if (punsParentTransform == null)
        {
            FindPunsParent();
        }

        // Prefabを生成（位置は(0,0,0)に固定、「Puns」の子として）
        GameObject instance = Instantiate(punDisplayPrefab, Vector3.zero, Quaternion.identity, punsParentTransform);
        
        // PunDisplayShowerコンポーネントを取得してテキストを設定
        PunDisplayShower punDisplayShower = instance.GetComponent<PunDisplayShower>();
        if (punDisplayShower != null)
        {
            // テキストを設定
            punDisplayShower.SetText(punData.text);
            
            // ConcentrateLineのMaterialのDelayを0~10でランダムに設定
            SetupConcentrateLineMaterial(punDisplayShower);
        }
        else
        {
            Debug.LogWarning("PunDisplayGenerator: PunDisplayShowerコンポーネントが見つかりません。");
        }
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

