using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using UploadBlobStorage.Interface;
using UploadBlobStorage.Models;

namespace UploadBlobStorage.Repos
{
    public class FotoRepo : IFoto
    {
        private readonly string? _storageConnectionString;
        private readonly string? _storageContainerName;
        public FotoRepo(IConfiguration config)
        {
            _storageConnectionString = config.GetValue<string>("BlobConnectionString");
            _storageContainerName = config.GetValue<string>("BlobContainerName");
        }
        public bool ArquivoExiste(string nome)
        {
            BlobContainerClient cliente = new(_storageConnectionString, _storageContainerName);
            BlobClient arquivo = cliente.GetBlobClient(nome);
            if (arquivo.Exists())
                return true;
            else
                return false;
        }
        public async Task<List<Foto>> ListaAsync()
        {
            // Pega uma referência a um contêiner do Blob Storage em appsettings.json
            BlobContainerClient container = new(_storageConnectionString, _storageContainerName);

            // Cria um novo objeto de lista para Arquivos do Blob Storage
            List<Foto> arquivos = new();

            // Adicione cada arquivo recuperado do contêiner de armazenamento à lista de arquivos criando um objeto Blob
            await foreach (BlobItem arquivo in container.GetBlobsAsync())
            {
                var nome = arquivo.Name;
                arquivos.Add(new Foto
                {
                    Nome = nome,
                    TipoConteudo = arquivo.Properties.ContentType
                });
            }

            // Retorna todos os arquivos para o método solicitante
            return arquivos;
        }
        public FotoRetorno Renomea(string original, string novo)
        {
            try
            {
                BlobContainerClient cliente = new(_storageConnectionString, _storageContainerName);

                BlobClient arquivo = cliente.GetBlobClient(original);

                // Verifica se Arquivo Existe
                if (arquivo.Exists())
                {
                    // Copia dados do arquivo
                    var dados = arquivo.OpenRead();

                    // Criar novo Arquivo
                    cliente.UploadBlob(novo, dados);

                    // Excluiu arquivo Original
                    BlobClient file = cliente.GetBlobClient(original);
                    file.Delete();

                    FotoRetorno retorno = new()
                    {
                        Mensagem = $"Arquivo {original} renomado para {novo} !",
                        Arquivo = new Foto { Nome = novo }
                    };

                    return retorno;
                }
                else
                {
                    return new FotoRetorno { Mensagem = $"Arquivo {original} nao existe!", StatusCode = 500 };
                }
            }
            catch (RequestFailedException ex)
            {
                return new FotoRetorno { Mensagem = ex.Message.ToString(), StatusCode = ex.Status };
            }
        }
        public async Task<FotoRetorno> UploadAsync(IFormFile arquivo)
        {
            BlobContainerClient container = new(_storageConnectionString, _storageContainerName);
            try
            {
                BlobClient client = container.GetBlobClient(arquivo.FileName);
                await using (Stream? data = arquivo.OpenReadStream())
                {
                    await client.UploadAsync(data);
                }
                FotoRetorno retorno = new()
                {
                    Mensagem = $"Upload do arquivo {arquivo.FileName} realizado com Sucesso!",
                    Arquivo = new Foto { Nome = arquivo.FileName, TipoConteudo = arquivo.ContentType }
                };
                
                return retorno;
            }
            catch (RequestFailedException ex)
            {
                return new FotoRetorno { Mensagem = ex.Message.ToString(), StatusCode = ex.Status };
            }
        } 

        public async Task<FotoRetorno> DownloadAsync(string nome)
        {
            BlobContainerClient cliente = new(_storageConnectionString, _storageContainerName);
            
            try
            {
                BlobClient arquivo = cliente.GetBlobClient(nome);
                
                if (await arquivo.ExistsAsync())
                {
                    var dados = await arquivo.OpenReadAsync();
                    Stream blobContent = dados;
                    
                    var content = await arquivo.DownloadContentAsync();
                    
                    string contentType = content.Value.Details.ContentType;

                    FotoRetorno retorno = new()
                    {
                        Mensagem = $"Arquivo {nome} baixado com sucesso.",
                        Arquivo = new Foto { Conteudo = blobContent, Nome = nome, TipoConteudo = contentType }
                    };

                    return retorno;
                }
                else
                {
                    return new FotoRetorno { Mensagem = $"Arquivo {nome} nao existe.", StatusCode = 500 };
                }
            }
            catch (RequestFailedException ex)
            {
                return new FotoRetorno { Mensagem = ex.Message.ToString(), StatusCode = ex.Status};
            }
        }

        public async Task<FotoRetorno> ExcluiAsync(string arquivo)
        {
            BlobContainerClient cliente = new(_storageConnectionString, _storageContainerName);

            BlobClient arq = cliente.GetBlobClient(arquivo);

            try
            {
                await arq.DeleteAsync();
            }
            catch (RequestFailedException ex)
            {
                return new FotoRetorno { Mensagem = $"Arquivo {arquivo} nao foi encontrado.", StatusCode = ex.Status };
            }

            return new FotoRetorno { Mensagem = $"Arquivo: {arquivo} foi excluido com sucesso." };

        }

    }
}
