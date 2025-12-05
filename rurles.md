Unityで2Dゲームを作成しています。パッケージは UniTask と DOTween を用いています。

ダジャレをテーマにしたゲームを作成します。
マップ内からダジャレを探し、ダジャレを見つけることでポイントが加算されます。一定のポイントに達することでクリアとなります。
画面内にあるオブジェクトをドラッグアンドドロップして動かすことがダジャレを発生させるトリガーです。
主に物理演算を用いています。

## コーディングルール
インスペクタにゲームオブジェクトなどをアサインするコードを書く際、ユーザーがしていない場合に、ゲーム内から検索する機能を入れないでください。インスペクタに場合はエラーを出してください。
新しいInput.Systenを用いてください


このワーニングに気をつけて
- Assets\Components\Puns\Scripts\DragAndDropManager.cs(40,26): warning CS0618: 'Object.FindObjectOfType<T>()' is obsolete: 'Object.FindObjectOfType has been deprecated. Use Object.FindFirstObjectByType instead or if finding any instance is acceptable the faster Object.FindAnyObjectByType'
- Assets\Components\Puns\Scripts\DragAndDropManager.cs(98,63): warning CS0618: 'Rigidbody2D.isKinematic' is obsolete: 'isKinematic has been deprecated. Please use bodyType.'