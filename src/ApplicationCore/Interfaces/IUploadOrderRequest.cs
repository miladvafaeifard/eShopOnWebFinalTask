using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IUploadOrderRequest
{
    public Task Save(int basketId);
}
