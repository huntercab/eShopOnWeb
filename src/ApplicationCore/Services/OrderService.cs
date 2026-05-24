using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.ApplicationCore.Entities.Shared;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;

    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer, IMediator mediator,
        HttpClient httpClient)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _mediator = mediator;
        _httpClient = httpClient;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress, string azureFunction = "", string serviceBusConnectionString = "", string queueName = "")
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
            return orderItem;
        }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, items);
        await _orderRepository.AddAsync(order);
        OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent(order);
        await _mediator.Publish(orderCreatedEvent);

        
        if (!string.IsNullOrEmpty(serviceBusConnectionString) && !string.IsNullOrEmpty(queueName))
        {
            var orderRequest = new OrderRequest
            {
                OrderId = order.Id.ToString(),
                Items = order.OrderItems.Select(item => new OrderItemRequest
                {
                    ItemId = item.ItemOrdered.CatalogItemId,
                    Quantity = item.Units

                }).ToList()
            };

            var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            var sender = serviceBusClient.CreateSender(queueName);
            var json = JsonSerializer.Serialize( orderRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var message = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = "OrderRequested",
                MessageId = orderRequest.OrderId
            };

            await sender.SendMessageAsync(message);
        }

        if (!string.IsNullOrWhiteSpace(azureFunction))
        {
            var json = JsonSerializer.Serialize(order);
            var request = new HttpRequestMessage(HttpMethod.Post, azureFunction);
            request.Content = JsonContent.Create(order);

            var response = await _httpClient.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
        }
    }
}
