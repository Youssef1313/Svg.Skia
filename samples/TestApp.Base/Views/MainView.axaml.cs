﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using TestApp.ViewModels;

namespace TestApp.Views;

public class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

        if (!e.Data.Contains(DataFormats.FileNames))
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.FileNames))
        {
            var paths = e.Data.GetFileNames();
            if (paths is { })
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    try
                    {
                        vm.Drop(paths);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
    }

    private void FileItem_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is FileItemViewModel fileItemViewModel)
        {
            Process.Start("explorer", fileItemViewModel.Path);
        }
    }
}
