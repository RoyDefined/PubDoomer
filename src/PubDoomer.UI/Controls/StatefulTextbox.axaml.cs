using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PubDoomer.Controls;

public partial class StatefulTextbox : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<StatefulTextbox, string>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object> AdditionalContentProperty =
        AvaloniaProperty.Register<StatefulTextbox, object>(nameof(AdditionalContent));

    private readonly ContentPresenter? _contentPresenter;
    private readonly Button? _resetButton;
    private readonly Button? _saveButton;

    private readonly TextBox? _textBox;

    public StatefulTextbox()
    {
        InitializeComponent();

        _textBox = this.FindControl<TextBox>("PART_TextBox");
        _resetButton = this.FindControl<Button>("PART_ResetButton");
        _saveButton = this.FindControl<Button>("PART_SaveButton");
        _contentPresenter = this.FindControl<ContentPresenter>("PART_AdditionalContent");

        if (_textBox != null)
        {
            _textBox.Text = Text;
            _textBox.PropertyChanged += OnTextChanged;
        }

        this.GetObservable(TextProperty).Subscribe(text =>
        {
            if (_textBox != null) _textBox.Text = text;

            UpdateButtonStates();
        });

        this.GetObservable(AdditionalContentProperty).Subscribe(content =>
        {
            if (_contentPresenter != null) _contentPresenter.Content = content;
        });
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public object AdditionalContent
    {
        get => GetValue(AdditionalContentProperty);
        set => SetValue(AdditionalContentProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnTextChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBox.TextProperty) UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (_textBox != null && _resetButton != null && _saveButton != null)
        {
            var hasChanges = !_textBox.Text?.Equals(Text, StringComparison.Ordinal) ?? false;
            _resetButton.IsEnabled = hasChanges;
            _saveButton.IsEnabled = hasChanges;
        }
    }

    private void OnResetClicked(object? sender, RoutedEventArgs e)
    {
        if (_textBox != null) _textBox.Text = Text;
        UpdateButtonStates();
    }

    private void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (_textBox?.Text != null) Text = _textBox.Text;
        UpdateButtonStates();
    }
}