using System.Data;
using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Host.WA.Modules.Translations.Models
{
    public record ImportedTranslationDto : TranslationDto
    {
        public DataRow? OriginalRow { get; set; }
        public bool ShouldImport { get; set; } = true;
        public ImportStatus Status { get; set; } = ImportStatus.Pending;
        public string? StatusMessage { get; set; }
    }

    public enum ImportStatus
    {
        Pending,
        InProgress,
        Success,
        Failed
    }
}
