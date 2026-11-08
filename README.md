# CaptureStacker

## Overview

Windows 標準の範囲キャプチャ（Win + Shift + S）を、Win32 API とグローバルフックで自作してみた学習用プロジェクトです。標準機能にはない「連続で撮って溜める」「撮った画像をその場で前後に切り替えて確認する」動きを加えています。

CaptureStacker は、画面を連続でキャプチャして画像を溜めていく（スタックする）Windows 用ツールです。
ホットキーを押しながらマウスで操作するだけで、範囲・ウィンドウ・モニターの画像を連番ファイルとして保存できます。
グローバルなマウス／キーボードフックを使うため、他のアプリを操作中でもキャプチャできます。

ホットキー（初期値: 左 Shift）を押したままドラッグして範囲を選びます。

![demo](https://user-images.githubusercontent.com/76393141/199930065-59b56c3f-ea95-4fc2-9e74-3b1db7bd0470.gif)

## 技術的なポイント

| 項目 | 内容 |
|---|---|
| グローバルフック | `SetWindowsHookEx` による低レベルのマウス／キーボードフックで、他アプリの操作中でも入力を取得します。 |
| Win32 API の利用 | P/Invoke で `WindowFromPoint`、`GetParent`、`EnumDisplayMonitors` などを呼び出しています。 |
| ウィンドウ範囲の取得 | DWM の `DwmGetWindowAttribute`（ExtendedFrameBounds）で、影を除いた実際のウィンドウ範囲を取得します。 |
| マルチモニター対応 | 全モニターの位置とサイズを列挙し、クリック位置から対象モニターを判定します。 |
| 範囲指定の枠 | `TransparencyKey` で内側を透明にした枠ウィンドウを自前で描画し、ドラッグによる移動とリサイズに対応しています。 |
| 設定の外部化 | ホットキー、保存先、ファイル名、ウィンドウサイズを `App.config` で変更できます。 |

## 動作環境

- Windows 10 / 11
- .NET Framework 4.8
- ビルド: Visual Studio（`CaptureStacker.sln`）

## 使い方

1. `CaptureStacker.exe` を起動します（小さなウィンドウに現在のモードが表示されます）。
2. **ホットキー（初期値: 左 Shift）を押したまま** 以下の操作をします。

### キャプチャモードの切り替え

ホットキーを押しながら数字キーを押すと、モードが切り替わります。

| キー | モード | 動作 |
|---|---|---|
| `1` | RANGE（範囲指定） | ドラッグで黄色い枠を作り、ホットキーを離すと枠内を保存します。ホットキーを押したまま、枠の辺をドラッグで移動、下の角をドラッグでリサイズできます。 |
| `2` | WINDOW（ウィンドウ指定） | クリックした位置のウィンドウ全体を保存します。 |
| `3` | MONITOR（モニター指定） | クリックした位置のモニター全体を保存します（マルチモニター対応）。 |
| `4` | PREVIEW（プレビュー） | ウィンドウが広がり、保存済み画像のサムネイルを表示します。 |

### プレビュー

PREVIEW モード中に、ホットキーを押しながら `←` / `→` で前後の画像を切り替えます。
表示位置は「3 / 10」のように表示されます。

## 保存先とファイル名

画像は `ImageStoragePath` のフォルダに、`FileName` + 連番 + `Extension` の名前で保存されます
（例: `ImageStorage/Image1.jpg`, `Image2.jpg`, ...）。
連番は、起動時にフォルダ内にあるファイル数の続きから始まります。

保存先フォルダが存在しない場合は、起動時および保存時に自動で作成されます。

## 設定（App.config）

`CaptureStacker.exe.config`（ソースでは `App.config`）の `appSettings` で変更できます。

| キー | 初期値 | 説明 |
|---|---|---|
| `HotKey` | `LShiftKey` | ホットキー。`System.Windows.Forms.Keys` の名前で指定します。 |
| `ImageStoragePath` | `ImageStorage/` | 画像の保存先フォルダ（末尾に `/` を付けてください）。 |
| `FileName` | `Image` | ファイル名の前半部分。 |
| `Extension` | `.jpg` | 保存する拡張子。 |
| `FrameBorderSize` | `5` | 範囲指定枠の線の太さ（px）。 |
| `CaptureStackerWidth` / `CaptureStackerHeight` | `280` / `85` | 通常時のウィンドウサイズ。 |
| `PreviewWidth` / `PreviewHeight` | `580` / `600` | プレビュー時のウィンドウサイズ。 |
| `RootDirectoryPath` | `../CaptureStacker` | 現在のコードでは未使用です。 |

## プロジェクト構成

| パス | 内容 |
|---|---|
| `WinUI/CaptureStackerForm.cs` | メイン画面。モード管理、キャプチャ、保存、プレビュー。 |
| `WinUI/FrameForm.cs` | 範囲指定用の枠ウィンドウ。 |
| `WinUI/OverWrapForm.cs` | 範囲指定中に画面全体へ重ねるオーバーレイ。 |
| `Hook/` | マウス／キーボードのグローバルフック。 |
| `Win32API/` | Win32 API 呼び出し（P/Invoke）、クリップボード処理。 |
| `Entity/` | モニター情報などのデータクラス。 |
| `01_参考資料/` | 実装時に参考にしたサンプル（本体ではありません）。 |
