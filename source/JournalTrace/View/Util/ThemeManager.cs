using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JournalTrace.View.Util
{
    /// <summary>
    /// Applies the application's fixed purple color scheme to a control tree
    /// and colors the native window title bar to match.
    /// This is a small, dependency-free "theming" helper: it just walks every
    /// control recursively and sets sensible colors based on its type.
    /// The app no longer supports switching themes - purple is always on.
    /// </summary>
    public static class ThemeManager
    {
        // ---- Purple palette ----
        private static readonly Color Back = Color.FromArgb(24, 18, 38);          // window background
        private static readonly Color Fore = Color.FromArgb(232, 228, 240);       // main text (white/greyish)
        private static readonly Color MenuBack = Color.FromArgb(38, 28, 58);      // menus/toolbars/status bar
        private static readonly Color InputBack = Color.FromArgb(32, 24, 50);     // textboxes/grids/trees
        private static readonly Color InputFore = Color.FromArgb(225, 220, 235);  // text inside inputs
        private static readonly Color GridHeaderBack = Color.FromArgb(48, 36, 74);
        private static readonly Color GridLines = Color.FromArgb(74, 58, 104);
        private static readonly Color Accent = Color.FromArgb(92, 72, 126);       // calm purple accent
        private static readonly Color AccentHover = Color.FromArgb(72, 58, 98);   // neutral hover
        private static readonly Color AccentPressed = Color.FromArgb(61, 49, 84); // neutral pressed
        private static readonly Color AccentBorder = Color.FromArgb(126, 104, 162);

        // Windows 11 caption/border color (BGR, not RGB!)
        private static readonly int TitleBarColorBgr = ToBgr(Color.FromArgb(30, 22, 46));
        private static readonly int TitleBarTextColorBgr = ToBgr(Color.FromArgb(232, 228, 240));

        /// <summary>
        /// Applies the purple theme to the control tree rooted at <paramref name="root"/>
        /// and, if <paramref name="root"/> is a Form, colors its title bar as well.
        /// Safe to call again (e.g. after new controls were added) to re-theme them.
        /// </summary>
        public static void Apply(Control root)
        {
            Form form = root as Form;
            if (form != null)
            {
                form.BackColor = Back;
                form.ForeColor = Fore;
                ApplyTitleBar(form);
            }

            ApplyToControls(root.Controls);

            root.Invalidate(true);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is MenuStrip || control is ToolStrip || control is StatusStrip)
                {
                    ToolStrip strip = (ToolStrip)control;
                    strip.BackColor = MenuBack;
                    strip.ForeColor = Fore;
                    strip.RenderMode = ToolStripRenderMode.Professional;
                    strip.Renderer = new NeutralToolStripRenderer();
                    ApplyToToolStripItems(strip.Items);
                }
                else if (control is ContextMenuStrip)
                {
                    ContextMenuStrip cms = (ContextMenuStrip)control;
                    cms.BackColor = MenuBack;
                    cms.ForeColor = Fore;
                    cms.RenderMode = ToolStripRenderMode.Professional;
                    cms.Renderer = new NeutralToolStripRenderer();
                    ApplyToToolStripItems(cms.Items);
                }
                else if (control is DataGridView)
                {
                    DataGridView grid = (DataGridView)control;
                    grid.BackgroundColor = Back;
                    grid.ForeColor = InputFore;
                    grid.GridColor = GridLines;
                    grid.BorderStyle = BorderStyle.FixedSingle;
                    grid.EnableHeadersVisualStyles = false;
                    grid.DefaultCellStyle.BackColor = InputBack;
                    grid.DefaultCellStyle.ForeColor = InputFore;
                    grid.DefaultCellStyle.SelectionBackColor = AccentHover;
                    grid.DefaultCellStyle.SelectionForeColor = Color.White;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBack;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Fore;
                    grid.RowHeadersDefaultCellStyle.BackColor = GridHeaderBack;
                    grid.RowHeadersDefaultCellStyle.ForeColor = Fore;
                }
                else if (control is TreeView)
                {
                    control.BackColor = InputBack;
                    control.ForeColor = Color.White;
                    ApplyNativeDarkTheme(control);
                }
                else if (control is TextBoxBase || control is ComboBox || control is ListBox)
                {
                    control.BackColor = InputBack;
                    control.ForeColor = InputFore;
                    ComboBox combo = control as ComboBox;
                    if (combo != null) combo.FlatStyle = FlatStyle.Flat;
                    ApplyNativeDarkTheme(control);
                }
                else if (control is Button)
                {
                    Button btn = (Button)control;
                    btn.BackColor = Accent;
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = AccentBorder;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.MouseOverBackColor = AccentHover;
                    btn.FlatAppearance.MouseDownBackColor = AccentPressed;
                }
                else if (control is ProgressBar)
                {
                    control.BackColor = InputBack;
                    control.ForeColor = Accent;
                }
                else if (control is Label && control.Cursor == Cursors.Hand)
                {
                    // Labels used as clickable links (hand cursor) keep an accent
                    // color instead of plain text color so they still read as links.
                    control.BackColor = Back;
                    control.ForeColor = AccentBorder;
                }
                else
                {
                    control.BackColor = Back;
                    control.ForeColor = Fore;
                }

                if (control.HasChildren)
                {
                    ApplyToControls(control.Controls);
                }
            }
        }

        private static void ApplyToToolStripItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.BackColor = MenuBack;
                item.ForeColor = Fore;

                ToolStripDropDownItem dropDown = item as ToolStripDropDownItem;
                if (dropDown != null)
                {
                    ApplyToToolStripItems(dropDown.DropDownItems);
                }
            }
        }


        private sealed class NeutralToolStripRenderer : ToolStripProfessionalRenderer
        {
            public NeutralToolStripRenderer() : base(new NeutralColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
                Color color = e.Item.Pressed || (e.Item is ToolStripMenuItem && ((ToolStripMenuItem)e.Item).DropDown.Visible)
                    ? AccentPressed
                    : e.Item.Selected ? AccentHover : MenuBack;
                using (SolidBrush brush = new SolidBrush(color)) e.Graphics.FillRectangle(brush, rect);
            }
        }

        private sealed class NeutralColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => AccentHover;
            public override Color MenuItemBorder => AccentBorder;
            public override Color MenuItemPressedGradientBegin => AccentPressed;
            public override Color MenuItemPressedGradientMiddle => AccentPressed;
            public override Color MenuItemPressedGradientEnd => AccentPressed;
            public override Color ToolStripDropDownBackground => MenuBack;
            public override Color ImageMarginGradientBegin => MenuBack;
            public override Color ImageMarginGradientMiddle => MenuBack;
            public override Color ImageMarginGradientEnd => MenuBack;
            public override Color SeparatorDark => GridLines;
            public override Color SeparatorLight => GridLines;
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private static void ApplyNativeDarkTheme(Control control)
        {
            try
            {
                SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
                control.Invalidate();
            }
            catch { }
        }

        #region native title bar coloring (DWM)

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;      // Win10 1809+/Win11: dark title bar
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;  // Win10 1809 build fallback
        private const int DWMWA_CAPTION_COLOR = 35;                // Win11 22000+: custom title bar color
        private const int DWMWA_TEXT_COLOR = 36;                   // Win11 22000+: title bar text color
        private const int DWMWA_BORDER_COLOR = 34;                 // Win11 22000+: window border color

        /// <summary>
        /// Colors the form's native title bar dark/purple using DWM window attributes.
        /// On Windows 11 the caption is painted purple to match the app; on older
        /// Windows 10 builds it falls back to the standard dark title bar.
        /// Safe no-op on unsupported systems (calls simply fail silently).
        /// </summary>
        public static void ApplyTitleBar(Form form)
        {
            if (form == null) return;

            try
            {
                IntPtr handle = form.Handle; // forces handle creation if needed

                int enabled = 1;
                if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref enabled, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref enabled, sizeof(int));
                }

                int captionColor = TitleBarColorBgr;
                DwmSetWindowAttribute(handle, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));

                int borderColor = TitleBarColorBgr;
                DwmSetWindowAttribute(handle, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));

                int textColor = TitleBarTextColorBgr;
                DwmSetWindowAttribute(handle, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));

                // Re-draw the non-client area so the new colors show immediately.
                form.Invalidate();
            }
            catch
            {
                // DWM not available (very old Windows) - ignore, window keeps default title bar.
            }
        }

        private static int ToBgr(Color c)
        {
            return c.R | (c.G << 8) | (c.B << 16);
        }

        #endregion
    }
}
