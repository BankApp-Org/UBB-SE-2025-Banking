using Common.Models;
using Microsoft.UI.Xaml.Controls;
using StockApp.ViewModels;
using System;

namespace StockApp.Views.Components
{
    public sealed partial class CreateLoanDialog : Page
    {
        public CreateLoanDialogViewModel ViewModel { get; }

        public event EventHandler<LoanRequest>? LoanRequestSubmitted
        {
            add => ViewModel.LoanRequestSubmitted += value;
            remove => ViewModel.LoanRequestSubmitted -= value;
        }

        public event EventHandler? DialogClosed
        {
            add => ViewModel.DialogClosed += value;
            remove => ViewModel.DialogClosed -= value;
        }

        public CreateLoanDialog(CreateLoanDialogViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }

        public async void SubmitLoanRequest()
        {
            await ViewModel.SubmitRequestAsync();
        }

        public bool CanSubmitLoanRequest()
        {
            return ViewModel.IsValid;
        }

        public bool IsSubmitting
        {
            get => ViewModel.IsSubmitting;
        }
    }
}
