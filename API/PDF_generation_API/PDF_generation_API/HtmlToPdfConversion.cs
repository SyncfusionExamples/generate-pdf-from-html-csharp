using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System.Dynamic;
using Scriban.Runtime;
using Newtonsoft.Json;
using Syncfusion.Pdf.Graphics;

namespace PDF_generation_API
{
    public class HtmlToPdfConversion
    {
        public byte[] ConvertToPDF(string pageContent, string path, string modelData, ConversionOptions options)
        {

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
                blinkConverterSettings.ViewPortSize = new Syncfusion.Drawing.Size(595, 842);
            }

            blinkConverterSettings.Margin = new PdfMargins() { All = options.Margin };
            htmlConverter.ConverterSettings = blinkConverterSettings;

            //Convert HTML string to PDF
            PdfDocument document = htmlConverter.Convert(result, path);

            MemoryStream output = new MemoryStream();

            //Save and close the PDF document 
            document.Save(output);
            document.Close(true);


            output.Position = 0;
            byte[] pdfBytes = output.ToArray();
            output.Close();

            return pdfBytes;
        }

        private ScriptObject BuildScriptObject(ExpandoObject expando)
        {
            var dict = (IDictionary<string, object>)expando;
            var scriptObject = new ScriptObject();

            foreach (var kv in dict)
            {
                var renamedKey = StandardMemberRenamer.Rename(kv.Key);
                if (renamedKey.Equals("items"))
                {

                }

                if (kv.Value is ExpandoObject expandoValue)
                {
                    scriptObject.Add(renamedKey, BuildScriptObject(expandoValue));
                }
                else if (kv.Value is List<object>)
                {

                    var itemsList = new List<ScriptObject>();
                    foreach (var item in kv.Value as List<object>)
                    {
                        if (item is ExpandoObject expandoItem)
                        {
                            itemsList.Add(BuildScriptObject(expandoItem));
                        }
                    }

                    scriptObject.Add(renamedKey, itemsList);
                }
                else
                {
                    scriptObject.Add(renamedKey, kv.Value);
                }
            }

            return scriptObject;
        }
    }
}
