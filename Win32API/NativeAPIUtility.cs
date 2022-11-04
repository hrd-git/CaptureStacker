using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CaptureStacker.Win32API
{
    public static class NativeAPIUtility
    {
        /// <summary>
        /// フックプロシージャのデリゲート
        /// </summary>
        /// <param name="nCode">フックプロシージャに渡すフックコード</param>
        /// <param name="wParam">フックプロシージャに渡す値</param>
        /// <param name="lParam">フックプロシージャに渡す値</param>
        /// <returns></returns>
        public delegate IntPtr HOOKPROC(int nCode, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// 現在呼び出し元プロセスにロードされているexeやdllのメモリ上の位置を示すアドレスを返す
        /// </summary>
        /// <param name="moduleName">ハンドルを取得するモジュールの名前。拡張子を省略した場合は、.DLLとみなされる。</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string moduleName);


        /// <summary>
        /// </summary>
        /// <param name="idHook">フックタイプ</param>
        /// <param name="lpfn">フックプロシージャ</param>
        /// <param name="hMod">対象プロセスのハンドル</param>
        /// <param name="dwThreadId">スレッド識別子</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hMod, IntPtr dwThreadId);


        /// <summary>
        /// 現在のフックチェーン内の次のフックプロシージャに、フック情報を渡す
        /// </summary>
        /// <param name="hHook">現在のフックのハンドル</param>
        /// <param name="nCode">フックプロシージャに渡すフックコード</param>
        /// <param name="wParam">フックプロシージャに渡す値</param>
        /// <param name="lParam">フックプロシージャに渡す値</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// フックチェーン内にインストールされたフックプロシージャを削除する
        /// </summary>
        /// <param name="hHook">削除対象のフックプロシージャのハンドル</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);


        /// <summary>
        /// 親ウィンドウのハンドルを返す
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);


        /// <summary>
        /// デスクトップウィンドウのハンドルを取得する
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDesktopWindow();


        /// <summary>
        /// プロセスIDを取得する
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpdwProcessId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);


        /// <summary>
        /// 指定されたモジュールのベース名を取得する
        /// </summary>
        /// <param name="hWnd">モジュールを含むプロセスへのハンドル</param>
        /// <param name="hModule">モジュールへのハンドル。このパラメーターがNULLの場合、この関数は呼び出しプロセスの作成に使用されるファイルの名前を返す</param>
        /// <param name="lpBaseName">モジュールのベース名を受け取るバッファへのポインタ</param>
        /// <param name="nSize">lpBaseNameバッファーのサイズ（文字数）</param>
        /// <returns></returns>
        [DllImport("psapi.dll", CharSet = CharSet.Ansi)]
        public static extern int GetModuleBaseName(IntPtr hWnd, IntPtr hModule, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder lpBaseName, int nSize);


        /// <summary>
        /// 指定されたウィンドウが属するクラスの名前を取得する。
        /// </summary>
        /// <param name="hWnd">ウィンドウへのハンドルと、間接的に、ウィンドウが属するクラス</param>
        /// <param name="lpClassName">クラス名の文字列</param>
        /// <param name="nMaxCount">lpClassNameバッファーの長さ（文字数）</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);


        /// <summary>
        /// 各モニターの情報を取得する
        /// </summary>
        /// <param name="hMonitor"></param>
        /// <param name="hdcMonitor"></param>
        /// <param name="lpfnEnum"></param>
        /// <param name="dwData"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hMonitor, IntPtr hdcMonitor, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);


        /// <summary>
        /// 指定されたウィンドウを作成したスレッドをフォアグラウンドに移動し、ウィンドウをアクティブにする
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        /// <summary>
        /// 画面上のすべてのトップレベルウィンドウを列挙する
        /// </summary>
        /// <param name="lpEnumFunc">コールバック関数へのポインタ</param>
        /// <param name="lparam">コールバック関数に渡されるアプリケーション定義の値</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc,IntPtr lparam);

        public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);


        /// <summary>
        /// 指定されたウィンドウのタイトルバーテキストの長さを文字数で取得
        /// </summary>
        /// <param name="hWnd">ウィンドウまたはコントロールへのハンドル</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);


        /// <summary>
        /// 指定されたウィンドウのタイトルバー（ある場合）のテキストをバッファにコピーする
        /// </summary>
        /// <param name="hWnd">テキストを含むウィンドウまたはコントロールへのハンドル</param>
        /// <param name="lpString">テキストを受け取るばバッファ</param>
        /// <param name="nMaxCount">バッファにコピーする最大文字数</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        /// <summary>
        /// 指定されたウィンドウに関する情報を取得する
        /// </summary>
        /// <param name="hWnd">ウィンドウへのハンドルと、間接的に、ウィンドウが属するクラス</param>
        /// <param name="nIndex">取得する値へのゼロベースのオフセット</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public extern static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);


        /// <summary>
        /// モニタ情報を取得する
        /// </summary>
        /// <param name="hMonitor">対象モニタへのハンドル</param>
        /// <param name="lpmi">対象モニタに関する情報を受け取る構造体へのポインタ</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);


        /// <summary>
        /// 宛先デバイスコンテキストに指定されたソースデバイスコンテキストからピクセルの矩形に対応する色データのビットブロック転送を行う
        /// </summary>
        /// <param name="hDestDC">宛先デバイスコンテキストへのハンドル</param>
        /// <param name="x">宛先ユニットの長方形の左上隅のx座標（論理単位）</param>
        /// <param name="y">宛先ユニットの長方形の左上隅のy座標（論理単位）</param>
        /// <param name="nWidth">ソースとデスティネーションの四角形の論理単位での幅</param>
        /// <param name="nHeight">送信元と送信先の四角形の論理単位での高さ</param>
        /// <param name="hSrcDC">ソースデバイスコンテキストへのハンドル</param>
        /// <param name="xSrc">ソース長方形の左上隅のx座標（論理単位）</param>
        /// <param name="ySrc">ソース長方形の左上隅のy座標（論理単位）</param>
        /// <param name="dwRop">ラスタ操作コード</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);


        /// <summary>
        /// 指定されたメッセージを１つまたは複数のウィンドウに送信する
        /// </summary>
        /// <param name="hWnd">送信先ウィンドウのハンドル</param>
        /// <param name="Msg">メッセージ</param>
        /// <param name="wParam">メッセージの最初のパラメータ</param>
        /// <param name="lParam">メッセージの２番めのパラメータ</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern long SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, uint lParam);


        /// <summary>
        /// 表示されているウィンドウを指定したデバイスコンテキスト(通常はプリンタDC)にコピーする
        /// </summary>
        /// <param name="hwnd">コピーされるウィンドウのハンドル</param>
        /// <param name="hDC">デバイスコンテキストのハンドルを指定する</param>
        /// <param name="nFlags">描画オプションを指定する。PW_CLIENTONLYのみウィンドウのクライアント領域はhdcBltにコピーされる</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);


        /// <summary>
        /// 指定されたデバイスと互換性のあるメモリデバイスコンテキスト（DC）を作成する
        /// </summary>
        /// <param name="hdc">既存DCへのハンドル。。このハンドルがNULLの場合、関数はアプリケーションの現在の画面と互換性のあるメモリDCを作成する</param>
        /// <returns></returns>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);


        /// <summary>
        /// 指定されたデバイスコンテキストに関連付けられているデバイスと互換性のあるビットマップを作成する
        /// </summary>
        /// <param name="hdc">デバイスコンテキストへのハンドル</param>
        /// <param name="nWidth">ピクセル単位のビットマップの幅</param>
        /// <param name="nHeight">ピクセル単位のビットマップの高さ</param>
        /// <returns></returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);


        /// <summary>
        /// 指定されたデバイスコンテキスト（DC）に変換するオブジェクトを選択する
        /// </summary>
        /// <param name="hdc">デバイスコンテキストへのハンドル</param>
        /// <param name="hgdiobj">選択するオブジェクトへのハンドル</param>
        /// <returns></returns>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);


        /// <summary>
        /// 指定されたウィンドウのクライアント領域または画面全体のデバイスコンテキスト（DC）へのハンドルを取得する
        /// </summary>
        /// <param name="hWnd">デバイスコンテキストを取得するウィンドウへのハンドル。nullの場合画面全体のDC</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);


        /// <summary>
        /// 指定されたウィンドウの外接する四角形の寸法を取得する。寸法は画面の左上隅を基準にした画面座標で指定する
        /// </summary>
        /// <param name="hWnd">ウィンドウへのハンドル</param>
        /// <param name="lpRect">ウィンドウの左隅と右下隅の画像座標を受け取るRect構造体へのポインタ</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);


        /// <summary>
        /// デバイスコンテキストを解放する
        /// </summary>
        /// <param name="hwnd">開放されるウィンドウへのハンドル</param>
        /// <param name="hdc">開放されるデバイスコンテキストへのハンドル</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);


        /// <summary>
        /// アクティブウィンドウのハンドルを取得する
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();


        /// <summary>
        /// ウィンドウ領域のデバイスコンテキストを取得する
        /// </summary>
        /// <param name="hwnd">対象デバイスコンテキストを含むウィンドウへのハンドル。nullの場合、画面全体のデバイスコンテキストを取得する</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);


        /// <summary>
        /// 指定した矩形との交差の最大面積を有するディスプレイモニタへのハンドルを取得する
        /// </summary>
        /// <param name="lprc">仮想画面座標での対象の長方形を指定するRect構造体へのポインタ</param>
        /// <param name="dwFlags">四角形がディスプレイモニタと交差しない場合の関数の戻り地を決定する</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromRect(Rect lprc, MonitorOptions dwFlags);


        /// <summary>
        /// 指定されたウィンドウのクライアント領域を更新する。
        /// </summary>
        /// <param name="hWnd">更新するウィンドウへのハンドル</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);


        /// <summary>
        /// 指定されたウィンドウの祖先のハンドルを取得
        /// </summary>
        /// <param name="hwnd">取得対象のウィンドウへのハンドル</param>
        /// <param name="flags">取得するハンドルのパラメータ</param>
        /// <returns></returns>
        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);


        /// <summary>
        /// 指定されたウィンドウの更新領域に四角形を追加する
        /// </summary>
        /// <param name="hWnd">更新領域が変更されたウィンドウへのハンドル。NULLの場合全てのウィンドウを無効にして再描画する</param>
        /// <param name="lpRect">領域に追加される長方形のクライアント座標を含むRect構造体へのポインタ</param>
        /// <param name="bErase">更新領域の処理時に更新領域内の背景を消去するかどうかを指定</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);


        /// <summary>
        /// 指定されたウィンドウをペイント用に準備し、PAINTSTRUCT構造体にペイントに関する情報を入力する
        /// </summary>
        /// <param name="hwnd">再描画されるウィンドウへのハンドル</param>
        /// <param name="lpPaint">ペイント情報を受け取るPAINTSTRUCT構造体へのポインタ</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);


        /// <summary>
        /// BeginPaint関数の呼び出しごとに必要。ペイント完了後に呼び出す
        /// </summary>
        /// <param name="hWnd">再描画されたウィンドウのハンドル</param>
        /// <param name="lpPaint">BeginPaintによって取得された描画情報を含むPAINTSTRUCT構造体へのポインター</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);


        /// <summary>
        /// ウィンドウに適用されている指定されたデスクトップウィンドウマネージャー（DWM）属性の現在の値を取得する
        /// </summary>
        /// <param name="hwnd">属性値を取得するウィンドウへのハンドル</param>
        /// <param name="dwmWindowAttribute">取得する値を説明するフラグ</param>
        /// <param name="pvAttribute"></param>
        /// <param name="cbAttribute"></param>
        /// <returns></returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwmWindowAttribute, out bool pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwmWindowAttribute, out Rect pvAttribute, int cbAttribute);


        /// <summary>
        /// デスクトップウィンドウマネージャー（DWM）コンポジションが有効かどうかを示す値を取得する
        /// </summary>
        /// <returns></returns>
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();


        /// <summary>
        /// デスクトップウィンドウマネージャー（DWM）の非クライアントレンダリングのウィンドウ属性を指定するために使用するフラグ
        /// </summary>
        public enum DwmWindowAttribute : int
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }


        /// <summary>
        /// アプリケーションのための情報が含まれているこの情報は、そのアプリケーションが所有するウィンドウのクライアント領域を描画するために使用される
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public Rect rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
        }


        public const uint PW_CLIENTONLY = 0x1;

        /// <summary>
        /// GetAncestorで使用する。
        /// </summary>
        public enum GetAncestorFlags
        {
            GetParent = 1,      // 親ウィンドウを取得する。GetParent関数の場合とは異なり、これには所有者は含まれない。
            GetRoot = 2,        // 親ウィンドウのチェーンをたどることによってルートウィンドウを取得する。
            GetRootOwner = 3    // GetParentによって返された親ウィンドウと所有者ウィンドウのチェーンをたどることによって、所有されているルートウィンドウを取得する
        }


        /// <summary>
        /// 指定するウィンドウ情報
        /// </summary>
        public enum WindowInfoIndex : int
        {
            GWL_EXSTYLE = -20,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
        }


        /// <summary>
        /// 描画オプション
        /// </summary>
        [Flags]
        internal enum DrawingOptions
        {
            PRF_CHECKVISIBLE = 0x01,    // ウィンドウが表示されている場合のみ描画する
            PRF_NONCLIENT = 0x02,       // 表示されている全ての小ウィンドウを描画する
            PRF_CLIENT = 0x04,          // ウィンドウのクライアント領域を描画する
            PRF_ERASEBKGND = 0x08,      // ウィンドウを描画する前に背景を消去する
            PRF_CHILDREN = 0x10,        // ウィンドウの非クライアント領域を描画する
            PRF_OWNED = 0x20            // 所有している全てのウィンドウを描画する
        }


        /// <summary>
        /// ラスタ操作コード。これらのコードは、最終的な色を実現するためにソース長方形のカラーデータを
        /// 宛先長方形のカラーデータとどのように組み合わせるかを定義する
        /// </summary>
        public enum RasterOperation : int
        {
            SRCCOPY = 13369376,
            CAPTUREBLT = 1073741824
        }


        /// <summary>
        /// フックタイプ
        /// </summary>
        public enum HookType : int
        {
            WH_MSGFILTER = -1,
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14,
        }


        /// <summary>
        /// WindowsAPIのメッセージ
        /// </summary>
        public enum WindowsMessage : int
        {
            WM_PAINT = 0x000F,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318
        }


        /// <summary>
        /// ディスプレイモニタに関する情報
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfoEx
        {
            public int Size;
            public Rect Monitor;
            public Rect WorkArea;
            public int Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
        }


        /// <summary>
        /// 仮想画面座標内の対象ポイントを指定するPOINT構造
        /// </summary>
        public enum MonitorOptions : int
        {
            MONITOR_DEFAULTTONULL = 0x00000000,     // NULLを 返す
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,  // プライマリディスプレイモニターのハンドルを返す
            MONITOR_DEFAULTTONEAREST = 0x00000002   // ポイントに最も近いディスプレイモニターのハンドルを返す
        }


        /// <summary>
        /// 四角形の構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left, Top, Right, Bottom;

            public Rect(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }

            public Rect(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return this.Left; }
                set { this.Right -= (this.Left - value); this.Left = value; }
            }

            public int Y
            {
                get { return this.Top; }
                set { this.Bottom -= (this.Top - value); this.Top = value; }
            }

            public int Height
            {
                get { return this.Bottom - this.Top; }
                set { this.Bottom = value + this.Top; }
            }

            public int Width
            {
                get { return this.Right - this.Left; }
                set { this.Right = value + this.Left; }
            }

            public Point Location
            {
                get { return new Point(this.Left, this.Top); }
                set { this.X = value.X; this.Y = value.Y; }
            }

            public Size Size
            {
                get { return new Size(this.Width, this.Height); }
                set { this.Width = value.Width; this.Height = value.Height; }
            }

            public static implicit operator Rectangle(Rect r)
            {
                return new Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator Rect(Rectangle r)
            {
                return new Rect(r);
            }

            public static bool operator ==(Rect r1, Rect r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(Rect r1, Rect r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(Rect r)
            {
                return r.Left == this.Left && r.Top == this.Top && r.Right == this.Right && r.Bottom == this.Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is Rect)
                    return Equals((Rect)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new Rect((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", this.Left, this.Top, this.Right, this.Bottom);
            }
        }
    }
}
