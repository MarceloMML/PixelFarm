﻿//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;

namespace PixelFarm.Forms
{
    public static class Application
    {
        public static void EnableVisualStyles() { }
        public static void SetCompatibleTextRenderingDefault(bool value) { }
        public static event EventHandler Idle;
        public static void Run(Form form) { }
        public static void Run(ApplicationContext appContext) { }
    }

    public class Timer
    {
        public void Dispose() { }
        public bool Enabled { get; set; }
        public int Interval { get; set; }
        public event EventHandler Tick;
    }
    public class FormClosedEventArgs : EventArgs { }
    public class PreviewKeyEventArgs : EventArgs { }
    public class ApplicationContext
    {
        Form _mainForm;
        public ApplicationContext() { }
        public ApplicationContext(Form mainForm)
        {
            this._mainForm = mainForm;
        }

    }
    public class ControlCollection
    {
        Control _owner;
        List<Control> _children = new List<Control>();
        internal ControlCollection(Control owner)
        {
            _owner = owner;
        }
        public void Add(Control c)
        {
            if (_owner == c)
            {
                throw new NotSupportedException();
            }
            //
            _children.Add(c);

        }
        public bool Remove(Control c)
        {
            return _children.Remove(c);
        }
        public void Clear()
        {
            _children.Clear();
        }
    }
    public class Form : Control
    {

        public Form()
        {
            CreateNativeCefWindowHandle();
        }
        public void Hide() { }

        void CreateNativeCefWindowHandle()
        {

        }
        public void Invoke(Delegate ac)
        {
        }
        public virtual void Close()
        {
        }
        public event EventHandler<FormClosingEventArgs> FormClosing;
        public event EventHandler<FormClosedEventArgs> FormClosed;
    }





    public class Control : IDisposable
    {
        int _width;
        int _height;
        IntPtr _nativeHandle;
        ControlCollection _controls;
        public Control()
        {
            _controls = new ControlCollection(this);
        }
        internal static void SetNativeHandle(Control c, IntPtr nativeHandle)
        {
            c._nativeHandle = nativeHandle;
            c.OnHandleCreated(EventArgs.Empty);
        }

        protected bool DesignMode { get; set; }
        protected virtual void OnHandleCreated(EventArgs e)
        {
        }
        public virtual void Show()
        {
        }
        protected virtual void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
        }
        protected virtual void OnSizeChanged(EventArgs e)
        {
        }
        public ControlCollection Controls => _controls;
        public void Focus()
        {
            //TODO: implement this
        }
        public virtual int Width
        {
            get => _width;
            set
            {
                _width = value;
                //TODO: implement this
            }
        }
        public virtual int Height
        {
            get => _height;
            set
            {
                _height = value;
                //TODO: implement this
            }
        }
        public bool IsHandleCreated { get; set; }
        public virtual IntPtr Handle
        {
            get
            {
                return _nativeHandle;
            }
        }
        public void Dispose() { }
        public void SetSize(int w, int h)
        {
        }
        public bool Visible { get; set; }
        public virtual string Text { get; set; }

        public virtual Control TopLevelControl
        {
            get;
            set;
        }
        public Control Parent { get; set; }

        protected virtual void OnLoad(EventArgs e)
        {
        }

        public static Control CreateFromNativeWindowHwnd(IntPtr hwnd)
        {
            Control newControl = new Control();
            Control.SetNativeHandle(newControl, hwnd);
            return newControl;
        }
        public static Form CreateFromNativeWindowHwnd2(IntPtr hwnd)
        {
            Form newControl = new Form();
            Control.SetNativeHandle(newControl, hwnd);
            return newControl;
        }
    }
    public class FormClosingEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }

    public class PreviewKeyDownEventArgs : EventArgs { }
}