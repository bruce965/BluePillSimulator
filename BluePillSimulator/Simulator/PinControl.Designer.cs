namespace BluePillSimulator.Simulator
{
    partial class PinControl
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
            this.comboBoxInput = new System.Windows.Forms.ComboBox();
            this.textBoxMode = new System.Windows.Forms.TextBox();
            this.buttonToggle = new System.Windows.Forms.Button();
            this.labelPin = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // comboBoxInput
            // 
            this.comboBoxInput.FormattingEnabled = true;
            this.comboBoxInput.Items.AddRange(new object[] {
            "NC",
            "0V",
            "3.3V"});
            this.comboBoxInput.Location = new System.Drawing.Point(3, 3);
            this.comboBoxInput.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.comboBoxInput.Name = "comboBoxInput";
            this.comboBoxInput.Size = new System.Drawing.Size(60, 23);
            this.comboBoxInput.TabIndex = 0;
            this.comboBoxInput.Text = "NC";
            this.comboBoxInput.SelectedIndexChanged += new System.EventHandler(this.inputChanged);
            this.comboBoxInput.TextUpdate += new System.EventHandler(this.inputChanged);
            // 
            // textBoxMode
            // 
            this.textBoxMode.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxMode.Location = new System.Drawing.Point(92, 3);
            this.textBoxMode.Name = "textBoxMode";
            this.textBoxMode.ReadOnly = true;
            this.textBoxMode.Size = new System.Drawing.Size(83, 23);
            this.textBoxMode.TabIndex = 2;
            this.textBoxMode.TabStop = false;
            // 
            // buttonToggle
            // 
            this.buttonToggle.Location = new System.Drawing.Point(63, 3);
            this.buttonToggle.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.buttonToggle.Name = "buttonToggle";
            this.buttonToggle.Size = new System.Drawing.Size(23, 23);
            this.buttonToggle.TabIndex = 1;
            this.buttonToggle.Text = "T";
            this.buttonToggle.UseVisualStyleBackColor = true;
            this.buttonToggle.Click += new System.EventHandler(this.toggleInput);
            // 
            // labelPin
            // 
            this.labelPin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPin.Location = new System.Drawing.Point(241, 3);
            this.labelPin.Name = "labelPin";
            this.labelPin.Size = new System.Drawing.Size(56, 23);
            this.labelPin.TabIndex = 3;
            this.labelPin.Text = "PIN";
            this.labelPin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxOutput.Location = new System.Drawing.Point(181, 3);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.Size = new System.Drawing.Size(54, 23);
            this.textBoxOutput.TabIndex = 4;
            this.textBoxOutput.TabStop = false;
            this.textBoxOutput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // PinControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.labelPin);
            this.Controls.Add(this.buttonToggle);
            this.Controls.Add(this.textBoxMode);
            this.Controls.Add(this.comboBoxInput);
            this.Name = "PinControl";
            this.Size = new System.Drawing.Size(300, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected ComboBox comboBoxInput;
        protected TextBox textBoxMode;
        protected System.Windows.Forms.Button buttonToggle;
        protected Label labelPin;
        protected TextBox textBoxOutput;
    }
}
