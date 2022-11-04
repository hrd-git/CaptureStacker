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
    public static class KeyboardHook
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

            handle = NativeAPIUtility.SetWindowsHookEx((int)NativeAPIUtility.HookType.WH_KEYBOARD_LL, hookProc, hmodule, IntPtr.Zero);

            // 失敗した場合はWin32エラーを投げる
            if (handle == IntPtr.Zero)
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
        /// 拡張キーフラグ、イベント挿入フラグ、コンテキストコード、および遷移状態フラグ
        /// </summary>
        [Flags]
        public enum KbHookStructFlags : uint
        {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
        }


        /// <summary>
        /// キーボード入力イベントの構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class KBHookStruct
        {
            public uint vkCode;
            public uint scanCode;
            public KbHookStructFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }
    }
}
