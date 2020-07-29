﻿//Apache2, 2014-present, WinterDev
using System;
using Typography.Text;
namespace LayoutFarm.TextEditing
{
    partial class TextLineBox
    {
        //each textline has a special

        internal CharSource CharSource => _textFlowLayer._charSource;

        public TextRun CreateTextRun(int c)
        {
            //new char is add to internal char buffer
            return new TextRun(DefaultRunStyle, CharSource.NewSpan(c));
        }
        public TextRun CreateTextRun(char[] charbuff)
        {
            //new char is add to internal char buffer
            return new TextRun(DefaultRunStyle, CharSource.NewSpan(charbuff, 0, charbuff.Length));
        }
        public TextRun CreateTextRun(string text)
        {
            //new char is add to internal char buffer
            return new TextRun(DefaultRunStyle, CharSource.NewSpan(text));
        }
        public TextRun CreateTextRun(TextBufferSpan textspan)
        {
            return new TextRun(DefaultRunStyle, CharSource.NewSegment(textspan));
        }
        public TextRun CreateTextRun(ArraySegment<int> textspan)
        {
            return new TextRun(DefaultRunStyle, CharSource.NewSegment(textspan));
        }
        public TextRun CreateTextRun(ArraySegment<char> textspan)
        {
            return new TextRun(DefaultRunStyle, CharSource.NewSegment(textspan));
        }
        public void AddLast(Run v)
        {
            AddNormalRunToLast(v);
        }
        public void AddLineBreakAfterLastRun()
        {
            AddLineBreakAfter(this.LastRun);
        }
        public void AddLineBreakBeforeFirstRun()
        {
            AddLineBreakBefore(this.FirstRun);
        }
        public void AddFirst(Run v)
        {
            AddNormalRunToFirst(v);
        }

        RunStyle DefaultRunStyle => _textFlowLayer.DefaultRunStyle;

        public Run AddBefore(Run beforeVisRun, CharSpan v)
        {
            var newRun = new TextRun(DefaultRunStyle, v);
            AddBefore(beforeVisRun, newRun);
            return newRun;
        }

        public void AddBefore(Run beforeVisRun, Run v)
        {
            AddNormalRunBefore(beforeVisRun, v);
        }

        public TextRun AddAfter(Run afterVisRun, CopyRun v)
        {
            var newRun = CreateTextRun(v.RawContent);
            AddAfter(afterVisRun, newRun);
            return newRun;
        }
        public TextRun AddAfter(Run afterVisRun, CharSpan v)
        {
            var newRun = new TextRun(DefaultRunStyle, v);
            AddAfter(afterVisRun, newRun);
            return newRun;
        }
        public void AddAfter(Run afterVisRun, Run v)
        {
            AddNormalRunAfter(afterVisRun, v);
        }

    }
}