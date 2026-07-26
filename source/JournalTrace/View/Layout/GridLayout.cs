using JournalTrace.Language;
using JournalTrace.Entry;
using System;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using JournalTrace.View.Util;

namespace JournalTrace.View.Layout
{
    public partial class GridLayout : UserControl, ILayout
    {
        private EntryManager entryManager;
        private bool filterDeleteClose;
        private bool filterRenameNew;
        private bool filterRenameOld;

        public GridLayout(EntryManager mngr)
        {
            this.entryManager = mngr;
            
            InitializeComponent();

            comboSearch.SelectedIndex = 1;
            //campos de tradução
            datagJournalEntries.Tag = new string[] { null, "name", "date", "reason", "directory" };
            comboSearch.Tag = new string[] { null, "name", "date", "reason", "directory" };
        }
        public void Clean()
        {
            datagJournalEntries.DataSource = null;
            dataSourceEntries.Clear();
            dataSourceEntries.Dispose();
            datagJournalEntries.Rows.Clear();
            datagJournalEntries.Dispose();
            GC.Collect();
        }

        public Control GetControl()
        {
            return this;
        }

        public DataTable dataSourceEntries;

        public async void LoadData(FormMain frm)
        {
            dataSourceEntries = new DataTable();

            dataSourceEntries.Columns.Add("USN", typeof(long));
            dataSourceEntries.Columns.Add("name", typeof(string));
            dataSourceEntries.Columns.Add("date", typeof(string));
            dataSourceEntries.Columns.Add("reason", typeof(string));
            dataSourceEntries.Columns.Add("directory", typeof(string));

            await Task.Run(() =>
            {
                foreach (var item in entryManager.USNEntries)
                {
                    USNEntry entry = item.Value;
                    dataSourceEntries.Rows.Add(entry.USN, entry.Name, entry.Time, entry.Reason, entryManager.parentFileReferenceIdentifiers[entry.ParentFileReference].ResolvedID);
                }
            });

            datagJournalEntries.DataSource = dataSourceEntries;

            LanguageManager.INSTANCE.UpdateControl(datagJournalEntries);

            //tamanho das colunas
            int[] widthColumns = new int[] { 88, 200, 115, 300, 500 };

            for (int i = 0; i < widthColumns.Length; i++)
            {
                datagJournalEntries.Columns[i].Width = widthColumns[i];
            }

            frm.ShowLayoutOption(true);
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            ApplyCombinedFilter();
        }

        public void ApplyReasonFilters(bool deleteClose, bool renameNew, bool renameOld)
        {
            filterDeleteClose = deleteClose;
            filterRenameNew = renameNew;
            filterRenameOld = renameOld;
            ApplyCombinedFilter();
        }

        private void ApplyCombinedFilter()
        {
            if (dataSourceEntries == null) return;

            List<string> clauses = new List<string>();
            string search = txtSearch.Text.Replace("'", "''");
            if (!string.IsNullOrWhiteSpace(search))
            {
                string column = dataSourceEntries.Columns[comboSearch.SelectedIndex].ColumnName;
                clauses.Add("CONVERT([" + column + "], 'System.String') LIKE '%" + search + "%'");
            }

            List<string> reasonClauses = new List<string>();
            if (filterDeleteClose)
                reasonClauses.Add("[reason] = 'File delete | Close'");
            if (filterRenameNew)
                reasonClauses.Add("([reason] = 'Rename: new name' OR [reason] = 'Rename: new name | Close')");
            if (filterRenameOld)
                reasonClauses.Add("[reason] = 'Rename: old name'");

            if (reasonClauses.Count > 0)
                clauses.Add("(" + string.Join(" OR ", reasonClauses) + ")");

            dataSourceEntries.DefaultView.RowFilter = string.Join(" AND ", clauses);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btSearch_Click(this, null);
            }
        }

        private void btSearchClear_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            ApplyCombinedFilter();
        }

        private void datagJournalEntries_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            ContextMenuHelper.INSTANCE.ShowContext(datagJournalEntries, e);
        }
    }
}