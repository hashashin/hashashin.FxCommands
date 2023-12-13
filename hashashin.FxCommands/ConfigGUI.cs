using System.Windows.Forms;

namespace hashashin.FxCommands
{
    partial class FxCommandsActionConfWnd
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox TextCommand;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            TextCommand = new TextBox();
            SuspendLayout();
            // 
            // TextCommand
            // 
            TextCommand.Location = new System.Drawing.Point(0, 79);
            TextCommand.Name = "TextCommand";
            TextCommand.Size = new System.Drawing.Size(815, 30);
            TextCommand.TabIndex = 9;
            TextCommand.Text = "";
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(TextCommand);
            Name = "FxCommandsActionConfWnd";
            Size = new System.Drawing.Size(818, 211);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}