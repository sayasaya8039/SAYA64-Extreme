# SAYA64 Extreme

**Windows向け 高機能システム情報 & 診断ユーティリティ**

SAYA64 Extremeは、AIDA64にインスパイアされたオープンソースのシステム情報・ハードウェア監視・ベンチマークツールです。WPF + .NET 8で構築され、Catppuccin Mocha（ダーク）/ Latte（ライト）の切り替え可能なテーマUIを搭載しています。

<!-- スクリーンショットを追加する場合 -->
<!-- ![SAYA64 Extreme](docs/screenshot.png) -->

## 主な機能

### ハードウェア情報収集
- **CPU** - プロセッサ名、コア数、クロック、キャッシュ、ソケット
- **マザーボード** - メーカー、BIOS情報、シリアルナンバー
- **メモリ** - 容量、DIMM構成、速度、メーカー
- **ストレージ** - ドライブ情報、S.M.A.R.T.ステータス
- **GPU / ディスプレイ** - VRAM、ドライバ、解像度、リフレッシュレート
- **ネットワーク** - アダプタ、MAC/IP、リンク速度
- **OS** - バージョン、ビルド番号、インストール日時
- **ソフトウェア** - インストール済みプログラム、実行中プロセス、USBデバイス

### リアルタイムセンサー監視
[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)を利用した包括的なハードウェアセンサー監視。

| センサー種別 | 表示単位 |
|---|---|
| CPU / GPU 温度 | °C |
| 冷却ファン回転数 | RPM |
| 電圧 | V |
| 消費電力 | W |
| クロック周波数 | MHz |
| 使用率 | % |

- 1秒間隔のリアルタイムポーリング（100ms〜任意に設定可能）
- 最小値 / 最大値 / 現在値のトラッキング
- センサー種別ごとのフィルタリング

### パフォーマンスベンチマーク

**メモリベンチマーク（4テスト）**

| テスト | 計測内容 |
|---|---|
| メモリ読み取り | シーケンシャルリード (MB/s) |
| メモリ書き込み | パラレルライト (MB/s) |
| メモリコピー | Buffer.MemoryCopy (MB/s) |
| メモリレイテンシ | ポインタチェイス (ns) |

**CPUベンチマーク（4テスト）**

| テスト | 計測内容 |
|---|---|
| N-Queens (N=12) | バックトラッキング解探索 |
| ZLib圧縮 | DeflateStream (MB/s) |
| AES-256-CBC暗号化 | 64MBデータ暗号化 (MB/s) |
| SHA-256ハッシュ | 64MBデータハッシュ (MB/s) |

**FPUベンチマーク（3テスト）**

| テスト | 計測内容 |
|---|---|
| Julia Set (float32) | フラクタル演算 (MPixel/s) |
| Mandelbrot Set (float64) | 倍精度演算 (MPixel/s) |
| Sin(z) Julia | 三角関数演算 (MPixel/s) |

### その他の機能
- **ダーク / ライトモード切り替え** - Catppuccin Mocha（ダーク）/ Latte（ライト）をランタイムで即時切替、設定は自動保存
- **日本語 / 英語切り替え** - ランタイムで即座に言語変更
- **HTMLレポート出力** - 全システム情報をスタイル付きHTMLで出力
- **システムトレイ常駐** - 最小化でトレイに格納、ダブルクリックで復元

## システム要件

| 項目 | 要件 |
|---|---|
| OS | Windows 10 以降（64ビット） |
| ランタイム | .NET 8.0（インストーラー版はランタイム同梱） |
| 権限 | 管理者権限（センサー読み取りに必要） |
| ディスク | 約200MB（インストーラー版） |

## インストール

