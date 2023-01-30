using Microsoft.AspNetCore.Mvc;
using UploadBlobStorage.Interface;
using UploadBlobStorage.Models;

namespace UploadBlobStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FotosController : ControllerBase
    {
        private readonly IFoto _storage;

        public FotosController(IFoto blobService)
        {
            _storage = blobService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            List<Foto>? arquivos = await _storage.ListaAsync();

            return StatusCode(StatusCodes.Status200OK, arquivos);

        }

        [HttpGet("existe/{nome}")]
        public IActionResult Existir(string nome)
        {
            return StatusCode(StatusCodes.Status200OK, _storage.ArquivoExiste(nome));

        }
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile arquivo)
        {
            FotoRetorno? response = await _storage.UploadAsync(arquivo);
            
            return StatusCode(response.StatusCode, response);

        }

        [HttpGet("download/{nome}")]
        public async Task<IActionResult> Download(string nome)
        {
            FotoRetorno response = await _storage.DownloadAsync(nome);

            if ((response.Arquivo.Conteudo == null) || (response.Arquivo.TipoConteudo == null ))
            {
                return StatusCode(response.StatusCode, response.Mensagem);
            }
            else
            {
                return File(response.Arquivo.Conteudo, response.Arquivo.TipoConteudo, response.Arquivo.Nome);
            }
        }

        [HttpDelete()]
        public async Task<IActionResult> Excluir(string arquivo)
        {
            FotoRetorno response = await _storage.ExcluiAsync(arquivo);
            
            return StatusCode(response.StatusCode, response.Mensagem);
        }
        [HttpPut()]
        public IActionResult Renomear(string original, string novo)
        {
            FotoRetorno response =  _storage.Renomea(original,novo);

            return StatusCode(response.StatusCode, response.Mensagem);

        }

    }
}
