namespace Sep2026.WinFormsApp
{
    partial class FrmStudent
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnClick = new Button();
            label1 = new Label();
            txtNo = new TextBox();
            txtName = new TextBox();
            label2 = new Label();
            label3 = new Label();
            txtClassNo = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // btnClick
            // 
            btnClick.Location = new Point(454, 282);
            btnClick.Name = "btnClick";
            btnClick.Size = new Size(150, 46);
            btnClick.TabIndex = 0;
            btnClick.Text = "Click";
            btnClick.UseVisualStyleBackColor = true;
            btnClick.Click += btnClick_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(91, 42);
            label1.Name = "label1";
            label1.Size = new Size(51, 32);
            label1.TabIndex = 1;
            label1.Text = "No.";
            // 
            // txtNo
            // 
            txtNo.AcceptsReturn = true;
            txtNo.Location = new Point(298, 39);
            txtNo.Name = "txtNo";
            txtNo.Size = new Size(200, 39);
            txtNo.TabIndex = 2;
            // 
            // txtName
            // 
            txtName.Location = new Point(298, 116);
            txtName.Name = "txtName";
            txtName.Size = new Size(200, 39);
            txtName.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(91, 119);
            label2.Name = "label2";
            label2.Size = new Size(78, 32);
            label2.TabIndex = 4;
            label2.Text = "Name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(91, 196);
            label3.Name = "label3";
            label3.Size = new Size(106, 32);
            label3.TabIndex = 5;
            label3.Text = "Class No";
            // 
            // txtClassNo
            // 
            txtClassNo.Location = new Point(298, 193);
            txtClassNo.Name = "txtClassNo";
            txtClassNo.Size = new Size(200, 39);
            txtClassNo.TabIndex = 6;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(298, 282);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(150, 46);
            btnSave.TabIndex = 7;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(142, 282);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(150, 46);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // FrmStudent
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(698, 364);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(txtClassNo);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtName);
            Controls.Add(txtNo);
            Controls.Add(label1);
            Controls.Add(btnClick);
            Name = "FrmStudent";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Student Form";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnClick;
        private Label label1;
        private TextBox txtNo;
        private TextBox txtName;
        private Label label2;
        private Label label3;
        private TextBox txtClassNo;
        private Button btnSave;
        private Button btnCancel;
    }
}
