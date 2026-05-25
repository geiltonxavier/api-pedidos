using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTO;
using Core.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests;

public sealed class OrdersApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OrdersApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateOrderDto ValidStandardOrder() => new(
        OrderType.Standard,
        new List<CreateItemDto>
        {
            new("Produto A", 2, 50.00m),
            new("Produto B", 1, 30.00m)
        });

    private async Task<(Guid OrderId, HttpResponseMessage Response)> CreateOrderAsync(CreateOrderDto? dto = null)
    {
        dto ??= ValidStandardOrder();
        var response = await _client.PostAsJsonAsync("/v1/orders", dto);
        var body = await response.Content.ReadFromJsonAsync<CreatedOrderResponse>(JsonOptions);
        return (body?.OrderId ?? Guid.Empty, response);
    }

    // ── POST ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_PedidoValido_Retorna201()
    {
        var (orderId, response) = await CreateOrderAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotEqual(Guid.Empty, orderId);
        Assert.Contains("orders/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Post_SemItens_Retorna400()
    {
        var dto = new CreateOrderDto(OrderType.Standard, new List<CreateItemDto>());

        var response = await _client.PostAsJsonAsync("/v1/orders", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_QuantidadeZero_Retorna400()
    {
        var dto = new CreateOrderDto(
            OrderType.Standard,
            new List<CreateItemDto> { new("Item X", 0, 10m) });

        var response = await _client.PostAsJsonAsync("/v1/orders", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PrecoUnitarioNegativo_Retorna400()
    {
        var dto = new CreateOrderDto(
            OrderType.Standard,
            new List<CreateItemDto> { new("Item X", 1, -5m) });

        var response = await _client.PostAsJsonAsync("/v1/orders", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Express_AplicaSobretaxa()
    {
        var dto = new CreateOrderDto(
            OrderType.Express,
            new List<CreateItemDto> { new("Produto", 1, 100m) });

        var (orderId, _) = await CreateOrderAsync(dto);

        var summary = await _client.GetFromJsonAsync<OrderSummaryDto>($"/v1/orders/{orderId}", JsonOptions);

        Assert.NotNull(summary);
        Assert.Equal(100m, summary!.SubTotal);
        Assert.True(summary.Total > summary.SubTotal, "Express deve ter sobretaxa.");
    }

    [Fact]
    public async Task Post_Subscription_AplicaDesconto()
    {
        var dto = new CreateOrderDto(
            OrderType.Subscription,
            new List<CreateItemDto> { new("Produto", 1, 100m) });

        var (orderId, _) = await CreateOrderAsync(dto);

        var summary = await _client.GetFromJsonAsync<OrderSummaryDto>($"/v1/orders/{orderId}", JsonOptions);

        Assert.NotNull(summary);
        Assert.Equal(100m, summary!.SubTotal);
        Assert.True(summary.Total < summary.SubTotal, "Subscription deve ter desconto.");
    }

    // ── GET ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_PedidoExistente_Retorna200ComDados()
    {
        var (orderId, _) = await CreateOrderAsync();

        var response = await _client.GetAsync($"/v1/orders/{orderId}");
        var summary = await response.Content.ReadFromJsonAsync<OrderSummaryDto>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(summary);
        Assert.Equal(orderId, summary!.Id);
        Assert.Equal(2, summary.Items.Count);
        Assert.True(summary.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task Get_PedidoInexistente_Retorna404()
    {
        var response = await _client.GetAsync($"/v1/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── PUT ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AtualizaItem_Retorna200ComNovoValor()
    {
        var (orderId, _) = await CreateOrderAsync();

        var order = await _client.GetFromJsonAsync<OrderSummaryDto>($"/v1/orders/{orderId}", JsonOptions);
        var itemId = order!.Items[0].Id;

        var updateDto = new UpdateItemDto("Produto Atualizado", 5, 20m);
        var putResponse = await _client.PutAsJsonAsync($"/v1/orders/{orderId}/items/{itemId}", updateDto);
        var updated = await putResponse.Content.ReadFromJsonAsync<OrderSummaryDto>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Contains(updated!.Items, i => i.Description == "Produto Atualizado" && i.Quantity == 5);
    }

    [Fact]
    public async Task Put_PedidoInexistente_Retorna404()
    {
        var updateDto = new UpdateItemDto("X", 1, 10m);

        var response = await _client.PutAsJsonAsync($"/v1/orders/{Guid.NewGuid()}/items/{Guid.NewGuid()}", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ItemInexistente_Retorna404()
    {
        var (orderId, _) = await CreateOrderAsync();

        var updateDto = new UpdateItemDto("X", 1, 10m);
        var response = await _client.PutAsJsonAsync($"/v1/orders/{orderId}/items/{Guid.NewGuid()}", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DELETE ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_RemoveItem_Retorna204()
    {
        var dto = new CreateOrderDto(
            OrderType.Standard,
            new List<CreateItemDto>
            {
                new("Item 1", 1, 10m),
                new("Item 2", 1, 20m)
            });

        var (orderId, _) = await CreateOrderAsync(dto);
        var order = await _client.GetFromJsonAsync<OrderSummaryDto>($"/v1/orders/{orderId}", JsonOptions);
        var itemId = order!.Items[0].Id;

        var deleteResponse = await _client.DeleteAsync($"/v1/orders/{orderId}/items/{itemId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await _client.GetFromJsonAsync<OrderSummaryDto>($"/v1/orders/{orderId}", JsonOptions);
        Assert.Single(afterDelete!.Items);
    }

    [Fact]
    public async Task Delete_UltimoItem_Retorna404()
    {
        var dto = new CreateOrderDto(
            OrderType.Standard,
            new List<CreateItemDto> { new("Unico", 1, 10m) });

        var (orderId, _) = await CreateOrderAsync(dto);
        var order = await _client.GetFromJsonAsync<OrderSummaryDto>($"/v1/orders/{orderId}", JsonOptions);
        var itemId = order!.Items[0].Id;

        var response = await _client.DeleteAsync($"/v1/orders/{orderId}/items/{itemId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_PedidoInexistente_Retorna404()
    {
        var response = await _client.DeleteAsync($"/v1/orders/{Guid.NewGuid()}/items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── IDEMPOTÊNCIA ────────────────────────────────────────────────────

    [Fact]
    public async Task Post_MesmaIdempotencyKey_RetornaMesmoId()
    {
        var dto = ValidStandardOrder();
        var key = Guid.NewGuid().ToString();

        var request1 = new HttpRequestMessage(HttpMethod.Post, "/v1/orders") { Content = JsonContent.Create(dto) };
        request1.Headers.Add("Idempotency-Key", key);
        var response1 = await _client.SendAsync(request1);
        var body1 = await response1.Content.ReadFromJsonAsync<CreatedOrderResponse>(JsonOptions);

        var request2 = new HttpRequestMessage(HttpMethod.Post, "/v1/orders") { Content = JsonContent.Create(dto) };
        request2.Headers.Add("Idempotency-Key", key);
        var response2 = await _client.SendAsync(request2);
        var body2 = await response2.Content.ReadFromJsonAsync<CreatedOrderResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        Assert.Equal(body1!.OrderId, body2!.OrderId);
    }

    // ── HEALTH CHECK ────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheck_Retorna200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
