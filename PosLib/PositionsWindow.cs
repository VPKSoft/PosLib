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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Controls;
using VPKSoft.Utils;

namespace VPKSoft.PosLib
{
    /// <summary>
    /// A class for saving and loading a WPF Window class instance positioning information.
    /// </summary>    
    public class PositionsWindow: PositionCore
    {
        /// <summary>
        /// Ads a WPF window to the list of windows to which position is to be saved and loaded within the PosLib library.
        /// </summary>
        /// <param name="window">A WPF window to be added to positioning.</param>
        /// <param name="szm">An enumeration to describe how a Form or a Window
        /// <para/>should be resized if it gets out of screen's boundaries.</param>
        public static void Add(Window window, SizeChangeMode szm = SizeChangeMode.MoveTopLeft)
        {
            // Throw an exception if there is nothing to get a handle of the window..
            if (window.GetRefName() == string.Empty)
            {
                throw new Exception("Window must have an Uid (x:Uid) or Tag (Tag=Name=...) or a Name (x:Name).");
            }

            WindowDefaults def = new WindowDefaults(); // With WPF these seem to be useless..
            def.DefaultSizemode = szm;
            def.DefaultSize = new DefaultSize();
            formWindowDefaults.Add(new Tuple<object, Type, WindowDefaults>(window, window.GetType(), def));

            SetResizeEvents(window, true);
        }

