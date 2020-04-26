﻿//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
using LayoutFarm.RenderBoxes;
namespace LayoutFarm
{
    public enum BoxContentLayoutKind : byte
    {
        Absolute,
        VerticalStack,
        HorizontalStack,
        HorizontalFlow,
    }

    public interface IContainerRenderElement
    {
        void AddChild(RenderElement renderE);
        void AddFirst(RenderElement renderE);
        void InsertAfter(RenderElement afterElem, RenderElement renderE);
        void InsertBefore(RenderElement beforeElem, RenderElement renderE);
        void RemoveChild(RenderElement renderE);
        void ClearAllChildren();
        RootGraphic Root { get; }
    }


#if DEBUG
    [System.Diagnostics.DebuggerDisplay("RenderBoxBase {dbugGetCssBoxInfo}")]
#endif
    public abstract class RenderBoxBase : RenderElement, IContainerRenderElement
    {
        BoxContentLayoutKind _contentLayoutKind;
        PlainLayer _defaultLayer;
        public RenderBoxBase(RootGraphic rootgfx, int width, int height)
            : base(rootgfx, width, height)
        {
            this.MayHasViewport = true;
            this.MayHasChild = true;
        }

        public override void ChildrenHitTestCore(HitChain hitChain)
        {
            if (_defaultLayer != null)
            {
                _defaultLayer.HitTestCore(hitChain);
#if DEBUG
                debug_RecordLayerInfo(_defaultLayer);
#endif
            }
        }

        public override sealed void TopDownReCalculateContentSize()
        {
            //if (!ForceReArrange && this.HasCalculatedSize)
            //{
            //    return;
            //}
#if DEBUG
            dbug_EnterTopDownReCalculateContent(this);
#endif
            int cHeight = this.Height;
            int cWidth = this.Width;
            Size ground_contentSize = Size.Empty;
            if (_defaultLayer != null)
            {
                _defaultLayer.TopDownReCalculateContentSize();
                ground_contentSize = _defaultLayer.CalculatedContentSize;
            }
            int finalWidth = ground_contentSize.Width;
            if (finalWidth == 0)
            {
                finalWidth = this.Width;
            }
            int finalHeight = ground_contentSize.Height;
            if (finalHeight == 0)
            {
                finalHeight = this.Height;
            }
            switch (GetLayoutSpecificDimensionType(this))
            {
                case RenderElementConst.LY_HAS_SPC_HEIGHT:
                    {
                        finalHeight = cHeight;
                    }
                    break;
                case RenderElementConst.LY_HAS_SPC_WIDTH:
                    {
                        finalWidth = cWidth;
                    }
                    break;
                case RenderElementConst.LY_HAS_SPC_SIZE:
                    {
                        finalWidth = cWidth;
                        finalHeight = cHeight;
                    }
                    break;
            }


            SetCalculatedSize(this, finalWidth, finalHeight);
#if DEBUG
            dbug_ExitTopDownReCalculateContent(this);
#endif

        }
        public override void ResetRootGraphics(RootGraphic rootgfx)
        {
            if (this.Root != rootgfx)
            {
                DirectSetRootGraphics(this, rootgfx);
                if (_defaultLayer != null)
                {
                    foreach (var r in _defaultLayer.GetRenderElementIter())
                    {
                        r.ResetRootGraphics(rootgfx);
                    }
                }
            }
        }

        public virtual void AddChild(RenderElement renderE)
        {
            if (_defaultLayer == null)
            {
                _defaultLayer = new PlainLayer();
            }
            _defaultLayer.AddChild(this, renderE);
        }
        public virtual void AddFirst(RenderElement renderE)
        {
            if (_defaultLayer == null)
            {
                _defaultLayer = new PlainLayer();
            }
            _defaultLayer.AddFirst(this, renderE);
        }