### インストーラー版（推奨）
[Releases](https://github.com/sayasaya8039/SAYA64-Extreme/releases/latest)から `SAYA64Extreme_v2.2.0_Setup.exe` をダウンロードして実行してください。.NET 8ランタイムを含む自己完結型のため、追加のランタイムインストールは不要です。

### ポータブル版
同じく[Releases](https://github.com/sayasaya8039/SAYA64-Extreme/releases/latest)から `SAYA64Extreme.exe` をダウンロードし、任意のフォルダに配置して実行してください。インストール不要の単一ファイル版です。

### ソースからビルド

```bash
# リポジトリをクローン
git clone <repository-url>
cd "SAYA64 Extreme"

# ビルド
dotnet build src/SAYA64Extreme/SAYA64Extreme.csproj

# 実行（管理者権限が必要）
dotnet run --project src/SAYA64Extreme/SAYA64Extreme.csproj
```

### リリースビルド（自己完結型）

```bash
dotnet publish src/SAYA64Extreme/SAYA64Extreme.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true
```

## 技術スタック

| コンポーネント | 技術 |
|---|---|
| フレームワーク | .NET 8.0 (Windows) |
| UI | WPF (Windows Presentation Foundation) |
| アーキテクチャ | MVVM (CommunityToolkit.Mvvm 8.4.0) |
| ハードウェア監視 | LibreHardwareMonitorLib 0.9.4 |
| ハードウェア情報 | WMI (Windows Management Instrumentation) |
| テーマ | Catppuccin Mocha (ダーク) / Latte (ライト) |
| インストーラー | Inno Setup 6 |

## プロジェクト構成

```
SAYA64 Extreme/
├── src/SAYA64Extreme/
│   ├── Models/              # データモデル
│   │   ├── BenchmarkResult.cs
│   │   ├── CategoryNode.cs
│   │   ├── SensorConfig.cs
│   │   ├── SensorReading.cs
│   │   └── SystemItem.cs
│   ├── ViewModels/          # MVVM ViewModel
│   │   ├── MainViewModel.cs
│   │   ├── BenchmarkViewModel.cs
│   │   ├── CategoryViewModel.cs
│   │   └── SensorViewModel.cs
│   ├── Services/            # ビジネスロジック
│   │   ├── WmiService.cs
│   │   ├── SensorService.cs
│   │   ├── SoftwareService.cs
│   │   ├── LanguageService.cs
│   │   ├── ThemeService.cs
│   │   ├── ReportService.cs
│   │   ├── MemoryBenchmarkService.cs
│   │   ├── CpuBenchmarkService.cs
│   │   ├── FpuBenchmarkService.cs
│   │   └── ...
│   ├── Resources/           # リソース
│   │   ├── Strings.ja.xaml
│   │   ├── Strings.en.xaml
│   │   ├── Styles.xaml
│   │   ├── Theme.Dark.xaml
│   │   ├── Theme.Light.xaml
│   │   └── app.ico
│   ├── MainWindow.xaml      # メインウィンドウUI
│   └── SAYA64Extreme.csproj
├── installer/
│   ├── setup.iss            # Inno Setupスクリプト
│   └── LICENSE.txt
└── README.md
```

## 使い方

1. **アプリ起動** - 管理者権限で自動起動されます
2. **カテゴリ選択** - 左ペインのツリーからカテゴリを選択
3. **センサー監視** - 「センサー」カテゴリでリアルタイム値を確認
4. **ベンチマーク** - ツールバーまたはメニューから実行
5. **レポート出力** - ツールバーの「レポート」ボタンでHTML出力
6. **テーマ切替** - メニュー > 表示 からダーク/ライトモードを選択
7. **言語切り替え** - メニュー > 言語 から日本語/英語を選択
8. **トレイ格納** - ウィンドウ最小化でシステムトレイに常駐

## ライセンス

Copyright (c) 2024-2026 SAYA64 Project. All rights reserved.

### 使用ライブラリ
- [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - MPL-2.0 License
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MIT License

## 開発

このアプリケーションはClaude Code（Anthropic）を活用して開発されました。
