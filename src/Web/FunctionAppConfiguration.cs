using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Web;

public class FunctionAppConfiguration: IFunctionAppConfiguration 
{
    public const string CONFIG_NAME = "functionApp";
    public string? ApiBase { get; set; }
    public string? Code { get; set; }
}
