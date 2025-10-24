

using DigitalSignClient.Models;

namespace DigitalSignClient.Services.Interfaces
{
    public interface IDocumentTypeService
    {
        Task<List<DocumentType>?> GetAllAsync();
        Task<DocumentType?> CreateAsync(DocumentType dto);
        Task<DocumentType?> UpdateAsync(Guid id, DocumentType dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
