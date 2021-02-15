#region License
/*
PosLib

A library for storing and loading application's window positioning.
Copyright (C) 2018 VPKSoft, Petteri Kautonen

Contact: vpksoft@vpksoft.net

This file is part of PosLib.

PosLib is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PosLib is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with PosLib.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VPKSoft.PosLib;

namespace PosTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            // Add this form to be positioned..
            PositionForms.Add(this, PositionCore.SizeChangeMode.MoveTopLeft);

            InitializeComponent();

            ListViewItem lvi = new ListViewItem("Test row 1");
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "Test value 1"));
            listView1.Items.Add(lvi);
            lvi = new ListViewItem("Test row 2");
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "Test value 2"));
            listView1.Items.Add(lvi);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void testForm2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormTest().Show();
        }
    }
}
