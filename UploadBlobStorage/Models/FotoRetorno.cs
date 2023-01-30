namespace UploadBlobStorage.Models
{
    public class FotoRetorno
    {
        public string? Mensagem { get; set; }
        public Foto Arquivo { get; set; }
        public int StatusCode { get; set; }
        public FotoRetorno()
        {
            Arquivo = new Foto();
            StatusCode = 200;
        }
    }
}
