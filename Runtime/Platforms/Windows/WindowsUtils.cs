#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
#define WINDOWS
#endif // UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PKGE
{
    public class WindowsUtils
    {
        //https://github.com/Unity-Technologies/FPSSample/blob/6b8b27aca3690de9e46ca3fe5780af4f0eff5faa/Assets/Scripts/Utils/WindowsUtil.cs
        #region FPSSample
#if WINDOWS
        [DllImport("user32.dll", EntryPoint = nameof(SetWindowPos))]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);

        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect"/>
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref LpRect rectangle);
#endif // WINDOWS

        [Conditional("WINDOWS")]
        public static void GetProcessWindows(int processId, List<IntPtr> output)
        {
#if WINDOWS
            IntPtr winPtr = IntPtr.Zero;

            do
            {
                winPtr = FindWindowEx(IntPtr.Zero, winPtr, windowClass: null, windowTitle: null);
                _ = GetWindowThreadProcessId(winPtr, out int id);

                if (id == processId)
                    output.Add(winPtr);

            } while (winPtr != IntPtr.Zero);
#endif // WINDOWS
        }

#if WINDOWS
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/windef/ns-windef-rect"/>
        /// <summary>
        /// The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct LpRect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        private static LpRect RectIntToLpRect(RectInt rectInt) => new LpRect{
            Left = rectInt.xMin,
            Top = rectInt.yMin,
            Right = rectInt.xMax,
            Bottom = rectInt.yMax,
        };

        private static RectInt LpRectToRectInt(LpRect lpRect) => new RectInt(
            xMin: lpRect.Left,
            yMin: lpRect.Top,
            width: lpRect.Right - lpRect.Left,
            height: lpRect.Bottom - lpRect.Top
        );

        private static void GetProcessRect(Process process, ref LpRect rect, ref bool gotProcessRect)
        {
            gotProcessRect = false;

            using var _0 = UnityEngine.Pool.ListPool<IntPtr>.Get(out var winPtrs);
            GetProcessWindows(process.Id, winPtrs);

            for (int i = 0; i < winPtrs.Count; i++)
            {
                bool gotRect = GetWindowRect(winPtrs[i], ref rect);
                if (gotRect && rect.Left != 0 && rect.Top != 0)
                {
                    gotProcessRect = true;
                    break;
                }
            }
        }
#endif // WINDOWS

        [Conditional("WINDOWS")]
        public static void GetProcessRect(Process process, ref RectInt rectInt, ref bool gotProcessRect)
        {
#if WINDOWS
            var rect = RectIntToLpRect(rectInt);

            GetProcessRect(process, ref rect, ref gotProcessRect);

            //rectInt.SetMinMax(new Vector2Int(rect.Left, rect.Top), new Vector2Int(rect.Right, rect.Bottom));
            rectInt = LpRectToRectInt(rect);
#endif // WINDOWS
        }

        [Conditional("WINDOWS")]
        public static void SetWindowPosition(int x, int y, int sizeX = 0, int sizeY = 0)
        {
#if WINDOWS
            var process = Process.GetCurrentProcess();
            process.Refresh();

            _ = EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                _ = GetWindowThreadProcessId(wnd, out int id);

                if (id == process.Id)
                {
                    _ = SetWindowPos(wnd, 0, x, y, sizeX, sizeY, sizeX * sizeY == 0 ? 1 : 0);
                    return false;
                }

                return true;
            }, IntPtr.Zero);
#endif // WINDOWS
        }
        #endregion // FPSSample
    }
}
