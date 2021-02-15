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
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;
using VPKSoft.Utils;

namespace VPKSoft.PosLib
{
    /// <summary>
    /// The core functionality for Windows Forms and Windows Presentation Foundation
    /// <para/>application window/form position saving.
    /// </summary>
    public class PositionCore
    {
        /// <summary>
        /// An internal list of WinForms or WPF windows to be included in the PosLib's size recording.
        /// </summary>
        internal static List<Tuple<object, Type, WindowDefaults>> formWindowDefaults = new List<Tuple<object, Type, WindowDefaults>>();

        

        /// <summary>
        /// Types, which values to save if they exist in the list and their
        /// <para/>names are listed in case of WinForms in the SaveFormProperties list and
        /// <para/>in case of WPF in the SaveWindowProperties array.
        /// <remarks>Collection property definitions can get their own TypeDescriptor class
        /// <para/>instance from an item in a collection or from a CustomTypeDescriptors list.</remarks>
        /// </summary>
        public static List<Type> EnumerableTypes = new List<Type>(new Type[] {typeof(int),
                                                                              typeof(double),
                                                                              typeof(long),
                                                                              typeof(float),
                                                                              typeof(uint),
                                                                              typeof(ulong)});

        /// <summary>
        /// A list of TypeConverter class instances paired to a type.
        /// </summary>
        public static List<KeyValuePair<Type, TypeConverter>> CustomTypeConverters = new List<KeyValuePair<Type, TypeConverter>>();

        /// <summary>
        /// Gets a value indicating if the CustomTypeConverters list contains a TypeConverter for a given type.
        /// </summary>
        /// <param name="type">A type to check for a TypeDescriptor.</param>
        /// <returns>True if the CustomTypeConverters list contains a TypeConverter for a given type, otherwise false.</returns>
        public static bool CustomTypeConverterExists(Type type)
        {
            return GetCustomTypeConverter(type) != null;
        }

        /// <summary>
        /// Indicates if the DPI setting was changed after the last application start.
        /// </summary>
        private static bool DPIChanged = false;

        /// <summary>
        /// Indicates if the screen setting changed after the last application start.
        /// </summary>
        private static bool screensChanded = false;

        /// <summary>
        /// A value indicating if a form/window position load
        /// <para/>should be skipped if screen setting changed after the last application start.
        /// </summary>
        private static bool skipLoadOnScreenChanged = true;

        /// <summary>
        /// Gets a value indicating if a form/window position load
        /// <para/>should be skipped.
        /// </summary>
        public static bool SkipLoadScreen
        {
            get
            {
                return (DPIChanged || screensChanded) && skipLoadOnScreenChanged;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if a form/window position load
        /// <para/>should be skipped if screen setting changed after the last application start.
        /// </summary>
        public bool SkipLoadOnScreenChange
        {
            get
            {
                return skipLoadOnScreenChanged;
            }

            set
            {
                skipLoadOnScreenChanged = value;
            }                
        }


        /// <summary>
        /// Loads default data and attaches a SystemEvents.DisplaySettingsChanged event handler.
        /// </summary>
        /// <param name="appType">The type of the application to bind to.</param>
        public static void Bind(ApplicationType appType)
        {
            VPKNml position = new VPKNml("position");

            LoadConversions(appType);

            if (File.Exists(Paths.GetAppSettingsFolder(appType == ApplicationType.WinForms ? Misc.AppType.Winforms : Misc.AppType.WPF) + "position.vnml"))
            {
                position.Load(Paths.GetAppSettingsFolder(appType == ApplicationType.WinForms ? Misc.AppType.Winforms : Misc.AppType.WPF) + "position.vnml");
            }

            DPIChanged = position["DPI", "value", ScreenUtils.GetDPI()].ToString() != ScreenUtils.GetDPI().ToString();

            Screen[] screens = Screen.AllScreens;
            screensChanded = position["Screens", "count", screens.Length].ToString() != screens.Length.ToString();

            if (!screensChanded)
            {
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Rectangle));
                for (int i = 0; i < screens.Length; i++)
                {                    
                    if (position["Screens", i.ToString(), tc.ConvertToString(screens[0].WorkingArea)].ToString() != tc.ConvertToString(screens[0].WorkingArea))
                    {
                        screensChanded = true;
                        break;
                    }
                }
            }

            if (appType == ApplicationType.Wpf)
            {
                PositionsWindow.SetScreenChangeHandler();
            }
            else if (appType == ApplicationType.WinForms)
            {
                PositionForms.SetScreenChangeHandler();
            }
        }

