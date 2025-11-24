using System;
using Avalonia.Controls;

namespace RPGCreator.Core.Types.Windows;

public class TextInputDialog : ConfirmDialog
{
    // Used to override the Confirmed event to handle input text
    public new event Action<string>? Confirmed;

    private TextBox _inputTextBox;
    private bool _allowEmpty;
    public bool AllowEmpty
    {
        get => _allowEmpty;
        set
        {
            _allowEmpty = value;
        }
    }
    
    public TextInputDialog(string title = "Input Required", string message = "Please enter the text below:", string confirmText = "Confirm", string cancelText = "Cancel", bool allowEmpty = true) : base(title, message, confirmText, cancelText)
    {
        _inputTextBox = new TextBox
        {
            Margin = new Avalonia.Thickness(5),
            Width = 300,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
     
        _inputTextBox.TextChanged += InputTextBoxOnTextChanged;
        
        var panel = Content as StackPanel;
        panel?.Children.Insert(1, _inputTextBox);
    }

    private void InputTextBoxOnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_allowEmpty)
            return;

        ConfirmButton.IsEnabled = !string.IsNullOrWhiteSpace(_inputTextBox.Text);
    }

    protected override void OnConfirm()
    {
        Confirmed?.Invoke(_inputTextBox.Text ?? string.Empty);
        if (AutoClose)
        {
            Close();
        }
    }
}