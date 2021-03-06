﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions.Abstractions;
using QueixaAki.Models;
using QueixaAki.Services;
using QueixaAki.ViewModels.Base;
using Xamarin.Forms;
using MediaFile = Plugin.Media.Abstractions.MediaFile;

namespace QueixaAki.ViewModels
{
    public class InicioViewModel : BaseViewModel
    {
        public ICommand QueixaAkiCommand { get; set; }
        public string TextButton => "QUEIXA\nAKI";

        private ConexaoService _conexaoService;

        public InicioViewModel()
        {
            _conexaoService = new ConexaoService();
            VeficarConexaoBanco();

            QueixaAkiCommand = new Command(QueixaAKi);
        }

        private async void QueixaAKi()
        {
            if (!await VerificarAcessoInternet())
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Erro",
                    MessageText = "Sem acesso à internet!"
                }, "Message");
                return;
            }

            if (App.PermissaoLocalizacao == PermissionStatus.Granted && App.PermissaoMidia == PermissionStatus.Granted)
            {
                var mediaFile = await GravarVideo();
                if (mediaFile != null)
                    MessagingCenter.Send(mediaFile, "QueixaAki");
            }
            else
            {
                var permiss = App.PermissaoLocalizacao != PermissionStatus.Granted ? "Localização" : "";
                permiss += !string.IsNullOrEmpty(permiss) && App.PermissaoMidia != PermissionStatus.Granted
                    ? ", Mídia"
                    : string.IsNullOrEmpty(permiss) && App.PermissaoMidia != PermissionStatus.Granted
                        ? "Mídia" : "";

                MessagingCenter.Send(new Message
                {
                    Title = "Permissões Necessárias",
                    MessageText = $"Favor dar permissão as seguintes solicitações nas configurações do seu telefone: {permiss}"
                }, "Message");
            }
        }

        private async Task<MediaFile> GravarVideo()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsTakeVideoSupported || !CrossMedia.Current.IsCameraAvailable)
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Ops",
                    MessageText = "Nenhuma câmera detectada!"
                }, "Message");
                return null;
            }

            var mediaFile = await CrossMedia.Current.TakeVideoAsync(
                new StoreVideoOptions
                {
                    Directory = "Queixas",
                    DesiredLength = new TimeSpan(0, 1, 0),
                    CompressionQuality = 0,
                    Quality = VideoQuality.Low,
                    SaveToAlbum = true,
                    DefaultCamera = CameraDevice.Rear
                });

            return mediaFile;
        }

        public void VeficarConexaoBanco()
        {
            try
            {
                if (!string.IsNullOrEmpty(App.ConnectionBanco)) return;

                var (conexao, erro) = _conexaoService.BuscarConexaoId(App.IdConexao);

                if (string.IsNullOrEmpty(erro))
                    App.ConnectionBanco = $"Server={conexao.Servidor}; Initial Catalog={conexao.Banco}; User ID={conexao.Usuario}; Password={conexao.Senha}; Connection Timeout=0";
                else
                    MessagingCenter.Send(new Message
                    {
                        Title = "Erro ao Buscar Conexao",
                        MessageText = $"{erro}"
                    }, "Message");
            }
            catch (Exception e)
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Erro ao Verificar Conexao",
                    MessageText = $"{e.Message}"
                }, "Message");
            }
        }
    }
}
