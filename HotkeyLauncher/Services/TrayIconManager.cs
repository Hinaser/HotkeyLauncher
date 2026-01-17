using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using Forms = System.Windows.Forms;

namespace HotkeyLauncher.Services;

public class TrayIconManager : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ContextMenuStrip _contextMenu;
    private bool _disposed;

    public event EventHandler? SettingsRequested;
    public event EventHandler? ExitRequested;

    public TrayIconManager()
    {
        _contextMenu = CreateContextMenu();

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "HotkeyLauncher",
            ContextMenuStrip = _contextMenu,
            Visible = false
        };

        _notifyIcon.DoubleClick += OnSettingsClick;
    }

    private Forms.ContextMenuStrip CreateContextMenu()
    {
        var menu = new Forms.ContextMenuStrip
        {
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            ShowImageMargin = false,
            Renderer = new DarkMenuRenderer()
        };

        var settingsItem = new Forms.ToolStripMenuItem("Settings")
        {
            ForeColor = Color.White,
            Padding = new Forms.Padding(8, 4, 8, 4)
        };
        settingsItem.Click += OnSettingsClick;

        var separator = new Forms.ToolStripSeparator();

        var exitItem = new Forms.ToolStripMenuItem("Exit")
        {
            ForeColor = Color.FromArgb(240, 85, 69),
            Padding = new Forms.Padding(8, 4, 8, 4)
        };
        exitItem.Click += OnExitClick;

        menu.Items.Add(settingsItem);
        menu.Items.Add(separator);
        menu.Items.Add(exitItem);

        return menu;
    }

    public void Show()
    {
        _notifyIcon.Icon = CreateIcon();
        _notifyIcon.Visible = true;
    }

    public void Hide()
    {
        _notifyIcon.Visible = false;
    }

    public void ShowBalloonTip(string title, string text, Forms.ToolTipIcon icon = Forms.ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, text, icon);
    }

    private static Icon CreateIcon()
    {
        const int size = 32;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // Background circle with gradient
        using (var gradientBrush = new LinearGradientBrush(
            new Rectangle(0, 0, size, size),
            Color.FromArgb(0, 120, 212),
            Color.FromArgb(0, 90, 180),
            LinearGradientMode.ForwardDiagonal))
        {
            g.FillEllipse(gradientBrush, 1, 1, size - 2, size - 2);
        }

        // Draw "H" letter
        using var font = new Font("Segoe UI", 16, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
        using var textBrush = new SolidBrush(Color.White);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString("H", font, textBrush, new RectangleF(0, 0, size, size), format);

        // Small lightning bolt accent
        using var accentPen = new Pen(Color.FromArgb(255, 200, 50), 2);
        g.DrawLine(accentPen, size - 8, 6, size - 5, 10);
        g.DrawLine(accentPen, size - 5, 10, size - 8, 10);
        g.DrawLine(accentPen, size - 8, 10, size - 5, 14);

        return Icon.FromHandle(bitmap.GetHicon());
    }

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Icon?.Dispose();
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }

        _disposed = true;
    }

    ~TrayIconManager()
    {
        Dispose(false);
    }
}

internal class DarkMenuRenderer : Forms.ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkColorTable()) { }

    protected override void OnRenderItemText(Forms.ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.ForeColor;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderMenuItemBackground(Forms.ToolStripItemRenderEventArgs e)
    {
        if (e.Item.Selected)
        {
            using var brush = new SolidBrush(Color.FromArgb(60, 60, 60));
            e.Graphics.FillRectangle(brush, new Rectangle(System.Drawing.Point.Empty, e.Item.Size));
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }
    }

    protected override void OnRenderSeparator(Forms.ToolStripSeparatorRenderEventArgs e)
    {
        using var pen = new Pen(Color.FromArgb(70, 70, 70));
        int y = e.Item.Height / 2;
        e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
    }
}

internal class DarkColorTable : Forms.ProfessionalColorTable
{
    public override Color MenuBorder => Color.FromArgb(60, 60, 60);
    public override Color MenuItemBorder => Color.Transparent;
    public override Color MenuItemSelected => Color.FromArgb(60, 60, 60);
    public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 45);
    public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 45);
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 60);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, 60, 60);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(50, 50, 50);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(50, 50, 50);
    public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 45);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 45);
    public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 45);
    public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 45);
    public override Color SeparatorDark => Color.FromArgb(70, 70, 70);
    public override Color SeparatorLight => Color.FromArgb(70, 70, 70);
}
