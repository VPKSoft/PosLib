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

namespace VPKSoft.PosLib
{
    /// <summary>
    /// A class holding a (default) size in X, Y, Width and Height values.
    /// </summary>
    public class DefaultSize
    {
        /// <summary>
        /// The x-coordinate.
        /// </summary>
        private int x;

        /// <summary>
        /// The y-coordinate.
        /// </summary>
        private int y;

        /// <summary>
        /// The width value of the size.
        /// </summary>
        private int width;

        /// <summary>
        /// The height value of the size.
        /// </summary>
        private int height;

        /// <summary>
        /// A constructor for empty DefaultSize.
        /// </summary>
        public DefaultSize()
        {
            this.x = 0;
            this.y = 0;
            this.width = 0;
            this.height = 0;
        }

        /// <summary>
        /// A constructor with given dimensions.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="width">The width value of the size.</param>
        /// <param name="height">The height value of the size.</param>
        public DefaultSize(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// A constructor with given dimensions as double values
        /// <para/>which will be truncated to integer values.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="width">The width value of the size.</param>
        /// <param name="height">The height value of the size.</param>
        public DefaultSize(double x, double y, double width, double height)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.width = (int)width;
            this.height = (int)height;
        }

        /// <summary>
        /// Converts the size this class instance holds to a WinForms Form suitable boundaries.
        /// </summary>
        /// <returns>A System.Drawing.Rectangle class instance converted from this class instance.</returns>
        public System.Drawing.Rectangle ToFormBounds()
        {
            return new System.Drawing.Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Converts the size this class instance holds to a WPF Window suitable boundaries.
        /// </summary>
        /// <returns>A System.Windows.Rect class instance converted from this class instance.</returns>
        public Rect ToWindowBounds()
        {
            return new Rect(x, y, width, height);
        }

        /// <summary>
        /// Converts the size this class instance holds to a string value which can be
        /// <para/>converted back with the static methdod called Parse.
        /// </summary>
        /// <returns>The size in string format. For example [X=609, Y=601,Width=691,Height=431].</returns>
        public override string ToString()
        {
            return "X=" + x + ", Y=" + y + ",Width=" + width + ",Height=" + height;
        }

        /// <summary>
        /// Converts a string presentation of the size to a
        /// <para/>new instance of this class.
        /// </summary>
        /// <param name="s">A string to parse.</param>
        /// <returns>An instance of this class with a size in the given string.</returns>
        public static DefaultSize Parse(string s)
        {
            s = s.Replace("X=", string.Empty).Replace("Y=", string.Empty).Replace("Width=", string.Empty).Replace("Height=", string.Empty);
            s = s.Trim(' ');

            string[] dimensions = s.Split(',');
            return new DefaultSize(int.Parse(dimensions[0]),
                                   int.Parse(dimensions[1]),
                                   int.Parse(dimensions[2]),
                                   int.Parse(dimensions[3]));
        }

        /// <summary>
        /// Gets a value indicating if the size is empty (x, y, width and height are all 0-valued).
        /// </summary>
        /// <returns>True if the size is empty, otherwise false.</returns>
        public bool IsEmpty()
        {
            return x == 0 && y == 0 && width == 0 && height == 0;
        }

        /// <summary>
        /// Same as the Parse method, except there is no exception
        /// <para/>thrown if the size conversion from string failed.
        /// </summary>
        /// <param name="s">A string to parse.</param>
        /// <param name="result">An instance of this class with a size in the given string or null in case of failure.</param>
        /// <returns>True if the operation succeeded, otherwise false.</returns>
        public static bool TryParse(string s, out DefaultSize result)
        {
            result = null;
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// A constructor that initializes this class with WinForms Form RestoreBounds property.
        /// </summary>
        /// <param name="form">A System.Windows.Form class instance.</param>
        public DefaultSize(Form form)
        {
            this.x = form.RestoreBounds.X;
            this.y = form.RestoreBounds.Y;
            this.width = form.RestoreBounds.Width;
            this.height = form.RestoreBounds.Height;
        }

        /// <summary>
        /// A constructor that initializes this class with WPF Window RestoreBounds property.
        /// </summary>
        /// <param name="window">A System.Windows.Window class instance.</param>
        public DefaultSize(Window window)
        {
            this.x = (int)window.RestoreBounds.X;
            this.y = (int)window.RestoreBounds.Y;
            this.width = (int)window.RestoreBounds.Width;
            this.height = (int)window.RestoreBounds.Height;
        }
    }
}
