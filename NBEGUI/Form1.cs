using NXSBinEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NBEGUI {
    public partial class NBEGUI : Form {
        public NBEGUI() {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                try {
                    listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                } catch { }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            } catch { }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.ShowDialog();
        }

        BinHelper Editor;
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            Editor = new BinHelper(File.ReadAllBytes(openFileDialog1.FileName));

            listBox1.Items.Clear();
            foreach (string str in Editor.Import()) {
                listBox1.Items.Add(str);
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) { 
            List<string> Rst = new List<string>();
            foreach (string str in listBox1.Items)
                Rst.Add(str);

            File.WriteAllBytes(saveFileDialog1.FileName, Editor.Export(Rst.ToArray()));

            MessageBox.Show("Saved");
        }
    }
}
