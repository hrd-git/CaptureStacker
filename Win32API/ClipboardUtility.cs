using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CaptureStacker.Win32API
{
    class ClipboardUtility
    {
        /// <summary>
        /// 指定されたウィンドウを、システムが管理するクリップボード形式のリスナーリストに配置する
        /// </summary>
        /// <param name="hwnd"></param>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void RemoveClipboardFormatListener(IntPtr hwnd);
    }
}
