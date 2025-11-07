using GSheetToDataCore; // Use the core library
using System;
using System.Collections.Generic;
using System.IO; // Added for file operations

// This is a mock ValueRange object that simulates the data from Google Sheets.
var mockValueRange = new Google.Apis.Sheets.v4.Data.ValueRange()
{
    Values = new List<IList<object>>
    {
        new List<object> { "int", "string", "float", "bool", "pair<int,string>", "int[]", "pair<string,int>[]" }, // Field Types
        new List<object> { "Id", "Name", "Price", "IsAvailable", "ItemPair", "Count", "Resource" }, // Field Names
        new List<object> { "1", "Apple", "1.2", "true", "(10,Red)", "1,2,3", "(Wood, 12), (Stone, 4)" },
        new List<object> { "2", "Banana", "0.8", "false", "(20,Yellow)", "4,5", "(Gold, 1), (Silver, 10)" },
        new List<object> { "3", "Orange", "1.5", "true", "(30,Orange)", "6", "(Water, 100)" }
    }
};

const string sheetName = "FieldTransform";//"ItemData";

const string clientSecretFileName = "client_secret.json";
const string tokenStoreFolderName = "OAuthToken";
string clientSecretPath = Path.Combine(AppContext.BaseDirectory, clientSecretFileName);
string tokenStorePath = Path.Combine(AppContext.BaseDirectory, tokenStoreFolderName);

var loader = new SheetLoader();
var values = await loader.LoadSheetAsync(
    "1_2Y3BtltwsyXTovWuWV6J32x_Ebe2Sy8vybGNhzkIsM",
    sheetName,
    clientSecretPath,
    tokenStorePath);

var parser = new DataParser();
var parsedData = parser.Parse(sheetName, values);// mockValueRange);

if (!string.IsNullOrEmpty(parsedData.ClassName))
{
    Console.WriteLine($"ClassName: {parsedData.ClassName}");
    Console.WriteLine();

    Console.WriteLine("Field Types:");
    Console.WriteLine(string.Join(", ", parsedData.FieldTypes));
    Console.WriteLine();

    Console.WriteLine("Field Names:");
    Console.WriteLine(string.Join(", ", parsedData.FieldNames));
    Console.WriteLine();

    Console.WriteLine("Data Rows:");
    foreach (var row in parsedData.DataRows)
    {
        Console.WriteLine(string.Join(", ", row));
    }

    Console.WriteLine("\n--- Generated JSON ---");
    var jsonGenerator = new JsonGenerator();
    string jsonString = jsonGenerator.GenerateJsonString(parsedData);
    Console.WriteLine(jsonString);

    Console.WriteLine("\n--- Generated C# Class ---");
    var classGenerator = new ClassGenerator();
    string classString = classGenerator.GenerateClassString(parsedData);
    Console.WriteLine(classString);

    // Save to files
    string outputDirectory = Path.Combine(AppContext.BaseDirectory, "Output");
    Directory.CreateDirectory(outputDirectory);

    string jsonFilePath = Path.Combine(outputDirectory, $"{parsedData.ClassName}.json");
    File.WriteAllText(jsonFilePath, jsonString);
    Console.WriteLine($"\nJSON saved to: {jsonFilePath}");

    string classFilePath = Path.Combine(outputDirectory, $"{parsedData.ClassName}.cs");
    File.WriteAllText(classFilePath, classString);
    Console.WriteLine($"C# Class saved to: {classFilePath}");
}
else
{
    Console.WriteLine("Failed to parse data.");
}
