namespace PR2_Speedrun_Tools
{
	partial class TokenManagerForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.usersList = new System.Windows.Forms.ListBox();
            this.removeBtn = new System.Windows.Forms.Button();
            this.addBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.userBox = new System.Windows.Forms.TextBox();
            this.tokenBox = new System.Windows.Forms.TextBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // usersList
            // 
            this.usersList.FormattingEnabled = true;
            this.usersList.Location = new System.Drawing.Point(12, 12);
            this.usersList.Name = "usersList";
            this.usersList.Size = new System.Drawing.Size(114, 95);
            this.usersList.TabIndex = 0;
            this.usersList.SelectedIndexChanged += new System.EventHandler(this.usersList_SelectedIndexChanged);
            // 
            // removeBtn
            // 
            this.removeBtn.Location = new System.Drawing.Point(132, 12);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new System.Drawing.Size(59, 23);
            this.removeBtn.TabIndex = 1;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = true;
            this.removeBtn.Click += new System.EventHandler(this.removeBtn_Click);
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(132, 84);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(59, 23);
            this.addBtn.TabIndex = 1;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 119);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "User:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 141);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Token:";
            // 
            // userBox
            // 
            this.userBox.Enabled = false;
            this.userBox.Location = new System.Drawing.Point(59, 116);
            this.userBox.Name = "userBox";
            this.userBox.Size = new System.Drawing.Size(95, 20);
            this.userBox.TabIndex = 3;
            this.userBox.TextChanged += new System.EventHandler(this.userBox_TextChanged);
            // 
            // tokenBox
            // 
            this.tokenBox.Enabled = false;
            this.tokenBox.Location = new System.Drawing.Point(59, 138);
            this.tokenBox.Name = "tokenBox";
            this.tokenBox.Size = new System.Drawing.Size(127, 20);
            this.tokenBox.TabIndex = 3;
            this.tokenBox.TextChanged += new System.EventHandler(this.tokenBox_TextChanged);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(111, 164);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 4;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // TokenManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 195);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.tokenBox);
            this.Controls.Add(this.userBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.removeBtn);
            this.Controls.Add(this.usersList);
            this.Name = "TokenManagerForm";
            this.Text = "Token Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Token_Manager_FormClosing);
            this.Load += new System.EventHandler(this.Token_Manager_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox usersList;
		private System.Windows.Forms.Button removeBtn;
		private System.Windows.Forms.Button addBtn;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox userBox;
		private System.Windows.Forms.TextBox tokenBox;
		private System.Windows.Forms.Button saveBtn;
	}
}