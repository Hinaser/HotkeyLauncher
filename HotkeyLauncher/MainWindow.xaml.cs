using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using HotkeyLauncher.Models;
using HotkeyLauncher.Services;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace HotkeyLauncher;

public partial class MainWindow : Window
{
    private ObservableCollection<HotkeyConfig> _hotkeys = [];
    private HotkeyConfig? _selectedConfig;
    private uint _currentModifiers;
    private uint _currentKey;
    private bool _isCapturing;
    private AppTheme _currentTheme = AppTheme.Dark;
    private bool _isInitializing = true;
    private System.Windows.Point _dragStartPoint;
    private HotkeyConfig? _draggedItem;

    public event EventHandler<AppSettings>? SettingsSaved;

    public MainWindow()
    {
        InitializeComponent();
        HotkeyListBox.ItemsSource = _hotkeys;
        _hotkeys.CollectionChanged += (s, e) => UpdateEmptyState();
        UpdateButtonStates();
        UpdateEmptyState();
        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        ThemeManager.ApplyTheme(this, _currentTheme);
    }

    public void LoadSettings(AppSettings settings, string? settingsPath = null)
    {
        _hotkeys.Clear();
        foreach (var hotkey in settings.Hotkeys)
        {
            _hotkeys.Add(new HotkeyConfig
            {
                Id = hotkey.Id,
                Name = hotkey.Name,
                Modifiers = hotkey.Modifiers,
                Key = hotkey.Key,
                ApplicationPath = hotkey.ApplicationPath,
                Arguments = hotkey.Arguments,
                WorkingDirectory = hotkey.WorkingDirectory,
                RunAsAdmin = hotkey.RunAsAdmin
            });
        }
        ClearInputFields();

        if (!string.IsNullOrEmpty(settingsPath))
        {
            SettingsPathText.Text = settingsPath;
        }

        StartWithWindowsCheckBox.IsChecked = StartupManager.IsRegistered;

        _currentTheme = settings.Theme;
        ThemeComboBox.SelectedIndex = _currentTheme == AppTheme.Dark ? 0 : 1;

        if (IsLoaded)
        {
            ThemeManager.ApplyTheme(this, _currentTheme);
        }

        _isInitializing = false;
    }

    private void HotkeyListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _selectedConfig = HotkeyListBox.SelectedItem as HotkeyConfig;

        if (_selectedConfig != null)
        {
            NameTextBox.Text = _selectedConfig.Name;
            _currentModifiers = _selectedConfig.Modifiers;
            _currentKey = _selectedConfig.Key;
            UpdateHotkeyDisplay();
            PathTextBox.Text = _selectedConfig.ApplicationPath;
            ArgumentsTextBox.Text = _selectedConfig.Arguments;
            WorkingDirTextBox.Text = _selectedConfig.WorkingDirectory;
            RunAsAdminCheckBox.IsChecked = _selectedConfig.RunAsAdmin;
        }

