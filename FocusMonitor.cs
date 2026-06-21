using System;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;

namespace injection
{
    class FocusMonitor
    {
        private static NotifyIcon try_icon = new NotifyIcon();

        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MAXIMIZE = 3;

        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        public static void MaximizeAndLockConsole()
        {
            IntPtr consoleWindow = GetConsoleWindow();
            ShowWindow(consoleWindow, SW_MAXIMIZE);
            int currentStyle = GetWindowLong(consoleWindow, GWL_STYLE);
            SetWindowLong(consoleWindow, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX);
        }


        public static SpeechSynthesizer voice_notification = new SpeechSynthesizer();
        static FocusMonitor()
        {
            voice_notification.SelectVoice("Microsoft Zira Desktop");
            voice_notification.Volume = 100; //range: 0 - 100
            voice_notification.Rate = -1; // speed range: (-10) to 10

            try_icon.Icon = System.Drawing.SystemIcons.Information;
            try_icon.Visible = true;
        }



        public static void StartMonitoring()
        {


            // 2 = GA_ROOT (Gets the top-level window)
            IntPtr myConsoleRoot = GetAncestor(GetConsoleWindow(), 2);
            //SpeechSynthesizer voice_notification = new SpeechSynthesizer();
            Thread monitorThread = new Thread(() =>
            {
                while (true)
                {
                    IntPtr activeRoot = GetAncestor(GetForegroundWindow(), 2);

                    if (activeRoot != myConsoleRoot)
                    {
                        if (TextManager.is_reading)
                        {
                            try_icon.ShowBalloonTip(
                            1500,
                            "Mind Injection",
                            "Focus lost! Return to your session.",
                            ToolTipIcon.Warning);
                            voice_notification.Speak("Return to Mind Injection");
                        }

                    }
                    Thread.Sleep(500);
                }
            });
            monitorThread.IsBackground = true;
            monitorThread.Start();
        }
    }
}