        /// <summary>
        /// Releases all attached event handlers by the PosLib library and saves default data.
        /// </summary>
        /// <param name="appType">The type of the application to bind to.</param>
        public static void UnBind(ApplicationType appType)
        {
            VPKNml position = new VPKNml("position");

            if (File.Exists(Paths.GetAppSettingsFolder(appType == ApplicationType.WinForms ? Misc.AppType.Winforms : Misc.AppType.WPF) + "position.vnml"))
            {
                position.Load(Paths.GetAppSettingsFolder(appType == ApplicationType.WinForms ? Misc.AppType.Winforms : Misc.AppType.WPF) + "position.vnml");
            }


            position["DPI", "value"] = ScreenUtils.GetDPI();

            Screen[] screens = Screen.AllScreens;
            position["Screens", "count"] = screens.Length;

                TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Rectangle));
                for (int i = 0; i < screens.Length; i++)
                {
                    position["Screens", i.ToString()] = tc.ConvertToString(screens[0].WorkingArea);
                }

            position.Save(Paths.GetAppSettingsFolder(appType == ApplicationType.WinForms ? Misc.AppType.Winforms : Misc.AppType.WPF) + "position.vnml");

            if (appType == ApplicationType.Wpf)
            {
                PositionsWindow.ReleaseScreenChangeHandler();
                PositionsWindow.ReleaseAllHandlers();
            }
            else if (appType == ApplicationType.WinForms)
            {
                PositionForms.ReleaseScreenChangeHandler();
                PositionForms.ReleaseAllHandlers();
            }
        }

        /// <summary>
        /// Gets a value if the PosLib should skip loading the position data.
        /// <para/>This is indicated with giving a command line argument --skipPos
        /// <para/>for the application's command line.
        /// </summary>
        public static bool SkipLoad
        {
            get
            {
                ProgramArgumentCollection pac = new ProgramArgumentCollection();
                pac.IgnoreCase = true;
                return pac.ArgumentExists("--skipPos");
            }
        }

        /// <summary>
        /// Gets a custom TypeConverter class instance contained in the CustomTypeConverters list.
        /// </summary>
        /// <param name="type">A System.Type to get a TypeConverter class instance for.</param>
        /// <returns>A TypeConverter class instance if found, otherwise null.</returns>
        public static TypeConverter GetCustomTypeConverter(Type type)
        {
            if (CustomTypeConverters.Exists((t) => t.Key == type))
            {
                return CustomTypeConverters.Single((t) => t.Key == type).Value;
            }
            return null;
        }

        /// <summary>
        /// Gets a custom TypeConverter class instance contained in the CustomTypeConverters list.
        /// <para/>If the CustomTypeConverters list does not contain a TypeConverter for the given object,
        /// <para/>the the TypeDescriptor.GetConverter method is used to get a TypeConverter.
        /// </summary>
        /// <param name="value">An object to get a TypeConverter class instance for.</param>
        /// <returns>A TypeConverter class instance.</returns>
        public static TypeConverter GetCustomTypeConverterOrDefault(object value)
        {
            if (CustomTypeConverters.Exists((t) => t.Key == value.GetType()))
            {
                return CustomTypeConverters.Single((t) => t.Key == value.GetType()).Value;
            }
            return TypeDescriptor.GetConverter(value);
        }

        /// <summary>
        /// A value indicating if the TypeDescriptor.GetConverter method
        /// <para/>should be used to get a TypeDescriptor for 
        /// <para/>value-string-value conversion.
        /// </summary>
        private static bool getTypeDescriptors = true;

        /// <summary>
        /// Gets or sets a value indicating if the TypeDescriptor.GetConverter 
        /// <para/>method should be used to get a TypeDescriptor for 
        /// <para/>value-string-value conversion.
        /// </summary>
        public static bool GetTypeDescriptors
        {
            get
            {
                return getTypeDescriptors;
            }

            set
            {
                getTypeDescriptors = value;
            }
        }

        /// <summary>
        /// A value indicating if exceptions occurring while
        /// <para/>converting from a type to string or from a string to type
        /// <para/>should be handled internally.
        /// </summary>
        private static bool excludeTypeConversionExceptions = true;

        /// <summary>
        /// Gets or sets a value indicating if exceptions occurring while
        /// <para/>converting from a type to string or from a string to type
        /// <para/>should be handled internally.
        /// </summary>
        public static bool ExcludeTypeConversionExceptions
        {
            get
            {
                return excludeTypeConversionExceptions;
            }

            set
            {
                excludeTypeConversionExceptions = value;
            }
        }

        /// <summary>
        /// A class for passing internal exception information trough an event to the user code.
        /// </summary>
        public class PosLibExceptionEventArgs: EventArgs
        {
            /// <summary>
            /// An exception to pass.
            /// </summary>
            public Exception ex;

            /// <summary>
            /// A class constructor for the PosLibExceptionEventArgs class.
            /// </summary>
            /// <param name="ex">An exception to be passed with an event.</param>
            public PosLibExceptionEventArgs(Exception ex): base()
            {
                this.ex = ex;
            }
        }

        /// <summary>
        /// A delegate for an event to pass internal exception information trough an event to the user code
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A PosLibExceptionEventArgs class instance indicating the internal exception.</param>
        public delegate void OnPosLibException(object sender, PosLibExceptionEventArgs e);

        /// <summary>
        /// An event which is fired on an exception if exception handling
        /// <para/> is set to be internal within the PosLib library.
        /// </summary>
        static public event OnPosLibException PosLibException = null;

        /// <summary>
        /// Raises the PosLibException event if any handlers are attached to it. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ex">An exception to forward with the event.</param>
        internal static void RaisePoslibException(object sender, Exception ex)
        {
            if (PosLibException != null)
            {
                PosLibException(sender, new PosLibExceptionEventArgs(ex));
            }
        }
        
        /// <summary>
        /// A class containg default parameters to a WinForms Form ar to a WPF Window.
        /// </summary>
        public class WindowDefaults
        {
            /// <summary>
            /// A default size for a Form or a Window.
            /// </summary>
            public DefaultSize DefaultSize = new DefaultSize();

            /// <summary>
            /// An enumeration indicating how a Form or a Window size should be
            /// <para/>handled if it gets out of screen boundaries.
            /// </summary>
            public SizeChangeMode DefaultSizemode = SizeChangeMode.MoveTopLeft;

            /// <summary>
            /// A last Form or Window state.
            /// </summary>
            public WindowState LastState = WindowState.StateNormal; 
        }

        /// <summary>
        /// A wrapper to store System.Windows.Forms.FormWindowState and
        /// <para/>System.Windows.WindowState enumerations for convinience.
        /// </summary>
        public enum WindowState
        {
            /// <summary>
            /// A window or form state is maximized.
            /// </summary>
            StateMaximized,

            /// <summary>
            /// A window or form state is normal.
            /// </summary>
            StateNormal,

            /// <summary>
            /// A window or form state is minimized.
            /// </summary>
            StateMinimized
        }

        /// <summary>
        /// An enumeration to describe how a Form or a Window
        /// <para/>should be resized if it gets out of screen's boundaries.
        /// </summary>
        public enum SizeChangeMode
        {
            /// <summary>
            /// A Form or a Window should be resized to fit the screen.
            /// </summary>
            ResizeFit,

            /// <summary>
            /// The top and left position of the Form or a Window should
            /// <para/>be adjusted so that an interaction with it is possible.
            /// </summary>
            MoveTopLeft
        }

        /// <summary>
        /// Converts a WindowState enumeration declared in this
        /// <para/>class to a System.Windows.Forms.FormWindowState enumeration.
        /// </summary>
        /// <param name="state">A WindowState to convert.</param>
        /// <returns>A System.Windows.Forms.FormWindowState enumeration value.</returns>
        public static FormWindowState ToFormWindowState(WindowState state)
        {
            switch (state)
            {                
                case WindowState.StateMaximized:
                    return FormWindowState.Maximized;
                case WindowState.StateMinimized:
                    return FormWindowState.Minimized;
                case WindowState.StateNormal:
                    return FormWindowState.Normal;
            }
            return FormWindowState.Normal;
        }

        /// <summary>
        /// Converts a WindowState enumeration declared in this
        /// <para/>class to a System.Windows.WindowState enumeration.
        /// </summary>
        /// <param name="state">A WindowState to convert.</param>
        /// <returns>A System.System.Windows.WindowState enumeration value.</returns>
        public static System.Windows.WindowState ToWindowWindowState(WindowState state)
        {
            switch (state)
            {
                case WindowState.StateMaximized:
                    return System.Windows.WindowState.Maximized;
                case WindowState.StateMinimized:
                    return System.Windows.WindowState.Minimized;
                case WindowState.StateNormal:
                    return System.Windows.WindowState.Normal;
            }
            return System.Windows.WindowState.Normal;
        }

        /// <summary>
        /// Gets a WindowState of a given WinForms Form instance
        /// <para/>and converts it to a WindowState enumeration declared in this class.
        /// </summary>
        /// <param name="form">A WinForms Form class instance.</param>
        /// <returns>A WindowState enumeration value.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
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



        /// <summary>
        /// Gets all FrameworkElement class instance children of a given 
        /// <para/>FrameworkElement class instance recursively, which have an assigned x:Uid, x:Name or Tag (Name=objectName).
        /// </summary>
        /// <param name="element">A class instace which is or is inherited from a FrameworkElement class.</param>
        /// <returns>A list of FrameworkElement class instances with an assinged x:Uid or x:Name.</returns>
        public static List<FrameworkElement> GetUidElements(FrameworkElement element)
        {
            List<FrameworkElement> elements = new List<FrameworkElement>();
            foreach (var e in LogicalTreeHelper.GetChildren(element))
            {
                if (e is FrameworkElement)
                {
                    FrameworkElement el = (FrameworkElement)e;
                    if (el.GetRefName() != string.Empty)
                    {
                        elements.Add(el);
                    }
                    elements.AddRange(GetUidElements(el));
                }
            }
            return elements;
        }

        /// <summary>
        /// Gets all DependencyObject class instances from a given
        /// <para/>base DependencyObject class instance.
        /// </summary>
        /// <param name="obj">A DependencyObject class instance.</param>
        /// <returns>A list of DependencyObject class instances from a given base DependencyObject class instance.</returns>
        public static List<DependencyObject> GetObjects(DependencyObject obj)
        {
            List<DependencyObject> objects = new List<DependencyObject>();

            int iChildCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < iChildCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child.GetType() == typeof(System.Windows.Controls.ListView))
                {
                    try
                    {
                        GridView gv = (GridView)(child as System.Windows.Controls.ListView).View;
                        objects.Add(gv);
                    }
                    catch
                    {

                    }
                }

                if (child is System.Windows.Controls.ViewBase || child is FrameworkElement)
                {
                    objects.Add(child);
                }
                objects.AddRange(GetObjects(child));
            }
            return objects;
        }

        /// <summary>
        /// An helper to get some string "reference" for a WPF object through reflection.
        /// </summary>
        /// <param name="obj">A WPF object.</param>
        /// <param name="fallBack">A fallback object, which string representation to get if no string "reference" was gotten otherwise.</param>
        /// <returns>A string "reference" of a given WPF object
        /// <para/>in this order: Uid, Name, Tag (Tag=Name=...), fallback object's string representation.
        /// <para/>NOTE: An empty string may still be returned if the fallback object is null or the 
        /// <para/>fallback object's string representation is an empty string.</returns>
        public static string TryGetNameWPF(object obj, object fallBack)
        {
            string retval = string.Empty;
            try
            {
                retval = obj.GetType().GetProperty("Uid").GetValue(obj).ToString();
            }
            catch
            {
                // nothing to do
            }

            if (retval == string.Empty)
            {
                return TryGetName(obj, fallBack);
            }

            return retval;
        }

        /// <summary>
        /// An helper to get a string "reference" for an object through reflection.
        /// </summary>
        /// <param name="obj">An object, which string "reference" to get.</param>
        /// <param name="fallBack">A fallback object, which string representation to get if no string "reference" was gotten otherwise.</param>
        /// <returns>A string "reference" of a given object
        /// <para/>in this order: Name, Tag (Tag=Name=...), fallback object's string representation.
        /// <para/>NOTE: An empty string may still be returned if the fallback object is null or 
        /// <para/>the fallback object's string representation is an empty string.</returns>
        public static string TryGetName(object obj, object fallBack)
        {
            string retval = string.Empty;

            try
            {
                retval = obj.GetType().GetProperty("Name").GetValue(obj).ToString();
            }
            catch
            {
                // nothing to do
            }

            try
            {
                retval = obj.GetType().GetProperty("Tag").GetValue(obj)?.ToString() ?? string.Empty;
                if (retval.StartsWith("Name="))
                {
                    retval = retval.Replace("Name=", string.Empty);
                }
            }
            catch
            {
                // nothing to do
            }

            if (retval == string.Empty && fallBack != null)
            {
                retval = fallBack.ToString();
            }

            return retval;
        }

        /// <summary>
        /// Load the type - value pairs defined in PosLib.vnml file if any.
        /// </summary>
        public static void LoadConversions(ApplicationType appType)
        {
            if (File.Exists(AssemblyPath + "PosLib.vnml"))
            {
                if (appType == ApplicationType.Wpf)
                {
                    VPKNml vnml = new VPKNml("WPF");
                    vnml.Load(AssemblyPath + "PosLib.vnml");
                    int i = 1;
                    while (vnml["SaveProperties", "Property" + i] != null)
                    {
                        try
                        {
                            string proptype = vnml["SaveProperties", "Property" + i].ToString();
                            i++;
                            string prop = proptype.Split('|')[1];
                            proptype = proptype.Split('|')[0];
                            if (PositionsWindow.SaveWindowProperties.Exists((k) => k.Key == proptype && k.Value == prop))
                            {
                                continue;
                            }
                            else
                            {
                                PositionsWindow.SaveWindowProperties.Add(new KeyValuePair<string, string>(proptype, prop));
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                else if (appType == ApplicationType.WinForms)
                {
                    VPKNml vnml = new VPKNml("WinForms");
                    vnml.Load(AssemblyPath + "PosLib.vnml");
                    int i = 1;
                    while (vnml["SaveProperties", "Property" + i] != null)
                    {
                        try
                        {
                            string proptype = vnml["SaveProperties", "Property" + i].ToString();
                            i++;
                            string prop = proptype.Split('|')[1];
                            proptype = proptype.Split('|')[0];
                            if (PositionForms.SaveFormProperties.Exists((k) => k.Key == proptype && k.Value == prop))
                            {
                                continue;
                            }
                            else
                            {
                                PositionForms.SaveFormProperties.Add(new KeyValuePair<string, string>(proptype, prop));
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the file name and path of the PosLib library.
        /// </summary>
        public static string AssemblyFile
        {
            get
            {
                return Assembly.GetAssembly(typeof(PositionCore)).Location;
            }
        }

        /// <summary>
        /// Gets the path of the PosLib library.
        /// </summary>
        public static string AssemblyPath
        {
            get
            {
                string path = Path.GetDirectoryName(AssemblyFile);
                return path[path.Length - 1] == Path.DirectorySeparatorChar ? path : path + Path.DirectorySeparatorChar;
            }
        }
    }
}
