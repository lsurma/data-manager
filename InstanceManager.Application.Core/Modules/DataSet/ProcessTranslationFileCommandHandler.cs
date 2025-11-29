using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using InstanceManager.Application.Contracts.Modules.DataSets;
using MediatR;

namespace InstanceManager.Application.Core.Modules.DataSet;

public class ProcessTranslationFileCommandHandler : IRequestHandler<ProcessTranslationFileCommand>
{
    public Task Handle(ProcessTranslationFileCommand request, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{request.DataSetId}_{request.FileName}");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();

        if (fileExtension == ".csv")
        {
            ProcessCsv(filePath);
        }
        else if (fileExtension == ".xlsx")
        {
            ProcessXlsx(filePath);
        }
        else
        {
            throw new NotSupportedException("Unsupported file format.");
        }

        // For now, just delete the file after processing.
        File.Delete(filePath);

        return Task.CompletedTask;
    }

    private void ProcessCsv(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<dynamic>();
            // TODO: Add logic to process the CSV records.
        }
    }

    private void ProcessXlsx(string filePath)
    {
        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheets.Worksheet(1);
            // TODO: Add logic to process the XLSX worksheet.
        }
    }
}