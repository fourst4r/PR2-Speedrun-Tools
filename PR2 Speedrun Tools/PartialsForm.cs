using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PR2_Speedrun_Tools
{
    public partial class PartialsForm : Form
    {
        public PartialsForm()
        {
            InitializeComponent();
        }

        public RecordedChannel mov;

        // Zones
        List<MovieZone> zones = new List<MovieZone>();

        // Functions that were in MovieEditor in DMM ME
        public byte[] GetPSave(int start, int end)
        {
            if (start < 0 || end > mov.Frames || start >= end)
            { MessageBox.Show("Start and end must be within the movie, with start being before end."); return null; }

            byte[] sBytes = new byte[(end - start) * 2 + 4];
            // Write length
            BitConverter.GetBytes(end - start).CopyTo(sBytes, 0);
            // Get first frame
            sBytes[5] = mov._Keys[start]; // It's at 5 because byte before indicates number of frames
            int lastFrameAt = 5;
            // Loop through remaining frames
            for (int i = start + 1; i < end; i++)
            {
                byte cFrame = mov._Keys[i];
                bool newFrame = true;
                if (cFrame == sBytes[lastFrameAt])
                {
                    if (sBytes[lastFrameAt - 1] < 255)
                    { sBytes[lastFrameAt - 1] += 1; newFrame = false; }
                }
                if (newFrame)
                {
                    lastFrameAt += 2;
                    sBytes[lastFrameAt] = cFrame;
                }
            }
            lastFrameAt += 1;
            Array.Resize(ref sBytes, lastFrameAt);

            return sBytes;
        }
        public void UsePSave(byte[] sBytes, int start, bool replace = false)
        {
            // Load data into a new byte array
            int cFrame = 0;
            byte[] lBytes = new byte[BitConverter.ToInt32(sBytes, 0)];
            for (int i = 5; i < sBytes.Length; i += 2)
            {
                for (int iF = -1; iF < sBytes[i - 1]; iF++)
                {
                    // Copy data to new array
                    lBytes[cFrame] = sBytes[i];
                    cFrame += 1;
                }
            }

            mov._Keys.InsertRange(start, lBytes);
            if (replace)
            {
                int delen = lBytes.Length;
                if (delen + start + lBytes.Length > mov._Keys.Count)
                    delen = mov._Keys.Count - (start + lBytes.Length);
                mov._Keys.RemoveRange(start + lBytes.Length, delen);
            }
        }

        // Loading
        private void PartialsForm_Load(object sender, EventArgs e)
        {
            MovieZone main = new MovieZone();
            main.start = 0;
            main.length = mov.Frames;
            main.name = "Main Zone";
            main.bytes = GetPSave(0, mov.Frames);

            zones.Add(main);
            listZones.Items.Add(main.name + " - length: " + main.length);
        }

        // Get a zone from current movie
        private void btnSetZone_Click(object sender, EventArgs e)
        {
            MovieZone z = new MovieZone();
            z.start = (int)numSetStart.Value;
            z.length = (int)(numSetEnd.Value - numSetStart.Value);
            z.bytes = GetPSave(z.start, z.start + z.length);
            z.name = "Zone " + zones.Count;

            zones.Add(z);
            listZones.Items.Add(z.name + " - length: " + z.length);
        }

        // Select a zone
        private MovieZone cZone
        { get { return zones[listZones.SelectedIndex]; } }
        private void listZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AutoChange)
                return;
            if (listZones.SelectedIndex == -1)
            { txtNameZone.Text = ""; txtNameZone.Enabled = false; return; }
            AutoChange = true;
            txtNameZone.Text = cZone.name;
            txtNameZone.Enabled = true;
            AutoChange = false;
        }
        // Change a name
        bool AutoChange = false;
        private void txtNameZone_TextChanged(object sender, EventArgs e)
        {
            if (AutoChange || listZones.SelectedIndex == -1)
                return;
            AutoChange = true;
            cZone.name = txtNameZone.Text;
            listZones.Items[listZones.SelectedIndex] = cZone.getStr();
            AutoChange = false;
        }


        List<ZoneAction> actions = new List<ZoneAction>();
        private ZoneAction cAction
        { get { return actions[listActions.SelectedIndex]; } }
        // Place a zone
        private void btnAddZone_Click(object sender, EventArgs e)
        {
            ZoneAction a = new ZoneAction();
            a.zone = cZone;
            a.placeAt = (int)numPlaceStart.Value;
            a.replace = chkReplace.Checked;

            actions.Add(a);
            listActions.Items.Add(a.getStr());
        }
        // Change action
        private void numPlaceStart_ValueChanged(object sender, EventArgs e)
        {
            if (listActions.SelectedIndex == -1)
                return;
            cAction.placeAt = (int)numPlaceStart.Value;
            listActions.Items[listActions.SelectedIndex] = cAction.getStr();
        }
        private void chkReplace_CheckedChanged(object sender, EventArgs e)
        {
            if (listActions.SelectedIndex == -1)
                return;
            cAction.replace = chkReplace.Checked;
            listActions.Items[listActions.SelectedIndex] = cAction.getStr();
        }

        // Save/load a zone
        const string tempSavePath = "C:\\Users\\Tommy\\Documents\\VB Programs\\PR2 Speedrun Tools\\";
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (listZones.SelectedIndex == -1)
            { MessageBox.Show("Select a zone to save!"); return; }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = tempSavePath;
            dialog.DefaultExt = ".pmf";
            dialog.Filter = "Partial Movie File|*.pmf";
            dialog.AddExtension = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                System.IO.File.WriteAllBytes(dialog.FileName, cZone.bytes);
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = tempSavePath;
            dialog.DefaultExt = ".pmf";
            dialog.Filter = "Partial Movie File|*.pmf";
            dialog.AddExtension = true;
            dialog.ShowDialog();

            if (!System.IO.File.Exists(dialog.FileName))
            { MessageBox.Show("File does not exist."); return; }

            MovieZone z = new MovieZone();
            z.name = dialog.SafeFileName;
            z.start = 0;
            z.bytes = System.IO.File.ReadAllBytes(dialog.FileName);
            z.length = BitConverter.ToInt32(z.bytes, 0);

            zones.Add(z);
            listZones.Items.Add(z.getStr());
        }

        // Move action up/down list
        private void btnUp_Click(object sender, EventArgs e)
        {
            if (listActions.SelectedIndex == 0)
                return;
            ZoneAction a = cAction;
            actions[listActions.SelectedIndex] = actions[listActions.SelectedIndex - 1];
            actions[listActions.SelectedIndex - 1] = a;
            // Update display
            listActions.Items[listActions.SelectedIndex] = cAction.getStr();
            listActions.Items[listActions.SelectedIndex - 1] = a.getStr();
            listActions.SelectedIndex -= 1;
        }
        private void btnDown_Click(object sender, EventArgs e)
        {
            if (listActions.SelectedIndex == listActions.Items.Count - 1)
                return;
            ZoneAction a = cAction;
            actions[listActions.SelectedIndex] = actions[listActions.SelectedIndex + 1];
            actions[listActions.SelectedIndex + 1] = a;
            // Update display
            listActions.Items[listActions.SelectedIndex] = cAction.getStr();
            listActions.Items[listActions.SelectedIndex + 1] = a.getStr();
            listActions.SelectedIndex += 1;
        }


        // Take action!
        private void btnFinalize_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < actions.Count; i++)
                UsePSave(actions[i].zone.bytes, actions[i].placeAt, actions[i].replace);

            MessageBox.Show("Actions taken.");
            listActions.Items.Clear();
            actions.Clear();
        }
    }

    public class MovieZone
    {
        public string name;

        public int start;
        public int length;
        public byte[] bytes;

        public string getStr()
        {
            return name + " - length: " + length;
        }
    }
    class ZoneAction
    {
        public MovieZone zone;
        public int placeAt;
        public bool replace;

        public string getStr()
        {
            string str = "insert at";
            if (replace) str = "replace at";
            str = zone.name + " (" + zone.length + "): " + str + placeAt;

            return str;
        }
    }
}
