using DigitalSignClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalSignClient.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<Document>?> GetDocumentsAsync();
        Task<Document?> UploadDocumentAsync(string filePath, string? description, Guid documentTypeId);
        Task<bool> UploadSignedDocumentAsync(Guid documentId, string signedFilePath);
        Task<bool> SignDocumentAsync(Guid documentId, string reason, string location);
        Task<byte[]?> DownloadDocumentAsync(Guid documentId);
        Task<bool> DeleteDocumentAsync(Guid documentId);
    }
}
