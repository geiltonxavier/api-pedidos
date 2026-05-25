# Orders API (Clean Architecture) - .NET 10

Projects:
- src/Core: Domain entities (Order, OrderItem, OrderType)
- src/Application: Services and discount strategies (Strategy Pattern)
- src/Infrastructure: EF Core InMemory DbContext and repository
- src/Api: ASP.NET Core Web API (wires DI and endpoints)
- tests/Tests: xUnit unit tests for the Application service layer

Discount rules implemented (Strategy Pattern):
- Standard: no discount
- Express: 15% surcharge (fast delivery fee)
- Subscription: 10% discount

Quick start (macOS / terminal):

1. Build everything:

```bash
cd /Users/geiltonxavier/coding/api-pedidos
dotnet build
```

2. Run the API (from workspace root):

```bash
cd src/Api
dotnet run
```

The API exposes the following endpoints:

- `POST /api/orders`
  - Request body: order type and items list
  - Response: created order ID and `Location` header

```json
{
  "type": "Express",
  "items": [
    { "description": "Product A", "quantity": 1, "unitPrice": 100.0 }
  ]
}
```

- `GET /api/orders/{orderId}`
  - Response: order summary with subtotal, total and discount value

```json
{
  "id": "<order-id>",
  "type": "Express",
  "subTotal": 100.0,
  "total": 115.0,
  "discountValue": 15.0,
  "items": [
    { "description": "Product A", "quantity": 1, "unitPrice": 100.0, "total": 100.0 }
  ]
}
```

3. Run tests:

```bash
cd /Users/geiltonxavier/coding/api-pedidos
dotnet test
```

Notes:
- Infrastructure uses EF Core InMemory provider.
- Service layer is fully unit-tested (xUnit) covering Standard, Express and Subscription flows.

If you want, I can now:
- Add more endpoints (GET/list)
- Persist to a real database
- Add integration tests or CI pipeline

*** End of README ***
