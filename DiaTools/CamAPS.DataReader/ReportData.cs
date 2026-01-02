using BasalRateCalculator.Model;

namespace CamAPS.DataReader;

// Compatibility shim: keep a ReportData type in the CamAPS.DataReader namespace that inherits the model type.
// This preserves existing callers that reference CamAPS.DataReader.ReportData while the canonical type lived in the model project
public class ReportData : BasalRateCalculator.Model.ReportData
{
}