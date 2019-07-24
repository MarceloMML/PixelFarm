﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using LayoutFarm.UI;
namespace LayoutFarm.CustomWidgets
{


    public class ListView : AbstractBox
    {


        public delegate void ListItemMouseHandler(object sender, UIMouseEventArgs e);
        public delegate void ListItemKeyboardHandler(object sender, UIKeyEventArgs e);

        //composite          

        List<ListItem> _items = new List<ListItem>();
        int _selectedIndex = -1;//default = no selection
        ListItem _selectedItem = null;
        public event ListItemMouseHandler ListItemMouseEvent;
        public event ListItemKeyboardHandler ListItemKeyboardEvent;

        public ListView(int width, int height)
            : base(width, height)
        {

#if DEBUG
            dbugBreakMe = true;
#endif
            this.ContentLayoutKind = BoxContentLayoutKind.VerticalStack;
            this.BackColor = Color.LightGray;
            this.MouseDown += panel_MouseDown;
            this.MouseDoubleClick += panel_MouseDoubleClick;
            this.AcceptKeyboardFocus = true;
            this.KeyDown += simpleBox_KeyDown;
            this.NeedClipArea = true;
        }


        void simpleBox_KeyDown(object sender, UIKeyEventArgs e)
        {
            if (_selectedItem != null && ListItemKeyboardEvent != null)
            {
                e.UIEventName = UIEventName.KeyDown;
                ListItemKeyboardEvent(this, e);
            }
        }
        void panel_MouseDoubleClick(object sender, UIMouseEventArgs e)
        {
            //raise event mouse double click
            var src = e.SourceHitElement as ListItem;
            if (src != null && ListItemMouseEvent != null)
            {
                e.UIEventName = UIEventName.DblClick;
                ListItemMouseEvent(this, e);
            }
        }
        void panel_MouseDown(object sender, UIMouseEventArgs e)
        {
            //check what item is selected
            var src = e.SourceHitElement as ListItem;
            if (src != null)
            {
                //make this as current selected list item
                //find index ?
                //TODO: review, for faster find list item index method
                int found = -1;
                for (int i = _items.Count - 1; i >= 0; --i)
                {
                    if (_items[i] == src)
                    {
                        found = i;
                        break;
                    }
                }
                if (found > -1)
                {
                    SelectedIndex = found;
                }
                if (ListItemMouseEvent != null)
                {
                    e.UIEventName = UIEventName.MouseDown;
                    ListItemMouseEvent(this, e);
                }
            }
        }

        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx)
        {
            if (!HasReadyRenderElement)
            {
                CustomRenderBox newone = base.GetPrimaryRenderElement(rootgfx) as CustomRenderBox;
                newone.LayoutHint = this.ContentLayoutKind;
                newone.HasSpecificWidthAndHeight = true;
                return newone;
            }
            else
            {
                return base.GetPrimaryRenderElement(rootgfx);
            }
        }
        public void AddItem(ListItem ui)
        {
            _items.Add(ui);
            AddChild(ui);
        }
        //
        public int ItemCount => _items.Count;
        //
        public void RemoveAt(int index)
        {
            var item = _items[index];
            RemoveChild(item);
            _items.RemoveAt(index);
        }
        public ListItem GetItem(int index)
        {
            if (index < 0)
            {
                return null;
            }
            else
            {
                return _items[index];
            }
        }
        public void Remove(ListItem item)
        {
            _items.Remove(item);
            RemoveChild(item);
        }
        public void ClearItems()
        {
            _selectedIndex = -1;
            _items.Clear();
            ClearChildren();
        }
        //----------------------------------------------------

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < this.ItemCount)
                {
                    if (value < 0)
                    {
                        value = -1;
                    }
                    //-----------------------------
                    if (_selectedIndex != value)
                    {
                        //1. current item
                        if (_selectedIndex > -1)
                        {
                            //switch back    
                            GetItem(_selectedIndex).BackColor = Color.LightGray;
                        }

                        _selectedIndex = value;
                        if (value == -1)
                        {
                            //no selection
                            _selectedItem = null;
                        }
                        else
                        {
                            //highlight selection item
                            _selectedItem = GetItem(value);
                            _selectedItem.BackColor = Color.Yellow;
                        }
                    }
                }
                else
                {
                    //do nothing 
                }
            }
        }

        public override int InnerHeight
        {
            get
            {
                if (_items.Count > 0)
                {
                    ListItem lastOne = _items[_items.Count - 1];
                    return lastOne.Bottom;
                }
                return this.Height;
            }
        }

        public void ScrollToSelectedItem()
        {
            //EnsureSelectedItemVisible();
            if (_selectedIndex > -1)
            {
                //find the item height
                int topPos = _selectedItem.Top;
                SetViewport(ViewportLeft, topPos);
            }
        }
        public void EnsureSelectedItemVisibleToTopItem()
        {
            if (_selectedIndex > -1)
            {
                //check if selected item is visible
                //if not bring them into view 
                int newtop = _selectedItem.Top;
                SetViewport(ViewportLeft, newtop);
            }

        }
        public void EnsureSelectedItemVisible()
        {
            if (_selectedIndex > -1)
            {
                //check if selected item is visible
                //if not bring them into view 
                if (_selectedItem.Top < ViewportTop)
                {
                    //must see entire item
                    int newtop = _selectedItem.Top - (Height / 3);
                    if (newtop < 0)
                    {
                        newtop = 0;
                    }
                    SetViewport(ViewportLeft, newtop);
                }
                else if (_selectedItem.Bottom > ViewportTop + Height)
                {
                    int newtop = _selectedItem.Top - (Height * 2 / 3);
                    if (newtop < 0)
                    {
                        newtop = 0;
                    }
                    SetViewport(ViewportLeft, newtop);
                }
            }

        }

    }


    public class ListItem : AbstractRectUI
    {
        CustomRenderBox _primElement;
        CustomTextRun _listItemText;
        string _itemText;
        Color _backColor;
        RequestFont _font;
        //
        public ListItem(int width, int height)
            : base(width, height)
        {
            this.TransparentAllMouseEvents = true;

        }
        //
        public override RenderElement CurrentPrimaryRenderElement => _primElement;
        //
        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx)
        {
            if (_primElement == null)
            {
                //1.
                var element = new CustomRenderBox(rootgfx, this.Width, this.Height);
                element.SetLocation(this.Left, this.Top);
                element.BackColor = _backColor;
                element.SetController(this);
                //
                _listItemText = new CustomTextRun(rootgfx, 200, this.Height);
                if (_font != null)
                {
                    _listItemText.RequestFont = _font;
                    //TODO: review how to find 
                    int blankLineHeight = (int)rootgfx.TextServices.MeasureBlankLineHeight(_font);
                    _listItemText.SetHeight(blankLineHeight);
                    element.SetHeight(blankLineHeight);
                }


                element.AddChild(_listItemText);
                _listItemText.TransparentForAllEvents = true;
                if (_itemText != null)
                {
                    _listItemText.Text = _itemText;
                }
                _primElement = element;
            }
            return _primElement;
        }
        public Color BackColor
        {
            get => _backColor;
            set
            {
                _backColor = value;
                if (HasReadyRenderElement)
                {
                    _primElement.BackColor = value;
                }
            }
        }
        public string Text
        {
            get => _itemText;
            set
            {
                //set content has some effect to its layout
                _itemText = value;
                if (_listItemText != null)
                {
                    _listItemText.Text = value;
                }
            }
        }
        public override void SetFont(RequestFont font)
        {
            //set content has some effect to its layout
            _font = font;
            if (font != null && HasReadyRenderElement)
            {
                _listItemText.RequestFont = font;
            }
        }
    }
}