﻿//MIT, 2014-2017, WinterDev
using System;
using System.Collections.Generic;
using System.Numerics;
using PixelFarm.Agg;
using PixelFarm.Drawing.Fonts;
using Typography.OpenFont;
using Typography.Rendering;
namespace SampleWinForms.UI
{
    class GlyphTriangleInfo
    {
        public GlyphTriangleInfo(int triangleId, EdgeLine e0, EdgeLine e1, EdgeLine e2, double centroidX, double centroidY)
        {
            this.Id = triangleId;
            this.E0 = e0;
            this.E1 = e1;
            this.E2 = e2;
            this.CentroidX = centroidX;
            this.CentroidY = centroidY;
        }
        public int Id { get; private set; }
        public double CentroidX { get; set; }
        public double CentroidY { get; set; }
        public EdgeLine E0 { get; set; }
        public EdgeLine E1 { get; set; }
        public EdgeLine E2 { get; set; }
        public override string ToString()
        {
            return this.CentroidX + "," + CentroidY;

        }
    }

    class DebugGlyphVisualizer : GlyphOutlineWalker
    {
        DebugGlyphVisualizerInfoView _infoView;

        //

        Typeface _typeface;
        float _sizeInPoint;
        GlyphPathBuilder builder;
        VertexStorePool _vxsPool = new VertexStorePool();
        CanvasPainter painter;
        float _pxscale;
        HintTechnique _latestHint;
        char _testChar;
        public CanvasPainter CanvasPainter { get { return painter; } set { painter = value; } }
        public void SetFont(Typeface typeface, float sizeInPoint)
        {
            _typeface = typeface;
            _sizeInPoint = sizeInPoint;
            builder = new GlyphPathBuilder(typeface);
            FillBackGround = true;//default 

        }

        public bool UseLcdTechnique { get; set; }
        public bool FillBackGround { get; set; }
        public bool DrawBorder { get; set; }
        public bool OffsetMinorX { get; set; }
        public bool ShowTess { get; set; }

        public string MinorOffsetInfo { get; set; }
        public DebugGlyphVisualizerInfoView VisualizeInfoView
        {
            get { return _infoView; }
            set
            {
                _infoView = value;
                value.Owner = this;
                value.RequestGlyphRender += (s, e) =>
                {
                    //refresh render output 
                    RenderChar(_testChar, _latestHint);
                };
            }
        }
        public void DrawMarker(float x, float y, PixelFarm.Drawing.Color color, float sizeInPx = 8)
        {
            painter.FillRectLBWH(x, y, sizeInPx, sizeInPx, color);
        }
        public float GlyphEdgeOffset { get; set; }
        public void RenderChar(char testChar, HintTechnique hint)
        {
            builder.SetHintTechnique(hint);
#if DEBUG
            GlyphBoneJoint.dbugTotalId = 0;//reset
            builder.dbugAlwaysDoCurveAnalysis = true;
#endif
            _infoView.Clear();
            _latestHint = hint;
            _testChar = testChar;
            //----------------------------------------------------
            builder.Build(testChar, _sizeInPoint);
            var txToVxs1 = new GlyphTranslatorToVxs();
            builder.GlyphEdgeOffset = this.GlyphEdgeOffset;

            builder.ReadShapes(txToVxs1);

#if DEBUG 
            var ps = txToVxs1.dbugGetPathWriter();
            _infoView.ShowOrgBorderInfo(ps.Vxs);
#endif
            VertexStore vxs = new VertexStore();

            txToVxs1.WriteOutput(vxs, _vxsPool);
            //----------------------------------------------------

            //----------------------------------------------------
            painter.UseSubPixelRendering = this.UseLcdTechnique;
            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            painter.Clear(PixelFarm.Drawing.Color.White);

            RectD bounds = new RectD();
            BoundingRect.GetBoundingRect(new VertexStoreSnap(vxs), ref bounds);
            //----------------------------------------------------
            float scale = _typeface.CalculateToPixelScaleFromPointSize(_sizeInPoint);
            _pxscale = scale;
            this._infoView.PxScale = scale;

            var leftControl = this.LeftXControl;
            var left2 = leftControl * scale;
            int floor_1 = (int)left2;
            float diff = left2 - floor_1;
            //----------------------------------------------------
            if (OffsetMinorX)
            {
                MinorOffsetInfo = left2.ToString() + " =>" + floor_1 + ",diff=" + diff;
            }
            else
            {
                MinorOffsetInfo = left2.ToString();
            }


            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            painter.Clear(PixelFarm.Drawing.Color.White);

            if (FillBackGround)
            {
                //5.2 
                painter.FillColor = PixelFarm.Drawing.Color.Black;

                float xpos = 5;// - diff;
                if (OffsetMinorX)
                {
                    xpos -= diff;
                }

                painter.SetOrigin(xpos, 10);
                painter.Fill(vxs);
            }
            if (DrawBorder)
            {
                //5.4  
                painter.StrokeColor = PixelFarm.Drawing.Color.Green;
                //user can specific border width here... 
                //5.5 
                painter.Draw(vxs);
                //--------------
                int markOnVertexNo = _infoView.DebugMarkVertexCommand;
                double x, y;
                vxs.GetVertex(markOnVertexNo, out x, out y);
                painter.FillRectLBWH(x, y, 4, 4, PixelFarm.Drawing.Color.Red);
                //--------------
                _infoView.ShowFlatternBorderInfo(vxs);
                //--------------
            }
#if DEBUG
            builder.dbugAlwaysDoCurveAnalysis = false;
#endif

            if (ShowTess)
            {
                RenderTessTesult();
            }

            //if (DrawDynamicOutline)
            //{
            //    GlyphDynamicOutline dynamicOutline = builder.LatestGlyphFitOutline;
            //    WalkDynamicOutline(painter, dynamicOutline, scale, DrawRegenerateOutline);

            //}

        }

