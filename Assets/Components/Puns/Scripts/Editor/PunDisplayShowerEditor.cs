using UnityEngine;
using UnityEditor;

/// <summary>
/// PunDisplayShower用のカスタムエディター
/// デバッグ用の再生成ボタンを追加
/// </summary>
[CustomEditor(typeof(PunDisplayShower))]
public class PunDisplayShowerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // デフォルトのインスペクターを表示
        DrawDefaultInspector();

        // スペーサー
        EditorGUILayout.Space();

        // ターゲットを取得
        PunDisplayShower punDisplayShower = (PunDisplayShower)target;

        // デバッグ用セクション
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

        // 再生成ボタン
        if (GUILayout.Button("再生成", GUILayout.Height(30)))
        {
            // エディター再生中でない場合のみ実行
            if (Application.isPlaying)
            {
                // オブジェクトを有効化
                punDisplayShower.gameObject.SetActive(true);
                
                // アニメーションを再生成
                punDisplayShower.StartDisplayAnimation();
            }
            else
            {
                EditorUtility.DisplayDialog("警告", "再生成ボタンは再生モード中のみ使用できます。", "OK");
            }
        }

        // 再生モードでない場合の警告
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("再生成ボタンは再生モード中のみ使用できます。", MessageType.Info);
        }
    }
}

