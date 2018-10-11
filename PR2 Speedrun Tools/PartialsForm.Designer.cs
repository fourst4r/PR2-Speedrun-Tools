namespace PR2_Speedrun_Tools
{
    partial class PartialsForm
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
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtNameZone = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnFinalize = new System.Windows.Forms.Button();
            this.chkReplace = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numPlaceStart = new System.Windows.Forms.NumericUpDown();
            this.numSetEnd = new System.Windows.Forms.NumericUpDown();
            this.btnAddZone = new System.Windows.Forms.Button();
            this.numSetStart = new System.Windows.Forms.NumericUpDown();
            this.btnSetZone = new System.Windows.Forms.Button();
            this.listActions = new System.Windows.Forms.ListBox();
            this.listZones = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numPlaceStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSetEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSetStart)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(243, 214);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(42, 22);
            this.btnDown.TabIndex = 29;
            this.btnDown.Text = "down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(185, 214);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(56, 22);
            this.btnUp.TabIndex = 28;
            this.btnUp.Text = "move up";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 224);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Actions:";
            // 
            // txtNameZone
            // 
            this.txtNameZone.Enabled = false;
            this.txtNameZone.Location = new System.Drawing.Point(185, 160);
            this.txtNameZone.Name = "txtNameZone";
            this.txtNameZone.Size = new System.Drawing.Size(100, 20);
            this.txtNameZone.TabIndex = 26;
            this.txtNameZone.Text = "Zone 0";
            this.txtNameZone.TextChanged += new System.EventHandler(this.txtNameZone_TextChanged);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(90, 158);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 25;
            this.btnLoad.Text = "Load Zone";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(9, 158);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 24;
            this.btnSave.Text = "Save Zone";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnFinalize
            // 
            this.btnFinalize.Location = new System.Drawing.Point(210, 339);
            this.btnFinalize.Name = "btnFinalize";
            this.btnFinalize.Size = new System.Drawing.Size(75, 23);
            this.btnFinalize.TabIndex = 23;
            this.btnFinalize.Text = "Finalize";
            this.btnFinalize.UseVisualStyleBackColor = true;
            this.btnFinalize.Click += new System.EventHandler(this.btnFinalize_Click);
            // 
            // chkReplace
            // 
            this.chkReplace.AutoSize = true;
            this.chkReplace.Location = new System.Drawing.Point(218, 189);
            this.chkReplace.Name = "chkReplace";
            this.chkReplace.Size = new System.Drawing.Size(67, 17);
            this.chkReplace.TabIndex = 22;
            this.chkReplace.Text = "replace?";
            this.chkReplace.UseVisualStyleBackColor = true;
            this.chkReplace.CheckedChanged += new System.EventHandler(this.chkReplace_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "zone start        zone end";
            // 
            // numPlaceStart
            // 
            this.numPlaceStart.Location = new System.Drawing.Point(147, 188);
            this.numPlaceStart.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numPlaceStart.Name = "numPlaceStart";
            this.numPlaceStart.Size = new System.Drawing.Size(65, 20);
            this.numPlaceStart.TabIndex = 19;
            this.numPlaceStart.ValueChanged += new System.EventHandler(this.numPlaceStart_ValueChanged);
            // 
            // numSetEnd
            // 
            this.numSetEnd.Location = new System.Drawing.Point(79, 20);
            this.numSetEnd.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numSetEnd.Name = "numSetEnd";
            this.numSetEnd.Size = new System.Drawing.Size(65, 20);
            this.numSetEnd.TabIndex = 18;
            // 
            // btnAddZone
            // 
            this.btnAddZone.Location = new System.Drawing.Point(9, 185);
            this.btnAddZone.Name = "btnAddZone";
            this.btnAddZone.Size = new System.Drawing.Size(75, 23);
            this.btnAddZone.TabIndex = 16;
            this.btnAddZone.Text = "Place Zone";
            this.btnAddZone.UseVisualStyleBackColor = true;
            this.btnAddZone.Click += new System.EventHandler(this.btnAddZone_Click);
            // 
            // numSetStart
            // 
            this.numSetStart.Location = new System.Drawing.Point(9, 20);
            this.numSetStart.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numSetStart.Name = "numSetStart";
            this.numSetStart.Size = new System.Drawing.Size(65, 20);
            this.numSetStart.TabIndex = 17;
            // 
            // btnSetZone
            // 
            this.btnSetZone.Location = new System.Drawing.Point(150, 17);
            this.btnSetZone.Name = "btnSetZone";
            this.btnSetZone.Size = new System.Drawing.Size(75, 23);
            this.btnSetZone.TabIndex = 15;
            this.btnSetZone.Text = "Set Zone";
            this.btnSetZone.UseVisualStyleBackColor = true;
            this.btnSetZone.Click += new System.EventHandler(this.btnSetZone_Click);
            // 
            // listActions
            // 
            this.listActions.FormattingEnabled = true;
            this.listActions.Location = new System.Drawing.Point(9, 238);
            this.listActions.Name = "listActions";
            this.listActions.Size = new System.Drawing.Size(276, 95);
            this.listActions.TabIndex = 13;
            // 
            // listZones
            // 
            this.listZones.FormattingEnabled = true;
            this.listZones.Location = new System.Drawing.Point(9, 46);
            this.listZones.Name = "listZones";
            this.listZones.Size = new System.Drawing.Size(276, 108);
            this.listZones.TabIndex = 14;
            this.listZones.SelectedIndexChanged += new System.EventHandler(this.listZones_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(95, 190);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "zone start";
            // 
            // PartialsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(298, 372);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtNameZone);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnFinalize);
            this.Controls.Add(this.chkReplace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numPlaceStart);
            this.Controls.Add(this.numSetEnd);
            this.Controls.Add(this.btnAddZone);
            this.Controls.Add(this.numSetStart);
            this.Controls.Add(this.btnSetZone);
            this.Controls.Add(this.listActions);
            this.Controls.Add(this.listZones);
            this.Controls.Add(this.label3);
            this.Name = "PartialsForm";
            this.Text = "PartialsForm";
            this.Load += new System.EventHandler(this.PartialsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numPlaceStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSetEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSetStart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtNameZone;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnFinalize;
        private System.Windows.Forms.CheckBox chkReplace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numPlaceStart;
        private System.Windows.Forms.NumericUpDown numSetEnd;
        private System.Windows.Forms.Button btnAddZone;
        private System.Windows.Forms.NumericUpDown numSetStart;
        private System.Windows.Forms.Button btnSetZone;
        private System.Windows.Forms.ListBox listActions;
        private System.Windows.Forms.ListBox listZones;
        private System.Windows.Forms.Label label3;
    }
}