        public void RenderTessTesult()
        {
#if DEBUG

            GlyphDynamicOutline dynamicOutline = builder.LatestGlyphFitOutline;
            if (dynamicOutline != null)
            {
                this.Walk(dynamicOutline);
            }
#endif
        }
        public float LeftXControl
        {
            get { return builder.LeftXControl; }
        }



        public bool DrawDynamicOutline { get; set; }
        public bool DrawRegenerateOutline { get; set; }
        public bool DrawEndLineHub { get; set; }
        public bool DrawPerpendicularLine { get; set; }
        //
#if DEBUG
        void DrawPointKind(CanvasPainter painter, GlyphPoint point)
        {
            switch (point.kind)
            {
                case PointKind.C3Start:
                case PointKind.C3End:
                case PointKind.C4Start:
                case PointKind.C4End:
                case PointKind.LineStart:
                case PointKind.LineStop:
                    painter.FillRectLBWH(point.x * _pxscale, point.y * _pxscale, 5, 5, PixelFarm.Drawing.Color.Red);
                    break;

            }
        }

        void DrawEdge(CanvasPainter painter, EdgeLine edge)
        {
            if (edge.IsOutside)
            {
                //free side      
                {
                    GlyphPoint p = edge.GlyphPoint_P;
                    GlyphPoint q = edge.GlyphPoint_Q;

                    DrawPointKind(painter, p);
                    DrawPointKind(painter, q);
                    _infoView.ShowEdge(edge);
                    switch (edge.SlopeKind)
                    {
                        default:
                            painter.StrokeColor = PixelFarm.Drawing.Color.Green;
                            break;
                        case LineSlopeKind.Vertical:
                            if (edge.IsLeftSide)
                            {
                                painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                            }
                            else
                            {
                                painter.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                            }
                            break;
                        case LineSlopeKind.Horizontal:

                            if (edge.IsUpper)
                            {
                                painter.StrokeColor = PixelFarm.Drawing.Color.Red;
                            }
                            else
                            {
                                //lower edge
                                painter.StrokeColor = PixelFarm.Drawing.Color.Magenta;
                            }
                            break;
                    }
                }
                float scale = this._pxscale;
                //show info: => edge point
                if (this.DrawPerpendicularLine && _infoView.HasDebugMark)
                {
                    double prevWidth = painter.StrokeWidth;
                    painter.StrokeWidth = 3;
                    painter.Line(edge.x0 * scale, edge.y0 * scale, edge.x1 * scale, edge.y1 * scale, PixelFarm.Drawing.Color.Yellow);
                    painter.StrokeWidth = prevWidth;
                    GlyphEdge glyphEdge = edge.dbugGlyphEdge;
                    if (glyphEdge != null)
                    {
                        //draw
                        GlyphPoint p = edge.GlyphPoint_P;
                        GlyphPoint q = edge.GlyphPoint_Q;

                        //
                        AssocBoneCollection p_bones = glyphEdge._P.dbugGetAssocBones();
                        if (p_bones != null)
                        {
                            Vector2 v2 = new Vector2(q.x, q.y);
                            foreach (GlyphBone b in p_bones)
                            {
                                Vector2 v3 = b.GetMidPoint();
                                painter.Line(v2.X * scale, v2.Y * scale, v3.X * scale, v3.Y * scale, PixelFarm.Drawing.Color.Yellow);
                            }
                        }

                        AssocBoneCollection q_bones = glyphEdge._Q.dbugGetAssocBones();
                        if (q_bones != null)
                        {
                            Vector2 v2 = new Vector2(p.x, p.y);
                            foreach (GlyphBone b in q_bones)
                            {

                                //Vector2 v2 = new Vector2(q.x, q.y);
                                Vector2 v3 = b.GetMidPoint();
                                painter.Line(v2.X * scale, v2.Y * scale, v3.X * scale, v3.Y * scale, PixelFarm.Drawing.Color.Green);
                            }
                        }

                        {
                            Vector2 orginal_MidPoint = glyphEdge.GetMidPoint() * _pxscale;
                            Vector2 newMidPoint = glyphEdge.GetNewMidPoint() * _pxscale;
                            painter.FillRectLBWH(newMidPoint.X, newMidPoint.Y, 3, 3, PixelFarm.Drawing.Color.Red);
                            painter.Line(newMidPoint.X, newMidPoint.Y, orginal_MidPoint.X, orginal_MidPoint.Y, PixelFarm.Drawing.Color.LightGray);


                            painter.FillRectLBWH(glyphEdge.newEdgeCut_P_X * _pxscale, glyphEdge.newEdgeCut_P_Y * _pxscale, 6, 6, PixelFarm.Drawing.Color.Blue);
                            painter.FillRectLBWH(glyphEdge.newEdgeCut_Q_X * _pxscale, glyphEdge.newEdgeCut_Q_Y * _pxscale, 6, 6, PixelFarm.Drawing.Color.Blue);

                        }
                    }
                }
                else
                {
                    painter.Line(edge.x0 * scale, edge.y0 * scale, edge.x1 * scale, edge.y1 * scale);
                }

                {


                    GlyphEdge glyphEdge = edge.dbugGlyphEdge;
                    if (glyphEdge != null)
                    {
                        GlyphPoint p = edge.GlyphPoint_P;
                        GlyphPoint q = edge.GlyphPoint_Q;
                        //---------   
                        {
                            Vector2 orginal_MidPoint = glyphEdge.GetMidPoint() * _pxscale;
                            Vector2 newMidPoint = glyphEdge.GetNewMidPoint() * _pxscale;
                            painter.FillRectLBWH(newMidPoint.X, newMidPoint.Y, 3, 3, PixelFarm.Drawing.Color.Red);
                            painter.Line(newMidPoint.X, newMidPoint.Y, orginal_MidPoint.X, orginal_MidPoint.Y, PixelFarm.Drawing.Color.LightGray);


                            painter.FillRectLBWH(glyphEdge.newEdgeCut_P_X * _pxscale, glyphEdge.newEdgeCut_P_Y * _pxscale, 4, 4, PixelFarm.Drawing.Color.Blue);
                            painter.FillRectLBWH(glyphEdge.newEdgeCut_Q_X * _pxscale, glyphEdge.newEdgeCut_Q_Y * _pxscale, 4, 4, PixelFarm.Drawing.Color.Blue);

                        }
                        //---------   
                        if (this.DrawPerpendicularLine)
                        {
                            {
                                //p
                                AssocBoneCollection p_bones = glyphEdge._P.dbugGetAssocBones();
                                PixelFarm.Drawing.Color cc = PixelFarm.Drawing.Color.Red;
                                switch (p_bones.CutPointKind)
                                {
                                    case BoneCutPointKind.MoreThanOnePerpendicularBones:
                                        cc = PixelFarm.Drawing.Color.Magenta;
                                        break;
                                    case BoneCutPointKind.NotPendicularCutPoint:
                                        cc = PixelFarm.Drawing.Color.Aqua;
                                        break;
                                    case BoneCutPointKind.PerpendicularToBoneGroup:
                                        cc = PixelFarm.Drawing.Color.Green;
                                        break;
                                }
                                Vector2 v2 = new Vector2(q.x, q.y);
                                Vector2 cutpoint = p_bones.CutPoint;

                                painter.Line(
                                    v2.X * _pxscale, v2.Y * _pxscale,
                                    cutpoint.X * _pxscale, cutpoint.Y * _pxscale,
                                    cc);
                            }

                            {
                                //q
                                AssocBoneCollection q_bones = glyphEdge._Q.dbugGetAssocBones();
                                PixelFarm.Drawing.Color cc = PixelFarm.Drawing.Color.Red;
                                switch (q_bones.CutPointKind)
                                {
                                    case BoneCutPointKind.MoreThanOnePerpendicularBones:
                                        cc = PixelFarm.Drawing.Color.Magenta;
                                        break;
                                    case BoneCutPointKind.NotPendicularCutPoint:
                                        cc = PixelFarm.Drawing.Color.Aqua;
                                        break;
                                    case BoneCutPointKind.PerpendicularToBoneGroup:
                                        cc = PixelFarm.Drawing.Color.Green;
                                        break;
                                }
                                Vector2 v2 = new Vector2(p.x, p.y);
                                Vector2 cutpoint = q_bones.CutPoint;
                                painter.Line(
                                    v2.X * _pxscale, v2.Y * _pxscale,
                                    cutpoint.X * _pxscale, cutpoint.Y * _pxscale,
                                    cc);
                            }
                        }
                    }

                }
            }
            else
            {
                //inside edge
                //switch (edge.SlopeKind)
                //{
                //    default:
                //        painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                //        break;
                //    case LineSlopeKind.Vertical:
                //        painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                //        break;
                //    case LineSlopeKind.Horizontal:
                //        painter.StrokeColor = PixelFarm.Drawing.Color.Yellow;
                //        break;
                //}
                painter.StrokeColor = PixelFarm.Drawing.Color.Gray;
                painter.Line(edge.x0 * _pxscale, edge.y0 * _pxscale, edge.x1 * _pxscale, edge.y1 * _pxscale);
            }
        }

