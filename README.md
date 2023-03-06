# Generate PDF documents from dynamic HTML using Syncfusion PDF generator API 

The [Syncfusion HTML-to-PDF converter library](https://www.syncfusion.com/document-processing/pdf-framework/net/html-to-pdf) in combination with [ASP.NET Core Minimal Web API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0&tabs=visual-studio) offers a simple and straightforward approach for dynamically generating PDFs from HTML templates. 

In this repository, we will learn how to create an ASP.NET Core Minimal Web API that dynamically generates a PDF document from an HTML template using Syncfusion HTML-to-PDF converter library.  

## Steps to create PDF invoice from HTML template using ASP.NET Core Minimal Web API
1. Create HTML template with CSS styling 
2. Create a minimal Web API project with ASP.NET Core (Server application)  
3. Create Blazor WASM with .NET 7 (Client application)  
4. Launching the Server and Invoking the Web API from the Client  

### Create HTML template with CSS styling 

In HTML file, define the structure of your page, including the head and body sections, any other elements you'd like to include, such as image, header, footer, etc.  

Also, it contains placeholders with {{mustache}} syntax and it is used to bind the actual data to the HTML template. For this example, we'll use the [Scriban scripting language](https://github.com/scriban/scriban) to create the placeholders. It's a lightweight scripting language and engine for .NET. 

N> To learn more about the Scriban scripting language, refer to the [documentation](https://github.com/scriban/scriban/tree/master/doc).

**index.html**
```html

<html lang="en">
<head>
    <meta charset="UTF-8" />
    <title>JobOfferLetter</title>
    <link rel="stylesheet" href="style.css" media="all" />
</head>

<body>
    <div class="grid-container ">
        <img class="logo" src="logo.png" style="width:70px;height:70px;margin-left: 50px;" />
        <div style="margin-left:70px; margin-top:55px">
            <b>
                AMAZE <br />
                FOX
            </b>
        </div>
        <div style="font-size: 12px; margin-left: 300px; margin-top: 55px">
            <div><b>{{generate_customer_address.street}} </b></div>
            <div><b>{{generate_customer_address.city}} </b></div>
            <div><b>Phone: {{generate_customer_address.phone}} </b></div>
            <div><b>{{generate_customer_address.website}} </b></div>
        </div>
    </div>
    <br />
    <div class="body-content" style="margin-left: 50px;">
        <p><b>Dear John Smith,</b></p>
        <p>We are pleased to offer you the position of Accountant at Amaze Fox. We feel confident that you will contribute your skills and experience towards the growth of our organization.</p>
        <p>As per the discussion, your starting date will be {{starting_date}}. Please find the employee handbook enclosed herewith which contains the medical and retirement benefits offered by our organizations.</p>
        <p>Please confirm your acceptance of this offer by signing and returning a copy of this offer letter.</p>
        <p>If you have any questions regarding the same, contact the manager or me via email or phone: {{generate_customer_address.phone}}</p>
        <p>We look forward to welcoming you on board.</p>
    </div>

    <div class="body-content" style="margin-left: 50px;">
        <p><b>Sincerely,<br /> {{company_name}} <br />{{person_name}} <br />(Managing Director)</b></p>
    </div>

    <div id="footer">
        <div class="footer-columns" style="margin-left: 50px;">
            <p>AMAZE FOX PVT LTD</p>
            <p>{{generate_customer_address.website}}</p>
        </div>
    </div>
</body>
</html>

```

By default, the properties and methods of .NET objects are automatically exposed with lowercase and _ names. This means that a property like *CompanyName* will be exposed as *company_name*.name and while performing the conversion, the values can be imported from respective JSON file. 

<img src="Templates/Screenshots/JSON_data.png" alt="Job offer letter JSON data" width="100%" Height="Auto"/>

#### CSS Styling

Create CSS file to control the appearance of your HTML template. In this CSS file, you'll define styles for your page elements, such as font sizes, colors, and images.   

You can get this HTML template with CSS and fonts from this [location](Templates/JobOfferLetter//).  

**style.css**
```css

@font-face {
    font-family: "OpenSans";
    src: url("OpenSans-Regular.ttf");
}

.grid-container {
    display: grid;
    grid-template-columns: 60px auto auto;
    padding: 10px;
    background-color: darkblue;
    font-size: 25px;
    text-align: left;
    font-family: OpenSans;
    color: white;
    height: 170px;
}

.body-content {
    font-size: 16px;
    font-family: OpenSans;
    padding: 20px;
    padding-right: 2.5rem;
    text-align: left;
}

.logo {
    margin-top: 50px;
}

#footer {
    font-size: 12px;
    font-family: OpenSans;
    text-align: left;
    font-weight: 500;
    color: white;
    margin-top: 0.5rem;
    bottom: 0.5rem;
    position: absolute;
    width: 100%;
    background-color: darkblue;
}

.footer-columns {
    display: flex;
    justify-content: space-between;
    padding-left: 1.5rem;
    padding-right: 2.5rem;
}

``` 

The following screenshot shows the output of the HTML template with styled CSS.
<img src="Templates/Screenshots/HTML_template.png" alt="Job offer letter HTML Template" width="100%" Height="Auto"/>

Furthermore, any additional resources such as fonts, images, JSON files, etc. should be located in the same folder as the HTML file. Please refer to the screenshot below for visual reference. 
<img src="Templates/Screenshots/Folder_structure.png" alt="Job offer letter folder structure" width="100%" Height="Auto"/>

### Create a minimal Web API project with ASP.NET Core 
Minimal APIs are architected to create HTTP APIs with minimal dependencies. They are ideal for microservices and apps that want to include only the minimum files, features, and dependencies in ASP.NET Core. Kindly refer the below link to create the project in Visual Studio 2022. 
[Steps to create minimal API project with ASP.NET Core 7.0](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0&tabs=visual-studio)

Within this Web API application, the [Program.cs](API/PDF_generation_API/PDF_generation_API/Program.cs) file employs the [StreamReader](https://learn.microsoft.com/en-us/dotnet/api/system.io.streamreader?view=net-7.0) class to transform the HTML and JSON files, supplied by the client's request, into textual format. Subsequently, combining the HTML text with the required assets via the CopyAssets() method. 

```csharp

app.MapPost("/api/convertToPDF", async (HttpContext context) =>
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

app.UseCors("AllowBlazorClient");
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
    else
        Directory.CreateDirectory("template/");

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

```

Next, the [HtmlToPdfConverter](https://help.syncfusion.com/cr/file-formats/Syncfusion.HtmlConverter.HtmlToPdfConverter.html) is utilized to turn the HTML string, accompanied by the assets, into a PDF document via the blink rendering engine within the [HtmlToPdfConversion.cs](API/PDF_generation_API/PDF_generation_API/HtmlToPdfConversion.cs) file. During this conversion, we have established the size and margin of the PDF page and the viewport size using the [BlinkConverterSettings](https://help.syncfusion.com/cr/file-formats/Syncfusion.HtmlConverter.BlinkConverterSettings.html) class. Please refer to the accompanying code example for further information.  

```csharp

var expando = JsonConvert.DeserializeObject<ExpandoObject>(modelData);
var sObject = BuildScriptObject(expando);
var templateCtx = new Scriban.TemplateContext();
templateCtx.PushGlobal(sObject);
var template = Scriban.Template.Parse(pageContent);
var result = template.Render(templateCtx);

//Initialize HTML to PDF converter with Blink rendering engine
HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
if (options.Width != 0 && options.Height != 0)
{
    blinkConverterSettings.PdfPageSize = new Syncfusion.Drawing.SizeF(options.Width, options.Height);
}
else
{
    blinkConverterSettings.ViewPortSize= new Syncfusion.Drawing.Size(595, 842);
}
blinkConverterSettings.Margin = new PdfMargins() { All = options.Margin };
htmlConverter.ConverterSettings = blinkConverterSettings;
//Convert HTML string to PDF
PdfDocument document = htmlConverter.Convert(result, path);

//Save and close the PDF document 
MemoryStream output = new MemoryStream();
document.Save(output);
document.Close(true);

```

### Create Blazor WASM application (client)  

The client application in this implementation is a Blazor WASM application built with .NET version 7.0. To create a new ASP.NET Core Blazor WebAssembly application using Visual Studio 2022, please follow the guidance provided in [this](https://help.syncfusion.com/file-formats/pdf/create-pdf-document-in-blazor#steps-to-create-pdf-document-in-blazor-client-side-application) link. Within the application, we utilize the *HttpClient.PostAsync* method to send a POST request to the specified URI as an asynchronous operation.

```csharp

//Send request to server 
var response = await client.PostAsync("https://localhost:7045/api/convertToPDF", content); 

```

#### Incorporating HTML files and additional assets 

The inclusion of the HTML and CSS files in the HttpClient is necessary. Additionally, any supplementary assets such as fonts, images, and PDF size specifications should be sent along with the request using the *MultipartFormDataContent*.  

```csharp

private async Task ConvertToPDF()
{
    //Create http client to send both files and json data
    using (var client = new HttpClient())
    {
            //Create multipart form data content
            using (var content = new MultipartFormDataContent())
            {
                var html = await Http.GetByteArrayAsync("JobOfferLetter/index.html");
                var css = await Http.GetByteArrayAsync("JobOfferLetter/style.css");
                var data = await Http.GetByteArrayAsync("JobOfferLetter/Data.json");
                var logo = await Http.GetByteArrayAsync("JobOfferLetter/logo.png");
                var font = await Http.GetByteArrayAsync("JobOfferLetter/OpenSans-Regular.ttf");

                //Add file to content
                content.Add(CreateContent("index.html", "index.html", html));
                content.Add(CreateContent("style.css", "style.css", css));
                content.Add(CreateContent("Data.json", "Data.json", data));
                content.Add(CreateContent("logo.png", "logo.png", logo));
                content.Add(CreateContent("OpenSans-Regular.ttf", "OpenSans-Regular.ttf", font));
                var json = new JsonObject
                {
                        ["index"] = "index.html",
                        ["data"] = "Data.json",
                        ["width"] = 0,
                        ["height"] = 0,
                        ["margin"] = 40,
                        ["assets"] = new JsonArray
                        {
                          "style.css",
                          "logo.png",
                          "OpenSans-Regular.ttf"
                        }
                };
                //Add json data to content
                content.Add(new StringContent(json.ToString()), "application/json");
                //Send request to server
                var response = await client.PostAsync("https://localhost:7045/api/convertToPDF", content);
                if (response.StatusCode == HttpStatusCode.OK)
               {
                var responseContent = await response.Content.ReadAsStreamAsync();
                using var Content = new DotNetStreamReference(stream: responseContent);
                await JS.InvokeVoidAsync("SubmitHTML", "HTMLToPDF.pdf", Content);
               }
        }
    }
}

```

Once the requested response status code is OK, then invoke the JavaScript (JS) function in index.html file to save the PDF document.  

```html

<script>
    window.SubmitHTML = async (fileName, contentStreamReference) => {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
        URL.revokeObjectURL(url);
    }
</script>

```

While building and running the application, the website will open in your default browser. 
<img src="Templates/Screenshots/Blazor_UI.png" alt="Blazor UI" width="100%" Height="Auto"/>

### Launching the Server and Invoking the PDF Generation API from the Client 

Here are the steps to launching the server and invoking the PDF generation API from the client application.  

Step 1: Run the Web API application, which will launch the published web API in the browser.   

Step 2: To generate a PDF document using the client application, send an asynchronous POST request to the specified URI (e.g., https://localhost:7094/api/convertToPDF) on the localhost. This will send the request to the server application, which will convert the HTML to PDF and send the response back to the client. After running the client application, click the "Convert to PDF" button, which will initiate the HTML to PDF conversion process and generate a PDF document named "HTMLToPDF.pdf" in the designated folder.  
<img src="Templates/Screenshots/Blazor_UI.png" alt="Blazor UI" width="100%" Height="Auto"/>

Upon successful conversion, you will receive a PDF document as illustrated in the following screenshot.  
<img src="Templates/Screenshots/JobOfferLetter.png" alt="Job offer letter output" width="100%" Height="Auto"/>

## Sample Templates 

Template Name | HTML Template | Description 
--- | --- | ---
[Invoice](https://github.com/SyncfusionExamples/generate-pdf-from-html-csharp/tree/master/Templates/Invoice) | <img src="Templates/Screenshots/Invoice.jpg" alt="Invoice HTML Template" width="100%" Height="Auto"/> | An invoice is a commercial document issued by a seller to a buyer relating to a sale transaction and indicating the products, quantities, and agreed-upon prices for products or services the seller had provided the buyer. 
[BoardingPass](https://github.com/SyncfusionExamples/generate-pdf-from-html-csharp/tree/master/Templates/BoardingPass) | <img src="Templates/Screenshots/BoardingPass.jpg" alt="BoardingPass HTML Template" width="100%" Height="Auto"/> | A boarding pass is a document provided by an airline during check-in, giving a passenger permission to board the airplane for a particular flight.
[Lease Agreement](https://github.com/SyncfusionExamples/generate-pdf-from-html-csharp/tree/master/Templates/LeaseAgreement) | <img src="Templates/Screenshots/LeaseAgreement.jpg" alt="LeaseAgreement HTML Template" width="100%" Height="Auto"/> | A rental agreement is a contract of rental, usually written, between the owner of a property and a renter who desires to have temporary possession of the property.
[EmployeeCertificate](https://github.com/SyncfusionExamples/generate-pdf-from-html-csharp/tree/master/Templates/Certificate) |  <img src="Templates/Screenshots/Certificate.jpg" alt="Certificate of appreciation HTML Template" width="100%" Height="Auto"/> | This certificate is one of the way to say thank you to the employees who work in their organization.
[HospitalDischarge](https://github.com/SyncfusionExamples/generate-pdf-from-html-csharp/tree/master/Templates/HospitalDischarge) |  <img src="Templates/Screenshots/HospitalDischarge.png" alt="Certificate of appreciation HTML Template" width="100%" Height="Auto"/> | This hospital discharge template captures the medical information related to patient during the stay in the hospital.
[JobOfferLetter](https://github.com/SyncfusionExamples/generate-pdf-from-html-csharp/tree/master/Templates/JobOfferLetter) | <img src="Templates/Screenshots/JobOfferLetter.png" alt="JobOfferLetter HTML Template" width="100%" Height="Auto"/> | Job offer letter refers to an official document employer gives to an employee in order to provide them with an offer of employment.
[PatientMedicalRecord](https://github.com/SyncfusionExamples/generate-pdf-from-html-csharp/tree/master/Templates/PatientMedicalRecord) | <img src="Templates/Screenshots/MedicalRecord.png" alt="JobOfferLetter HTML Template" width="100%" Height="Auto"/> | The patient medical records are used to describe the systematic documentation of a single patient's medical history.

# Resources
*   **Product page:** [Syncfusion PDF Framework](https://www.syncfusion.com/document-processing/pdf-framework/net)
*   **Documentation page:** [Syncfusion .NET PDF library](https://help.syncfusion.com/file-formats/pdf/overview)
*   **Online demo:** [Syncfusion .NET PDF library - Online demos](https://ej2.syncfusion.com/aspnetcore/PDF/CompressExistingPDF#/bootstrap5)
*   **Blog:** [Syncfusion .NET PDF library - Blog](https://www.syncfusion.com/blogs/category/pdf)
*   **Knowledge Base:** [Syncfusion .NET PDF library - Knowledge Base](https://www.syncfusion.com/kb/windowsforms/pdf)
*   **EBooks:** [Syncfusion .NET PDF library - EBooks](https://www.syncfusion.com/succinctly-free-ebooks)
*   **FAQ:** [Syncfusion .NET PDF library - FAQ](https://www.syncfusion.com/faq/)

# Support and feedback
*   For any other queries, reach our [Syncfusion support team](https://www.syncfusion.com/support/directtrac/incidents/newincident?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples) or post the queries through the [community forums](https://www.syncfusion.com/forums?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples).
*   Request new feature through [Syncfusion feedback portal](https://www.syncfusion.com/feedback?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples).

# License
This is a commercial product and requires a paid license for possession or use. Syncfusion’s licensed software, including this component, is subject to the terms and conditions of [Syncfusion's EULA](https://www.syncfusion.com/eula/es/?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples). You can purchase a licnense [here](https://www.syncfusion.com/sales/products?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples) or start a free 30-day trial [here](https://www.syncfusion.com/account/manage-trials/start-trials?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples).

# About Syncfusion
Founded in 2001 and headquartered in Research Triangle Park, N.C., Syncfusion has more than 26,000+ customers and more than 1 million users, including large financial institutions, Fortune 500 companies, and global IT consultancies.

Today, we provide 1600+ components and frameworks for web ([Blazor](https://www.syncfusion.com/blazor-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [ASP.NET Core](https://www.syncfusion.com/aspnet-core-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [ASP.NET MVC](https://www.syncfusion.com/aspnet-mvc-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [ASP.NET WebForms](https://www.syncfusion.com/jquery/aspnet-webforms-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Angular](https://www.syncfusion.com/angular-ui-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [React](https://www.syncfusion.com/react-ui-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Vue](https://www.syncfusion.com/vue-ui-components?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), and [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples)), mobile ([Xamarin](https://www.syncfusion.com/xamarin-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), and [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples)), and desktop development ([WinForms](https://www.syncfusion.com/winforms-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [WPF](https://www.syncfusion.com/wpf-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [WinUI(Preview)](https://www.syncfusion.com/winui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples) and [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=github&utm_medium=listing&utm_campaign=github-docio-examples)). We provide ready-to-deploy enterprise software for dashboards, reports, data integration, and big data processing. Many customers have saved millions in licensing fees by deploying our software.

