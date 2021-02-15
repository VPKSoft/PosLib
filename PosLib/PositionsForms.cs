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
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using VPKSoft.Utils;

namespace VPKSoft.PosLib
{
    /// <summary>
    /// A class for saving and loading a WinForms Form class instance positioning information.
    /// </summary>
    public class PositionForms: PositionCore
    {
        /// <summary>
        /// Ads a WinForms form to the list of forms to which position is to be saved and loaded within the PosLib library.
        /// </summary>
        /// <param name="form">A WinForms form to be added to positioning.</param>
        /// <param name="szm">An enumeration to describe how a Form or a Window
        /// <para/>should be resized if it gets out of screen's boundaries.</param>
        public static void Add(Form form, SizeChangeMode szm = SizeChangeMode.MoveTopLeft)
        {
            WindowDefaults def = new WindowDefaults();
            def.DefaultSizemode = szm;
            def.DefaultSize = new DefaultSize(form.RestoreBounds.X, form.RestoreBounds.Y, form.RestoreBounds.Width, form.RestoreBounds.Height);
            formWindowDefaults.Add(new Tuple<object, Type, WindowDefaults>(form, form.GetType(), def));
            SetResizeEvents(form, true);
        }
        
        /// <summary>
        /// Saves a form position to a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// </summary>
        /// <param name="form">A Form class instance which position to save.</param>
        public static void SavePosition(Form form)
        {
            Paths.MakeAppSettingsFolder(Misc.AppType.Winforms);
            VPKNml position = new VPKNml("position");

            try
            {
                if (File.Exists(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml"))
                {
                    position.Load(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml");
                }

                RectangleConverter rc = new RectangleConverter();

                if (form.WindowState != FormWindowState.Maximized &&
                    form.WindowState != FormWindowState.Minimized)
                {
                    position[form.Name, "bounds"] = rc.ConvertToString(form.Bounds);
                }
                else
                {
                    position[form.Name, "bounds"] = rc.ConvertToString(form.RestoreBounds);
                }

                position[form.Name, "defaultBounds"] = new DefaultSize(form);
                position[form.Name, "sizeMode"] = formWindowDefaults.Single((f) => f.Item2 == form.GetType() && f.Item1 == form).Item3.DefaultSizemode;
                position[form.Name, "windowState"] = GetWindowState(form);
                position[form.Name, "screenIndex"] = ScreenUtils.GetScreenIndex(form);
            }
            catch (Exception ex)
            {
                if (!PositionCore.ExcludeTypeConversionExceptions)
                {
                    throw ex;
                }
                else
                {
                    RaisePoslibException(form, ex);
                }
            }


            FieldInfo[] fields = form.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo fi in fields)
            {
                if (SaveFormProperties.Exists((f) => f.Key == fi.FieldType.ToString()))
                {
                    List<KeyValuePair<string, string>> controlProperties = SaveFormProperties.FindAll((f) => f.Key == fi.FieldType.ToString());
                    if (fi.Name == string.Empty)
                    {
                        continue;
                    }

                    foreach (KeyValuePair<string, string> prop in controlProperties)
                    {
                        object obj = fi.GetValue(form) as object;
                        if (obj == null)
                        {
                            continue;
                        }

                        if (prop.Value.StartsWith("[]")) // walk through a collection
                        {
                            try
                            {
                                List<string> collectionProps = new List<string>(prop.Value.TrimStart('[', ']').Split('.'));

                                PropertyInfo piList = obj.GetType().GetProperty(collectionProps[0]);
                                IList propList = (IList)piList.GetValue(obj); // Assume type of IList
                                TypeConverter converter;
                                object oValue;
                                for (int i = 0; i < propList.Count; i++)
                                {
                                    var cProp = propList[i];
                                    string nString = TryGetName(cProp, i);
                                    switch (collectionProps.Count)
                                    {
                                        case 1:
                                            oValue = cProp;
                                            if (!PositionCore.CustomTypeConverterExists(oValue.GetType()) &&
                                                !PositionCore.GetTypeDescriptors)
                                            {
                                                continue;
                                            }
                                            converter = PositionCore.GetCustomTypeConverterOrDefault(oValue);
                                            position[form.Name + "." + fi.Name + ".[]" + string.Join(".", collectionProps.ToArray()), nString] = converter.ConvertToString(oValue) + "|" + oValue.GetType().ToString();
                                            break;
                                        case 2:
                                            oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                            if (!PositionCore.CustomTypeConverterExists(oValue.GetType()) &&
                                                !PositionCore.GetTypeDescriptors)
                                            {
                                                continue;
                                            }
                                            converter = PositionCore.GetCustomTypeConverterOrDefault(oValue);
                                            position[form.Name + "." + fi.Name + ".[]" + string.Join(".", collectionProps.ToArray()), nString] = converter.ConvertToString(oValue) + "|" + oValue.GetType().ToString();
                                            break;
                                        case 3:
                                            oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                            oValue = oValue.GetType().GetProperty(collectionProps[2]).GetValue(oValue);
                                            if (!PositionCore.CustomTypeConverterExists(oValue.GetType()) &&
                                                !PositionCore.GetTypeDescriptors)
                                            {
                                                continue;
                                            }
                                            converter = PositionCore.GetCustomTypeConverterOrDefault(oValue);
                                            position[form.Name + "." + fi.Name + ".[]" + string.Join(".", collectionProps.ToArray()), nString] = converter.ConvertToString(oValue) + "|" + oValue.GetType().ToString();
                                            break;
                                    }
                                }

                                continue;
                            }
                            catch (Exception ex)
                            {
                                if (!PositionCore.ExcludeTypeConversionExceptions)
                                {
                                    throw ex;
                                }
                                else
                                {
                                    RaisePoslibException(form, ex);
                                }
                            }
                        }

                        try
                        {
                            PropertyInfo pi = obj.GetType().GetProperty(prop.Value);
                            object value = pi.GetValue(obj);

                            if (value == null)
                            {
                                continue;
                            }

                            if (EnumerableTypes.Contains(value.GetType()))
                            {
                                position[form.Name + "." + fi.Name, prop.Value] = value + "|" + value.GetType().ToString();
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
                                RaisePoslibException(form, ex);
                            }
                        }
                    }
                }
            }

            position.Save(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml");
        }    

        /// <summary>
        /// Deletes a form position data from a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// <para/>Also all event handlers related to saving the forms position are detached.
        /// </summary>
        /// <param name="form">A form which position to "reset".</param>
        public static void ResetPosition(Form form)
        {
            ResetPositionData(form);

            LoadPosition(form); // Just a test if the loading would fail on an empty data

            SetResizeEvents(form, false, true);
        }

        /// <summary>
        /// Deletes a form position data from a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// </summary>
        /// <param name="form">A form which position to "reset".</param>
        private static void ResetPositionData(Form form)
        {
            VPKNml position = new VPKNml("position");

            if (File.Exists(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml"))
            {
                position.Load(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml");
            }

            position.DeleteSections(form.Name + "*");

            position.Save(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml");
        }

        /// <summary>
        /// Loads a form position from a vnml (VPKNml) file (position.vnml) under
        /// <para/>the %LOCALAPPDATA%\[host application product name] folder.
        /// </summary>
        /// <param name="form">A Form class instance which position to load.</param>
        public static void LoadPosition(Form form)
        {
            if (SkipLoad || SkipLoadScreen)
            {
                ResetPositionData(form);
                return;
            }

            VPKNml position = new VPKNml("position");

            if (File.Exists(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml"))
            {
                position.Load(Paths.GetAppSettingsFolder(Misc.AppType.Winforms) + "position.vnml");
            }
            RectangleConverter rc = new RectangleConverter();

            try
            {
                if (position[form.Name, "bounds"] != null)
                {
                    form.Bounds = (Rectangle)rc.ConvertFromString(position[form.Name, "bounds"].ToString());
                }

                if (position[form.Name, "defaultBounds"] != null)
                {
                    formWindowDefaults.Single((f) => f.Item2 == form.GetType() && f.Item1 == form).Item3.DefaultSize = DefaultSize.Parse(position[form.Name, "defaultBounds"].ToString());
                }

                if (position[form.Name, "windowState"] != null)
                {
                    if (ToFormWindowState((WindowState)Enum.Parse(typeof(WindowState), position[form.Name, "windowState"].ToString())) == FormWindowState.Maximized)
                    {
                        form.Bounds = formWindowDefaults.Single((f) => f.Item2 == form.GetType() && f.Item1 == form).Item3.DefaultSize.ToFormBounds();
                    }
                    form.WindowState = ToFormWindowState((WindowState)Enum.Parse(typeof(WindowState), position[form.Name, "windowState"].ToString()));
                }



                if (position[form.Name, "sizeMode"] != null)
                {
                    formWindowDefaults.Single((f) => f.Item2 == form.GetType() && f.Item1 == form).Item3.DefaultSizemode = (SizeChangeMode)Enum.Parse(typeof(SizeChangeMode), position[form.Name, "sizeMode"].ToString());
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
                    RaisePoslibException(form, ex);
                }
            }

            try
            {
                FieldInfo[] fields = form.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                foreach (FieldInfo fi in fields)
                {
                    if (SaveFormProperties.Exists((f) => f.Key == fi.FieldType.ToString()))
                    {
                        List<KeyValuePair<string, string>> controlProperties = SaveFormProperties.FindAll((f) => f.Key == fi.FieldType.ToString());
                        if (fi.Name == string.Empty)
                        {
                            continue;
                        }

                        foreach (KeyValuePair<string, string> prop in controlProperties)
                        {
                            object obj = fi.GetValue(form) as object;
                            if (obj == null)
                            {
                                continue;
                            }

                            if (prop.Value.StartsWith("[]"))
                            {
                                try
                                {
                                    List<string> collectionProps = new List<string>(prop.Value.TrimStart('[', ']').Split('.'));

                                    PropertyInfo piList = obj.GetType().GetProperty(collectionProps[0]);
                                    IList propList = (IList)piList.GetValue(obj); // Assume type of IList
                                    for (int i = 0; i < propList.Count; i++) // TODO::Reference by index (i)
                                    {
                                        var cProp = propList[i];
                                        string nString = TryGetName(cProp, i);
                                        object oValue;

                                        if (position[form.Name + "." + fi.Name + ".[]" + string.Join(".", collectionProps.ToArray()), nString] == null)
                                        {
                                            continue;
                                        }
                                        string valuePart = position[form.Name + "." + fi.Name + ".[]" + string.Join(".", collectionProps.ToArray()), nString].ToString().Split('|')[0];
                                        string typePart = position[form.Name + "." + fi.Name + ".[]" + string.Join(".", collectionProps.ToArray()), nString].ToString().Split('|')[1];

                                        switch (collectionProps.Count)
                                        {
                                            case 1:
                                                oValue = cProp;
                                                break;
                                            case 2:
                                                {
                                                    oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                                    TypeConverter converter = TypeDescriptor.GetConverter(oValue);
                                                    cProp.GetType().GetProperty(collectionProps[1]).SetValue(cProp, converter.ConvertFrom(valuePart));
                                                }
                                                break;

                                            case 3:
                                                {

                                                    oValue = cProp.GetType().GetProperty(collectionProps[1]).GetValue(cProp);
                                                    oValue = oValue.GetType().GetProperty(collectionProps[2]).GetValue(oValue);
                                                    TypeConverter converter = TypeDescriptor.GetConverter(oValue);
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
                                        RaisePoslibException(form, ex);
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
                                if (position[form.Name + "." + fi.Name, prop.Value] != null)
                                {
                                    TypeConverter converter = TypeDescriptor.GetConverter(value);
                                    var val = converter.ConvertFrom(position[form.Name + "." + fi.Name, prop.Value].ToString().Split('|')[0]);
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
                    RaisePoslibException(form, ex);
                }
            }
            ResizeFit(form);
        }

        /// <summary>
        /// A list of types and their value to be loaded/saved to a position.vnml file.
        /// <remarks>If a string describing a a property value starts
        /// <para/>with [] (square brackets) the value is considered as a collection,
        /// <para/>which can be cast to IList interface for enumeration.</remarks>
        /// </summary>
        public static List<KeyValuePair<string, string>> SaveFormProperties = 
            new List<KeyValuePair<string ,string>>(new KeyValuePair<string, string>[] 
            {
             new KeyValuePair<string, string>("System.Windows.Forms.SplitContainer", "SplitterDistance"),
             new KeyValuePair<string, string>("System.Windows.Forms.Splitter", "SplitPosition"),
             new KeyValuePair<string, string>("System.Windows.Forms.ListView", "[]Columns.Width"),
             new KeyValuePair<string, string>("System.Windows.Forms.DataGridView", "[]Columns.Width")
            });

        /// <summary>
        /// Attach or detach event handlers from a given form
        /// <para/>relating to form sizing and resizing.
        /// </summary>
        /// <param name="form">A form to attach to or detach from the event handlers.</param>
        /// <param name="attach">Whether to attach or to detach.</param>
        /// <param name="leaveResizeReaction">
        /// Indicates if the capability to react to resize operation should left attached on detach.</param>
        private static void SetResizeEvents(Form form, bool attach, bool leaveResizeReaction = false)
        {
            if (attach)
            {
                form.Shown += form_Shown;
                form.FormClosed += Form_FormClosed;
                form.ResizeEnd += form_ResizeEnd;
                form.Resize += form_Resize;
                form.Disposed += form_Disposed;
            }
            else
            {
                form.Shown -= form_Shown;
                form.FormClosed -= Form_FormClosed;
                form.ResizeEnd -= form_ResizeEnd;
                if (!leaveResizeReaction)
                {
                    form.Resize -= form_Resize;
                }
                form.Disposed -= form_Disposed;


                for (int i = formWindowDefaults.Count - 1; i >= 0; i--)
                {
                    if (formWindowDefaults[i].Item2 == form.GetType() && formWindowDefaults[i].Item1 == form)
                    {
                        formWindowDefaults.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Resizes a form to fit to screen based on the DefaultSizemode enumeration.
        /// </summary>
        /// <param name="form">A form to resize.</param>
        private static void ResizeFit(Form form)
        {
            Tuple<object, Type, WindowDefaults> formParams = formWindowDefaults.Single((f) => f.Item2 == form.GetType() && f.Item1 == form);
            HaltResize(form);

            Screen screen = ScreenUtils.GetScreenOnDefault(form);


            switch (formParams.Item3.DefaultSizemode)
            {
                case SizeChangeMode.MoveTopLeft: // Only move top and left position
                    if (form.Left < screen.WorkingArea.Left || form.Left > screen.WorkingArea.Right)
                    {
                        form.Left = screen.WorkingArea.Left;
                    }
                    if (form.Top < screen.WorkingArea.Top || form.Top > screen.WorkingArea.Bottom - SystemInformation.CaptionHeight)
                    {
                        form.Top = screen.WorkingArea.Top;
                    }
                    break;
                case SizeChangeMode.ResizeFit: // Adjust size as well to fit the form completely on the screen.
                    if (ScreenUtils.GetScreenOn(form) != null)
                    {
                        break;
                    }
                    while (form.Bounds.Left < screen.WorkingArea.Left)
                    {
                        form.Left++;
                    }
                    while (form.Bounds.Left > screen.WorkingArea.Right)
                    {
                        form.Left--;
                    }
                    while (form.Bounds.Top  < screen.WorkingArea.Top)
                    {
                        form.Top++;
                    }
                    while (form.Top + SystemInformation.CaptionHeight > screen.WorkingArea.Bottom)
                    {
                        form.Top--;
                        form.Top = screen.WorkingArea.Bottom - SystemInformation.CaptionHeight * 2;
                    }
                    while (form.Right > screen.WorkingArea.Right)
                    {
                        if (form.MinimumSize.Width == form.Width)
                        {
                            break;
                        }
                        int w = form.Width;
                        form.Width--;
                        if (w == form.Width)
                        {
                            break;                            
                        }
                    }
                    while (form.Bottom > screen.WorkingArea.Bottom)
                    {
                        if (form.MinimumSize.Height == form.Height)
                        {
                            break;
                        }
                        int h = form.Height;
                        form.Height--;
                        if (h == form.Height)
                        {
                            break;
                        }
                    }
                    break;
            }

            ContinueResize(form);
        }

        /// <summary>
        /// Detaches all event handlers monitoring a form resizing.
        /// <para/>This is used when the form size is being set programmatically.
        /// </summary>
        /// <param name="form">A form to detach the event handlers from.</param>
        static void HaltResize(Form form)
        {
            form.ResizeEnd -= form_ResizeEnd;
            form.Resize -= form_Resize;
        }

        /// <summary>
        /// Attaches all event handlers monitoring a form resizing.
        /// <para/>This is used after the form size was set set programmatically.
        /// </summary>
        /// <param name="form">A form to attach the event handlers to.</param>
        static void ContinueResize(Form form)
        {
            form.ResizeEnd += form_ResizeEnd;
            form.Resize += form_Resize;
        }

        /// <summary>
        /// Saves the last window state of a form before it is changed to a new value.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void form_Resize(object sender, EventArgs e)
        {
            Form form = (sender as Form);
            if (formWindowDefaults.Single((f) => f.Item2 == sender.GetType() && f.Item1 == form).Item3.LastState != GetWindowState(form))
            {
                formWindowDefaults.Single((f) => f.Item2 == sender.GetType() && f.Item1 == form).Item3.LastState = GetWindowState(form);
            }
        }

        /// <summary>
        /// Detach all avent handlers from a form when it's destroyed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void form_Disposed(object sender, EventArgs e)
        {
            Form form = (sender as Form);
            SetResizeEvents(form, false);
        }

        /// <summary>
        /// An event handler attached to form's Shown event.
        /// <para/>This load the positioning data from a file and sets it to the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void form_Shown(object sender, EventArgs e)
        {
            LoadPosition(sender as Form);
        }

        /// <summary>
        /// This event handler gets fired after a form has been resized.
        /// <para/>The form size is afterwards resized programmatically if needed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void form_ResizeEnd(object sender, EventArgs e)
        {
            Tuple<object, Type, WindowDefaults> formParams = formWindowDefaults.Single((f) => f.Item2 == sender.GetType() && f.Item1 == sender);

            ResizeFit(sender as Form);
        }

        /// <summary>
        /// Saves to form position after it was closed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            SavePosition(sender as Form);
        }

        /// <summary>
        /// Attaches an event handler for the SystemEvents.DisplaySettingsChanged
        /// <para/>to handle all application forms after display settings changed.
        /// </summary>
        public static void SetScreenChangeHandler()
        {
            SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        }

        /// <summary>
        /// Detaches all event handlers relating to window sizing and resizing of all open application forms.
        /// </summary>
        public static void ReleaseAllHandlers()
        {
            formWindowDefaults.Clear();
            foreach (Form form in System.Windows.Forms.Application.OpenForms)
            {
                SetResizeEvents(form, false);
            }
        }

        /// <summary>
        /// Occurs when the user changes the display settings.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        static void DisplaySettingsChanged(object sender, EventArgs e)
        {
            foreach (Form form in System.Windows.Forms.Application.OpenForms)
            {
                if (formWindowDefaults.Exists((f) => f.Item2 == form.GetType() && f.Item1 == form))
                {
                    ResizeFit(form);
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
