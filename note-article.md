# Claude Codeだけで「AIDA64クローン」を作った話 ― SAYA64 Extreme開発記

## はじめに

PCの詳細なハードウェア情報を一覧表示し、リアルタイムでセンサー値を監視し、ベンチマークまで実行できるWindows定番ツール「AIDA64」。あのツールを**Claude Codeだけで、ゼロからフルスクラッチ**で作ってみました。

完成したのが**SAYA64 Extreme v2.2.0** ― WPF + .NET 8製のシステム情報&診断ユーティリティです。

この記事では、Claude Codeとの共同開発でどんなアプリが出来上がったのか、どういう開発プロセスだったのかを紹介します。

---

## 完成したアプリの概要

### SAYA64 Extreme とは

AIDA64のような「システム情報の収集・表示」「リアルタイムセンサー監視」「パフォーマンスベンチマーク」を1つにまとめたWindowsデスクトップアプリです。

**主な機能**
- CPU、マザーボード、メモリ、ストレージ、GPU、ネットワークなどの詳細情報をWMI経由で収集
- LibreHardwareMonitorによるリアルタイムセンサー監視（温度・ファン・電圧・クロック・電力・負荷）
- メモリ / CPU / FPU の11種類のベンチマークテスト
- HTMLレポート出力
- 日本語 / 英語のランタイム切り替え
- **ダーク / ライトモード切り替え** ― Catppuccin Mocha（ダーク）/ Latte（ライト）をランタイムで即時切替
- システムトレイ常駐

### 技術スタック

| 項目 | 技術 |
|---|---|
| 言語 | C# (.NET 8.0) |
| UI | WPF |
| アーキテクチャ | MVVM (CommunityToolkit.Mvvm) |
| センサー | LibreHardwareMonitorLib 0.9.4 |
| ハードウェア情報 | WMI |
| テーマ | Catppuccin Mocha / Latte（ダーク・ライト切替対応） |
| インストーラー | Inno Setup 6 |

---

## 開発の流れ

### Phase 1：MVP（基本骨格）

最初のプロンプトでは「AIDA64のようなツールをWPFで作って」という方向性だけ伝えました。Claude Codeが生成したのは：

- **MVVMの骨格** ― MainViewModel + CategoryViewModel + サービス群
- **WMIサービス** ― CPU、メモリ、マザーボード、ストレージ、GPU、OS情報を一括取得
- **ツリービュー + DataGrid** の2ペインレイアウト
- **ダークテーマ（Styles.xaml）** ― Catppuccin Mocha配色

この時点で、左のツリーでカテゴリを選ぶと右にハードウェア情報が表示される基本的なUIが動きました。

### Phase 2：機能拡充

続けて以下を追加：

- **ソフトウェア情報** ― インストール済みプログラム一覧、プロセスリスト、USBデバイス
- **メモリベンチマーク** ― Read / Write / Copy / Latency
- **HTMLレポート出力** ― スタイル付きの見やすいレポート
- **言語切り替え** ― `ResourceDictionary` + `DynamicResource` でランタイム日英切り替え

ここまでがPhase 2。v2.0.0としてコミット。

### Phase 3：センサー強化 + CPU/FPUベンチマーク

ここからが本番です。2つの大きなタスクを**並列のバックグラウンドエージェント**で同時進行しました。

**エージェント1：センサーモジュール強化**
- `SensorService` にLibreHardwareMonitorの`IVisitor`パターンを導入
- SubHardwareの再帰的な探索で、従来取得できなかったセンサー値を収集
- `SensorAlertService`（閾値アラート）、`SensorCorrectionService`（値補正）、`SensorSharingService`（共有機能）を追加

**エージェント2：CPU / FPUベンチマーク実装**
- CPU ― N-Queens、ZLib圧縮、AES-256暗号化、SHA-256ハッシュ
- FPU ― Julia Set、Mandelbrot Set、Sin(z) Julia

2つのエージェントが完了した後、UIに統合してv2.1.0に。

### Phase 4：ダーク / ライトモード切り替え

v2.2.0ではテーマ切り替え機能を追加しました。

- `Styles.xaml` からカラー/ブラシ定義を `Theme.Dark.xaml`（Catppuccin Mocha）/ `Theme.Light.xaml`（Catppuccin Latte）に分離
- 全ての `StaticResource` を `DynamicResource` に変更し、ランタイムでの即時反映を実現
- `ThemeService` を既存の `LanguageService` と同じ `ResourceDictionary` 動的差し替えパターンで実装
- テーマ選択は `theme.conf` に自動保存され、次回起動時に復元

メニュー「表示」からワンクリックでダーク⇔ライトを切り替えられます。既存のスタイル定義はそのまま活かしつつ、カラー定義だけを差し替える設計にしたため、影響範囲を最小限に抑えられました。

### バグとの戦い

ここからが面白い部分で、いくつかの実践的な問題に直面しました。

**1. センサー値が全てN/A**

原因は3つの複合問題でした：

- `app.manifest`が無く、管理者権限が取れていない（LibreHardwareMonitorのWinRing0ドライバが動作しない）
- `IsControllerEnabled = true` が未設定
- `IVisitor`パターンを使っていないためSubHardwareの更新が漏れていた

解決策として`requestedExecutionLevel="requireAdministrator"`のマニフェストを追加し、`UpdateVisitor`クラスを実装。再帰的にSubHardwareをトラバースするVisitorパターンに書き換えました。

```csharp
internal sealed class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }

    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (var sub in hardware.SubHardware)
            sub.Accept(this);
    }
    // ...
}
```

