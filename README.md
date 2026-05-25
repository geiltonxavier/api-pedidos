# Orders API — .NET 10

API para gerenciamento de pedidos de uma loja online, com cálculo automático de descontos por tipo de pedido.

## Arquitetura

Projeto estruturado em **Clean Architecture** com separação clara de responsabilidades:

```
src/
├── Core/           → Entidades de domínio (Order, OrderItem) e enums (OrderType)
├── Application/    → Serviços, DTOs, interfaces, estratégias de desconto e mapeamento
├── Infrastructure/ → Persistência com EF Core (InMemory) e implementação do repositório
└── Api/            → Controllers, middlewares, filtros e configuração da aplicação

tests/
└── Tests/          → Testes unitários com xUnit
```

## Regras de negócio — Tipos de pedido

Cada pedido possui um tipo que determina o cálculo do valor total (padrão **Strategy Pattern**):

| Tipo (enum) | Valor | Regra |
|-------------|-------|-------|
| `Standard`  | 0     | Sem desconto |
| `Express`   | 1     | Acréscimo de 15% (taxa de entrega rápida) |
| `Subscription` | 2  | Desconto de 10% (cliente assinante) |

## Endpoints

### `POST /orders` — Criar pedido

```json
// Request
{
  "type": 0,
  "items": [
    { "description": "Camiseta", "quantity": 2, "unitPrice": 49.90 },
    { "description": "Boné", "quantity": 1, "unitPrice": 29.90 }
  ]
}

// Response (201 Created)
{ "orderId": "guid-do-pedido" }
```

**Headers opcionais:**
- `Idempotency-Key: <uuid>` — evita criação duplicada em caso de retry

### `GET /orders/{orderId}` — Consultar resumo do pedido

```json
// Response (200 OK)
{
  "id": "guid-do-pedido",
  "type": 0,
  "subTotal": 129.70,
  "total": 129.70,
  "discountValue": 0.00,
  "items": [
    { "id": "guid-item", "description": "Camiseta", "quantity": 2, "unitPrice": 49.90, "total": 99.80 },
    { "id": "guid-item", "description": "Boné", "quantity": 1, "unitPrice": 29.90, "total": 29.90 }
  ]
}
```

### `PUT /orders/{orderId}/items/{itemId}` — Atualizar item

```json
// Request
{ "description": "Camiseta G", "quantity": 3, "unitPrice": 54.90 }

// Response (200 OK) — retorna o resumo atualizado do pedido
```

### `DELETE /orders/{orderId}/items/{itemId}` — Remover item

```
// Response (204 No Content)
```

## Como executar

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Rodar a API

```bash
dotnet run --project src/Api/Api.csproj
```

A API estará disponível em `http://localhost:5024`.

- **Swagger UI:** http://localhost:5024/swagger
- **OpenAPI doc:** http://localhost:5024/openapi/v1.json

### Rodar os testes

```bash
dotnet test tests/Tests/Tests.csproj
```

## Funcionalidades técnicas

| Feature | Descrição |
|---------|-----------|
| **Clean Architecture** | Camadas Core → Application → Infrastructure → Api |
| **Strategy Pattern** | Cálculo de desconto desacoplado com `IDiscountStrategy` |
| **EF Core InMemory** | Banco de dados em memória (sem dependência externa) |
| **Idempotência** | Header `Idempotency-Key` no POST previne pedidos duplicados |
| **Correlation ID** | Header `X-Correlation-Id` propagado em todos os logs da request |
| **Serilog** | Logging estruturado com enriquecimento de contexto |
| **Swagger UI** | Documentação interativa dos endpoints |

## Testes unitários

Cobertura dos cenários da camada de serviço:

- Criação de pedido **Standard** — sem desconto
- Criação de pedido **Express** — acréscimo de 15%
- Criação de pedido **Subscription** — desconto de 10%
- Consulta de pedido com validação de `DiscountValue`
- Consulta de pedido inexistente retorna `null`

## Postman

Collection completa disponível em `docs/Orders API.postman_collection.json`. Importe no Postman para testar todos os endpoints.

## Tecnologias

- .NET 10 / ASP.NET Core
- Entity Framework Core (InMemory)
- Serilog
- xUnit
- Swagger / OpenAPI
