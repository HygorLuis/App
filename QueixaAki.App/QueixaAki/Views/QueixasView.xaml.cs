﻿using System;
using System.Linq;
using System.Threading;
using QueixaAki.Models;
using QueixaAki.ViewModels;
using Xamarin.Forms;

namespace QueixaAki.Views
{
    public partial class QueixasView
    {
        private QueixasViewModel _viewModel;
        public QueixasView()
        {
            InitializeComponent();

            _viewModel = new QueixasViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<Message>(this, "Message", msg =>
            {
                DisplayAlert(msg.Title, msg.MessageText, "OK");
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<Message>(this, "Message");
        }

        private async void DownloadBtn_OnClicked(object sender, EventArgs e)
        {
            var button = (ImageButton) sender;
            await _viewModel.BaixarArquivo(long.Parse(button.CommandParameter.ToString()));
        }
    }
}