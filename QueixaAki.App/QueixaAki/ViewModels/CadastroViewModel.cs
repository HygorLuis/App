﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using QueixaAki.Helpers;
using QueixaAki.Models;
using QueixaAki.Services;
using QueixaAki.ViewModels.Base;
using Xamarin.Forms;

namespace QueixaAki.ViewModels
{
    public class CadastroViewModel : BaseViewModel
    {
        public ICommand SalvarCommand { get; set; }

        private string _cepAntes;

        private Usuario _usuario;
        public Usuario Usuario
        {
            get => _usuario;
            set
            {
                _usuario = value;
                OnPropertyChanged();
            }
        }
        public string ConfirmarSenha { get; set; }

        private UsuarioService _usuarioService;

        public CadastroViewModel()
        {
            _usuarioService = new UsuarioService();
            Usuario = new Usuario
            {
                DataNascimento = DateTime.Today,
                Excluido = false
            };

            SalvarCommand = new Command(Salvar);
        }

        private async Task<bool> Validar()
        {
            #region CAMPOS OBRIGATORIOS

            var campos = "";

            #region LOGIN

            if (string.IsNullOrEmpty(Usuario.Email))
                campos += string.IsNullOrEmpty(campos) ? "E-Mail" : ", E-Mail";

            if (string.IsNullOrEmpty(Usuario.Senha))
                campos += string.IsNullOrEmpty(campos) ? "Senha" : ", Senha";

            if (string.IsNullOrEmpty(ConfirmarSenha))
                campos += string.IsNullOrEmpty(campos) ? "Confirmar Senha" : ", Confirmar Senha";

            #endregion

            #region DADOS PESSOAIS

            if (string.IsNullOrEmpty(Usuario.Nome))
                campos += string.IsNullOrEmpty(campos) ? "Nome" : ", Nome";

            if (string.IsNullOrEmpty(Usuario.Sobrenome))
                campos += string.IsNullOrEmpty(campos) ? "Sobrenome" : ", Sobrenome";

            if (string.IsNullOrEmpty(Usuario.CPF))
                campos += string.IsNullOrEmpty(campos) ? "CPF" : ", CPF";

            #endregion

            #region CONTATO

            if (string.IsNullOrEmpty(Usuario.Telefone1) && string.IsNullOrEmpty(Usuario.Telefone2))
                campos += string.IsNullOrEmpty(campos) ? "Telefone" : ", Telefone";

            #endregion

            #region ENDEREÇO

            if (string.IsNullOrEmpty(Usuario.Cep))
                campos += string.IsNullOrEmpty(campos) ? "Cep" : ", Cep";

            if (string.IsNullOrEmpty(Usuario.Rua))
                campos += string.IsNullOrEmpty(campos) ? "Rua" : ", Rua";

            if (string.IsNullOrEmpty(Usuario.Numero))
                campos += string.IsNullOrEmpty(campos) ? "Número" : ", Número";

            if (string.IsNullOrEmpty(Usuario.Bairro))
                campos += string.IsNullOrEmpty(campos) ? "Bairro" : ", Bairro";

            if (string.IsNullOrEmpty(Usuario.Cidade))
                campos += string.IsNullOrEmpty(campos) ? "Cidade" : ", Cidade";

            if (string.IsNullOrEmpty(Usuario.Estado))
                campos += string.IsNullOrEmpty(campos) ? "Estado" : ", Estado";

            #endregion

            if (!string.IsNullOrEmpty(campos))
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Campos em Branco",
                    MessageText = campos
                }, "Message");
                return false;
            }

            #endregion

            #region VALIDAR CAMPOS

            var usuarios = await _usuarioService.BuscarTodos();

            #region LOGIN

            if (!Usuario.Email.ValidarEMail())
            {
                MessagingCenter.Send(new Message
                {
                    Title = "E-Mail",
                    MessageText = "E-Mail não é válido!"
                }, "Message");
                return false;
            }

            var tt = usuarios.Where(x => x.Email == Usuario.Email).ToList().Count > 1;
            if (usuarios.Any(x => x.Email == Usuario.Email))
            {
                MessagingCenter.Send(new Message
                {
                    Title = "E-Mail",
                    MessageText = "E-Mail já cadastrado!"
                }, "Message");
                return false;
            }

            if (Usuario.Senha.Length < 8)
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Senha",
                    MessageText = "Necessário conter no mínimo 8 caracteres!"
                }, "Message");
                return false;
            }

            if (!Usuario.Senha.ValidarSenha())
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Senha",
                    MessageText = "Necessário conter apenas números e letras!"
                }, "Message");
                return false;
            }

            if (Usuario.Senha != ConfirmarSenha)
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Confirmar Senha",
                    MessageText = "As senhas estão diferentes!"
                }, "Message");
                return false;
            }

            #endregion

            #region DADOS PESSOAIS

            if (!Usuario.Nome.ValidarNome())
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Nome",
                    MessageText = "Necessário conter apenas letras!"
                }, "Message");
                return false;
            }

            if (!Usuario.Sobrenome.ValidarNome())
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Sobrenome",
                    MessageText = "Necessário conter apenas letras!"
                }, "Message");
                return false;
            }

            if (!string.IsNullOrEmpty(Usuario.RG) && usuarios.Any(x => x.RG == Usuario.RG))
            {
                MessagingCenter.Send(new Message
                {
                    Title = "RG",
                    MessageText = "RG já cadastrado!"
                }, "Message");
                return false;
            }

            if (!Usuario.CPF.ValidarCPF())
            {
                MessagingCenter.Send(new Message
                {
                    Title = "CPF",
                    MessageText = "CPF não é válido!"
                }, "Message");
                return false;
            }

            if (usuarios.Any(x => x.CPF == Usuario.CPF))
            {
                MessagingCenter.Send(new Message
                {
                    Title = "CPF",
                    MessageText = "CPF já cadastrado!"
                }, "Message");
                return false;
            }

            if ((DateTime.Now.Year - Usuario.DataNascimento.Year) < 18)
            {
                MessagingCenter.Send(new Message
                {
                    Title = "Data de Nascimento",
                    MessageText = "Cadastro apenas para maiores de 18 anos!"
                }, "Message");
                return false;
            }

            #endregion

            #endregion

            return true;
        }

        public async void Salvar()
        {
            try
            {
                Carregando = true;

                if (!await Validar()) return;

                Usuario.DataCriacao = DateTime.Now;

                var result = await _usuarioService.Incluir(Usuario);
                if (result)
                {
                    MessagingCenter.Send(new Message
                    {
                        Title = "Sucesso",
                        MessageText = "Usuário salvo com sucesso!"
                    }, "Message");
                }
                else
                {
                    MessagingCenter.Send(new Message
                    {
                        Title = "Erro",
                        MessageText = "Erro ao salvar usuário.\nTente novamente mais tarde!"
                    }, "Message");
                }

            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                Carregando = false;
            }
        }

        public async Task BuscaCep(string cep)
        {
            try
            {
                if (string.IsNullOrEmpty(cep) || string.IsNullOrWhiteSpace(cep)) return;
                if (!cep.ValidarCep()) return;
                if (_cepAntes == cep) return;

                Carregando = true;
                _cepAntes = cep;

                var request = (HttpWebRequest)WebRequest.Create("https://viacep.com.br/ws/" + cep + "/json/");
                request.AllowAutoRedirect = false;
                var checaServidor = (HttpWebResponse)await request.GetResponseAsync();

                if (checaServidor.StatusCode != HttpStatusCode.OK)
                {
                    MessagingCenter.Send(new Message
                    {
                        Title = "Servidor indisponível",
                        MessageText = "Erro ao buscar cep!"
                    }, "Message");
                    return;
                }

                using (var webStream = checaServidor.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (var responseReader = new StreamReader(webStream))
                        {
                            var response = responseReader.ReadToEnd();
                            response = Regex.Replace(response, "[{},]", string.Empty);
                            response = response.Replace("\"", "");

                            var substrings = response.Split('\n');

                            if (substrings[1].Contains("erro"))
                            {
                                MessagingCenter.Send(new Message
                                {
                                    Title = "Buscar cep",
                                    MessageText = "Cep não encontrado!"
                                }, "Message");
                                return;
                            }

                            Usuario.Rua = substrings[2].Split(":".ToCharArray())[1];
                            Usuario.Bairro = substrings[4].Split(":".ToCharArray())[1];
                            Usuario.Cidade = substrings[5].Split(":".ToCharArray())[1];
                            Usuario.Estado = substrings[6].Split(":".ToCharArray())[1];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                Carregando = false;
            }
        }
    }
}
