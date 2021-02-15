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
using System.Windows;


namespace VPKSoft.PosLib
{
    /// <summary>
    /// A class containing some extension methods for System.Windows.Window
    /// <para/>and System.Windows.FrameworkElement.
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// Sets a Window size to a given Rect structure.
        /// </summary>
        /// <param name="wnd">A System.Windows.Window class instance.</param>
        /// <param name="size">A System.Windows.Rect structure describing the Window size.</param>
        public static void SetSize(this Window wnd, Rect size)
        {
            wnd.Top = size.Top;
            wnd.Left = size.Left;
            wnd.Height = size.Height;
            wnd.Width = size.Width;
        }

        /// <summary>
        /// Gets a Window size as a Rect structure.
        /// </summary>
        /// <param name="wnd">A System.Windows.Window class instance.</param>
        /// <returns>A System.Windows.Rect structure describing the Window size.</returns>
        public static Rect GetSize(this Window wnd)
        {
            return new Rect(wnd.Left, wnd.Top, wnd.Width, wnd.Height);
        }

        /// <summary>
        /// Gets a "name" to reference a DependencyObject class instance with.
        /// </summary>
        /// <param name="obj">A DependencyObject class instance which "name" to get.</param>
        /// <returns>A string to refrerence a given DependencyObject class instance with.
        /// <para/>A string reference is gotten in the following order: Uid, Name, Tag (Tag=Name=...).
        /// <para/>All mentioned members being empty an empty string is returned.</returns>
        public static string GetRefName(this DependencyObject obj)
        {
            if (obj is FrameworkElement)
            {
                return (((FrameworkElement)obj).GetRefName());
            }

            return PositionCore.TryGetNameWPF(obj, string.Empty);
        }

        /// <summary>
        /// Gets a "name" to reference a FrameworkElement class instance with.
        /// </summary>
        /// <param name="element">A FrameworkElement class instance which "name" to get.</param>
        /// <returns>A string to refrerence a given FrameworkElement class instance with.
        /// <para/>A string reference is gotten in the following order: Uid, Name, Tag (Tag=Name=...).
        /// <para/>All mentioned members being empty an empty string is returned.</returns>
        public static string GetRefName(this FrameworkElement element)
        {
            string name = element.Uid == string.Empty ? element.Name : element.Uid;
            if (name == string.Empty && element.Tag != null)
            {
                string tmpName = element.Tag.ToString();
                if (tmpName.StartsWith("Name="))
                {
                    name = tmpName.Replace("Name=", string.Empty);
                }
            }
            return name;
        }
    }
}
