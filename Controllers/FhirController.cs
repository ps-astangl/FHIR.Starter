using System.Security.Cryptography.Xml;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Endpoint = Hl7.Fhir.Model.Endpoint;
using Task = System.Threading.Tasks.Task;

namespace FHIR.Starter.Controllers;

[ApiController]
[Route("[controller]")]
public class FhirController : ControllerBase
{
    private readonly ILogger<FhirController> _logger;
    private readonly ISyntheaWrapper _syntheaWrapper;

    public FhirController(ILogger<FhirController> logger, ISyntheaWrapper syntheaWrapper)
    {
        _logger = logger;
        _syntheaWrapper = syntheaWrapper;
    }

    [ProducesResponseType(typeof(Resource), 200)]
    [ProducesResponseType(typeof(OperationOutcome), 400)]
    [SwaggerOperation(Summary = "Get a FHIR resource", Description = "Retrieve a FHIR resource based on the type specified in the URL.", OperationId = "1", Tags = ["Resources"])]
    [HttpGet("{resource}")]
    public Task<IActionResult> Get([FromRoute] string resource)
    {
        var supportedResource = Enum.TryParse<FHIRDefinedType>(resource, true, out var result);
        if (!supportedResource)
        {
            return Task.FromResult<IActionResult>(Ok(new Hl7.Fhir.Model.OperationOutcome
            {
                Id = Guid.NewGuid().ToString(),
                Issue = new List<Hl7.Fhir.Model.OperationOutcome.IssueComponent>
                {
                    new Hl7.Fhir.Model.OperationOutcome.IssueComponent
                    {
                        Severity = Hl7.Fhir.Model.OperationOutcome.IssueSeverity.Error,
                        Code = Hl7.Fhir.Model.OperationOutcome.IssueType.NotSupported,
                        Diagnostics = $"The resource type '{resource}' is not supported.",
                        Details = new Hl7.Fhir.Model.CodeableConcept
                        {
                            Text = "Unsupported resource type"
                        }
                    }
                }
            }));
        }

        Patient patient = new Patient
        {
            Id = "3fc720e3-2389-487d-8e51-21f03d6f384f",
            Identifier = new List<Identifier>
            {
                new Identifier
                {
                    Use = Identifier.IdentifierUse.Usual,
                    Type = new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding("http://terminology.hl7.org/CodeSystem/v2-0203", "MR", "Medical Record Number")
                        },
                        Text = "MRN"
                    },
                    System = "http://hospital.smarthealthit.org",
                    Value = "123456"
                }
            },
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Use = HumanName.NameUse.Official,
                    Family = "BLACKSTONE",
                    Given = new[] { "VERONICA", "SMITH" }
                },
                new HumanName
                {
                    Given = new[] { "Verni" },
                    Family = "BLACKSTONE",
                    Use = HumanName.NameUse.Nickname,
                },
                new HumanName
                {
                    Given = new[] { "VERONICA" },
                    Family = "BLACKSTONE",
                    Use = HumanName.NameUse.Usual,
                }
            },
            Gender = AdministrativeGender.Female,
            BirthDate = "1998-06-18",
            Address = new List<Address>
            {
                new Address
                {
                    Use = Address.AddressUse.Home,
                    Line = new List<string> { "123 Main Street" },
                    City = "Baltimore",
                    State = "MD",
                    PostalCode = "21201",
                    Country = "USA"
                }
            }
        };

        // Return the patient resource as a task
        return Task.FromResult<IActionResult>(Ok(patient));
    }

    [HttpGet("[action]")]
    public IActionResult Test(CancellationToken cancellationToken)
    {
        try
        {
            List<Resource> resources = _syntheaWrapper.RunSyntheaAsync(cancellationToken);
            return Ok(resources);
        }
        catch (Exception exception)
        {
            var oo = OperationOutcome.ForException(exception, OperationOutcome.IssueType.Exception);
            _logger.LogError(exception, "An error occurred while running Synthea");
            return BadRequest(oo);
        }
    }
}