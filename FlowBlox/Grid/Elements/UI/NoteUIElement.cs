using System;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.UserControls;

namespace FlowBlox.Grid.Elements.UI
{
    public class NoteUIElement : FlowBlockUIElement
    {
        public static Type GridElementType => typeof(NoteFlowBlock);

        private const int NoteElementHeight = 150;

        private System.Windows.Forms.RichTextBox tbNote = new System.Windows.Forms.RichTextBox();

        public NoteUIElement() : base() 
        {
            Initialize();
        }

        private void Initialize()
        {
            this.tbNote.ReadOnly = true;
            this.tbNote.BackColor = System.Drawing.Color.FloralWhite;
            this.tbNote.Dock = DockStyle.Fill;
            this.tbNote.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbNote.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tbNote.Name = "tbNote";
            this.tbNote.TabIndex = 1;
            this.Controls.Add(tbNote);
            this.tbNote.BringToFront();
        }

        public NoteUIElement(BaseFlowBlock baseGridElement) : base(baseGridElement)
        {
            Initialize();
        }

        public override void RefreshSize()
        {
            this.Height = NoteElementHeight;
            UpdateBackColor();
        }

        public override void UpdateContent(bool keepAnchor = false)
        {
            this.tbNote.Text =((NoteFlowBlock)this.InternalFlowBlock).Note;
            this.Name = this.InternalFlowBlock.Name;
            FlowBloxStyle.ApplyStyle(this);
            UpdateFlags();
            base.RefreshSize(keepAnchor);
        }
    }
}