        void DrawBoneJoint(CanvasPainter painter, GlyphBoneJoint joint)
        {
            //-------------- 
            EdgeLine p_contactEdge = joint.dbugGetEdge_P();
            //mid point
            Vector2 jointPos = joint.Position * _pxscale;//scaled joint pos
            painter.FillRectLBWH(jointPos.X, jointPos.Y, 4, 4, PixelFarm.Drawing.Color.Yellow);
            if (joint.TipEdgeP != null)
            {
                EdgeLine tipEdge = joint.TipEdgeP;
                float p_x = tipEdge.GlyphPoint_P.x * _pxscale;
                float p_y = tipEdge.GlyphPoint_P.y * _pxscale;
                float q_x = tipEdge.GlyphPoint_Q.x * _pxscale;
                float q_y = tipEdge.GlyphPoint_Q.y * _pxscale;

                //
                painter.Line(
                   jointPos.X, jointPos.Y,
                   p_x, p_y,
                   PixelFarm.Drawing.Color.White);
                painter.FillRectLBWH(p_x, p_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker

                //
                painter.Line(
                    jointPos.X, jointPos.Y,
                    q_x, q_y,
                    PixelFarm.Drawing.Color.White);
                painter.FillRectLBWH(q_x, q_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker
            }
            if (joint.TipEdgeQ != null)
            {
                EdgeLine tipEdge = joint.TipEdgeQ;

                float p_x = tipEdge.GlyphPoint_P.x * _pxscale;
                float p_y = tipEdge.GlyphPoint_P.y * _pxscale;
                float q_x = tipEdge.GlyphPoint_Q.x * _pxscale;
                float q_y = tipEdge.GlyphPoint_Q.y * _pxscale;

                //
                painter.Line(
                   jointPos.X, jointPos.Y,
                   p_x, p_y,
                   PixelFarm.Drawing.Color.White);
                painter.FillRectLBWH(p_x, p_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker

                //
                painter.Line(
                    jointPos.X, jointPos.Y,
                    q_x, q_y,
                    PixelFarm.Drawing.Color.White);
                painter.FillRectLBWH(q_x, q_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker
            }

        }

        Vector2 _branchHeadPos;
        protected override void OnBeginDrawingBoneLinks(System.Numerics.Vector2 branchHeadPos, int startAt, int endAt)
        {
            _branchHeadPos = branchHeadPos;
        }
        protected override void OnEndDrawingBoneLinks()
        {

        }


        protected override void OnDrawBone(GlyphBone bone, int boneIndex)
        {

            float newRelativeLen = 1.5f;
            float pxscale = this._pxscale;
            GlyphBoneJoint jointA = bone.JointA;
            GlyphBoneJoint jointB = bone.JointB;

            bool valid = false;
            if (jointA != null && jointB != null)
            {

                Vector2 jointAPoint = jointA.Position;
                Vector2 jointBPoint = jointB.Position;

                painter.Line(
                    jointAPoint.X * pxscale, jointAPoint.Y * pxscale,
                    jointBPoint.X * pxscale, jointBPoint.Y * pxscale,
                    bone.IsLongBone ? PixelFarm.Drawing.Color.Yellow : PixelFarm.Drawing.Color.Magenta);
                valid = true;

                _infoView.ShowBone(bone, jointA, jointB);
            }
            if (jointA != null && bone.TipEdge != null)
            {
                Vector2 jointAPoint = jointA.Position;
                Vector2 mid = bone.TipEdge.GetMidPoint();

                painter.Line(
                    jointAPoint.X * pxscale, jointAPoint.Y * pxscale,
                    mid.X * pxscale, mid.Y * pxscale,
                    bone.IsLongBone ? PixelFarm.Drawing.Color.Yellow : PixelFarm.Drawing.Color.Magenta);
                valid = true;
                _infoView.ShowBone(bone, jointA, bone.TipEdge);
            }



            //if (bone.PerpendicularEdge != null)
            //{
            //    Vector2 midBone = bone.GetMidPoint() * pxscale;
            //    Vector2 cut_point = bone.cutPoint_onEdge * pxscale;
            //    painter.Line(
            //        cut_point.X, cut_point.Y,
            //        midBone.X, midBone.Y,
            //        PixelFarm.Drawing.Color.White);

            //    //draw new line
            //    Vector2 delta = bone.cutPoint_onEdge - bone.GetMidPoint();
            //    double currentLen = delta.Length(); //unscale version
            //    delta = delta.NewLength(currentLen * newRelativeLen);
            //    //
            //    Vector2 v2 = midBone + (delta * pxscale);
            //    painter.Line(
            //        midBone.X, midBone.Y,
            //        v2.X, v2.Y,
            //        PixelFarm.Drawing.Color.Red);

            //    //----------------------                
            //    //create green point at mid of GlyphEdge
            //    Vector2 midEdge = bone.PerpendicularEdge.GetMidPoint();
            //    painter.FillRectLBWH(midEdge.X * pxscale, midEdge.Y * pxscale, 5, 5, PixelFarm.Drawing.Color.Green);

            //    //----------------------                
            //    //Vector2 delta3 = bone.cutPoint_onEdge - bone.GetMidPoint();
            //    //delta3 = delta3.NewLength(100);// currentLen * newRelativeLen * 0.5);
            //    //Vector2 midEdge2 = midEdge + delta3;
            //    //painter.FillRectLBWH(midEdge2.X * pxscale, midEdge2.Y * pxscale, 5, 5, PixelFarm.Drawing.Color.Red);
            //    //painter.Line(midEdge.X * pxscale, midEdge.Y * pxscale,
            //    //     midEdge2.X * pxscale, midEdge2.Y * pxscale, PixelFarm.Drawing.Color.Blue);
            //    //----------------------


            //    //draw marker at midEdge
            //    //create a new perpendicular line
            //    //
            //    //create a new perpendicular line
            //    //------------------------------------------
            //    //create a line that parallel with the bone
            //    //Vector2 boneVector = bone.GetBoneVector();
            //    //var boneLen = boneVector.Length();
            //    //Vector2 boneVec2 = boneVector.NewLength(boneLen * 0.5);
            //    //Vector2 v2up = v2 + boneVec2;
            //    //Vector2 v2down = v2 - boneVec2;


            //    ////test only
            //    //painter.Line(
            //    //   v2.X, v2.Y,
            //    //   v2up.X, v2up.Y,
            //    //   PixelFarm.Drawing.Color.Red);

            //    ////test only
            //    //painter.Line(
            //    //   v2.X, v2.Y,
            //    //   v2down.X, v2down.Y,
            //    //   PixelFarm.Drawing.Color.Red);
            //    //////------------------------------------------

            //}


            if (boneIndex == 0)
            {
                //for first bone 
                painter.FillRectLBWH(_branchHeadPos.X * pxscale, _branchHeadPos.Y * pxscale, 5, 5, PixelFarm.Drawing.Color.DeepPink);
            }
            if (!valid)
            {
                throw new NotSupportedException();
            }
        }


        protected override void OnTriangle(int triangleId, EdgeLine e0, EdgeLine e1, EdgeLine e2, double centroidX, double centroidY)
        {

            DrawEdge(painter, e0);
            DrawEdge(painter, e1);
            DrawEdge(painter, e2);

            _infoView.ShowTriangles(new GlyphTriangleInfo(triangleId, e0, e1, e2, centroidX, centroidY));

        }

        protected override void OnGlyphEdgeN(GlyphEdge e)
        {
            float pxscale = this._pxscale;
            Vector2 cut_p = new Vector2(e.newEdgeCut_P_X, e.newEdgeCut_P_Y) * pxscale;
            Vector2 cut_q = new Vector2(e.newEdgeCut_Q_X, e.newEdgeCut_Q_Y) * pxscale;


            painter.FillRectLBWH(cut_p.X, cut_p.Y, 3, 3, PixelFarm.Drawing.Color.Red);
            //painter.FillRectLBWH(x1 * pxscale, y1 * pxscale, 6, 6, PixelFarm.Drawing.Color.OrangeRed);

            _infoView.ShowGlyphEdge(e,
                e.newEdgeCut_P_X, e.newEdgeCut_P_Y,
                e.newEdgeCut_Q_X, e.newEdgeCut_Q_Y);
        }
        protected override void OnCentroidLine(double px, double py, double qx, double qy)
        {

            float pxscale = this._pxscale;
            painter.Line(
                px * pxscale, py * pxscale,
                qx * pxscale, qy * pxscale,
                PixelFarm.Drawing.Color.Red);

            painter.FillRectLBWH(px * pxscale, py * pxscale, 2, 2, PixelFarm.Drawing.Color.Yellow);
            painter.FillRectLBWH(qx * pxscale, qy * pxscale, 2, 2, PixelFarm.Drawing.Color.Yellow);
        }
        protected override void OnCentroidLineTip_P(double px, double py, double tip_px, double tip_py)
        {
            float pxscale = this._pxscale;
            painter.Line(px * pxscale, py * pxscale,
                         tip_px * pxscale, tip_py * pxscale,
                         PixelFarm.Drawing.Color.Blue);
        }
        protected override void OnCentroidLineTip_Q(double qx, double qy, double tip_qx, double tip_qy)
        {
            float pxscale = this._pxscale;
            painter.Line(qx * pxscale, qy * pxscale,
                         tip_qx * pxscale, tip_qy * pxscale,
                         PixelFarm.Drawing.Color.Green);
        }
        protected override void OnBoneJoint(GlyphBoneJoint joint)
        {
            DrawBoneJoint(painter, joint);
            _infoView.ShowJoint(joint);
        }
        //----------------------
        protected override void OnBegingLineHub(float centerX, float centerY)
        {

        }
        protected override void OnEndLineHub(float centerX, float centerY, GlyphBoneJoint joint)
        {

            if (DrawEndLineHub)
            {
                //line hub cebter
                painter.FillRectLBWH(centerX * _pxscale, centerY * _pxscale, 7, 7,
                       PixelFarm.Drawing.Color.White);
                //this line hub is connected with other line hub at joint
                if (joint != null)
                {
                    Vector2 joint_pos = joint.Position;
                    painter.Line(
                            joint_pos.X * _pxscale, joint_pos.Y * _pxscale,
                            centerX * _pxscale, centerY * _pxscale,
                            PixelFarm.Drawing.Color.Magenta);
                }
            }
        }

        //        public void WalkDynamicOutline(CanvasPainter painter, GlyphDynamicOutline dynamicOutline, float pxscale, bool withRegenerateOutlines)
        //        {

        //#if DEBUG
        //            dynamicOutline.dbugDrawRegeneratedOutlines = withRegenerateOutlines;
        //#endif
        //             //dynamicOutline.Walk();
        //        }
        //void DrawBoneRib(CanvasPainter painter, Vector2 vec, GlyphBoneJoint joint, float pixelScale)
        //{
        //    Vector2 jointPos = joint.Position;
        //    painter.FillRectLBWH(vec.X * pixelScale, vec.Y * pixelScale, 4, 4, PixelFarm.Drawing.Color.Green);
        //    painter.Line(jointPos.X * pixelScale, jointPos.Y * pixelScale,
        //        vec.X * pixelScale,
        //        vec.Y * pixelScale, PixelFarm.Drawing.Color.White);
        //}

#endif

        static System.Drawing.PointF FindCutPoint(System.Drawing.PointF p0, System.Drawing.PointF p1, System.Drawing.PointF p2, float cutAngle)
        {
            //a line from p0 to p1
            //p2 is any point
            //return p3 -> cutpoint on p0,p1

            //from line equation
            //y = mx + b ... (1)
            //from (1)
            //b = y- mx ... (2) 
            //----------------------------------
            //line1:
            //y1 = (m1 * x1) + b1 ...(3)            
            //line2:
            //y2 = (m2 * x2) + b2 ...(4)
            //----------------------------------
            //from (3),
            //b1 = y1 - (m1 * x1) ...(5)
            //b2 = y2 - (m2 * x2) ...(6)
            //----------------------------------
            //y1diff = p1.Y-p0.Y  ...(7)
            //x1diff = p1.X-p0.X  ...(8)
            //
            //m1 = (y1diff/x1diff) ...(9)
            //m2 = cutAngle of m1 ...(10)
            //
            //replace value (x1,y1) and (x2,y2)
            //we know b1 and b2         
            //----------------------------------              
            //at cutpoint of line1 and line2 => (x1,y1)== (x2,y2)
            //or find (x,y) where (3)==(4)
            //---------------------------------- 
            //at cutpoint, find x
            // (m1 * x1) + b1 = (m2 * x1) + b2  ...(11), replace x2 with x1
            // (m1 * x1) - (m2 * x1) = b2 - b1  ...(12)
            //  x1 * (m1-m2) = b2 - b1          ...(13)
            //  x1 = (b2-b1)/(m1-m2)            ...(14), now we know x1
            //---------------------------------- 
            //at cutpoint, find y
            //  y1 = (m1 * x1) + b1 ... (15), replace x1 with value from (14)
            //Ans: (x1,y1)
            //---------------------------------- 

            double y1diff = p1.Y - p0.Y;
            double x1diff = p1.X - p0.X;

            if (x1diff == 0)
            {
                //90 or 180 degree
                return new System.Drawing.PointF(p1.X, p2.Y);
            }
            //------------------------------
            //
            //find slope 
            double m1 = y1diff / x1diff;
            //from (2) b = y-mx, and (5)
            //so ...
            double b1 = p0.Y - (m1 * p0.X);
            // 
            //from (10)
            //double invert_m = -(1 / slope_m);
            //double m2 = -1 / m1;   //rotate m1
            //---------------------
            double angle = Math.Atan2(y1diff, x1diff); //rad in degree 
                                                       //double m2 = -1 / m1;

            double m2 = cutAngle == 90 ?
                //short cut
                (-1 / m1) :
                //or 
                Math.Tan(
                //radial_angle of original line + radial of cutAngle
                //return new line slope
                Math.Atan2(y1diff, x1diff) +
                DegreesToRadians(cutAngle)); //new m 
                                             //---------------------


            //from (6)
            double b2 = p2.Y - (m2) * p2.X;
            //find cut point

            //check if (m1-m2 !=0)
            double cutx = (b2 - b1) / (m1 - m2); //from  (14)
            double cuty = (m1 * cutx) + b1;  //from (15)
            return new System.Drawing.PointF((float)cutx, (float)cuty);


            //------
            //at cutpoint of line1 and line2 => (x1,y1)== (x2,y2)
            //or find (x,y) where (3)==(4)
            //-----
            //if (3)==(4)
            //(m1 * x1) + b1 = (m2 * x2) + b2;
            //from given p0 and p1,
            //now we know m1 and b1, ( from (2),  b1 = y1-(m1*x1) )
            //and we now m2 since => it is a 90 degree of m1.
            //and we also know x2, since at the cut point x2 also =x1
            //now we can find b2...
            // (m1 * x1) + b1 = (m2 * x1) + b2  ...(5), replace x2 with x1
            // b2 = (m1 * x1) + b1 - (m2 * x1)  ...(6), move  (m2 * x1)
            // b2 = ((m1 - m2) * x1) + b1       ...(7), we can find b2
            //---------------------------------------------
        }
        static System.Drawing.PointF FindCutPoint(
            System.Drawing.PointF p0, System.Drawing.PointF p1,
            System.Drawing.PointF p2, System.Drawing.PointF p3)
        {
            //find cut point of 2 line 
            //y = mx + b
            //from line equation
            //y = mx + b ... (1)
            //from (1)
            //b = y- mx ... (2) 
            //----------------------------------
            //line1:
            //y1 = (m1 * x1) + b1 ...(3)            
            //line2:
            //y2 = (m2 * x2) + b2 ...(4)
            //----------------------------------
            //from (3),
            //b1 = y1 - (m1 * x1) ...(5)
            //b2 = y2 - (m2 * x2) ...(6)
            //----------------------------------
            //at cutpoint of line1 and line2 => (x1,y1)== (x2,y2)
            //or find (x,y) where (3)==(4)
            //---------------------------------- 
            //at cutpoint, find x
            // (m1 * x1) + b1 = (m2 * x1) + b2  ...(11), replace x2 with x1
            // (m1 * x1) - (m2 * x1) = b2 - b1  ...(12)
            //  x1 * (m1-m2) = b2 - b1          ...(13)
            //  x1 = (b2-b1)/(m1-m2)            ...(14), now we know x1
            //---------------------------------- 
            //at cutpoint, find y
            //  y1 = (m1 * x1) + b1 ... (15), replace x1 with value from (14)
            //Ans: (x1,y1)
            //----------------------------------

            double y1diff = p1.Y - p0.Y;
            double x1diff = p1.X - p0.X;


            if (x1diff == 0)
            {
                //90 or 180 degree
                return new System.Drawing.PointF(p1.X, p2.Y);
            }
            //------------------------------
            //
            //find slope 
            double m1 = y1diff / x1diff;
            //from (2) b = y-mx, and (5)
            //so ...
            double b1 = p0.Y - (m1 * p0.X);

            //------------------------------
            double y2diff = p3.Y - p2.Y;
            double x2diff = p3.X - p2.X;
            double m2 = y2diff / x2diff;

            // 
            //from (6)
            double b2 = p2.Y - (m2) * p2.X;
            //find cut point

            //check if (m1-m2 !=0)
            double cutx = (b2 - b1) / (m1 - m2); //from  (14)
            double cuty = (m1 * cutx) + b1;  //from (15)
            return new System.Drawing.PointF((float)cutx, (float)cuty);

        }
        const double degToRad = System.Math.PI / 180.0f;
        const double radToDeg = 180.0f / System.Math.PI;
        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees">An angle in degrees</param>
        /// <returns>The angle expressed in radians</returns>
        public static double DegreesToRadians(double degrees)
        {

            return degrees * degToRad;
        }

        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        /// <returns>The angle expressed in degrees</returns>
        public static double RadiansToDegrees(double radians)
        {

            return radians * radToDeg;
        }
    }

}