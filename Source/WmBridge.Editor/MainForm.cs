using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WmBridge.Editor
{
    public partial class MainForm : Form
    {
        List<ConnectionEntry> connections = new List<ConnectionEntry>();
        Pen blackPen = new Pen(Blend(Color.Black, Color.FromKnownColor(KnownColor.Control), 0.125), 3);
        Brush blacBrush = new SolidBrush(Blend(Color.Black, Color.FromKnownColor(KnownColor.Control), 0.25));
        StringFormat centerStringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        ComboItem[] execPolicyItems = new[] {
            new ComboItem("Not Set", ""),
            new ComboItem("Restricted", "Restricted"),
            new ComboItem("All Signed", "AllSigned"),
            new ComboItem("Remote Signed", "RemoteSigned"),
            new ComboItem("Unrestricted", "Unrestricted"),
            new ComboItem("Bypass", "Bypass")
        };

        ConnectionEntry selectedEntry;
        bool isNew = true;
        bool savingCanceled;

        class ComboItem
        {
            public string Caption;
            public string Name;

            public override string ToString()
            {
                return Caption;
            }

            public ComboItem(string caption, string name)
            {
                Caption = caption;
                Name = name;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            blackPen.DashPattern = new float[] { 2, 2 };

            cmbExecPolicy.Items.AddRange(execPolicyItems);

            HookupControl(txtDescription, cmbGroup, txtHost, cmbUserName, cmbURL, cmbStartupScript);
        }

        void HookupControl(params Control[] c)
        {
            foreach (var item in c)
            {
                item.Leave += Inputs_Leave;
                item.KeyDown += Inputs_KeyDown;
            }
        }

        private void Inputs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                FillSelectedEntry();
                e.Handled = true;
            }
        }

        private void Inputs_Leave(object sender, EventArgs e)
        {
            FillSelectedEntry();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            NewFile();
        }

        class NameComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                int c = StringComparer.InvariantCultureIgnoreCase.Compare(x, y);

                if (c != 0 && (x == "" || y == ""))
                {
                    if (x == "")
                        return +1;
                    if (y == "")
                        return -1;
                }

                return c;
            }
        }

        void ApplyDpi()
        {
            float dx, dy;

            using (Graphics g = this.CreateGraphics())
            {
                dx = g.DpiX;
                dy = g.DpiY;

                int desired = (int)(21.0 / 96.0 * dy);

                if (treeView.ItemHeight != desired)
                {
                    treeView.ItemHeight = desired;
                    treeView.Indent = treeView.ItemHeight;
                }
            }
        }

        private void ReloadTree()
        {
            ApplyDpi();

            btnSort.Enabled = connections.Count > 0;

            var expanded = new HashSet<string>(treeView.Nodes.Cast<TreeNode>().Where(n => n.IsExpanded).Select(n => n.Text));

            SortedList<string, List<ConnectionEntry>> groups = new SortedList<string, List<ConnectionEntry>>(new NameComparer());
            foreach (var item in connections)
            {
                string key = item.GroupName ?? "";
                if (!groups.ContainsKey(key))
                    groups.Add(key, new List<ConnectionEntry>());

                groups[key].Add(item);
            }

            treeView.BeginUpdate();

            treeView.Nodes.Clear();

            foreach (var g in groups)
            {
                if (g.Key == "")
                    treeView.Nodes.AddRange(CreateChildNodes(g.Value).ToArray());
                else
                {
                    var n = new TreeNode(g.Key, 0, 0, CreateChildNodes(g.Value).ToArray());
                    if (expanded.Contains(n.Text))
                        n.Expand();
                    treeView.Nodes.Add(n);
                }
            }

            treeView.EndUpdate();

            FillCommonValues();
        }

        private IEnumerable<TreeNode> CreateChildNodes(IEnumerable<ConnectionEntry> list)
        {
            foreach (var item in list)
                yield return new TreeNode(item.ToString(), 1, 1) { Tag = item };
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AskAndSaveChanges() && openFileDialog.ShowDialog(this) == DialogResult.OK)
                Open();
        }

        private void Open()
        {
            try
            {
                connections = WinRMXFile.Load(openFileDialog.FileName);

                ReloadTree();

                saveFileDialog.FileName = openFileDialog.FileName;
                isNew = false;

                FileNameChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenCsv()
        {
            try
            {
                connections = CsvFile.Load(openFileDialogCsv.FileName, openFileDialogCsv.FilterIndex == 2);

                ReloadTree();

                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(openFileDialogCsv.FileName) + ".winrmx";
                isNew = true;

                FileNameChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                return;

            BindEntry(e.Node.Tag as ConnectionEntry);
        }

        private void BindEntry(ConnectionEntry entry)
        {
            selectedEntry = null;

            btnRemove.Enabled = entry != null;
            btnCopy.Enabled = btnRemove.Enabled;
            panelInputs.Enabled = btnRemove.Enabled;
            btnSort.Enabled = connections.Count > 0;

            if (entry != null)
            {
                txtDescription.Text = entry.Description;
                txtHost.Text = entry.HostName;
                checkCredSSP.Checked = entry.UseCredSSP;
                checkMonitor.Checked = entry.ShowAvailability;

                cmbGroup.Text = entry.GroupName;
                cmbStartupScript.Text = entry.StartupScript;
                cmbUserName.Text = entry.UserName;
                cmbURL.Text = entry.WebServiceURL;
                cmbExecPolicy.SelectedItem = execPolicyItems.Where(x => x.Name == (entry.ExecutionPolicy ?? "")).SingleOrDefault();
            }

            selectedEntry = entry;

            ValidateInputs();
        }

        private void ValidateInputs()
        {
            var controlTextColor = Color.FromKnownColor(KnownColor.ControlText);
            var errorTextColor = Color.Red;

            labHost.ForeColor = string.IsNullOrEmpty(txtHost.Text) ? errorTextColor : controlTextColor;
            labAccount.ForeColor = string.IsNullOrEmpty(cmbUserName.Text) ? errorTextColor : controlTextColor;
            labURL.ForeColor = string.IsNullOrEmpty(cmbURL.Text) ? errorTextColor : controlTextColor;
            labExecPolicy.ForeColor = string.IsNullOrEmpty(cmbExecPolicy.Text) ? errorTextColor : controlTextColor;
        }

        private void FillSelectedEntry()
        {
            ValidateInputs();

            if (selectedEntry != null)
            {
                var entry = selectedEntry;

                bool needReload = (entry.Description != txtDescription.Text ||
                    entry.HostName != txtHost.Text ||
                    entry.GroupName != cmbGroup.Text);

                entry.Description = txtDescription.Text;
                entry.HostName = txtHost.Text;
                entry.UseCredSSP = checkCredSSP.Checked;
                entry.ShowAvailability = checkMonitor.Checked;

                entry.GroupName = cmbGroup.Text;
                entry.StartupScript = cmbStartupScript.Text;
                entry.UserName = cmbUserName.Text;
                entry.WebServiceURL = cmbURL.Text;
                entry.ExecutionPolicy = ((ComboItem)cmbExecPolicy.SelectedItem).Name;

                if (needReload)
                {
                    ReloadTree();
                    treeView.SelectedNode = treeView.FindNode(entry);
                }
                else
                {
                    FillCommonValues();
                }
            }
        }

        class DistinctValues
        {
            HashSet<string> values = new HashSet<string>();
            Func<ConnectionEntry, string> selector;
            ComboBox cmb;

            public DistinctValues(Func<ConnectionEntry, string> selector, ComboBox cmb)
            {
                this.selector = selector;
                this.cmb = cmb;
            }

            public void Process(ConnectionEntry entry)
            {
                values.Add(selector(entry));
            }

            public void Bind()
            {
                cmb.Items.Clear();
                cmb.Items.AddRange(values.Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase).ToArray());
            }
        }

        private void FillCommonValues()
        {
            var dsts = new[] {
                new DistinctValues(x => x.GroupName, cmbGroup),
                new DistinctValues(x => x.StartupScript, cmbStartupScript),
                new DistinctValues(x => x.UserName, cmbUserName),
                new DistinctValues(x => x.WebServiceURL, cmbURL)};

            connections.ForEach(entry => Array.ForEach(dsts, x => x.Process(entry)));
            Array.ForEach(dsts, x => x.Bind());
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var item = new ConnectionEntry() {
                ExecutionPolicy = "Unrestricted",
                UseCredSSP = true,
                ShowAvailability = true,
                HostName = "localhost"
            };

            if (cmbURL.Items.Count == 1)
                item.WebServiceURL = cmbURL.Items[0].ToString();

            if (cmbUserName.Items.Count == 1)
                item.UserName = cmbUserName.Items[0].ToString();

            connections.Add(item);

            ReloadTree();

            treeView.SelectedNode = treeView.FindNode(item);

            txtHost.Focus();
            txtHost.SelectAll();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var entry = treeView.SelectedNode?.Tag as ConnectionEntry;

            if (entry != null)
            {
                treeView.SelectedNode.Remove();
                connections.Remove(entry);

                FillCommonValues();

                if (treeView.SelectedNode == null)
                    BindEntry(null);
                else
                {
                    // check if folder is empty
                    if (treeView.SelectedNode.ImageIndex == 0 && treeView.SelectedNode.Nodes.Count == 0)
                        treeView.SelectedNode.Remove();
                }
            }

            btnSort.Enabled = connections.Count > 0;
        }

        private void checkCredSSP_CheckedChanged(object sender, EventArgs e)
        {
            FillSelectedEntry();
        }

        private void checkMonitor_CheckedChanged(object sender, EventArgs e)
        {
            FillSelectedEntry();
        }

        private void cmbExecPolicy_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillSelectedEntry();
        }

        private void treeView_AfterDragDrop(object sender, DragEventArgs e)
        {
            CollectConnectionFromNodes();
            BindEntry(selectedEntry);
        }

        private void CollectConnectionFromNodes()
        {
            connections = treeView.GetNodesFlattened().Select(n =>
            {
                if (n.ImageIndex == 0 && n.Nodes.Count == 0) // clear empty folder
                    n.Remove();

                var e = n.Tag as ConnectionEntry;
                if (e != null)
                    e.GroupName = n.Parent?.Text;

                return n.Tag;

            }).OfType<ConnectionEntry>().ToList();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        void NewFile()
        {
            if (AskAndSaveChanges())
            {
                connections = new List<ConnectionEntry>();

                ReloadTree();
                BindEntry(null);

                saveFileDialog.FileName = "Untitled.winrmx";
                isNew = true;

                FileNameChanged();
            }
        }

        void FileNameChanged()
        {
            Text = Path.GetFileName(saveFileDialog.FileName);
        }

        bool ValidateAll()
        {
            foreach (var item in connections)
            {
                if (item.IsValid() == false)
                {
                    treeView.SelectedNode = treeView.FindNode(item);
                    MessageBox.Show("Fill missing values!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    savingCanceled = true;
                    return false;
                }
            }
            return true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValidateAndSave();
        }

        void ValidateAndSave()
        {
            if (ValidateAll())
            {
                if (isNew)
                    DoSaveAs();
                else
                    DoSave();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ValidateAll() == false)
                return;

            DoSaveAs();
        }

        private void DoSaveAs()
        {
            var mb = saveFileDialog.ShowDialog(this);
            if (mb == DialogResult.OK)
                DoSave();
            else if (mb == DialogResult.Cancel)
                savingCanceled = true;
        }

        private void DoSave()
        {
            try
            {
                WinRMXFile.Save(saveFileDialog.FileName, connections);
                isNew = false;
                FileNameChanged();
            }
            catch (Exception ex)
            {
                savingCanceled = true;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AskAndSaveChanges() // return false if action was canceled
        {
            if (isNew && connections.Count == 0)
                return true;

            savingCanceled = false;

            var mb = MessageBox.Show($"Save changes to \"{Path.GetFileName(saveFileDialog.FileName)}\"?", "WinRM", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

            if (mb == DialogResult.Cancel)
                return false;

            if (mb == DialogResult.Yes)
                ValidateAndSave();

            return !savingCanceled;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AskAndSaveChanges() == false)
                e.Cancel = true;
        }

        private void importCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AskAndSaveChanges() && openFileDialogCsv.ShowDialog(this) == DialogResult.OK)
            {
                OpenCsv();
            }
        }

        private void exportCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ValidateAll() == false)
                return;

            try
            {
                if (saveFileDialogCsv.ShowDialog(this) == DialogResult.OK)
                {
                    CsvFile.Save(saveFileDialogCsv.FileName, connections, saveFileDialogCsv.FilterIndex == 2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void panelDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = ((string[])e.Data.GetData(DataFormats.FileDrop)).FirstOrDefault() ?? "";

                if (file.EndsWith(".winrmx", StringComparison.InvariantCultureIgnoreCase) ||
                    file.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void panelDrop_DragDrop(object sender, DragEventArgs e)
        {
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop)).FirstOrDefault();

            if (!string.IsNullOrEmpty(file))
            {
                if (AskAndSaveChanges())
                {
                    if (file.EndsWith(".winrmx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        openFileDialog.FileName = file;
                        Open();
                    }
                    if (file.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        openFileDialogCsv.FileName = file;
                        OpenCsv();
                    }

                }
            }
        }

        private void panelDrop_Paint(object sender, PaintEventArgs e)
        {
            using (var path = RoundedRect(Rectangle.Inflate(panelDrop.ClientRectangle, -10, -10), 15))
            {
                e.Graphics.DrawString("Drop .winrmx, .csv files here", panelDrop.Font, blacBrush, 
                    new Point(panelDrop.ClientRectangle.Width / 2, panelDrop.ClientRectangle.Height / 2), centerStringFormat);

                var sm = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawPath(blackPen, path);
                e.Graphics.SmoothingMode = sm;
            }
        }

        private void panelDrop_Resize(object sender, EventArgs e)
        {
            panelDrop.Invalidate();
        }

        static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var entry = treeView.SelectedNode?.Tag as ConnectionEntry;

            if (entry != null)
            {
                var newEntry = entry.Clone();
                connections.Insert(connections.IndexOf(entry) + 1, newEntry);
                ReloadTree();
                treeView.SelectedNode = treeView.FindNode(newEntry);
            }
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            var entry = treeView.SelectedNode?.Tag as ConnectionEntry;

            connections = connections.OrderBy(x => x.ToString(), StringComparer.InvariantCultureIgnoreCase).ToList();
            ReloadTree();

            if (entry != null)
                treeView.SelectedNode = treeView.FindNode(entry);
        }

        private void treeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (treeView.SelectedNode == null)
                BindEntry(null);
        }

        static Color Blend(Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
    }
}