        /// <summary>
        /// Saves a window position to a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// </summary>
        /// <param name="window">A Window class instance which position to save.</param>
        private static void SavePosition(Window window)
        {
            Paths.MakeAppSettingsFolder(Misc.AppType.WPF);
            VPKNml position = new VPKNml("position");

            if (File.Exists(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml"))
            {
                position.Load(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml");
            }

            string refName = window.GetRefName();

            try
            {

                position[refName, "bounds"] = window.GetSize().ToString();

                position[refName, "sizeMode"] = formWindowDefaults.Single((f) => f.Item2 == window.GetType()).Item3.DefaultSizemode;
                position[refName, "windowState"] = GetWindowState(window);
                position[refName, "screenIndex"] = ScreenUtils.GetScreenIndex(window);

                List<FrameworkElement> elements = GetUidElements(window);

                foreach (FrameworkElement el in elements)
                {
                    if (SaveWindowProperties.Exists((f) => f.Key == el.GetType().ToString()))
                    {
                        List<KeyValuePair<string, string>> controlProperties = SaveWindowProperties.FindAll((f) => f.Key == el.GetType().ToString());

                        string elementName = el.GetRefName();

                        if (elementName == string.Empty)
                        {
                            continue;
                        }

                        foreach (KeyValuePair<string, string> prop in controlProperties)
                        {
                            object obj = el;
                            if (obj == null)
                            {
                                continue;
                            }

                            try
                            {
                                if (Casts.Exists((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == prop.Value)) // 10 points!
                                {
                                    KeyValuePair<string, TypeCast> cast = Casts.Single((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == prop.Value);

                                    obj = Convert.ChangeType(el.GetType().GetProperty(cast.Value.Property).GetValue(el),
                                            cast.Value.TypeFromName);
                                }
                                else if (Casts.Exists((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == string.Empty))
                                {
                                    KeyValuePair<string, TypeCast> cast = Casts.Single((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == string.Empty);
                                    obj = Convert.ChangeType(el, cast.Value.TypeFromName);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!PositionCore.ExcludeTypeConversionExceptions)
                                {
                                    throw ex;
                                }
                                else
                                {
                                    RaisePoslibException(window, ex);
                                }
                            }

                            if (prop.Value.StartsWith("[]")) // walk through a collection
                            {
                                try
                                {
                                    List<string> collectionProps = new List<string>(prop.Value.TrimStart('[', ']').Split('.'));

                                    try
                                    {
                                        PropertyInfo piList = obj.GetType().GetProperty(collectionProps[0]);
                                        IList propList = (IList)piList.GetValue(obj); // Assume type of IList
                                        TypeConverter converter;
                                        object oValue;
                                        for (int i = 0; i < propList.Count; i++)
                                        {
                                            var cProp = propList[i];
                                            string nString = TryGetNameWPF(cProp, i);
                                            switch (collectionProps.Count)
                                            {
                                                case 1:
                                                    oValue = cProp;
                                                    converter = TypeDescriptor.GetConverter(oValue);
                                                    position[refName + "." + elementName + ".[]" + string.Join(".", collectionProps.ToArray()), nString] = converter.ConvertToString(oValue) + "|" + oValue.GetType().ToString();
                                                    break;
                                                case 2:
                                                    oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                                    converter = TypeDescriptor.GetConverter(oValue);
                                                    position[refName + "." + elementName + ".[]" + string.Join(".", collectionProps.ToArray()), nString] = converter.ConvertToString(oValue) + "|" + oValue.GetType().ToString();
                                                    break;
                                                case 3:
                                                    oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                                    oValue = oValue.GetType().GetProperty(collectionProps[2]).GetValue(oValue);
                                                    converter = TypeDescriptor.GetConverter(oValue);
                                                    position[refName + "." + elementName + ".[]" + string.Join(".", collectionProps.ToArray()), nString] = converter.ConvertToString(oValue) + "|" + oValue.GetType().ToString();
                                                    break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!PositionCore.ExcludeTypeConversionExceptions)
                                        {
                                            throw ex;
                                        }
                                        else
                                        {
                                            RaisePoslibException(window, ex);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (!PositionCore.ExcludeTypeConversionExceptions)
                                    {
                                        throw ex;
                                    }
                                    else
                                    {
                                        RaisePoslibException(window, ex);
                                    }
                                } 
                                continue;
                            }

                            PropertyInfo pi = obj.GetType().GetProperty(prop.Value);
                            object value = pi.GetValue(obj);

                            if (value == null)
                            {
                                continue;
                            }

                            if (EnumerableTypes.Contains(value.GetType()))
                            {
                                position[refName + "." + elementName, prop.Value] = value + "|" + value.GetType().ToString();

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!PositionCore.ExcludeTypeConversionExceptions)
                {
                    throw ex;
                }
                else
                {
                    RaisePoslibException(window, ex);
                }
            }
            position.Save(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml");
        }

        /// <summary>
        /// Deletes a window position data from a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// <para/>Also all event handlers related to saving the window position are detached.
        /// </summary>
        /// <param name="window">A window which position to "reset".</param>
        public static void ResetPosition(Window window)
        {
            ResetPositionData(window);

            LoadPosition(window); // Just a test if the loading would fail on an empty data

            SetResizeEvents(window, false, true);
        }

        /// <summary>
        /// Deletes a window position data from a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// </summary>
        /// <param name="window">A window which position to "reset".</param>
        private static void ResetPositionData(Window window)
        {
            VPKNml position = new VPKNml("position");

            if (File.Exists(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml"))
            {
                position.Load(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml");
            }

            position.DeleteSections(window.Name + "*");

            position.Save(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml");
        }

        /// <summary>
        /// A dispatcher timer event handler that simulates System.Windows.Forms.Form ResizeEnd event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void dt_Tick(object sender, EventArgs e)
        {
            if (ResizeTimers.Exists((t) => t.Value == sender))
            {
                ResizeTimers.Single((t) => t.Value == sender).Value.Stop();
                ResizeFit(ResizeTimers.Single((t) => t.Value == sender).Key);
            }
        }

        /// <summary>
        /// A list of Window-DispatcherTimer pairs to emulate WinForms ResizeEnd event.
        /// </summary>
        private static List<KeyValuePair<Window, DispatcherTimer>> ResizeTimers = new List<KeyValuePair<Window, DispatcherTimer>>();

        /// <summary>
        /// Loads a window position from a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// </summary>
        /// <param name="window">A Window class instance which position to load.</param>
        private static void LoadPosition(Window window)
        {
            if (SkipLoad || SkipLoadScreen)
            {
                ResetPositionData(window);
                return;
            }

            VPKNml position = new VPKNml("position");

            if (File.Exists(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml"))
            {
                position.Load(Paths.GetAppSettingsFolder(Misc.AppType.WPF) + "position.vnml");
            }

            string refName = window.GetRefName();

            try
            {

                if (position[refName, "bounds"] != null)
                {
                    string str = (position[refName, "bounds"].ToString());
                    str = str.Replace(';', ',');
                    window.SetSize(Rect.Parse(str));
                }

                if (position[refName, "windowState"] != null)
                {
                    window.WindowState = ToWindowWindowState((WindowState)Enum.Parse(typeof(WindowState), position[refName, "windowState"].ToString()));
                }



                if (position[refName, "sizeMode"] != null)
                {
                    formWindowDefaults.Single((f) => f.Item2 == window.GetType() && f.Item1 == window).Item3.DefaultSizemode = (SizeChangeMode)Enum.Parse(typeof(SizeChangeMode), position[refName, "sizeMode"].ToString());
                }

                List<FrameworkElement> elements = GetUidElements(window);

                foreach (FrameworkElement el in elements)
                {
                    if (SaveWindowProperties.Exists((f) => f.Key == el.GetType().ToString()))
                    {
                        List<KeyValuePair<string, string>> controlProperties = SaveWindowProperties.FindAll((f) => f.Key == el.GetType().ToString());

                        string elementName = el.GetRefName();

                        if (elementName == string.Empty)
                        {
                            continue;
                        }

                        foreach (KeyValuePair<string, string> prop in controlProperties)
                        {
                            object obj = el;
                            if (obj == null)
                            {
                                continue;
                            }

                            try
                            {
                                if (Casts.Exists((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == prop.Value)) // 10 points!
                                {
                                    KeyValuePair<string, TypeCast> cast = Casts.Single((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == prop.Value);

                                    obj = Convert.ChangeType(el.GetType().GetProperty(cast.Value.Property).GetValue(el),
                                            cast.Value.TypeFromName);
                                }
                                else if (Casts.Exists((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == string.Empty))
                                {
                                    KeyValuePair<string, TypeCast> cast = Casts.Single((t) => t.Key == el.GetType().ToString() && t.Value.OnWindowProperty == string.Empty);
                                    obj = Convert.ChangeType(el, cast.Value.TypeFromName);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!PositionCore.ExcludeTypeConversionExceptions)
                                {
                                    throw ex;
                                }
                                else
                                {
                                    RaisePoslibException(window, ex);
                                }
                            }
                            
                            if (prop.Value.StartsWith("[]"))
                            {
                                try
                                {
                                    List<string> collectionProps = new List<string>(prop.Value.TrimStart('[', ']').Split('.'));

                                    try
                                    {
                                        PropertyInfo piList = obj.GetType().GetProperty(collectionProps[0]);
                                        IList propList = (IList)piList.GetValue(obj); // Assume type of IList
                                        for (int i = 0; i < propList.Count; i++) // TODO::Reference by index (i)
                                        {
                                            var cProp = propList[i];
                                            string nString = TryGetNameWPF(cProp, i);
                                            object oValue;

                                            if (position[refName + "." + elementName + ".[]" + string.Join(".", collectionProps.ToArray()), nString] == null)
                                            {
                                                continue;
                                            }
                                            string valuePart = position[refName + "." + elementName + ".[]" + string.Join(".", collectionProps.ToArray()), nString].ToString().Split('|')[0];
                                            string typePart = position[refName + "." + elementName + ".[]" + string.Join(".", collectionProps.ToArray()), nString].ToString().Split('|')[1];

                                            switch (collectionProps.Count)
                                            {
                                                case 1:
                                                    oValue = cProp;
                                                    break;
                                                case 2:
                                                    {
                                                        oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                                        if (!PositionCore.CustomTypeConverterExists(oValue.GetType()) &&
                                                            !PositionCore.GetTypeDescriptors)
                                                        {
                                                            continue;
                                                        }
                                                        TypeConverter converter = PositionCore.GetCustomTypeConverterOrDefault(oValue);
                                                        cProp.GetType().GetProperty(collectionProps[1]).SetValue(cProp, converter.ConvertFrom(valuePart));
                                                    }
                                                    break;

                                                case 3:
                                                    {

                                                        oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                                        oValue = oValue.GetType().GetProperty(collectionProps[2]).GetValue(oValue);
                                                        if (!PositionCore.CustomTypeConverterExists(oValue.GetType()) &&
                                                            !PositionCore.GetTypeDescriptors)
                                                        {
                                                            continue;
                                                        }
                                                        TypeConverter converter = PositionCore.GetCustomTypeConverterOrDefault(oValue);
                                                        oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                                        oValue.GetType().GetProperty(collectionProps[2]).SetValue(oValue, converter.ConvertFrom(valuePart));
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!PositionCore.ExcludeTypeConversionExceptions)
                                        {
                                            throw ex;
                                        }
                                        else
                                        {
                                            RaisePoslibException(window, ex);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (!PositionCore.ExcludeTypeConversionExceptions)
                                    {
                                        throw ex;
                                    }
                                    else
                                    {
                                        RaisePoslibException(window, ex);
                                    }
                                }
                                continue;
                            }

                            PropertyInfo pi = obj.GetType().GetProperty(prop.Value);
                            object value = pi.GetValue(obj);

                            if (value == null)
                            {
                                continue;
                            }

                            if (EnumerableTypes.Contains(value.GetType()))
                            {
                                if (position[refName + "." + elementName, prop.Value] != null)
                                {
                                    TypeConverter converter = TypeDescriptor.GetConverter(value);
                                    var val = converter.ConvertFrom(position[refName + "." + elementName, prop.Value].ToString().Split('|')[0]);
                                    pi.SetValue(obj, val);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!PositionCore.ExcludeTypeConversionExceptions)
                {
                    throw ex;
                }
                else
                {
                    RaisePoslibException(window, ex);
                }
            }
            ResizeFit(window);
        }

        /// <summary>
        /// A list of type names and their value to be loaded/saved to a position.vnml file.
        /// <remarks>If a string describing a property value starts
        /// <para/>with [] (square brackets) the value is considered as a collection,
        /// <para/>which can be cast to IList interface for enumeration.</remarks>
        /// </summary>
        public static List<KeyValuePair<string, string>> SaveWindowProperties =
        new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] 
        {
            new KeyValuePair<string, string>("System.Windows.Controls.Grid", "[]ColumnDefinitions.Width"),
            new KeyValuePair<string, string>("System.Windows.Controls.Grid", "[]RowDefinitions.Height"),
            new KeyValuePair<string, string>("System.Windows.Controls.ListView", "[]Columns.Width")    
        });

        /// <summary>
        /// A class to hold internal type cast definitions.
        /// </summary>
        public class TypeCast
        {
            /// <summary>
            /// An assembly where the type to convert to resides in.
            /// </summary>
            public Assembly AssemblyTypeIn;

            /// <summary>
            /// A type name with a namespace of a target type.
            /// </summary>
            public string NamespaceName;

            /// <summary>
            /// Gets the System.Type from the assembly AssemblyTypeIn containing a type named NamespaceName.
            /// </summary>
            public Type TypeFromName
            {
                get
                {
                    return AssemblyTypeIn.GetType(NamespaceName);
                }
            }

            /// <summary>
            /// A property name which value to cast.
            /// <para/>An empty string means that the object it self will be cast to another type.
            /// </summary>
            public string Property;

            /// <summary>
            /// A string referencing to the SaveWindowProperties list to determine
            /// <para/>when a cast is required.
            /// </summary>
            public string OnWindowProperty;
            
            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="assemblyTypeIn">The assembly the target type is in.</param>
            /// <param name="namespaceName">A name with a namespace of type to cast from to another type
            /// <para/>residing in <paramref name="assemblyTypeIn"/>.</param>
            /// <param name="property">A property name which value to cast.
            /// <para/>An empty string means that the object it self will be cast to another type.</param>
            /// <param name="onWindowProperty">A string referencing to the SaveWindowProperties list to determine
            /// <para/>when a cast is required.</param>
            public TypeCast(Assembly assemblyTypeIn, string namespaceName, string property, string onWindowProperty)
            {
                this.NamespaceName = namespaceName;
                this.AssemblyTypeIn = assemblyTypeIn;
                this.Property = property;
                this.OnWindowProperty = onWindowProperty;
            }
        }

        /// <summary>
        /// A List of KeyValuePair class instances which hold the
        /// <para/>predefined casts from Type to Type instructed by the TypeCast class.
        /// </summary>
        public static List<KeyValuePair<string, TypeCast>> Casts =
        new List<KeyValuePair<string, TypeCast>>(new KeyValuePair<string, TypeCast>[] 
        {
            new KeyValuePair<string, TypeCast>("System.Windows.Controls.ListView", new TypeCast(Assembly.GetAssembly(typeof(GridView)), "System.Windows.Controls.GridView", "View", "[]Columns.Width"))
        });

        /// <summary>
        /// Attach or detach event handlers from a given window
        /// <para/>relating to window sizing and resizing.
        /// </summary>
        /// <param name="window">A window to attach to or detach from the event handlers.</param>
        /// <param name="attach">Whether to attach or to detach.</param>
        /// <param name="leaveResizeReaction">
        /// Indicates if the capability to react to resize operation should left attached on detach.</param>
        private static void SetResizeEvents(Window window, bool attach, bool leaveResizeReaction = false)
        {
            if (attach)
            {
                window.Loaded += Window_Loaded;
                window.Closed += Window_Closed;

                if (!ResizeTimers.Exists((t) => t.Key == window)) // a dispatcher timer is declared for each window as there is no ResizeEnd event in WPF.
                {
                    DispatcherTimer dt = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000), IsEnabled = false };
                    dt.Tick += dt_Tick;
                    ResizeTimers.Add(new KeyValuePair<Window, DispatcherTimer>(window, dt));
                }

                window.SizeChanged += window_SizeChanged;
                window.LocationChanged += window_LocationChanged;
            }
            else
            {
                window.Loaded -= Window_Loaded;
                window.Closed -= Window_Closed;
                if (!leaveResizeReaction)
                {
                    window.SizeChanged -= window_SizeChanged;
                    window.LocationChanged -= window_LocationChanged;

                    for (int i = ResizeTimers.Count - 1; i >= 0; i--)
                    {
                        if (ResizeTimers[i].Key == window)
                        {
                            ResizeTimers[i].Value.Tick -= dt_Tick;
                            ResizeTimers.RemoveAt(i);
                        }
                    }
                }

                for (int i = formWindowDefaults.Count - 1; i >= 0; i--)
                {
                    if (formWindowDefaults[i].Item2 == window.GetType() && formWindowDefaults[i].Item1 == window)
                    {
                        formWindowDefaults.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Resizes a window to fit to screen based on the DefaultSizemode enumeration.
        /// </summary>
        /// <param name="window">A window to resize.</param>
        private static void ResizeFit(Window window)
        {
            Tuple<object, Type, WindowDefaults> formParams = formWindowDefaults.Single((f) => f.Item2 == window.GetType() && f.Item1 == window);
            HaltResize(window);

            Screen screen = ScreenUtils.GetScreenOnDefault(window);


            switch (formParams.Item3.DefaultSizemode)
            {
                case SizeChangeMode.MoveTopLeft:
                    if (window.Left < screen.WorkingArea.Left || window.Left > screen.WorkingArea.Right)
                    {
                        window.Left = screen.WorkingArea.Left;
                    }
                    if (window.Top < screen.WorkingArea.Top || window.Top > screen.WorkingArea.Bottom - SystemInformation.CaptionHeight)
                    {
                        window.Top = screen.WorkingArea.Top;
                    }
                    break;
                case SizeChangeMode.ResizeFit:
                    if (ScreenUtils.GetScreenOn(window) != null)
                    {
                        break;
                    }
                    while (window.Left < screen.WorkingArea.Left)
                    {
                        window.Left++;
                    }
                    while (window.Left > screen.WorkingArea.Right)
                    {
                        window.Left--;
                    }
                    while (window.Top < screen.WorkingArea.Top)
                    {
                        window.Top++;
                    }
                    while (window.Top + SystemInformation.CaptionHeight > screen.WorkingArea.Bottom)
                    {
                        window.Top--;
                        window.Top = screen.WorkingArea.Bottom - SystemInformation.CaptionHeight * 2;
                    }
                    while (window.Left + window.Width > screen.WorkingArea.Right)
                    {
                        if (window.MinWidth == window.Width)
                        {
                            break;
                        }
                        double w = window.Width;
                        window.Width--;
                        if (w == window.Width)
                        {
                            break;
                        }
                    }
                    while (window.Top + window.Height > screen.WorkingArea.Bottom)
                    {
                        if (window.MinHeight == window.Height)
                        {
                            break;
                        }
                        double h = window.Height;
                        window.Height--;
                        if (h == window.Height)
                        {
                            break;
                        }
                    }
                    break;
            }

            ContinueResize(window);
        }

        /// <summary>
        /// Detaches all event handlers monitoring a window resizing.
        /// <para/>This is used when the window size is being set programmatically.
        /// </summary>
        /// <param name="window">A window to detach the event handlers from.</param>    
        static void HaltResize(Window window)
        {
            window.SizeChanged -= window_SizeChanged;
            window.LocationChanged -= window_LocationChanged;
        }

        /// <summary>
        /// Attaches all event handlers monitoring a window resizing.
        /// <para/>This is used after the window size was set set programmatically.
        /// </summary>
        /// <param name="window">A window to attach the event handlers to.</param>
        static void ContinueResize(Window window)
        {
            window.SizeChanged += window_SizeChanged;
            window.LocationChanged += window_LocationChanged;
        }

        /// <summary>
        /// Sets the emulated WinForms style ResizeEnd event to launch.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            QueueDelayedResize(sender as Window);
        }

        /// <summary>
        /// Sets the emulated WinForms style ResizeEnd event to launch.
        /// </summary>
        /// <param name="window">A window which "ResizeEnd" event is set to simulate.</param>
        private static void QueueDelayedResize(Window window)
        {
            if (ResizeTimers.Exists((t) => t.Key == window))
            {
                ResizeTimers.Single((t) => t.Key == window).Value.Stop();
                ResizeTimers.Single((t) => t.Key == window).Value.Start();
            }
        }

        /// <summary>
        /// Sets the emulated WinForms style ResizeEnd event to launch.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void window_LocationChanged(object sender, EventArgs e)
        {
            QueueDelayedResize(sender as Window);
        }

        /// <summary>
        /// Detach all avent handlers from a window when it's closed (assumed to be desroyed too).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void Window_Closed(object sender, EventArgs e)
        {
            Window window = (sender as Window);
            SavePosition(window);
            SetResizeEvents(window, false);
        }

        /// <summary>
        /// An event handler attached to window's Loaded event.
        /// <para/>This loads the positioning data from a file and sets it to the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = (sender as Window);
            LoadPosition(window);
        }

        /// <summary>
        /// Attaches an event handler for the SystemEvents.DisplaySettingsChanged
        /// <para/>to handle all application windows after display settings changed.
        /// </summary>
        public static void SetScreenChangeHandler()
        {
            SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        }


        /// <summary>
        /// Detaches all event handlers relating to window sizing and resizing of all open application windows.
        /// </summary>
        public static void ReleaseAllHandlers()
        {
            formWindowDefaults.Clear();
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                SetResizeEvents(window, false);
            }
        }

        /// <summary>
        /// Occurs when the user changes the display settings.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void DisplaySettingsChanged(object sender, EventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (formWindowDefaults.Exists((f) => f.Item2 == window.GetType() && f.Item1 == window))
                {
                    ResizeFit(window);
                }
            }
        }

        /// <summary>
        /// Detaches the event handler for the SystemEvents.DisplaySettingsChanged event.
        /// </summary>
        public static void ReleaseScreenChangeHandler()
        {
            SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;
        }
    }
}