**2. DataGridにセンサー値が表示されない**

XAMLのDataGridが`{Binding Value}`で参照しているのに、`SensorReading`モデルには`CurrentValue`しかなかった。`Value`プロパティを追加し、`OnCurrentValueChanged`で`PropertyChanged`を発火させて解決。

**3. UseWindowsForms追加後のCS0104エラー**

システムトレイ実装のために`UseWindowsForms`を追加したところ、`Application`、`Timer`、`MessageBox`がWPF側とWinForms側で衝突。各ファイルに`using Application = System.Windows.Application;`のようなエイリアスを追加して解決。

---

## ベンチマークの実装詳細

ベンチマークは3カテゴリ11テストで、AIDA64を意識した構成です。

### メモリベンチマーク

`unsafe`ポインタ操作と`Parallel.For`を使い、マルチスレッドでメモリ帯域を計測します。

- **Read**: ピン留めした配列をパラレルでシーケンシャルリード
- **Write**: 64バイトストライドでパラレルライト
- **Copy**: `Buffer.MemoryCopy`でピン留め配列間コピー
- **Latency**: 128MBのランダムポインタチェイスで計測

### CPUベンチマーク

- **N-Queens (N=12)**: バックトラッキング法で全解を探索。マルチスレッド対応
- **ZLib圧縮**: `DeflateStream`で16MBデータを最高圧縮
- **AES-256-CBC**: `System.Security.Cryptography`で64MBデータを暗号化
- **SHA-256**: 64MBデータ × 5反復のハッシュ計算

### FPUベンチマーク

フラクタル演算で浮動小数点性能を計測。

- **Julia Set (float32)**: 1024x1024ピクセル、5反復
- **Mandelbrot Set (float64)**: 倍精度での標準マンデルブロ集合
- **Sin(z) Julia**: `sinh`/`cosh`を使った三角関数負荷テスト

---

## UIデザイン

### Catppuccin テーマ（ダーク / ライト）

AIDA64のクラシックなUIではなく、Catppuccin カラーパレットを採用。v2.2.0からダーク/ライトモードの切り替えに対応しています。

**ダークモード（Catppuccin Mocha）**

| 要素 | カラーコード |
|---|---|
| 背景（Primary） | `#1E1E2E` |
| 背景（Secondary） | `#2B2B3D` |
| ヘッダー | `#181825` |
| アクセント | `#7AA2F7` |
| テキスト | `#CDD6F4` |

**ライトモード（Catppuccin Latte）**

| 要素 | カラーコード |
|---|---|
| 背景（Primary） | `#EFF1F5` |
| 背景（Secondary） | `#E6E9EF` |
| ヘッダー | `#DCE0E8` |
| アクセント | `#1E66F5` |
| テキスト | `#4C4F69` |

### 2ペインレイアウト

左にカテゴリツリー、右にDataGridの定番レイアウト。ツリーのカテゴリにはUnicode絵文字アイコンを使い、視覚的に分かりやすくしています。

---

## Claude Codeとの開発で感じたこと

### 並列エージェントが強い

センサー強化とベンチマーク実装を同時に別々のエージェントで進行させたのは非常に効率的でした。それぞれが独立したファイルで作業するため、コンフリクトなく並行開発できます。

### ビルドエラーの修正が的確

型のあいまい参照（CS0104）のような.NET固有の問題も、原因を正確に特定して`using`エイリアスで解決してくれます。

### 実機テストは人間が必要

一方で、「センサー値がN/Aになる」のような実行環境依存の問題は、実際に動かしてみないと分かりません。ここは人間がフィードバックを返す必要がある部分です。

---

## インストール方法

### インストーラー版

[Releases](https://github.com/sayasaya8039/SAYA64-Extreme/releases/latest)から`SAYA64Extreme_v2.2.0_Setup.exe`をダウンロードして実行するだけ。.NET 8ランタイム同梱の自己完結型なので、追加インストール不要です。

### ポータブル版

同じく[Releases](https://github.com/sayasaya8039/SAYA64-Extreme/releases/latest)から`SAYA64Extreme.exe`をダウンロード。インストール不要の単一ファイル版です。

### ソースからビルド

```bash
git clone <repository-url>
cd "SAYA64 Extreme"
dotnet build src/SAYA64Extreme/SAYA64Extreme.csproj
```

管理者権限で実行してください。

---

## まとめ

Claude Codeを使って、AIDA64クローンの「SAYA64 Extreme」をフルスクラッチで開発しました。

**開発成果**
- C#ソースファイル 22個
- XAML 8ファイル（テーマ2ファイル追加）
- 15のサービスクラス（ThemeService追加）
- 11種類のベンチマークテスト
- 13メインカテゴリ + 20サブカテゴリ
- 125以上の国際化キー（日/英）
- ダーク / ライトモード切り替え
- Inno Setupインストーラー

AIコーディングアシスタントとのペアプロで、WMI、LibreHardwareMonitor、WPF MVVM、unsafeポインタ操作、マルチスレッドベンチマーク、Inno Setupインストーラーなど、幅広い技術領域をカバーしたアプリケーションが完成しました。

「AIDA64は便利だけど有料だし、自分好みのカスタマイズがしたい」という方は、ぜひオープンソース版を試してみてください。

---

**GitHub**: [sayasaya8039/SAYA64-Extreme](https://github.com/sayasaya8039/SAYA64-Extreme)

**使用ツール**: Claude Code (Anthropic)

**技術スタック**: C# / .NET 8.0 / WPF / LibreHardwareMonitor / Inno Setup
