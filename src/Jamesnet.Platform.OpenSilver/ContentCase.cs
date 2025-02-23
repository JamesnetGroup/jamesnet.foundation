using System;
using System.Windows;

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
