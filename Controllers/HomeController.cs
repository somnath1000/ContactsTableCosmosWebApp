using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ContactsTableCosmosWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;

namespace ContactsTableCosmosWebApp.Controllers
{
  public class HomeController : Controller
  {
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
      _configuration = configuration;
    }
    public IActionResult Index()
    {
      return View();
    }

    public async Task<IActionResult> Vault()
    {
      var keyVaultEndpoint = _configuration["MNKeyVault"];
      Dictionary<string, string> keyVaultList = null;
      if (!string.IsNullOrEmpty(keyVaultEndpoint))
      {
        AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
        KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        var secrets = await keyVaultClient.GetSecretsAsync(keyVaultEndpoint);

        keyVaultList = new Dictionary<string, string>();
        foreach (var item in secrets)
        {
          var secret = await keyVaultClient.GetSecretAsync(item.Id);
          keyVaultList.Add(item.Id, secret.Value);
        }
      }
      keyVaultList = new Dictionary<string, string>() { ["KeyVault"] = "Empty" };
      return View(keyVaultList);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