        public virtual void InsertAfter(RenderElement afterElem, RenderElement renderE)
        {
            _defaultLayer.InsertChildAfter(this, afterElem, renderE);
        }
        public virtual void InsertBefore(RenderElement beforeElem, RenderElement renderE)
        {
            _defaultLayer.InsertChildBefore(this, beforeElem, renderE);
        }
        public virtual void RemoveChild(RenderElement renderE)
        {
            _defaultLayer?.RemoveChild(this, renderE);
        }
        public virtual void ClearAllChildren()
        {
            _defaultLayer?.Clear();
            this.InvalidateGraphics();
        }

        public override RenderElement FindUnderlyingSiblingAtPoint(Point point)
        {
            if (this.MyParentLink != null)
            {
                return this.MyParentLink.FindOverlapedChildElementAtPoint(this, point);
            }

            return null;
        }

        public override Size InnerContentSize
        {
            get
            {
                if (_defaultLayer != null)
                {
                    Size s1 = _defaultLayer.CalculatedContentSize;
                    int s1_w = s1.Width;
                    int s1_h = s1.Height;

                    if (s1_w < this.Width)
                    {
                        s1_w = this.Width;
                    }
                    if (s1_h < this.Height)
                    {
                        s1_h = this.Height;
                    }
                    return new Size(s1_w, s1_h);
                }
                else
                {
                    return this.Size;
                }
            }
        }



        protected override void RenderClientContent(DrawBoard d, UpdateArea updateArea)
        {
            if (_defaultLayer != null)
            {
#if DEBUG
                if (!debugBreaK1)
                {
                    debugBreaK1 = true;
                }
#endif
                _defaultLayer.DrawChildContent(d, updateArea);
            }
        }

        public BoxContentLayoutKind LayoutKind
        {
            get => _contentLayoutKind;
            set
            {
                _contentLayoutKind = value;
                if (_defaultLayer != null)
                {
                    _defaultLayer.LayoutKind = value;
                }
            }
        }
#if DEBUG
        public bool debugDefaultLayerHasChild
        {
            get
            {
                return _defaultLayer != null && _defaultLayer.dbugChildCount > 0;
            }
        }

        public static bool debugBreaK1;
        //-----------------------------------------------------------------
        public void dbugForceTopDownReArrangeContent()
        {
            dbug_EnterReArrangeContent(this);
            dbug_topDownReArrContentPass++;
            this.dbug_BeginArr++;
            debug_PushTopDownElement(this);
            this.MarkValidContentArrangement();
            //IsInTopDownReArrangePhase = true;
            if (_defaultLayer != null)
            {
                _defaultLayer.TopDownReArrangeContent();
            }

            // BoxEvaluateScrollBar();


            this.dbug_FinishArr++;
            debug_PopTopDownElement(this);
            dbug_ExitReArrangeContent();
        }
        public void dbugTopDownReArrangeContentIfNeed()
        {
            bool isIncr = false;
            if (!this.NeedContentArrangement)
            {
                if (!this.dbugFirstArrangementPass)
                {
                    this.dbugFirstArrangementPass = true;
                    dbug_WriteInfo(dbugVisitorMessage.PASS_FIRST_ARR);
                }
                else
                {
                    isIncr = true;
                    this.dbugVRoot.dbugNotNeedArrCount++;
                    this.dbugVRoot.dbugNotNeedArrCountEpisode++;
                    dbug_WriteInfo(dbugVisitorMessage.NOT_NEED_ARR);
                    this.dbugVRoot.dbugNotNeedArrCount--;
                }
                return;
            }

            dbugForceTopDownReArrangeContent();
            if (isIncr)
            {
                this.dbugVRoot.dbugNotNeedArrCount--;
            }
        }
        public override void dbug_DumpVisualProps(dbugLayoutMsgWriter writer)
        {
            base.dbug_DumpVisualProps(writer);
            writer.EnterNewLevel();
            writer.LeaveCurrentLevel();
        }
        void debug_RecordLayerInfo(RenderElementLayer layer)
        {
            RootGraphic visualroot = RootGraphic.dbugCurrentGlobalVRoot;
            if (visualroot.dbug_RecordDrawingChain)
            {
                visualroot.dbug_AddDrawLayer(layer);
            }
        }
        static int dbug_topDownReArrContentPass = 0;
#endif

    }
}
