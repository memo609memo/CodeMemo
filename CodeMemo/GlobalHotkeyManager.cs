using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Diagnostics;

namespace CodeMemo
{
    public class GlobalHotkeyManager
    {
        private const int MOD_ALT = 0x1;
        private const int MOD_CONTROL = 0x2;
        private const int MOD_SHIFT = 0x4;
        private const int MOD_WIN = 0x8;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        private const int INPUT_KEYBOARD = 1;
        private const int KEYEVENTF_KEYUP = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        private IntPtr _windowHandle;
        private HwndSource _source;

        public GlobalHotkeyManager(Window window)
        {
            _windowHandle = new WindowInteropHelper(window).Handle;
            if (_windowHandle == IntPtr.Zero)
            {
                throw new ArgumentException("Hwnd of zero is not valid.");
            }

            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
        }

        public void RegisterHotKey(int id, ModifierKeys modifiers, Key key)
        {
            uint mod = 0;
            if (modifiers.HasFlag(ModifierKeys.Alt)) mod |= MOD_ALT;
            if (modifiers.HasFlag(ModifierKeys.Control)) mod |= MOD_CONTROL;
            if (modifiers.HasFlag(ModifierKeys.Shift)) mod |= MOD_SHIFT;
            if (modifiers.HasFlag(ModifierKeys.Windows)) mod |= MOD_WIN;

            bool result = RegisterHotKey(_windowHandle, id, mod, (uint)KeyInterop.VirtualKeyFromKey(key));
            Debug.WriteLine($"RegisterHotKey: id={id}, modifiers={modifiers}, key={key}, result={result}");
        }

        public void UnregisterHotKey(int id)
        {
            bool result = UnregisterHotKey(_windowHandle, id);
            Debug.WriteLine($"UnregisterHotKey: id={id}, result={result}");
        }

        private IntPtr HwndHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                Debug.WriteLine($"Hotkey pressed: id={id}");
                OnHotKeyPressed(id);
                handled = true;
            }

            return IntPtr.Zero;
        }

        public event Action<int> HotKeyPressed;

        protected virtual void OnHotKeyPressed(int id)
        {
            HotKeyPressed?.Invoke(id);
        }

        public async void InsertTextBlock(string text)
        {
            Debug.WriteLine($"InsertTextBlock called with text: {text}");

            // Convert text to uppercase
            text = text.ToUpper();

            // Introduce a delay before starting the process
            await Task.Delay(200); // 200 milliseconds delay

            // Get the handle of the currently active window
            IntPtr activeWindowHandle = GetForegroundWindow();
            Debug.WriteLine($"Active window handle: {activeWindowHandle}");

            // Bring the target window (e.g., Notepad) to the foreground
            if (activeWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(activeWindowHandle);
                Thread.Sleep(100);
            }

            foreach (char c in text)
            {
                ushort vkCode = (ushort)KeyInterop.VirtualKeyFromKey(KeyInterop.KeyFromVirtualKey((int)c));
                INPUT[] inputs = new INPUT[2];

                inputs[0].type = INPUT_KEYBOARD;
                inputs[0].u.ki.wVk = vkCode;
                inputs[0].u.ki.wScan = 0;
                inputs[0].u.ki.dwFlags = 0;
                inputs[0].u.ki.time = 0;
                inputs[0].u.ki.dwExtraInfo = IntPtr.Zero;

                inputs[1].type = INPUT_KEYBOARD;
                inputs[1].u.ki.wVk = vkCode;
                inputs[1].u.ki.wScan = 0;
                inputs[1].u.ki.dwFlags = KEYEVENTF_KEYUP;
                inputs[1].u.ki.time = 0;
                inputs[1].u.ki.dwExtraInfo = IntPtr.Zero;

                Debug.WriteLine($"Sending input: {c} (vkCode: 0x{vkCode:X})");
                uint result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
                if (result == 0)
                {
                    uint error = GetLastError();
                    Debug.WriteLine($"SendInput failed with error code: {error}");
                }
                else
                {
                    Debug.WriteLine($"SendInput result: {result}");
                }
            }
        }
    }
}
