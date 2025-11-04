
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using System.Threading.Tasks;

namespace GSheetToDataCore
{
    public class SheetLoader
    {
        private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };

        public async Task<ValueRange> LoadSheetAsync(string spreadsheetId, string sheetName, string serviceAccountKeyPath)
        {
            var credential = await GetCredentialsFromFile(serviceAccountKeyPath);
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleSheetToData",
            });

            var range = $"'{sheetName}'!A1:Z";
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);

            return await request.ExecuteAsync();
        }

        private async Task<GoogleCredential> GetCredentialsFromFile(string serviceAccountKeyPath)
        {
            await using var stream = new FileStream(serviceAccountKeyPath, FileMode.Open, FileAccess.Read);
            return GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }
    }
}
