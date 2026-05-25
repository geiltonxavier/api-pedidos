# Orders API — .NET 10

API para gerenciamento de pedidos de uma loja online, com cálculo automático de descontos por tipo de pedido.

## Arquitetura

Projeto estruturado em **Clean Architecture** com separação clara de responsabilidades:

```
src/
├── Core/           → Entidades de domínio (Order, OrderItem) e enums (OrderType)
├── Application/    → Serviços, DTOs, interfaces, validators, estratégias de desconto
├── Infrastructure/ → Persistência com EF Core (InMemory) e implementação do repositório
└── Api/            → Controllers, middlewares, filtros e configuração da aplicação

tests/
└── Tests/          → Testes unitários com xUnit (11 cenários)
```

## Regras de negócio — Tipos de pedido

Cada pedido possui um tipo que determina o cálculo do valor total (padrão **Strategy Pattern**):

| Tipo (enum) | Valor | Regra |
|-------------|-------|-------|
| `Standard`  | 0     | Sem desconto |
| `Express`   | 1     | Acréscimo de 15% (taxa de entrega rápida) |
| `Subscription` | 2  | Desconto de 10% (cliente assinante) |

## Endpoints

> Base URL: `http://localhost:5024/v1`

### `POST /v1/orders` — Criar pedido

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

### `GET /v1/orders/{orderId}` — Consultar resumo do pedido

```json
// Response (200 OK)
{
  "id": "guid-do-pedido",
  "type": 0,
  "subTotal": 129.70,
  "total": 129.70,
  "discountValue": 0.00,
  "createdAt": "2026-05-25T14:30:00Z",
  "items": [
    { "id": "guid-item", "description": "Camiseta", "quantity": 2, "unitPrice": 49.90, "total": 99.80 },
    { "id": "guid-item", "description": "Boné", "quantity": 1, "unitPrice": 29.90, "total": 29.90 }
  ]
}
```

### `PUT /v1/orders/{orderId}/items/{itemId}` — Atualizar item

```json
// Request
{ "description": "Camiseta G", "quantity": 3, "unitPrice": 54.90 }

// Response (200 OK) — retorna o resumo atualizado do pedido
```

### `DELETE /v1/orders/{orderId}/items/{itemId}` — Remover item

```
// Response (204 No Content)
```

### `GET /health` — Health check

```
// Response (200 OK)
Healthy
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

### Rodar via Docker

```bash
docker build -t orders-api .
docker run -p 8080:8080 orders-api
```

A API estará disponível em `http://localhost:8080`.

### Rodar os testes

```bash
dotnet test OrdersApi.slnx
```

## Funcionalidades técnicas

| Feature | Descrição |
|---------|-----------|
| **Clean Architecture** | Camadas Core → Application → Infrastructure → Api |
| **Strategy Pattern** | Cálculo de preço desacoplado com `IPricingStrategy` + `DiscountFactory` |
| **EF Core InMemory** | Banco de dados em memória (sem dependência externa) |
| **API Versioning** | Versionamento por URL segment (`/v1/orders`) com `Asp.Versioning.Mvc` |
| **Rate Limiting** | Fixed Window — 100 requests/minuto por IP (built-in .NET) |
| **FluentValidation** | Validação de DTOs com mensagens em português e resposta `400 ProblemDetails` |
| **Global Exception Handler** | `IExceptionHandler` com `ProblemDetails` (RFC 9457) e Correlation ID |
| **Idempotência** | Header `Idempotency-Key` no POST previne pedidos duplicados |
| **Correlation ID** | Header `X-Correlation-Id` propagado em todos os logs e respostas |
| **Serilog** | Logging estruturado com enriquecimento de contexto (`CorrelationId`) |
| **Health Check** | Endpoint `/health` para readiness probes (Kubernetes, Docker, load balancer) |
| **CancellationToken** | Propagado em toda a cadeia (Controller → Service → Repository) |
| **Swagger UI** | Documentação interativa dos endpoints via OpenAPI |
| **Dockerfile** | Multi-stage build (SDK → runtime), pronto para CI/CD |
| **GitHub Actions CI** | Pipeline de build + test automático em push/PR |

## Testes unitários

11 cenários cobrindo a camada de serviço com `FakeOrderRepository`:

| # | Cenário |
|---|---------|
| 1 | Criação de pedido **Standard** — sem desconto |
| 2 | Criação de pedido **Express** — acréscimo de 15% |
| 3 | Criação de pedido **Subscription** — desconto de 10% |
| 4 | Consulta de pedido com validação de `DiscountValue` |
| 5 | Consulta de pedido inexistente retorna `null` |
| 6 | Atualizar item existente recalcula totais |
| 7 | Atualizar item inexistente retorna `null` |
| 8 | Remover item existente retorna `true` |
| 9 | Remover último item do pedido é bloqueado (invariante de domínio) |
| 10 | Remover item inexistente retorna `false` |
| 11 | Atualizar item de pedido **Express** recalcula acréscimo |

## Postman

Collection completa disponível em `docs/Orders API.postman_collection.json`. Importe no Postman para testar todos os endpoints (10 requests com scripts de validação).

## Segurança — Autenticação

Esta API **não implementa autenticação** de forma intencional, adicionar JWT com chave simétrica hardcoded ("auth fake") dá uma falsa sensação de segurança e não agrega valor real, pelo contrário, polui o código e dificulta testes.

Em produção, a recomendação é:

1. **Identity Provider externo** (Microsoft Entra ID, Keycloak, Auth0) — a API apenas valida tokens, nunca emite
2. **JWT Bearer** com `AddAuthentication().AddJwtBearer()` apontando para o IdP
3. **Authorization policies** por role/scope no controller (`[Authorize(Policy = "orders:write")]`)
4. **API Gateway** (ex: Azure API Management) como camada adicional de proteção

O código já está preparado com o TODO comentado em `Program.cs` para ativar quando houver um IdP real.

## Tecnologias

- .NET 10 / ASP.NET Core
- Entity Framework Core (InMemory)
- FluentValidation
- Asp.Versioning.Mvc
- Serilog
- xUnit
- Swagger / OpenAPI
