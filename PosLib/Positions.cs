using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;


// TODO::Check these: throw new NotImplementedException();

namespace PosLib
{
    public class Positions
    {
        public enum WindowState
        {
            StateMaximized,
            StateNormal,
            StateMinimized
        }

        private static List<Form> forms = new List<Form>();
        private static List<Window> windows = new List<Window>();

        private static void SavePosition(Form form)
        {

        }    

        public static void Add(Window window)
        {
            window.Loaded += Window_Loaded;
            window.Closed += Window_Closed;
        }

        public static void Add(Form form)
        {
            form.Load += Form_Load;
            form.FormClosed += Form_FormClosed;
        }

        public bool IsOnScreen(Form form)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point topLeft = new System.Drawing.Point(form.Left, form.Top);

                if (screen.WorkingArea.Contains(topLeft))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsOnScreen(Window window)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point topLeft = new System.Drawing.Point((int)window.Left, (int)window.Top);

                if (screen.WorkingArea.Contains(topLeft))
                {
                    return true;
                }
            }

            return false;
        }

        static void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void Form_Load(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        static void Window_Closed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void SetScreenChangeHandler()
        {
            SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        }

        public static Screen GetCurrent(Form form)
        {
            return Screen.FromControl(form);
        }

        public static Screen GetCurrent(System.Drawing.Point p)
        {
            return Screen.FromPoint(p);
        }

        public static Screen GetCurrent(System.Windows.Point p)
        {
            return Screen.FromPoint(new System.Drawing.Point((int)p.X, (int)p.Y));            
        }

        public static Screen GetCurrent(Rectangle r)
        {
            return Screen.FromRectangle(r);
        }

        public static Screen GetCurrent(System.Windows.Rect r)
        {
            return Screen.FromRectangle(new Rectangle((int)r.Left, (int)r.Top, (int)r.Width, (int)r.Height));
        }

        public static Screen GetCurrent(IntPtr hwnd)
        {
            return Screen.FromHandle(hwnd);
        }

        public static Screen GetCurrent(Window window)
        {
            return GetCurrent(new System.Windows.Interop.WindowInteropHelper(window).Handle);
        }

        public static WindowState GetWindowState(Form form)
        {
            switch (form.WindowState)
            {
                case FormWindowState.Maximized:
                    return WindowState.StateMaximized;
                case FormWindowState.Minimized:
                    return WindowState.StateMinimized;
                case FormWindowState.Normal:
                    return WindowState.StateNormal;
            }
            return WindowState.StateNormal;
        }

        public static WindowState GetWindowState(Window window)
        {
            switch (window.WindowState)
            {
                case System.Windows.WindowState.Maximized:
                    return WindowState.StateMaximized;
                case System.Windows.WindowState.Minimized:
                    return WindowState.StateMinimized;
                case System.Windows.WindowState.Normal:
                    return WindowState.StateNormal;
            }
            return WindowState.StateNormal;
        }


        static void DisplaySettingsChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        public static void ReleaseScreenChangeHandler()
        {
            SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;
        }
    }
}
