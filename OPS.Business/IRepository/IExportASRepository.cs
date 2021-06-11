using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.ExportFileAS;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface IExportASRepository
    {
        CsvExport ExportFileAsFilter(ExportFileASFilterModel filterModel, out List<string> listErrorEncode, out List<int> listUpdateFlagCSV);
        void UpdateFlagCSV(List<int> listUpdateFlagCSV, int exportMode);
        List<string> ExportFileAsFilter(ExportFileASFilterModel filterModel, out List<int> listContractCd);
    }
}
