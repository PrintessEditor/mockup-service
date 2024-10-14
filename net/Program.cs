


using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Printess.Mockup
{

  class Program
  {

    public static void Main(string[] args)
    {
      var userId = "";  // get this from Printess Support support@printess.com
      var secret = "";  // get this from Printess Support support@printess.com

      if (File.Exists("../config.json")) // check for the config.json, content { "userId": "the user id", "secret": "the sealing secret" }
      {
        var dto = JsonSerializer.Deserialize<ConfigDto>(File.ReadAllText("../config.json")) ?? throw new InvalidOperationException("Cannot read secret.json file.");
        userId = dto.UserId;
        secret = dto.Secret;
      }

      if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(secret))
      {
        throw new InvalidOperationException("No userId or secret given.");
      }

      var sample = new MockupUrlOptions()
      {
        TemplateName = "Byxor", // you must at least provide a template name, all other options are optional
        MaximumWidth = 400,
        MaximumHeight = 300
      };
      var url = CreateMockupUrl(userId, secret, sample);
      Console.WriteLine(url);
    }

    public static string CreateMockupUrl(string userId, string secret, MockupUrlOptions dto)
    {
      var byteSecret = Encoding.UTF8.GetBytes(secret);
      var sealingParameters = new Dictionary<string, string?>();
      var url = string.Empty;

      dto.AdjustDefaults(); // max width/height of 1000
      sealingParameters.Add("t", dto.TemplateName);
      sealingParameters.Add("w", dto.MaximumWidth.ToString(CultureInfo.InvariantCulture));
      sealingParameters.Add("h", dto.MaximumHeight.ToString(CultureInfo.InvariantCulture));

      if (dto.UsePublishedVersion)
      {
        sealingParameters.Add("upv", "1");
      }

      if (!string.IsNullOrWhiteSpace(dto.DocumentName))
      {
        sealingParameters.Add("d", dto.DocumentName);
      }

      if (!string.IsNullOrWhiteSpace(dto.SnippetUrl))
      {
        sealingParameters.Add("su", dto.SnippetUrl);

        if (!string.IsNullOrWhiteSpace(dto.SnippetTargetDocumentName))
        {
          sealingParameters.Add("stdn", dto.SnippetTargetDocumentName);
        }
      }

      if (!string.IsNullOrWhiteSpace(dto.MergeTemplate1))
      {
        sealingParameters.Add("m1", dto.MergeTemplate1);

        if (!string.IsNullOrWhiteSpace(dto.MergeTemplate1Document))
        {
          sealingParameters.Add("m1d", dto.MergeTemplate1Document);
        }

        if (!string.IsNullOrWhiteSpace(dto.MergeTemplate1TargetDocument))
        {
          sealingParameters.Add("m1t", dto.MergeTemplate1TargetDocument);
        }
      }

      if (!string.IsNullOrWhiteSpace(dto.MergeTemplate2))
      {
        sealingParameters.Add("m2", dto.MergeTemplate2);

        if (!string.IsNullOrWhiteSpace(dto.MergeTemplate2Document))
        {
          sealingParameters.Add("m2d", dto.MergeTemplate2Document);
        }

        if (!string.IsNullOrWhiteSpace(dto.MergeTemplate2TargetDocument))
        {
          sealingParameters.Add("m2t", dto.MergeTemplate2TargetDocument);
        }
      }

      if (!string.IsNullOrWhiteSpace(dto.MergeTemplate3))
      {
        sealingParameters.Add("m3", dto.MergeTemplate3);

        if (!string.IsNullOrWhiteSpace(dto.MergeTemplate3Document))
        {
          sealingParameters.Add("m3d", dto.MergeTemplate3Document);
        }

        if (!string.IsNullOrWhiteSpace(dto.MergeTemplate3TargetDocument))
        {
          sealingParameters.Add("m3t", dto.MergeTemplate3TargetDocument);
        }
      }

      if (!string.IsNullOrWhiteSpace(dto.ContentTemplateName))
      {
        sealingParameters.Add("ctn", dto.ContentTemplateName);
      }

      if (dto.FormFields != null)
      {
        foreach (var (key, value) in dto.FormFields)
        {
          sealingParameters[key] = value;
        }
      }

      sealingParameters["uid"] = userId;

      using (var hmac = new System.Security.Cryptography.HMACSHA256(byteSecret))
      {
        var toSeal = string.Join("", sealingParameters.Keys.OrderBy(k => k, StringComparer.Ordinal).Select(k => k + "=" + sealingParameters[k]));
        var bytesToSeal = Encoding.UTF8.GetBytes(toSeal);
        sealingParameters["seal"] = ByteArrayToHexString(hmac.ComputeHash(bytesToSeal));
        var qs = QueryString.Create(sealingParameters);
        url = "https://mockups.printess.com/user/image.png" + qs;
      }

      return url;
    }


    public static string ByteArrayToHexString(byte[] ba)
    {
      return BitConverter.ToString(ba).Replace("-", "", StringComparison.Ordinal).ToLowerInvariant();
    }
  }

  public class ConfigDto
  {

    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("secret")]
    public required string Secret { get; set; }

  }

  public class MockupUrlOptions
  {

    // The Printess template name you want to render the mockup image for. 
    public required string TemplateName { get; set; }

    // The document you want to render. The mockup service will find this out automatically if you don't provide a specific one.
    // Usually preview documents are rendered and automatically picked.
    public string? DocumentName { get; set; }

    // Which version of the template should be used? For development you usually take the draft version.
    // When going live you should take the published version.
    public bool UsePublishedVersion { get; set; }

    // The maximum width of the mockup image generated. Maximum allowed value is 1000;
    public int MaximumWidth { get; set; }

    // The maximum height of the mockup image generated. Maximum allowed value is 1000;
    public int MaximumHeight { get; set; }

    // You can pass form fields to the mockup template. Provide the name and value here.
    // Please make sure you never name any form field in the template the same as any parameter here 
    public Dictionary<string, string?>? FormFields { get; set; }

    // The render mode to take. Defaults to "buyer" which renders the same way as the buyer would see things.
    // Other options are not public yet.
    public string RenderMode { get; set; }

    // The first merge template to apply.
    public string? MergeTemplate1 { get; set; }

    // The document to take from the merge template.
    public string? MergeTemplate1Document { get; set; }

    // The document to merge to. 
    public string? MergeTemplate1TargetDocument { get; set; }

    // The second merge template to apply.
    public string? MergeTemplate2 { get; set; }

    // The document to take from the merge template.
    public string? MergeTemplate2Document { get; set; }

    // The document to merge to. 
    public string? MergeTemplate2TargetDocument { get; set; }

    // The third merge template to apply.
    public string? MergeTemplate3 { get; set; }

    // The document to take from the merge template.
    public string? MergeTemplate3Document { get; set; }

    // The document to merge to. 
    public string? MergeTemplate3TargetDocument { get; set; }

    // You can apply snippet urls to be merged onto the template.
    public string? SnippetUrl { get; set; }

    // The target document to merge the snippet to. Sometimes needed when the merge should not happen on the preview document.
    public string? SnippetTargetDocumentName { get; set; }

    // The template or saveToken where you want to take content from. Works for images and simple text.
    public string? ContentTemplateName { get; set; }

    public MockupUrlOptions()
    {
      RenderMode = "buyer";
      MaximumWidth = 1000;
      MaximumHeight = 1000;
    }

    public void AdjustDefaults()
    {
      if (MaximumWidth <= 0 || MaximumWidth > 1000)
      {
        MaximumWidth = 1000;
      }

      if (MaximumHeight <= 0 || MaximumWidth > 1000)
      {
        MaximumHeight = 1000;
      }
    }

  }

}

