using OrderItemsReserver.Entities;

namespace OrderItemsReserver.Interfaces;

public interface IOrderRequestBlobUploader
{
    Task UploadAsync(OrderRequest order, CancellationToken cancellationToken);
}
