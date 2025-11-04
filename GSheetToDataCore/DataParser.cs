
using Google.Apis.Sheets.v4.Data;
using System.Linq;

namespace GSheetToDataCore
{
    public class DataParser
    {
        public ParsedSheetData Parse(string sheetName, ValueRange valueRange)
        {
            if (valueRange?.Values == null || valueRange.Values.Count < 2)
            {
                // Not enough data to parse, return an empty object.
                return new ParsedSheetData();
            }

            var parsedData = new ParsedSheetData
            {
                ClassName = sheetName,
                FieldTypes = valueRange.Values[0].Select(c => c?.ToString() ?? string.Empty).ToList(),
                FieldNames = valueRange.Values[1].Select(c => c?.ToString() ?? string.Empty).ToList(),
                DataRows = valueRange.Values.Skip(2).ToList()
            };

            return parsedData;
        }
    }
}
