using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Task = System.Threading.Tasks.Task;

namespace FHIR.Starter;
public class FhirJsonInputFormatter : TextInputFormatter
{
    private readonly FhirJsonParser _fhirJsonParser;
    public FhirJsonInputFormatter()
    {

        SupportedMediaTypes.Add("application/fhir+json");
        SupportedMediaTypes.Add("application/json");
        SupportedEncodings.Add(Encoding.UTF8);
        _fhirJsonParser = new FhirJsonParser();
    }

    protected override bool CanReadType(Type type)
    {
        return typeof(Resource).IsAssignableFrom(type);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
        var body = await reader.ReadToEndAsync();
        var resource = _fhirJsonParser.Parse<Resource>(body);

        return await InputFormatterResult.SuccessAsync(resource);
    }
}


public class FhirJsonOutputFormatter : TextOutputFormatter
{
    private readonly FhirJsonSerializer _fhirJsonSerializer;
    public FhirJsonOutputFormatter()
    {
        SupportedMediaTypes.Add("application/fhir+json");
        SupportedMediaTypes.Add("application/json");
        SupportedEncodings.Add(Encoding.UTF8);
        _fhirJsonSerializer = new FhirJsonSerializer();
    }

    protected override bool CanWriteType(Type type)
    {
        return typeof(Resource).IsAssignableFrom(type);
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var response = context.HttpContext.Response;

        // If no resource is provided, create a default OperationOutcome
        var resource = context.Object as Resource ?? new Hl7.Fhir.Model.OperationOutcome
        {
            Id = Guid.NewGuid().ToString(),
            Issue = new List<Hl7.Fhir.Model.OperationOutcome.IssueComponent>
            {
                new Hl7.Fhir.Model.OperationOutcome.IssueComponent
                {
                    Severity = Hl7.Fhir.Model.OperationOutcome.IssueSeverity.Error,
                    Code = Hl7.Fhir.Model.OperationOutcome.IssueType.Invalid,
                    Diagnostics = "No resource was provided for the response."
                }
            }
        };

        var json = await _fhirJsonSerializer.SerializeToStringAsync(resource);

        await response.WriteAsync(json, selectedEncoding);
    }
}