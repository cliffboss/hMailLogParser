﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// hMailLogParser
using hMailLogParser;
using hMailLogParser.Line;
using System.Threading.Tasks;

namespace hMailLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ICollectionView defaultView;

        public MainWindow()
        {
            InitializeComponent();
        }

       async private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                await this.LoadFile(args[1]);
            }
        }

        async private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Multiselect = true;
            dlg.Filter = "Log|*.log|All Files|*.*";
            dlg.DefaultExt = ".log"; // Default file extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                await this.LoadFile(dlg.FileNames);
            }
        }

        async protected Task LoadFile(params string[] filenames)
        {
            using (new CodeTimer(this.tbExectuionTime))
            {
                Parser p = new Parser();

                List<LogLine> lines = new List<LogLine>();
                foreach (var filename in filenames)
                {
                    var l = await p.Parse(filename);
                    lines.AddRange(l);
                }

                this.defaultView = CollectionViewSource.GetDefaultView(lines);
                this.defaultView.Filter =
                    w =>
                    {
                        bool statusFilter = true;
                        if (w is LogLine)
                        {
                            var smtpLine = w as LogLine;
                            switch (smtpLine.MessageStatus)
                            {
                                case MessageStatusLevel.Error:
                                    statusFilter = tbFilterError.IsChecked.GetValue();
                                    break;
                                case MessageStatusLevel.Warning:
                                    statusFilter = tbFilterTransient.IsChecked.GetValue();
                                    break;
                                case MessageStatusLevel.Infomation:
                                    statusFilter = tbFilterNormal.IsChecked.GetValue();
                                    break;
                            }

                        }

                        var line = w as LogLine;
                        return line.Message.Contains(this.txtSearch.Text) && statusFilter;
                    };

                this.dgLogViewer.ItemsSource = this.defaultView;
                this.tbNumberOfLines.Text = lines.Count().ToString();
            }

            this.txtSearch.IsEnabled = true;
            this.btnSearch.IsEnabled = true;
        }

        protected void Search()
        {
            using (new CodeTimer(this.tbExectuionTime))
            {
                this.defaultView.Refresh();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Search();
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            
            e.Handled = true;
            this.Search();
        }

        private void dgLogViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (new CodeTimer())
            {
                var line = this.dgLogViewer.SelectedItem as SessionBasedLine;
                if (line != null)
                {
                    var items = this.defaultView.SourceCollection.OfType<SessionBasedLine>().Where(x => x.SessionID == line.SessionID).ToArray();
                    winLineDetails dialog = new winLineDetails();
                    dialog.Owner = this;
                    dialog.Line = line;
                    dialog.RelatedLines = items;
                    dialog.Show();
                }
            }
        }

        private void tbFilter_Checked(object sender, RoutedEventArgs e)
        {
            this.Search();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            winAbout dialog = new winAbout();
            dialog.Owner = this;
            dialog.ShowDialog();
        }
    }
}
