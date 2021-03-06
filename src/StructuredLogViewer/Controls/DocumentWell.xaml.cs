﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StructuredLogViewer.Controls
{
    public partial class DocumentWell : UserControl
    {
        private ICollectionView tabsView;

        public DocumentWell()
        {
            InitializeComponent();
            tabControl.ItemsSource = Tabs;
            tabsView = CollectionViewSource.GetDefaultView(Tabs);
            Tabs.CollectionChanged += Tabs_CollectionChanged;

            var style = new Style();
            style.Setters.Add(new EventSetter(MouseDownEvent, (MouseButtonEventHandler)OnMouseDownEvent));

            tabControl.ItemContainerStyle = style;
        }

        private void OnMouseDownEvent(object sender, MouseButtonEventArgs args)
        {
            if (args.MiddleButton == MouseButtonState.Pressed && sender is SourceFileTab sourceFileTab)
            {
                Tabs.Remove(sourceFileTab);
            }
        }

        private void Tabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Visibility = Tabs.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        public ObservableCollection<SourceFileTab> Tabs { get; } = new ObservableCollection<SourceFileTab>();

        public SourceFileTab Find(string filePath)
        {
            return Tabs.FirstOrDefault(t => string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
        }

        public void CloseAllTabs()
        {
            Tabs.Clear();
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        public void DisplaySource(string sourceFilePath, string text, int lineNumber = 0, int column = 0, Action preprocess = null, bool displayPath = true)
        {
            var existing = Find(sourceFilePath);
            if (existing != null)
            {
                Visibility = Visibility.Visible;
                tabControl.SelectedItem = existing;
                var textViewer = existing.Content as TextViewerControl;
                if (textViewer != null)
                {
                    textViewer.SetPathDisplay(displayPath);

                    if (textViewer.Text != text)
                    {
                        textViewer.SetText(text);
                    }

                    textViewer.DisplaySource(lineNumber, column);
                }

                return;
            }

            var textViewerControl = new TextViewerControl();
            textViewerControl.DisplaySource(sourceFilePath, text, lineNumber, column, preprocess);
            var tab = new SourceFileTab()
            {
                FilePath = sourceFilePath,
                Text = text,
                Content = textViewerControl,
            };
            var header = new SourceFileTabHeader(tab);
            tab.Header = header;
            header.CloseRequested += t => Tabs.Remove(t);
            tab.HeaderTemplate = (DataTemplate)Application.Current.Resources["SourceFileTabHeaderTemplate"];
            textViewerControl.SetPathDisplay(displayPath);

            Tabs.Add(tab);
            tabControl.SelectedItem = tab;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
