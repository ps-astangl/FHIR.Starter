using System.Diagnostics;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;

public interface ISyntheaWrapper
{
    public List<Resource> RunSyntheaAsync(CancellationToken cancellationToken);
}

public class SyntheaWrapper : ISyntheaWrapper
{
    private readonly ILogger<SyntheaWrapper> _logger;
    private readonly FhirJsonParser _fhirJsonParser;

    public SyntheaWrapper(ILogger<SyntheaWrapper> logger)
    {
        _logger = logger;
        _fhirJsonParser = new FhirJsonParser();
    }

    public List<Resource> RunSyntheaAsync(CancellationToken cancellationToken)
    {
        string javaPath = Path.Combine(AppContext.BaseDirectory, "jdk", "jdk-11.0.2", "bin", "java");
        string command = javaPath;
        string arguments = "-jar synthea-with-dependencies.jar";

        using (Process process = new Process())
        {
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogInformation("{Data}", e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogError("{Data}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!process.HasExited)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Cancellation requested. Killing the process...");
                    process.Kill();
                    cancellationToken.ThrowIfCancellationRequested();
                }

                process.WaitForExit(1000);
            }

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                throw new Exception($"Synthea process exited with code {exitCode}");
            }
        }

        _logger.LogInformation("Process Complete");
        var resources = HandleGeneratedResources();
        return resources;
    }

    public List<Resource> HandleGeneratedResources()
    {
        string outputPath = Path.Combine("output", "fhir");
        var resources = new List<Resource>();

        if (!Directory.Exists(outputPath))
        {
            _logger.LogError($"The output path {outputPath} does not exist.");
            return resources;
        }

        var files = Directory.GetFiles(outputPath, "*.json");
        _logger.LogInformation("{Files}", files);
        foreach (var file in files)
        {
            _logger.LogInformation("{File}", file);
            try
            {
                string jsonContent = File.ReadAllText(file);
                var fhirResource = _fhirJsonParser.Parse<Bundle>(jsonContent);
                if (fhirResource != null)
                {
                    resources.Add(fhirResource);
                }

                // Delete the file after processing it
                File.Delete(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file {file}");
            }
        }

        return resources;
    }
}