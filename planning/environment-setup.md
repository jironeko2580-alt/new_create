# 開発環境構築手順（Unity × iOS）

Mac + Unity + Xcode で iPhone 向けゲームを作る際の初期環境構築手順。
有料登録が必要なものは「まずは無料の範囲で進め、後で必要になったら対応する」方針で最小限からスタートする。

## Step 0. 前提

- Mac（済み）
- Apple ID（Xcode/App Store利用に必要。持っていれば普段使いのもので可）

## Step 1. Xcodeのインストール

- Mac の App Store から「Xcode」を検索してインストール（無料）
- 容量が大きい（十数GB）ので時間に余裕がある時にインストール
- インストール後、一度起動して初回セットアップ（追加コンポーネントのインストール）を完了させておく
- ターミナルでコマンドラインツールの場所を確認しておくと後の作業がスムーズ:
  ```
  xcode-select -p
  ```

## Step 2. Apple ID / Developer Programについて

- **今すぐ有料登録（年間$99）は不要**。以下の範囲は無料のApple IDだけで可能:
  - Xcode Simulatorでの動作確認
  - Xcode上で自分のApple IDを「Personal Team」として設定し、自分のiPhone実機に一時的にインストールしてテスト（アプリは7日で失効、再インストールが必要）
- **有料のApple Developer Program登録が必要になるタイミング**:
  - App Store（TestFlight含む）でのリリース・配布
  - Push通知や一部の高度な機能を使う場合
- なので、今の初期構築段階では登録せず、App Store配信の準備に入る段階で改めて登録すればOK

## Step 3. Unity Hub と Unity Editor のインストール

1. [Unity公式サイト]からUnity Hubをダウンロード・インストール
2. Unity Hubを起動し、Unityアカウントを作成/ログイン（無料のPersonalプランでOK）
3. Unity Hub内で Unity Editor をインストール（バージョンはLTS＝長期サポート版を推奨）
4. インストール時のモジュール選択画面で **「iOS Build Support」に必ずチェック**（これがないとiOS向けビルドができない）

## Step 4. Unityプロジェクトの作成

1. Unity Hubで「新しいプロジェクト」を作成
2. テンプレートは **2D (Core)** を選択（縦画面のカジュアルゲームなので2Dで十分）
3. プロジェクト名・保存場所を設定して作成

## Step 5. 画面向き・解像度など基本設定

Unityエディタ内で以下を設定:

- `Edit > Project Settings > Player > iOS > Resolution and Presentation`
  - Default Orientation を **Portrait** に設定
- `File > Build Settings` で Platform一覧から **iOS** を選択し、「Switch Platform」を実行
  - 初回はアセットの再インポートなどで時間がかかる

## Step 6. Bundle Identifierの設定

- `Edit > Project Settings > Player > iOS > Other Settings > Identification`
  - Bundle Identifier を逆ドメイン形式で設定（例: `com.jironeko.golfgame`）
  - これはApp Store上でアプリを一意に識別するIDなので、後から変更すると配信時に面倒になりがち。早めに決めておく

## Step 7. 実機/シミュレータでの動作確認（初回ビルド確認）

1. Unityで `File > Build Settings > Build` を実行し、Xcodeプロジェクトを書き出す
2. 書き出されたフォルダ内の `.xcodeproj` をXcodeで開く
3. Xcode左側のプロジェクト設定 → `Signing & Capabilities` で Team に自分のApple ID（Personal Team）を選択
4. 実行先をシミュレータ（例: iPhone 15）に設定して ▶ 実行 → 何もない空のシーンが起動すれば環境構築は成功
5. 実機で試したい場合はiPhoneをMacにケーブル接続し、実行先を実機に切り替えて実行（初回は端末側で「このMacを信頼」などの許可が必要）

## Step 8. バージョン管理（Git）の準備

- Unityプロジェクトは専用のGitリポジトリで管理するのが望ましい（現在の `new_create` 練習リポジトリとは別にする想定）
- Unity公式が配布している `.gitignore`（Unity用）を使うと、Library/Temp等の生成物を除外できる
- 大きめのバイナリアセットが増えてきたら Git LFS の導入も検討

## まとめ: 今すぐやること / 後回しでよいこと

| やること | 今 or 後 |
|---|---|
| Xcodeインストール | 今 |
| Unity Hub / Unity Editor（iOS Build Support込み）インストール | 今 |
| Unity 2Dプロジェクト作成・iOSに切り替え | 今 |
| シミュレータで空プロジェクトの起動確認 | 今 |
| Apple Developer Program有料登録 | 後（App Store配信直前でOK） |
| Git LFS導入 | 後（アセットが増えてから） |
