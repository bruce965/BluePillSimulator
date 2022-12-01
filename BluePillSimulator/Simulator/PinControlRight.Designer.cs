namespace BluePillSimulator.Simulator
{
    partial class PinControlRight
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // comboBoxInput
            // 
            this.comboBoxInput.Location = new System.Drawing.Point(214, 3);
            this.comboBoxInput.TabIndex = 3;
            this.comboBoxInput.Text = "NC";
            // 
            // textBoxMode
            // 
            this.textBoxMode.Location = new System.Drawing.Point(125, 3);
            // 
            // buttonToggle
            // 
            this.buttonToggle.Location = new System.Drawing.Point(274, 3);
            this.buttonToggle.TabIndex = 4;
            // 
            // labelPin
            // 
            this.labelPin.Location = new System.Drawing.Point(3, 3);
            this.labelPin.TabIndex = 0;
            this.labelPin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(65, 3);
            this.textBoxOutput.TabIndex = 1;
            // 
            // PinControlRight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "PinControlRight";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
