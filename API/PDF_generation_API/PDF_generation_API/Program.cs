using Newtonsoft.Json;
using PDF_generation_API;
using System.Dynamic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/pdf", async (HttpContext context) =>
{
    try
    {
        var html = await context.Request.ReadFormAsync();
        var value = html.AsQueryable().ToList().Where(x => x.Key == "application/json").FirstOrDefault().Value.ToString();
        var options = JsonConvert.DeserializeObject<ConversionOptions>(value);

        string htmlText = "";
        string jsonData = "";
        if (options != null)
        {
            htmlText = ReadText(html.Files[options.Index].OpenReadStream());
            jsonData = ReadText(html.Files[options.Data].OpenReadStream());
            CopyAssets(options.Assets, html.Files);
        }


        String path = Path.GetFullPath("template/");

        var conversion = new HtmlToPdfConversion();
        var pdf = conversion.ConvertToPDF(htmlText, path, jsonData, options);

        context.Response.ContentType = "application/pdf";
        await context.Response.Body.WriteAsync(pdf);
    }
    catch (Exception exception)
    {

    }
});

app.Run();

void CopyAssets(List<string> assets, IFormFileCollection files)
{
    if (Directory.Exists("template/"))
    {
        System.IO.DirectoryInfo di = new DirectoryInfo("template/");

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
    }
    var formFiles = files.ToList();
    foreach (var asset in assets)
    {
        Stream stream = formFiles.FirstOrDefault(x => x.FileName == asset).OpenReadStream();
        if (stream != null)
        {
            var fileStream = new FileStream("template/" + asset, FileMode.Create);
            stream.CopyTo(fileStream);
            fileStream.Close();
            stream.Close();
        }
    }
}
string ReadText(Stream strem)
{
    StreamReader reader = new StreamReader(strem);
    string text = reader.ReadToEnd();
    reader.Close();
    strem.Dispose();

    return text;
}