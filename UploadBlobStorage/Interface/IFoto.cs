using UploadBlobStorage.Models;

namespace UploadBlobStorage.Interface
{
    public interface IFoto
    {
        Task<List<Foto>> ListaAsync();
        FotoRetorno Renomea(string original, string novo);
        Task<FotoRetorno> DownloadAsync(string nome);
        bool ArquivoExiste(string nome);
        Task<FotoRetorno> UploadAsync(IFormFile arquivo); 
        Task<FotoRetorno> ExcluiAsync(string arquivo);

    }
}
