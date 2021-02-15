#region License
/*
PosLib

A library for storing and loading application's window positioning.
Copyright (C) 2018 VPKSoft, Petteri Kautonen

Contact: vpksoft@vpksoft.net

This file is part of PosLib.

PosLib is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PosLib is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with PosLib.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;

namespace VPKSoft.PosLib
{
    /// <summary>
    /// Utilities for to detect visual objects current
    /// <para/>screen or screen index. 
    /// <para/>Some detection methods as "is something" on some screen are also included.
    /// </summary>
    public class ScreenUtils
    {
        /// <summary>
        /// Gets a value indicating if the form is on a screen.
        /// <para/>States Maximized and Minimized are considered to be true.
        /// </summary>
        /// <param name="form">A form to check.</param>
        /// <returns>True if the form is on a screen, otherwise false.</returns>
        public static bool IsOnScreen(Form form)
        {
            if (form.WindowState == FormWindowState.Maximized ||
                form.WindowState == FormWindowState.Minimized)
            {
                return true;
            }
            return GetScreenOn(form) == null ? false : true;
        }

        /// <summary>
        /// Gets a value indicating if the window is on a screen.
        /// <para/>States Maximized and Minimized are considered to be true.
        /// </summary>
        /// <param name="window">A window to check.</param>
        /// <returns>True if the window is on a screen, otherwise false.</returns>
        public bool IsOnScreen(Window window)
        {
            Screen[] screens = Screen.AllScreens;

            if (window.WindowState == WindowState.Maximized ||
                window.WindowState == WindowState.Minimized)
            {
                return true;
            }

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

        /// <summary>
        /// Gets a value indicating if the form is completely inside a screen boundaries.
        /// <para/>States Maximized and Minimized are considered to be true.
        /// </summary>
        /// <param name="form">A form to check.</param>
        /// <returns>True if the form is in a screen, otherwise false.</returns>
        public static bool IsInScreen(Form form)
        {
            if (form.WindowState == FormWindowState.Maximized ||
                form.WindowState == FormWindowState.Minimized)
            {
                return true;
            }
            return GetScreenIn(form) == null ? false : true;
        }

        /// <summary>
        /// Gets the screen the form is currently on.
        /// </summary>
        /// <param name="form">A form to check.</param>
        /// <returns>The screen the form is currently on
        /// <para/>or null if the form isn't currently on any screen.</returns>
        public static Screen GetScreenOn(Form form)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point topLeft = new System.Drawing.Point(form.Left, form.Top);

                if (screen.WorkingArea.Contains(topLeft))
                {
                    return screen;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the screen the form is currently on or Screen.PrimaryScreen 
        /// <para/>if the form isn't currently on any screen.
        /// </summary>
        /// <param name="form">A form to check.</param>
        /// <returns>The screen the form is currently on
        /// <para/>or Screen.PrimaryScreen if the form isn't currently on any screen.</returns>  
        public static Screen GetScreenOnDefault(Form form)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point topLeft = new System.Drawing.Point(form.Left, form.Top);

                if (screen.WorkingArea.Contains(topLeft))
                {
                    return screen;
                }
            }

            return Screen.PrimaryScreen;
        }

        /// <summary>
        /// Gets the screen the window is currently on or Screen.PrimaryScreen 
        /// <para/>if the window isn't currently on any screen.
        /// </summary>
        /// <param name="window">A window to check.</param>
        /// <returns>The screen the window is currently on
        /// <para/>or Screen.PrimaryScreen if the window isn't currently on any screen.</returns>  
        public static Screen GetScreenOnDefault(Window window)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point topLeft = new System.Drawing.Point((int)window.Left, (int)window.Top);

                if (screen.WorkingArea.Contains(topLeft))
                {
                    return screen;
                }
            }

            return Screen.PrimaryScreen;
        }


        /// <summary>
        /// Returns a current screen for a given IntPtr handle.
        /// </summary>
        /// <param name="hwnd">An IntPtr handle for a visual object which screen to get.</param>
        /// <returns>A screen the object is currently on.</returns>
        public static Screen GetCurrent(IntPtr hwnd)
        {
            return Screen.FromHandle(hwnd);
        }

        /// <summary>
        /// Returns a current screen for a given form.
        /// </summary>
        /// <param name="form">A form which screen to get.</param>
        /// <returns>A screen the form is currently on.</returns>
        public static Screen GetCurrent(Form form)
        {
            return Screen.FromControl(form);
        }

        /// <summary>
        /// Returns a current screen for a given window.
        /// </summary>
        /// <param name="window">A window which screen to get.</param>
        /// <returns>A screen the window is currently on.</returns>
        public static Screen GetCurrent(Window window)
        {
            return GetCurrent(new System.Windows.Interop.WindowInteropHelper(window).Handle);
        }

        /// <summary>
        /// Returns a current screen for a given point.
        /// </summary>
        /// <param name="p">A System.Drawing.Point which screen to get.</param>
        /// <returns>A screen the given point is currently on.</returns>
        public static Screen GetCurrent(System.Drawing.Point p)
        {
            return Screen.FromPoint(p);
        }

        /// <summary>
        /// Returns a current screen for a given point.
        /// </summary>
        /// <param name="p">A System.Windows.Point which screen to get.</param>
        /// <returns>A screen the given point is currently on.</returns>
        public static Screen GetCurrent(System.Windows.Point p)
        {
            return Screen.FromPoint(new System.Drawing.Point((int)p.X, (int)p.Y));
        }

        /// <summary>
        /// Returns a current screen for a given rectangle.
        /// </summary>
        /// <param name="r">A System.Drawing.Rectangle which screen to get.</param>
        /// <returns>A screen the given rectangle is currently on.</returns>
        public static Screen GetCurrent(Rectangle r)
        {
            return Screen.FromRectangle(r);
        }

        /// <summary>
        /// Returns a current screen for a given rectangle.
        /// </summary>
        /// <param name="r">A System.Windows.Rect which screen to get.</param>
        /// <returns>A screen the given rectangle is currently on.</returns>
        public static Screen GetCurrent(System.Windows.Rect r)
        {
            return Screen.FromRectangle(new Rectangle((int)r.Left, (int)r.Top, (int)r.Width, (int)r.Height));
        }

        /// <summary>
        /// Returns a screen index from a Screen.AllScreens array the given form is currently on.
        /// </summary>
        /// <param name="form">A form which screen index to get.</param>
        /// <returns>An integer index in the Screen.AllScreens array the form is currently on.</returns>
        public static int GetScreenIndex(Form form)
        {
            List<Screen> screens = new List<Screen>(Screen.AllScreens);
            return screens.IndexOf(GetScreenOn(form));
        }

        /// <summary>
        /// Returns a screen index from a Screen.AllScreens array the given window is currently on.
        /// </summary>
        /// <param name="window">A window which screen index to get.</param>
        /// <returns>An integer index in the Screen.AllScreens array the window is currently on.</returns>
        public static int GetScreenIndex(Window window)
        {
            List<Screen> screens = new List<Screen>(Screen.AllScreens);
            return screens.IndexOf(GetScreenOn(window));
        }

        /// <summary>
        /// Gets a screen the given form is currently in.
        /// </summary>
        /// <param name="form">A form which screen to get.</param>
        /// <returns>A screen the form is currently in or null,
        /// <para/>if none of the screens math the given form's boundaries.</returns>
        public static Screen GetScreenIn(Form form)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Rectangle topLeft = new System.Drawing.Rectangle(form.Left, form.Top, form.Width, form.Height);

                if (screen.WorkingArea.Contains(form.Bounds))
                {
                    return screen;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the screen the window is currently on.
        /// </summary>
        /// <param name="window">A window to check.</param>
        /// <returns>The screen the window is currently on
        /// <para/>or null if the window isn't currently on any screen.</returns>
        public static Screen GetScreenOn(Window window)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point topLeft = new System.Drawing.Point((int)window.Left, (int)window.Top);

                if (screen.WorkingArea.Contains(topLeft))
                {
                    return screen;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the system DPI setting.
        /// </summary>
        /// <returns>A PointF structure representing the system DPI setting.</returns>
        public static PointF GetDPI()
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                return new PointF(graphics.DpiX, graphics.DpiY);
            }
        }
    }
}
