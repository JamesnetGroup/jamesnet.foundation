using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Jamesnet.Platform.OpenSilver;

public class ContentCase : DependencyObject
{
    public static readonly DependencyProperty SourcePathProperty =
        DependencyProperty.Register(
            nameof(SourcePath),
            typeof(string),
            typeof(ContentCase),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SourceElementNameProperty =
        DependencyProperty.Register(
            nameof(SourceElementName),
            typeof(string),
            typeof(ContentCase),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SourceValueProperty =
        DependencyProperty.Register(
            nameof(SourceValue),
            typeof(object),
            typeof(ContentCase),
            new PropertyMetadata(null, OnSourceValueChanged));

    private static void OnSourceValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContentCase contentCase)
        {
            contentCase.SourceValueChanged?.Invoke(contentCase, EventArgs.Empty);
        }
    }

    public event EventHandler SourceValueChanged;

    public static readonly DependencyProperty MatchValueProperty =
        DependencyProperty.Register(
            nameof(MatchValue),
            typeof(object),
            typeof(ContentCase),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CaseContentProperty =
        DependencyProperty.Register(
            nameof(CaseContent),
            typeof(object),
            typeof(ContentCase),
            new PropertyMetadata(null));

    public string SourcePath
    {
        get => (string)GetValue(SourcePathProperty);
        set => SetValue(SourcePathProperty, value);
    }

    public string SourceElementName
    {
        get => (string)GetValue(SourceElementNameProperty);
        set => SetValue(SourceElementNameProperty, value);
    }

    public object SourceValue
    {
        get => GetValue(SourceValueProperty);
        set => SetValue(SourceValueProperty, value);
    }

    public object MatchValue
    {
        get => GetValue(MatchValueProperty);
        set => SetValue(MatchValueProperty, value);
    }

    public object CaseContent
    {
        get => GetValue(CaseContentProperty);
        set => SetValue(CaseContentProperty, value);
    }
}

public class ContentSwitcher : ContentControl
{
    private readonly ObservableCollection<ContentCase> _cases = new();

    public static readonly DependencyProperty CasesProperty =
        DependencyProperty.Register(
            nameof(Cases),
            typeof(ObservableCollection<ContentCase>),
            typeof(ContentSwitcher),
            new PropertyMetadata(null));

    public ObservableCollection<ContentCase> Cases
    {
        get => (ObservableCollection<ContentCase>)GetValue(CasesProperty);
        private set => SetValue(CasesProperty, value);
    }

    public ContentSwitcher()
    {
        Cases = _cases;
        Cases.CollectionChanged += Cases_CollectionChanged;
        this.DataContextChanged += ContentSwitcher_DataContextChanged;
        DefaultStyleKey = typeof(ContentSwitcher);
    }

    private void ContentSwitcher_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        foreach (var contentCase in Cases)
        {
            SetBinding(contentCase);
        }
        UpdateActiveContent();
    }

    private void Cases_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (ContentCase oldCase in e.OldItems)
            {
                oldCase.SourceValueChanged -= ContentCase_SourceValueChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (ContentCase newCase in e.NewItems)
            {
                SetBinding(newCase);
                newCase.SourceValueChanged += ContentCase_SourceValueChanged;
            }
        }
        UpdateActiveContent();
    }

    private void ContentCase_SourceValueChanged(object sender, EventArgs e)
    {
        UpdateActiveContent();
    }

    private void SetBinding(ContentCase contentCase)
    {
        try
        {
            var binding = new Binding
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            if (!string.IsNullOrEmpty(contentCase.SourceElementName))
            {
                binding.ElementName = contentCase.SourceElementName;
                var element = this.FindName(contentCase.SourceElementName);
                if (element == null) return;  // 요소가 없으면 바인딩 건너뜀
                binding.Source = element;
            }
            else if (this.DataContext != null)
            {
                binding.Source = this.DataContext;
            }

            if (!string.IsNullOrEmpty(contentCase.SourcePath))
            {
                binding.Path = new PropertyPath(contentCase.SourcePath);
            }

            contentCase.SetValue(ContentCase.SourceValueProperty, null);
            BindingOperations.SetBinding(contentCase, ContentCase.SourceValueProperty, binding);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting binding: {ex.Message}");
        }
    }

    private bool IsBindingSet(ContentCase contentCase)
    {
        if (string.IsNullOrEmpty(contentCase.SourceElementName))
        {
            return this.DataContext != null;
        }
        return this.FindName(contentCase.SourceElementName) != null;
    }

    private void UpdateActiveContent()
    {
        foreach (var contentCase in Cases)
        {
            if (!IsBindingSet(contentCase))
            {
                Console.WriteLine("Binding not established yet, skipping");
                continue;
            }

            var sourceValue = contentCase.SourceValue;
            var matchValue = contentCase.MatchValue;

            if (TryConvertMatchValue(sourceValue, matchValue, out var convertedMatchValue))
            {
                if (sourceValue is bool sourceBool && convertedMatchValue is bool matchBool)
                {
                    if (sourceBool == matchBool)
                    {
                        Content = contentCase.CaseContent;
                        return;
                    }
                }
                else if (Equals(sourceValue, convertedMatchValue))
                {
                    Content = contentCase.CaseContent;
                    return;
                }
            }
        }
        Content = null;
    }

    private bool TryConvertMatchValue(object sourceValue, object matchValue, out object result)
    {
        try
        {
            var sourceType = sourceValue.GetType();

            if (sourceType == typeof(bool) && matchValue is string boolStr)
            {
                result = bool.Parse(boolStr);
                return true;
            }

            if (matchValue?.GetType() == sourceType)
            {
                result = matchValue;
                return true;
            }

            result = Convert.ChangeType(matchValue, sourceType);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}