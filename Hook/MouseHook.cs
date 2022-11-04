using CaptureStacker.Win32API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CaptureStacker.Hook
{
    public static class MouseHook
    {
        /// <summary>
        /// フックプロシージャのハンドル
        /// </summary>
        private static IntPtr handle;

        /// <summary>
        /// マウスのグローバルフックを実行しているかどうかを取得、設定する
        /// </summary>
        public static bool IsHooking
        {
            get;
            private set;
        }


        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr WindowFromPoint(Point point);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        /// <summary>
        /// 自身のハンドルにマウス入力をフックする
        /// </summary>
        /// <param name="hookProc">フックプロシージャに渡すコールバック関数</param>
        public static void SetHook( NativeAPIUtility.HOOKPROC hookProc)
        {
            if (IsHooking)
            {
                return;
            }

            IsHooking = true;

            IntPtr hmodule = NativeAPIUtility.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            handle = NativeAPIUtility.SetWindowsHookEx((int)NativeAPIUtility.HookType.WH_MOUSE_LL, hookProc, hmodule, IntPtr.Zero);

            // 失敗した場合はWin32エラーを投げる
            if(handle == IntPtr.Zero)
            {
                IsHooking = false;

                throw new System.ComponentModel.Win32Exception();
            }
        }


        public static IntPtr CallNextHookEx(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return NativeAPIUtility.CallNextHookEx(handle, nCode, wParam, lParam);
        }


        /// <summary>
        /// キーボードのグローバルフックを停止し、フックプロシージャをフックチェーン内から削除する
        /// </summary>
        public static void Stop()
        {
            if (!IsHooking)
            {
                return;
            }

            if (handle != IntPtr.Zero)
            {
                IsHooking = false;
                NativeAPIUtility.UnhookWindowsHookEx(handle);
                handle = IntPtr.Zero;
            }
        }


        /// <summary>
        /// 挙動の列挙型
        /// </summary>
        public enum MouseHookStructFlags : uint
        {
            WM_LBUTTONDOWN = 0X0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0X0203,
            WM_RBUTTONDOWN = 0X0204,
            WM_RBUTTONUP = 0x0205,
            WM_MOUSEMOVE = 0x0200,
        }


        /// <summary>
        /// x軸、y軸の座標の構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int x;
            public int y;
        }


        /// <summary>
        /// マウス入力イベントの構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHookStruct
        {
            public Point pt;
            public int mouseData;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }
    }
}
