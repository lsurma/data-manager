using DataManager.Application.Contracts.Common;

namespace DataManager.Application.Contracts.Modules.Translations
{
    public class InternalGroupName1Filter : IQueryFilter
    {
        public string Value { get; set; }
        public string Name => "InternalGroupName1Filter";
        public bool IsActive() => !string.IsNullOrWhiteSpace(Value);
    }
}
