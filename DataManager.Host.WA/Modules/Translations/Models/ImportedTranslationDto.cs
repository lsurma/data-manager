using System.Data;
using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Host.WA.Modules.Translations.Models
{
    public record ImportedTranslationDto : TranslationDto
    {
        public DataRow? OriginalRow { get; set; }
    }
}
