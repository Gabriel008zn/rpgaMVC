using Microsoft.AspNetCore.Mvc;
using RpgMvc.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace RpgMvc.Controllers
{
    public class UsuariosController : Controller
    {
        public string uriBase = "http://leomiguel.somee.com/RpgApi/Usuarios/";

        [HttpGet]
        public ActionResult Index()
        {
            return View("CadastrarUsuario");
        }

        [HttpPost]
        public async Task<ActionResult> RegistrarAsync(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string uriComplementar = "Registrar";

                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uriBase + uriComplementar, content);

                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    TempData["Mensagem"] =
                        string.Format("Usuario {0} Registrado com sucesso! Faça o login para acessar.", u.Username);
                    return View("AutenticarUsuario");
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult IndexLogin()
        {
            return View("AutenticarUsuario");
        }

        [HttpPost]
        public async Task<ActionResult> AutenticarAsync(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string uriComplementar = "Autenticar";

                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uriBase + uriComplementar, content);

                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UsuarioViewModel uLogado = JsonConvert.DeserializeObject<UsuarioViewModel>(serialized);

                    HttpContext.Session.SetString("SessionTokenUsuario", uLogado.Token);
                    HttpContext.Session.SetString("SessionUsername", uLogado.Username);

                    HttpContext.Session.SetString("SessionPerfilUsuario", uLogado.Perfil);
                    HttpContext.Session.SetString("SessionIdUsuario", uLogado.Id.ToString());

                    TempData["Mensagem"] = string.Format("Bem-vindo {0}!!!", uLogado.Username);
                    return RedirectToAction("Index", "Personagens");
                }

                else
                {
                    throw new System.Exception(serialized);
                }
            }

            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return IndexLogin();
            }
        }

        [HttpGet]
        public async Task<ActionResult> IndexInformacoesAsync()
        {
            try
            {
                if(string.IsNullOrEmpty(HttpContext.Session.GetString("SessionIdUsuario")))
                {
                    return RedirectToAction("Sair", "Usuarios");
                }

                HttpClient httpClient = new HttpClient();

                //Novo: Recuperação informação da sessão
                string login = HttpContext.Session.GetString("SessionUsername");
                string uriComplementar = $"GetByLogin/{login}";
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UsuarioViewModel u = await Task.Run(() =>
                        JsonConvert.DeserializeObject<UsuarioViewModel>(serialized));
                    return View(u);
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AlterarEmail(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string uriComplementar = "AtualizarEmail";
                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PutAsync(uriBase + uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    TempData["Mensagem"] = "E-mail alterado com sucesso.";
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
            }
            return RedirectToAction("IndexInformacoes");
        }

        [HttpGet]
        public async Task<ActionResult> ObterDadosAlteracaoSenha()
        {
            UsuarioViewModel viewModel = new UsuarioViewModel();

            try
            {
                if(string.IsNullOrEmpty(HttpContext.Session.GetString("SessionIdUsuario")))
                {
                    return RedirectToAction("Sair", "Usuarios");
                }

                HttpClient httpClient = new HttpClient();
                string login = HttpContext.Session.GetString("SessionUsername");
                string uriComplementar = $"GetByLogin/{login}";
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                TempData["TituloModalExterno"] = "Alteração de Senha";

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    viewModel = await Task.Run(() =>
                        JsonConvert.DeserializeObject<UsuarioViewModel>(serialized));
                    return PartialView("_AlteracaoSenha", viewModel);
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("IndexInformacoes");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AlterarSenha(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string uriComplementar = "AlterarSenha";
                u.Username = HttpContext.Session.GetString("SessionUsername");

                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PutAsync(uriBase + uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string mensagem = "Senha alterada com sucesso.";
                    TempData["Mensagem"] = mensagem; // Mensagem guardada do TempData que aparecerá na página pai do modal
                    return Json(mensagem); // Mensagem que será exibida no alert da função que chamou este método
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EnviarFoto(UsuarioViewModel u)
        {
            try
            {
                if (Request.Form.Files.Count == 0)
                    throw new System.Exception("Selecione o arquivo.");
                else
                {
                    var file = Request.Form.Files[0];
                    var fileName = Path.GetFileName(file.FileName);
                    string nomeArquivoSemExtensao = Path.GetFileNameWithoutExtension(fileName);
                    var extensao = Path.GetExtension(fileName);

                    if (extensao != ".jpg" && extensao != ".jpeg" && extensao != ".png")
                        throw new System.Exception("O Arquivo selecionado não é uma foto.");

                    //var pastaUpload = @"\" + "";
                    //vat path = Path.Combine(pastaUpload, fileName);
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        u.Foto = ms.ToArray();
                        //string s = Convert.ToBase64String(fileBytes); //Escrever bytes numa string
                        //System.IO.File.WriteAllBytes(path, ms.ToArray()); //Escrever arquivo em uma pasta
                    }
                }
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string uriComplementar = "AtualizarFoto";
                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PutAsync(uriBase + uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    TempData["Mensagem"] = "Foto enviada com sucesso";
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
            }
            return RedirectToAction("IndexInformacoes");
        }

        [HttpGet]
        public async Task<ActionResult> BaixarFoto()
        {
            try
            {
                if(string.IsNullOrEmpty(HttpContext.Session.GetString("SessionIdUsuario")))
                {
                    return RedirectToAction("Sair", "Usuarios");
                }
                
                HttpClient httpClient = new HttpClient();
                string login = HttpContext.Session.GetString("SessionUsername");
                string uriComplementar = $"GetByLogin/{login}";
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UsuarioViewModel viewModel = await Task.Run(() =>
                        JsonConvert.DeserializeObject<UsuarioViewModel>(serialized));

                    //string contentType = "application/image";
                    string contentType = System.Net.Mime.MediaTypeNames.Application.Octet;

                    byte[] fileBytes = viewModel.Foto;
                    string fileName = $"Foto{viewModel.Username}_{DateTime.Now:ddMMyyyyHHmmss}.png"; // + extensao;

                    return File(fileBytes, contentType, fileName);
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("IndexInformacoes");
            }
        }

        [HttpGet]
        public ActionResult Sair()
        {
            try
            {
                HttpContext.Session.Remove("SessionTokenUsuario");
                HttpContext.Session.Remove("SessionUsername");
                HttpContext.Session.Remove("SessionPerfilUsuario");
                HttpContext.Session.Remove("SessionIdUsuario");

                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("IndexInformacoes");
            }
        }
    }
}
