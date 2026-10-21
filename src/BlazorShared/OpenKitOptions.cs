using System.ComponentModel.DataAnnotations;
namespace BlazorShared;
/// <summary>
/// OpenKit configuration options.
/// </summary>
/// <remarks>
/// <para>
/// These options are adapted to match the primary focus, which is sending bizevents.
/// To-Do: elaborate more before showing the following:
/// </para>
/// <list type="bullet">
///   <item><description>ApplicationName = ModelId</description></item>
///   <item><description>Namespace = Manufacturer</description></item>
/// </list>
/// <para>
/// The following two OpenKit configurations are intentionally left out since they are
/// unrelated to bizevent functionality:
/// </para>
/// <list type="number">
///   <item><description>DataCollectionLevel</description></item>
///   <item><description>CrashReportingLevel</description></item>
/// </list>
/// <para>
/// See the <see href="https://github.com/Dynatrace/openkit-dotnet">OpenKit GitHub documentation</see>.
/// </para>
/// </remarks>
public class OpenKitOptions
{
    /// <summary>
    /// Configuration section bound to these options
    /// (equivalent of the Spring prefix <c>com.rumagent.bizevents</c>).
    /// </summary>
    public const string SectionName = "RumAgent:BusinessEvents";
 
    /// <summary>The Dynatrace application ID.</summary>
    [Required]
    public string ApplicationId { get; set; } = string.Empty;
 
    /// <summary>The Dynatrace beacon endpoint URL.</summary>
    [Required, Url]
    public string EndpointUrl { get; set; } = string.Empty;
 
    /// <summary>Unique identifier of the device.</summary>
    public long DeviceId { get; set; }
 
    /// <summary>Application name (maps to the model ID).</summary>
    public string? ApplicationName { get; set; }
 
    /// <summary>Application version.</summary>
    public string? ApplicationVersion { get; set; }
 
    /// <summary>Operating system reported to Dynatrace.</summary>
    public string? OperatingSystem { get; set; }
 
    /// <summary>Manufacturer (maps to the namespace).</summary>
    public string? Manufacturer { get; set; }
 
    /// <summary>OpenKit log level (e.g. Debug, Info, Warn, Error).</summary>
    public string? LogLevel { get; set; }
 
    /// <summary>Whether to block until OpenKit initialization has completed.</summary>
    public bool WaitForInitCompletion { get; set; }
 
    /// <summary>Disables SSL certificate verification. Use only for testing.</summary>
    public bool DisableSslVerification { get; set; } = false;
}
