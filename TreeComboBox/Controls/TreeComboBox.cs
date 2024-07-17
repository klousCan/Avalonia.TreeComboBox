using System;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Reactive;

namespace Avalonia.TreeComboBox.Controls;

public class TreeComboBox : TreeView
{
    private bool _isPushTextChangedEvent = true;

    private Button ClearButton;
    private string _fixedPlaceholderText;
    private TextBox ComboBoxText;

    private object? _selectItem;

    private volatile bool _isLastSelectLeaf = true;

    public TreeComboBox()
    {
        TemplateApplied += (sender, args) =>
        {
            ClearButton = args.NameScope.Find<Button>("ClearButton");
            ComboBoxText = args.NameScope.Find<TextBox>("ComboBoxText");
            ComboBoxText.TextChanged += (sender, args) => { OwenTextChanged?.Invoke(sender, args); };
        };

        PlaceholderTextProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string>>(
            (s) =>
            {
                if (s.Sender != this) return;
                if (!string.IsNullOrWhiteSpace(_fixedPlaceholderText)) return;
                _fixedPlaceholderText = s.NewValue.Value;
            })!);
        IsDropDownOpenProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>((s) =>
            {
                if (s.Sender != this) return;
                if (s.NewValue == false && SelectedItem != null)
                {
                    SetDisplay(SelectedItem);
                    PlaceholderText = _fixedPlaceholderText;
                    _isPushTextChangedEvent = true;
                }
            })!);

        SelectedItemProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<object>>((s) =>
        {
            if (s.Sender != this) return;
            if (s.NewValue == null)
            {
                SetCurrentValue<string>(SelectTextProperty, string.Empty);
            }
        }));
        SelectionChanged += (sender, args) =>
        {
            _isPushTextChangedEvent = false;
            if (args.AddedItems.Count <= 0)
            {
                this.SetCurrentValue<string>(SelectTextProperty, string.Empty);
                ComboBoxText?.Focus();
                return;
            }

            if (string.IsNullOrEmpty(DisplayMember)) return;
            var item = args.AddedItems[0];
            if (!string.IsNullOrEmpty(LeafMember))
            {
                var type = item.GetType();
                var property = type.GetProperty(LeafMember);
                if (property != null)
                {
                    int.TryParse(property.GetValue(item).ToString(), out var leaf);
                    if (leaf == 0)
                    {
                        //当前选中不是叶子节点，但是也需要给当前SelectedItem赋值，
                        //那么怎么在给赋值的时候不重新触发
                        _isLastSelectLeaf = false;
                        SelectedItem = _selectItem;
                        args.Handled = true;
                        return;
                    }
                    else if(!_isLastSelectLeaf)
                    {
                        _isLastSelectLeaf = SelectedItem != _selectItem;
                    }

                    if (this.IsDropDownOpen && _isLastSelectLeaf)
                        this.SetCurrentValue<bool>(TreeComboBox.IsDropDownOpenProperty, !this.IsDropDownOpen);
                    _selectItem = SelectedItem;
                    _isLastSelectLeaf = true;
                }
            }
            else
            {
                if (this.IsDropDownOpen)
                    this.SetCurrentValue<bool>(TreeComboBox.IsDropDownOpenProperty, !this.IsDropDownOpen);
            }

            SetDisplay(item);
        };
    }


    private void SetDisplay(object item)
    {
        var type = item.GetType();
        var property = type.GetProperty(DisplayMember);
        this.SetCurrentValue<string>(SelectTextProperty, property.GetValue(item).ToString());
        ClearButton?.Focus();
    }

    public EventHandler<TextChangedEventArgs>? OwenTextChanged { get; set; }

    public static readonly StyledProperty<string> SelectTextProperty = AvaloniaProperty.Register<TreeComboBox, string>(
        "SelectText");

    public string SelectText
    {
        get => GetValue(SelectTextProperty);
        set => SetValue(SelectTextProperty, value);
    }

    public static readonly StyledProperty<string> DisplayMemberProperty =
        AvaloniaProperty.Register<TreeComboBox, string>(
            "DisplayMember");

    /// <summary>
    /// 显示的字段
    /// </summary>
    public string DisplayMember
    {
        get => GetValue(DisplayMemberProperty);
        set => SetValue(DisplayMemberProperty, value);
    }

    public static readonly StyledProperty<string> LeafMemberProperty = AvaloniaProperty.Register<TreeComboBox, string>(
        "LeafMember");

    /// <summary>
    /// 是否过滤不能选中的节点，需要过滤节点的字段
    /// </summary>
    public string LeafMember
    {
        get => GetValue(LeafMemberProperty);
        set => SetValue(LeafMemberProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="P:Avalonia.Controls.ComboBox.PlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<TreeComboBox, string>(nameof(PlaceholderText));

    /// <summary>Gets or sets the PlaceHolder text.</summary>
    public string? PlaceholderText
    {
        get => this.GetValue<string>(TreeComboBox.PlaceholderTextProperty);
        set => this.SetValue<string>(TreeComboBox.PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="P:Avalonia.Controls.ComboBox.IsDropDownOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<TreeComboBox, bool>(nameof(IsDropDownOpen));

    public bool IsDropDownOpen
    {
        get => this.GetValue<bool>(TreeComboBox.IsDropDownOpenProperty);
        set { this.SetValue<bool>(TreeComboBox.IsDropDownOpenProperty, value); }
    }

    public void Clear()
    {
        this.SetCurrentValue<string>(SelectTextProperty, string.Empty);
        SelectedItems.Clear();
        _selectItem = null;
        PlaceholderText = _fixedPlaceholderText;
        if (ComboBoxText != null)
        {
            ComboBoxText.Text = "";
            ComboBoxText.Focus();
        }
    }

    private Popup? _popup;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.Handled && e.Source is Visual source)
        {
            Popup popup = this._popup;
            if ((popup != null ? (popup.IsInsidePopup(source) ? 1 : 0) : 0) != 0)
            {
                e.Handled = true;
                return;
            }
        }

        this.PseudoClasses.Set(":pressed", true);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (SelectedItem != null)
        {
            PlaceholderText = SelectText;
            SelectText = string.Empty;
            ComboBoxText.Focus();
        }

        if (!e.Handled && e.Source is Visual source)
        {
            Popup popup = this._popup;
            if ((popup != null ? (popup.IsInsidePopup(source) ? 1 : 0) : 0) != 0)
            {
                if (this.UpdateSelectionFromEventSource(e.Source))
                {
                    this._popup?.Close();
                    e.Handled = true;
                }
            }
            else
            {
                if (!this.IsDropDownOpen)
                    this.SetCurrentValue<bool>(TreeComboBox.IsDropDownOpenProperty, !this.IsDropDownOpen);
                e.Handled = true;
            }
        }

        this.PseudoClasses.Set(":pressed", false);
        base.OnPointerReleased(e);
    }
}