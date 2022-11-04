using CaptureStacker.Entity;
using CaptureStacker.Hook;
using CaptureStacker.Win32API;
using CaptureStacker.WinUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CaptureStacker
{
    public partial class CaptureStackerForm : Form
    {
        // マウスプロシージャ
        private NativeAPIUtility.HOOKPROC mouseProc;

        // キーボードプロシージャ
        private NativeAPIUtility.HOOKPROC keyboardProc;

        // モニタプロシージャ
        private NativeAPIUtility.EnumMonitorsDelegate monitorProc;

        // 各モニター情報
        private List<MonitorInfo> monitorInfo = new List<MonitorInfo>();

        // 全モニターを合わせたwidthとheightの最大値
        private MonitorTotalMaxSize monitorTotalMaxSize = new MonitorTotalMaxSize();

        // hotKey + 左クリックで画面を選択する
        private readonly string hotKey = ConfigurationManager.AppSettings["HotKey"];
        private bool IsHotKey = false;

        private enum HotKeys
        {
            IsHotKy
        }

        // エラーで削除されなかった画像一覧
        private List<int> errorImageList = new List<int>();

        // 画像サムネイル一覧
        private List<Image> thumbnailList = new List<Image>();

        // プレビュー用のピクチャーボックス
        private PictureBox pictureBox = null;

        private int currentPictureIndex = 0;

        // 画像ファイルの総数
        private int imageStorageCount = 0;

        // 画像取得用の枠フレーム
        private FrameForm frame = null;

        // 範囲指定用wrapperForm
        private OverWrapForm overWrap = null;

        // キャプチャモード
        private enum caputreMode
        {
            RANGE,
            WINDOW,
            MONITOR,
            PREVIEW
        }

        // 現在のキャプチャモード
        private caputreMode currentMode;


        public CaptureStackerForm()
        {
            InitializeComponent();
        }


        private void CaptureStackerForm_Load(object sender, EventArgs e)
        {
            // フックプロシージャに渡すコールバック関数
            this.mouseProc = this.MyMouseHookProc;
            this.keyboardProc = this.MyKeyboardHookProc;
            this.monitorProc = this.MyMonitorEnumProc;

            // フックプロシージャをフックチェーン内にインストールする
            MouseHook.SetHook(this.mouseProc);
            KeyboardHook.SetHook(this.keyboardProc);

            // 画像ファイルの総数
            this.imageStorageCount = Directory.GetFiles(ConfigurationManager.AppSettings["ImageStoragePath"], "*", SearchOption.TopDirectoryOnly).Length;

            // 画像ファイル削除処理
            //this.CleanImageStorage();

            this.SetMonitorMaxSize();

            // pictureBox生成
            this.SetPictureBox();

            this.captureMode.Text = this.currentMode.ToString();
        }


        /// <summary>
        /// 自身のプロセスに対するメッセージを受け取る
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int) NativeAPIUtility.WindowsMessage.WM_PRINT:
                    return;
            }
            base.WndProc(ref m);
        }


        #region メソッド

        #region モニター領域画像取得
        /// <summary>
        /// マウス座標が所属するモニター画像を取得
        /// </summary>
        private void GetMonitorImage(int x, int y)
        {
            foreach (MonitorInfo info in this.monitorInfo)
            {
                // 座標のモニターを選択
                int monitorX = info.Location.X + info.Width;
                int monitorY = info.Location.Y + info.Height;

                //座標がモニター内に存在する場合
                if(x >= info.Location.X && x <= monitorX &&
                    y >= info.Location.Y && y <= monitorY)
                {
                    this.SaveImage(info.Width, info.Height, info.Location);

                    return;
                }

            }
        }
        #endregion


        #region ウィンドウ画像取得
        /// <summary>
        /// 対象ハンドルの画面キャプチャを取得する
        /// </summary>
        /// <param name="handle">画像取得対象のハンドル</param>
        private void GetWindowImage(IntPtr hwnd)
        {
            if(hwnd == IntPtr.Zero)
            {
                return;
            }

            NativeAPIUtility.SetForegroundWindow(hwnd);

            NativeAPIUtility.Rect winRect = new NativeAPIUtility.Rect();

            // Aero Glassの設定で取得するサイズを分ける
            if (NativeAPIUtility.DwmIsCompositionEnabled())
            {
                NativeAPIUtility.DwmGetWindowAttribute(hwnd, NativeAPIUtility.DwmWindowAttribute.ExtendedFrameBounds, out winRect, Marshal.SizeOf(typeof(NativeAPIUtility.Rect)));
            }
            else
            {
                NativeAPIUtility.GetWindowRect(hwnd, out winRect);
            }

            if(winRect.Width < 1 || winRect.Height < 1)
            {
                return;
            }

            this.SaveImage(winRect.Width, winRect.Height, new Point(winRect.Left, winRect.Top));
        }
        #endregion


        #region フレーム内画像取得
        /// <summary>
        /// フレーム枠内のキャプチャを取得
        /// </summary>
        private void GetFrameImage()
        {
            if(this.frame == null || this.frame.CorrectWidth < 1 || this.frame.CorrectHeight < 1)
            {
                return;
            }

            this.SaveImage(this.frame.CorrectWidth, this.frame.CorrectHeight, this.frame.CorrectLocation);

            this.frame.Close();
            this.frame = null;
        }
        #endregion


        #region 画像保存
        /// <summary>
        /// 画像を保存する
        /// </summary>
        /// <param name="width">保存対象ウィンドウの横幅</param>
        /// <param name="height">保存対象ウィンドウの高さ</param>
        /// <param name="location">保存対象ウィンドウの座標</param>
        private void SaveImage(int width, int height, Point location)
        {
            using (Bitmap bmp = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hDC = IntPtr.Zero;
                try
                {
                    hDC = g.GetHdc();
                    using (Graphics capture = Graphics.FromImage(bmp))
                    {
                        capture.CopyFromScreen(location, new Point(0, 0), bmp.Size);
                    }

                    string path = ConfigurationManager.AppSettings["ImageStoragePath"];
                    string fileName = ConfigurationManager.AppSettings["FileName"]; ;
                    string extension = ConfigurationManager.AppSettings["Extension"];

                    bmp.Save(path + fileName + (++this.imageStorageCount) + extension);
                }
                finally
                {
                    if (hDC != IntPtr.Zero)
                    {
                        g.ReleaseHdc(hDC);
                    }
                }
            }

        }
        #endregion


        #region トップウィンドウハンドル取得
        /// <summary>
        /// 一番トップの親ウィンドウハンドルを取得
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>親ウィンドウハンドル</returns>
        private IntPtr GetTopParentHandle(IntPtr hwnd)
        {
            if(hwnd == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            IntPtr res = hwnd;
            IntPtr hParent = hwnd;

            while (hParent != IntPtr.Zero)
            {
                res = hParent;
                hParent = NativeAPIUtility.GetParent(res);
            }

            return res;
        }
        #endregion


        #region ウィンドウ枠作成
        /// <summary>
        /// 渡されたウィンドウの枠を表示する
        /// </summary>
        /// <param name="hwind">表示する対象ハンドル</param>
        private void ShowWindowFrame(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            if(this.frame != null)
            {
                if (hwnd == this.frame.Handle)
                {
                    return;
                }

                this.frame.Close();
            }

            this.frame = new FrameForm();

            NativeAPIUtility.Rect winRect = new NativeAPIUtility.Rect();

            // Aero Glassの設定で取得するサイズを分ける
            if (NativeAPIUtility.DwmIsCompositionEnabled())
            {
                NativeAPIUtility.DwmGetWindowAttribute(hwnd, NativeAPIUtility.DwmWindowAttribute.ExtendedFrameBounds, out winRect, Marshal.SizeOf(typeof(NativeAPIUtility.Rect)));
            }
            else
            {
                NativeAPIUtility.GetWindowRect(hwnd, out winRect);
            }

            frame.Size = new Size(winRect.Width + this.frame.frameBorderSize * 2, winRect.Height + this.frame.frameBorderSize * 2);

            frame.Show();

            Point correctLocation = new Point(winRect.Left - this.frame.frameBorderSize, winRect.Top - this.frame.frameBorderSize);

            frame.Location = correctLocation;

            frame.Refresh();
        }
        #endregion


        #region 画像削除
        /// <summary>
        /// 保存した画像を削除する
        /// </summary>
        private void CleanImageStorage()
        {
            string directoryPath = ConfigurationManager.AppSettings["ImageStoragePath"];
            string fileName = ConfigurationManager.AppSettings["FileName"]; ;
            string extension = ConfigurationManager.AppSettings["Extension"];

            for (int i = 0; i < this.imageStorageCount; i++)
            {
                try
                {
                    string filePath = directoryPath + fileName + (i + 1) + extension;
                    FileInfo fi = new FileInfo(filePath);

                    if (fi.Exists)
                    {
                        // 読み取り専用属性の場合、標準属性に変更する
                        if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(filePath, FileAttributes.Normal);
                        }

                        fi.Delete();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(fileName + i + extension + ": ファイルへのアクセスが拒否されました。");
                    this.errorImageList.Add(i);
                    continue;
                }
            }

            this.imageStorageCount = 0;
        }
        #endregion


        #region モニター領域サイズ設定
        /// <summary>
        /// 各モニターのWidthとHeight、全モニターの合計WidthとHeightを設定する
        /// </summary>
        public void SetMonitorMaxSize()
        {
            // 各モニター情報の設定
            NativeAPIUtility.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, this.monitorProc, IntPtr.Zero);

            foreach (MonitorInfo info in this.monitorInfo)
            {
                // 全モニターの合計
                int monitorWidth = info.Location.X + info.Width;
                int monitorHeight = info.Location.Y + info.Height;

                if (this.monitorTotalMaxSize.Width < monitorWidth)
                {
                    this.monitorTotalMaxSize.Width = monitorWidth;
                }

                if(this.monitorTotalMaxSize.Height < monitorHeight)
                {
                    this.monitorTotalMaxSize.Height = monitorHeight;
                }
            }
        }
        #endregion


        #region オーバーラップを開く
        private void ShowOverWrap()
        {
            if (this.overWrap == null)
            {
                this.overWrap = new OverWrapForm(this.monitorTotalMaxSize.Width, this.monitorTotalMaxSize.Height);
                this.overWrap.Show();

                if (this.frame != null)
                {
                    this.frame.TopMost = true;
                }
            }
        }
        #endregion


        #region プレビューを開く
        private void OpenPreview()
        {
            this.Width = Convert.ToInt32(ConfigurationManager.AppSettings["PreviewWidth"]);
            this.Height = Convert.ToInt32(ConfigurationManager.AppSettings["PreviewHeight"]);
            this.TopMost = true;
            this.TopMost = false;

            this.SetThumbnail();
            this.SwhoImageIndex();

        }

        private void SwhoImageIndex()
        {
            int index = this.thumbnailList.Count == 0 ? 0 : this.currentPictureIndex + 1;

            this.PictureIndex.Text = $"{index} / {this.thumbnailList.Count}";
        }
        #endregion


        #region プレビューを閉じる
        private void ClosePreview()
        {
            this.Width = Convert.ToInt32(ConfigurationManager.AppSettings["CaptureStackerWidth"]);
            this.Height = Convert.ToInt32(ConfigurationManager.AppSettings["CaptureStackerHeight"]);
            this.PictureIndex.Text = "";
        }
        #endregion


        #region プレビュー用画像設定
        /// <summary>
        /// 画像フォルダのサムネイルとして保持していない画像を取得する
        /// </summary>
        private void SetThumbnail()
        {
            string directoryPath = ConfigurationManager.AppSettings["ImageStoragePath"];
            string fileName = ConfigurationManager.AppSettings["FileName"]; ;
            string extension = ConfigurationManager.AppSettings["Extension"];

            for (int i = this.thumbnailList.Count; i < this.imageStorageCount; i++)
            {
                string filePath = directoryPath + fileName + (i + 1) + extension;

                if (!File.Exists(filePath))
                {
                    continue;
                }

                using(FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (Bitmap org = new Bitmap(fs))
                    {
                        // widthの比率に合わせてHeightを設定する。
                        int resizeWidth = 500;
                        int resizeHeight = (int)(org.Height * ((double)resizeWidth / (double)org.Width));

                        using (Bitmap bmp = new Bitmap(resizeWidth, resizeHeight))
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(org, 0, 0, resizeWidth, resizeHeight);

                            //補間方法として高品質双三次補間を指定する
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                            this.thumbnailList.Add((Image)bmp.Clone());
                            this.pictureBox.Image = this.thumbnailList[this.currentPictureIndex];
                        }
                    }
                }
            }
        }
        #endregion


        #region ピクチャーボックスの設定
        /// <summary>
        /// ピクチャーボックスの設定
        /// </summary>
        private void SetPictureBox()
        {
            if(this.pictureBox != null)
            {
                return;
            }

            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Location = new Point(30, 50);
            pictureBox.Size = new Size(500, 500);
            pictureBox.Name = "pictureBox";
            this.Controls.Add(pictureBox);
        }
        #endregion

        #endregion


        #region コールバック関数

        #region マウスプロシージャ
        /// <summary>
        /// マウス入力イベントが発生した際に実行するコールバック関数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr MyMouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MouseHook.MSLLHookStruct mouseHookStruct = (MouseHook.MSLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHook.MSLLHookStruct));

            IntPtr hwnd = IntPtr.Zero;
            IntPtr parentHwnd = IntPtr.Zero;

            if (this.IsHotKey)
            {
                    switch (this.currentMode)
                    {
                        // モニタ指定モード
                        case caputreMode.MONITOR:

                            // 左クリックダウンイベント
                            if (wParam == (IntPtr)MouseHook.MouseHookStructFlags.WM_LBUTTONDOWN)
                            {
                                this.GetMonitorImage(mouseHookStruct.pt.x, mouseHookStruct.pt.y);
                            }

                            break;
                        
                        // ウィンドウ指定モード
                        case caputreMode.WINDOW:

                            // 左クリックダウンイベント
                            if (wParam == (IntPtr)MouseHook.MouseHookStructFlags.WM_LBUTTONDOWN)
                            {
                                // クリック時の座標にあるウィンドウのハンドルを取得する
                                hwnd = MouseHook.WindowFromPoint(mouseHookStruct.pt);

                                // 親のウィンドウハンドルを取得
                                parentHwnd = this.GetTopParentHandle(hwnd);

                                this.GetWindowImage(parentHwnd);
                            }

                            break;

                        // 範囲指定モード
                        case caputreMode.RANGE:

                            // 左クリックダウンイベント
                            if (wParam == (IntPtr)MouseHook.MouseHookStructFlags.WM_LBUTTONDOWN)
                            {
                                
                                if (this.frame != null)
                                {
                                    if(this.frame.IsMouseHover(mouseHookStruct.pt.x, mouseHookStruct.pt.y))
                                    {
                                        break;
                                    }

                                    this.frame.Close();
                                }

                                this.frame = new FrameForm();

                                this.frame.mouseDownPoint = new Point(mouseHookStruct.pt.x, mouseHookStruct.pt.y);
                                
                                this.frame.IsView = true;
                            }


                            // 右クリックダウンイベント
                            if(wParam == (IntPtr)MouseHook.MouseHookStructFlags.WM_RBUTTONDOWN)
                            {
                            }


                            // マウスムーブイベント
                            if(wParam == (IntPtr)MouseHook.MouseHookStructFlags.WM_MOUSEMOVE)
                            {
                                if (this.frame == null || !this.frame.IsView)
                                {
                                    break;
                                }

                                this.frame.CreateFrame(mouseHookStruct.pt.x, mouseHookStruct.pt.y);
                            }


                            // 左クリックアップイベント
                            if (wParam == (IntPtr)MouseHook.MouseHookStructFlags.WM_LBUTTONUP)
                            {
                                if(this.frame == null)
                                {
                                    break;
                                }

                                this.frame.IsView = false;
                            }


                            if (wParam == (IntPtr)MouseHook.MouseHookStructFlags.WM_RBUTTONUP)
                            {
                            }

                            break;

                        case caputreMode.PREVIEW:
                            
                            break;
                    }
            }

            return MouseHook.CallNextHookEx(nCode, wParam, lParam);
        }
        #endregion


        #region キーボードプロシージャ
        // キー入力イベントが発生した際に実行するコールバック関数
        private IntPtr MyKeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            KeyboardHook.KBHookStruct kbHookStruct = (KeyboardHook.KBHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHook.KBHookStruct));

            /* キーコードをKwys列挙体で入力キーに変換する */
            string pushKey = ((Keys)kbHookStruct.vkCode).ToString();

            if (wParam == (IntPtr)KeyboardHook.KbHookStructFlags.WM_KEYDOWN)
            {
                if (pushKey.Equals(this.hotKey))
                {
                    this.IsHotKey = true;

                    switch (this.currentMode)
                    {
                        case caputreMode.MONITOR:
                            this.ShowOverWrap();
                            break;

                        case caputreMode.RANGE:
                            this.ShowOverWrap();
                            break;
                    }
                }
            }

            if(wParam == (IntPtr)KeyboardHook.KbHookStructFlags.WM_KEYUP)
            {

                if (pushKey.Equals(hotKey))
                {
                    if (this.overWrap != null)
                    {
                        this.overWrap.Close();
                        this.overWrap = null;
                    }

                    if(this.frame != null && this.currentMode == caputreMode.RANGE)
                    {
                        this.GetFrameImage();
                    }

                    this.IsHotKey = false;
                }

                if (this.IsHotKey)
                {
                    switch (pushKey)
                    {
                        case "D1":
                            this.currentMode = caputreMode.RANGE;
                            this.ClosePreview();
                            break;

                        case "D2":
                            this.currentMode = caputreMode.WINDOW;
                            this.ClosePreview();
                            break;

                        case "D3":
                            this.currentMode = caputreMode.MONITOR;
                            this.ClosePreview();
                            break;

                        case "D4":
                            this.currentMode = caputreMode.PREVIEW;
                            this.OpenPreview();
                            break;

                        // 一つ前の画像を表示する
                        case "Left":
                            if (this.pictureBox != null && this.currentPictureIndex > 0)
                            {
                                this.pictureBox.Image = this.thumbnailList[--this.currentPictureIndex];
                                this.SwhoImageIndex();
                            }
                            break;

                        // 一つ後の画像を表示する
                        case "Right":
                            if (this.pictureBox != null && this.currentPictureIndex < this.thumbnailList.Count - 1)
                            {
                                this.pictureBox.Image = this.thumbnailList[++this.currentPictureIndex];
                                this.SwhoImageIndex();
                            }
                            break;
                    }

                    this.captureMode.Text = this.currentMode.ToString();
                }

            }

            return KeyboardHook.CallNextHookEx(nCode, wParam, lParam);
        }
        #endregion


        #region モニタープロシージャ
        // マルチモニターの各座標を取得する
        private bool MyMonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
        {
            NativeAPIUtility.MonitorInfoEx monitorInfo = new NativeAPIUtility.MonitorInfoEx();
            monitorInfo.Size = Marshal.SizeOf(typeof(NativeAPIUtility.MonitorInfoEx));

            // infoにモニター情報が入る
            NativeAPIUtility.GetMonitorInfo(hMonitor, ref monitorInfo);

            MonitorInfo info = new MonitorInfo(monitorInfo.Monitor.Width, monitorInfo.Monitor.Height, monitorInfo.Monitor.Location);

            this.monitorInfo.Add(info);

            return true;
        }
        #endregion

        #endregion



        // インストールしたフックプロシージャを解放する
        private void CaptureStackerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MouseHook.Stop();
            KeyboardHook.Stop();
        }
    }
}
