using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JewelleryERP.Helpers;

public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached(
            "Attach",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false, OnAttachChanged));

    public static string GetBoundPassword(DependencyObject obj)
    {
        return (string)obj.GetValue(BoundPasswordProperty);
    }

    public static void SetBoundPassword(DependencyObject obj, string value)
    {
        obj.SetValue(BoundPasswordProperty, value);
    }

    public static bool GetAttach(DependencyObject obj)
    {
        return (bool)obj.GetValue(AttachProperty);
    }

    public static void SetAttach(DependencyObject obj, bool value)
    {
        obj.SetValue(AttachProperty, value);
    }

    private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
        else
        {
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
        }
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        if (passwordBox.Password != (string?)e.NewValue)
        {
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
        }
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }

        SetBoundPassword(passwordBox, passwordBox.Password);

        var binding = BindingOperations.GetBindingExpression(passwordBox, BoundPasswordProperty);
        binding?.UpdateSource();
    }
}
