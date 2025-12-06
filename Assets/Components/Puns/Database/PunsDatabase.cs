using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ダジャレのIDとテキストを管理するデータベース
/// </summary>
[CreateAssetMenu(fileName = "PunsDatabase", menuName = "Puns/Puns Database")]
public class PunsDatabase : ScriptableObject
{
    [System.Serializable]
    public class PunData
    {
        [Tooltip("ダジャレのID")]
        public string id;
        
        [Tooltip("ダジャレのテキスト")]
        [TextArea(2, 4)]
        public string text;
        
        [Tooltip("フォントサイズ（デフォルト: 30）")]
        public float fontSize = 30f;
    }

    [Header("Puns Data")]
    [Tooltip("ダジャレのリスト")]
    [SerializeField] private List<PunData> puns = new List<PunData>();

    /// <summary>
    /// ダジャレをIDで取得
    /// </summary>
    public PunData GetPunById(string id)
    {
        return puns.Find(p => p.id == id);
    }

    /// <summary>
    /// すべてのダジャレを取得
    /// </summary>
    public List<PunData> GetAllPuns()
    {
        return new List<PunData>(puns);
    }

    /// <summary>
    /// ダジャレを追加（エディター用）
    /// </summary>
    public void AddPun(string id, string text)
    {
        // 既存のIDをチェック
        if (puns.Exists(p => p.id == id))
        {
            Debug.LogWarning($"PunsDatabase: ID {id} は既に存在します。");
            return;
        }

        puns.Add(new PunData { id = id, text = text });
    }
}

