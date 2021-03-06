﻿using System;
using System.Collections.Generic;
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

namespace Simula.Pages
{
    public partial class DeleteConfirmationDialog : UserControl
    {
        public DeleteConfirmationDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.Result = DialogResult.Confirm;
            MainWindow.InvokeDialogCloseCallback();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = DialogResult.Cancel;
            MainWindow.InvokeDialogCloseCallback();
        }

        public DialogResult Result { get; private set; } = DialogResult.Cancel;
    }
}
