﻿using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Bartz24.RandoWPF;

public class MultiSelectListBox : ListBox
{
    public IList SelectedItemsList
    {
        get => (IList)GetValue(SelectedItemsListProperty);
        set => SetValue(SelectedItemsListProperty, value);
    }

    public static readonly DependencyProperty SelectedItemsListProperty =
        DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(MultiSelectListBox), new PropertyMetadata(new List<string>()));

    public MultiSelectListBox() { }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        SelectedItemsList = SelectedItems;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == SelectedItemsListProperty)
        {
            SetSelectedItems(SelectedItemsList);
        }
    }
}