        UpdateButtonStates();
    }

    private void HotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;

        if (!_isCapturing) return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore modifier keys alone
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin)
        {
            return;
        }

        _currentModifiers = 0;
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            _currentModifiers |= NativeMethods.MOD_CONTROL;
        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            _currentModifiers |= NativeMethods.MOD_ALT;
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            _currentModifiers |= NativeMethods.MOD_SHIFT;
        if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
            _currentModifiers |= NativeMethods.MOD_WIN;

        _currentKey = (uint)KeyInterop.VirtualKeyFromKey(key);

        UpdateHotkeyDisplay();
    }

    private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _isCapturing = true;
        HotkeyTextBox.Text = "Press a key combination...";
    }

    private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _isCapturing = false;
        UpdateHotkeyDisplay();
    }

    private void ClearHotkeyButton_Click(object sender, RoutedEventArgs e)
    {
        _currentModifiers = 0;
        _currentKey = 0;
        UpdateHotkeyDisplay();
    }

    private void UpdateHotkeyDisplay()
    {
        if (_currentKey == 0)
        {
            HotkeyTextBox.Text = string.Empty;
            return;
        }

        var tempConfig = new HotkeyConfig { Modifiers = _currentModifiers, Key = _currentKey };
        HotkeyTextBox.Text = tempConfig.HotkeyDisplayText;
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button)
        {
            if (button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }

    private void BrowseApp_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            Title = "Select Application"
        };

        if (dialog.ShowDialog() == true)
        {
            PathTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select Folder to Open",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            PathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void PathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        PathPlaceholder.Visibility = string.IsNullOrEmpty(PathTextBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void BrowseDirButton_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select Working Directory",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            WorkingDirTextBox.Text = dialog.SelectedPath;
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput(excludeId: null)) return;

        var config = new HotkeyConfig
        {
            Name = NameTextBox.Text.Trim(),
            Modifiers = _currentModifiers,
            Key = _currentKey,
            ApplicationPath = PathTextBox.Text.Trim(),
            Arguments = ArgumentsTextBox.Text.Trim(),
            WorkingDirectory = WorkingDirTextBox.Text.Trim(),
            RunAsAdmin = RunAsAdminCheckBox.IsChecked == true
        };

        _hotkeys.Add(config);
        ClearInputFields();
        SaveSettings();
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedConfig == null || !ValidateInput(excludeId: _selectedConfig.Id)) return;

        _selectedConfig.Name = NameTextBox.Text.Trim();
        _selectedConfig.Modifiers = _currentModifiers;
        _selectedConfig.Key = _currentKey;
        _selectedConfig.ApplicationPath = PathTextBox.Text.Trim();
        _selectedConfig.Arguments = ArgumentsTextBox.Text.Trim();
        _selectedConfig.WorkingDirectory = WorkingDirTextBox.Text.Trim();
        _selectedConfig.RunAsAdmin = RunAsAdminCheckBox.IsChecked == true;

        HotkeyListBox.Items.Refresh();
        SaveSettings();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedConfig == null) return;

        // Store the ID before any selection changes occur
        var idToDelete = _selectedConfig.Id;
        var itemToDelete = _hotkeys.FirstOrDefault(h => h.Id == idToDelete);

        if (itemToDelete != null)
        {
            _hotkeys.Remove(itemToDelete);
            ClearInputFields();
            SaveSettings();
        }
    }

    private void SaveSettings()
    {
        var settings = new AppSettings
        {
            Hotkeys = [.. _hotkeys],
            Theme = _currentTheme
        };

        SettingsSaved?.Invoke(this, settings);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        StartupManager.SetStartup(StartWithWindowsCheckBox.IsChecked == true);
    }

    private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;

        _currentTheme = ThemeComboBox.SelectedIndex == 0 ? AppTheme.Dark : AppTheme.Light;
        ThemeManager.ApplyTheme(this, _currentTheme);
        SaveSettings();
    }

    private bool ValidateInput(Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter a name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (_currentKey == 0)
        {
            System.Windows.MessageBox.Show("Please set a hotkey.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(PathTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please select an application.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Check for duplicate hotkey
        var duplicate = _hotkeys.FirstOrDefault(h =>
            h.Modifiers == _currentModifiers &&
            h.Key == _currentKey &&
            h.Id != excludeId);

        if (duplicate != null)
        {
            System.Windows.MessageBox.Show(
                $"This hotkey is already assigned to \"{duplicate.Name}\".",
                "Duplicate Hotkey",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void ClearInputFields()
    {
        NameTextBox.Text = string.Empty;
        _currentModifiers = 0;
        _currentKey = 0;
        HotkeyTextBox.Text = string.Empty;
        PathTextBox.Text = string.Empty;
        PathPlaceholder.Visibility = Visibility.Visible;
        ArgumentsTextBox.Text = string.Empty;
        WorkingDirTextBox.Text = string.Empty;
        RunAsAdminCheckBox.IsChecked = false;
        _selectedConfig = null;
        HotkeyListBox.SelectedItem = null;
        UpdateButtonStates();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        ClearInputFields();
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _selectedConfig != null;
        AddButton.Visibility = hasSelection ? Visibility.Collapsed : Visibility.Visible;
        EditButtonsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        NewButton.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateEmptyState()
    {
        var isEmpty = _hotkeys.Count == 0;
        EmptyStatePanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        HotkeyListBox.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void HotkeyListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        var listBoxItem = FindAncestor<System.Windows.Controls.ListBoxItem>((DependencyObject)e.OriginalSource);
        if (listBoxItem != null)
        {
            _draggedItem = listBoxItem.Content as HotkeyConfig;
        }
    }

    private void HotkeyListBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggedItem == null)
            return;

        var currentPosition = e.GetPosition(null);
        var diff = _dragStartPoint - currentPosition;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            var dragData = new System.Windows.DataObject("HotkeyConfig", _draggedItem);
            DragDrop.DoDragDrop(HotkeyListBox, dragData, System.Windows.DragDropEffects.Move);
            _draggedItem = null;
        }
    }

    private void HotkeyListBox_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("HotkeyConfig"))
            return;

        var droppedData = e.Data.GetData("HotkeyConfig") as HotkeyConfig;
        if (droppedData == null)
            return;

        var listBoxItem = FindAncestor<System.Windows.Controls.ListBoxItem>((DependencyObject)e.OriginalSource);
        var targetItem = listBoxItem?.Content as HotkeyConfig;

        if (targetItem == null || droppedData == targetItem)
            return;

        var oldIndex = _hotkeys.IndexOf(droppedData);
        var newIndex = _hotkeys.IndexOf(targetItem);

        if (oldIndex >= 0 && newIndex >= 0)
        {
            _hotkeys.Move(oldIndex, newIndex);
            SaveSettings();
        }
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T t)
                return t;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
