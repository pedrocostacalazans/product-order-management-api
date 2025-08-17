# ProductOrderSystem

API em .NET para cadastro de produtos e criação de pedidos com validações de negócio, EF Core InMemory e Swagger.

## Visão Geral

- Camadas
  - Domain: Entidades (Product, Order, OrderItem) e exceções de negócio (BusinessException)
  - Data: DbContext (EF Core) e mapeamentos das entidades
  - Business: Regras de negócio e serviços (IProductService/ProductService, IOrderService/OrderService)
  - Api: Controllers, DTOs, DI, Swagger e middleware global de erros
- Banco: EF Core InMemory (dados apenas em memória; são perdidos ao reiniciar a aplicação)
- Documentação: Swagger UI

## Estrutura do Projeto

```
ProductOrderSystem/
└── src/
    ├── Api/
    │   ├── Controllers/
    │   │   ├── OrdersController.cs
    │   │   └── ProductsController.cs
    │   ├── DTOs/
    │   │   ├── OrderDtos.cs
    │   │   └── ProductDtos.cs
    │   ├── Middleware/
    │   │   └── ErrorHandlingMiddleware.cs
    │   ├── ProductOrderSystem.Api.csproj
    │   └── Program.cs
    ├── Business/
    │   ├── Interfaces/
    │   │   ├── IOrderService.cs
    │   │   └── IProductService.cs
    │   ├── Services/
    │   │   ├── OrderService.cs
    │   │   └── ProductService.cs
    │   └── ProductOrderSystem.Business.csproj
    ├── Data/
    │   ├── Contexts/
    │   │   └── AppDbContext.cs
    │   └── ProductOrderSystem.Data.csproj
    └── Domain/
        ├── Entities/
        │   ├── Order.cs
        │   ├── OrderItem.cs
        │   └── Product.cs
        ├── Exceptions/
        │   └── BusinessException.cs
        └── ProductOrderSystem.Domain.csproj
```

## Responsabilidades por Camada

- Domain
  - Entities: Product (Id, Name, Description, Price, StockQuantity), Order (Id, CustomerName, CreatedAt, Items e Total calculado), OrderItem (Id, ProductId, ProductName, UnitPrice, Quantity)
  - Exceptions: BusinessException (sinaliza erros de regra de negócio -> HTTP 400 pelo middleware)
- Data
  - Contexts: AppDbContext com DbSet<Product>, DbSet<Order>, DbSet<OrderItem> e mapeamentos (PKs, required, max length e relação 1:N Order -> Items com backing field)
- Business
  - Interfaces: IProductService, IOrderService
  - Services: ProductService (Create/GetAll/GetById) e OrderService (criação de pedido, validações de cliente/itens/estoque e dedução de estoque)
- Api
  - DTOs: CreateProductRequest/ProductResponse; CreateOrderRequest/CreateOrderItem/OrderResponse/OrderItemResponse
  - Controllers: ProductsController, OrdersController
  - Program.cs: DI, EF InMemory ("ProductOrderDb"), Swagger, middleware
  - Middleware de erro: captura BusinessException (400) e exceções gerais (500)

## Namespaces e Organização

- ProductOrderSystem.Domain.Entities: Product, Order, OrderItem
- ProductOrderSystem.Domain.Exceptions: BusinessException
- ProductOrderSystem.Data.Contexts: AppDbContext
- ProductOrderSystem.Business.Interfaces: IProductService, IOrderService
- ProductOrderSystem.Business.Services: ProductService, OrderService
- ProductOrderSystem.Api: Controllers, DTOs, Middleware

## Principais Dependências

- Target Framework: .NET 9 (net9.0)
- Microsoft.EntityFrameworkCore (InMemory)
- Swashbuckle.AspNetCore (Swagger)

## Pré-requisitos

- .NET SDK 9.0+ instalado

## Como Executar

Na raiz do repositório:

1. Restaurar e compilar (opcional)

- dotnet restore
- dotnet build ProductOrderSystem.sln

2. Executar a API

- Via projeto: dotnet run --project src/Api/ProductOrderSystem.Api.csproj --urls http://localhost:5086
- Via solução (selecionando a Api): dotnet run --project src/Api/ProductOrderSystem.Api.csproj --configuration Debug --urls http://localhost:5086

3. Acessar

- Swagger UI: http://localhost:5086/swagger
- Base URL: http://localhost:5086

Observações:

- Banco InMemory "ProductOrderDb" configurado em Program.cs
- Como os dados são InMemory, eles serão reiniciados a cada execução.

## Endpoints

- Produtos
  - POST /api/products
  - GET /api/products
  - GET /api/products/{id}
- Pedidos
  - POST /api/orders
  - GET /api/orders/{id}

## Tests (cURL)

Abaixo, uma bateria de testes para validar cenários comuns. Execute-os com a API rodando em http://localhost:5086.

Dica: Capturar o ID do recurso criado a partir do header Location:

- PRODUCT_ID=$(curl -is -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"Laptop","description":"13-inch","price":999.99,"stockQuantity":10}' | awk '/^Location:/ {print $2}' | awk -F/ '{print $NF}' | tr -d '\r')
- MOUSE_ID=$(curl -is -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"Mouse","description":"Wireless","price":29.99,"stockQuantity":50}' | awk '/^Location:/ {print $2}' | awk -F/ '{print $NF}' | tr -d '\r')

### Produtos

1. Criar produto válido (201 Created)

- curl -i -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"Laptop","description":"13-inch","price":999.99,"stockQuantity":10}'
  Explicação: Cria um produto válido; espera 201 e Location.

2. Criar outro produto válido (201 Created)

- curl -i -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"Mouse","description":"Wireless optical mouse","price":29.99,"stockQuantity":50}'
  Explicação: Segundo produto para testes com pedidos multi-itens.

3. Criar produto sem nome (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"","description":"Invalid","price":10.0,"stockQuantity":5}'
  Explicação: Nome obrigatório; BusinessException gera 400.

4. Criar produto com preço negativo (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"Free","description":"Invalid negative price","price":-1.0,"stockQuantity":1}'
  Explicação: Preço deve ser >= 0.

5. Criar produto com estoque negativo (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"BadStock","description":"Negative stock","price":1.0,"stockQuantity":-5}'
  Explicação: Estoque não pode ser negativo.

6. Criar produto com preço zero (201 Created)

- curl -i -X POST http://localhost:5086/api/products -H "Content-Type: application/json" -d '{"name":"Freebie","description":"Zero price allowed","price":0.0,"stockQuantity":1}'
  Explicação: Preço zero é permitido; retorna 201.

7. Listar produtos (200 OK)

- curl -i http://localhost:5086/api/products
  Explicação: Deve retornar 200 e um array de produtos.

8. Buscar produto inexistente (404 Not Found)

- curl -i http://localhost:5086/api/products/00000000-0000-0000-0000-000000000000
  Explicação: GUID válido porém não encontrado; retorna 404.

### Pedidos

Pré-requisito: Use PRODUCT_ID e MOUSE_ID válidos.

1. Criar pedido válido com 1 item (201 Created)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"Alice","items":[{"productId":"'$PRODUCT_ID'","quantity":2}]}'
  Explicação: Deduz estoque e cria pedido; retorna 201 e Location.

2. Criar pedido com múltiplos itens (201 Created)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"Frank","items":[{"productId":"'$PRODUCT_ID'","quantity":1},{"productId":"'$MOUSE_ID'","quantity":3}]}'
  Explicação: Pedido válido com dois produtos; retorna 201.

3. Nome do cliente vazio (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"","items":[{"productId":"'$PRODUCT_ID'","quantity":1}]}'
  Explicação: Nome obrigatório.

4. Pedido sem itens (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"Bob","items":[]}'
  Explicação: Pelo menos um item é necessário.

5. Produto inexistente (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"Carol","items":[{"productId":"00000000-0000-0000-0000-000000000000","quantity":1}]}'
  Explicação: Produto não encontrado nas validações do serviço.

6. Quantidade zero (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"Eve","items":[{"productId":"'$PRODUCT_ID'","quantity":0}]}'
  Explicação: Quantidade deve ser > 0.

7. Quantidade acima do estoque (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"Dave","items":[{"productId":"'$PRODUCT_ID'","quantity":1000}]}'
  Explicação: Reprova por falta de estoque.

8. Buscar pedido existente por ID (200 OK)

- ORDER_ID="<substitua pelo GUID do Location>"
- curl -i http://localhost:5086/api/orders/$ORDER_ID
  Explicação: Retorna os dados do pedido e seus itens.

9. Buscar pedido com GUID inválido (404 Not Found)

- curl -i http://localhost:5086/api/orders/not-a-guid
  Explicação: A rota não casa com GUID; retorna 404.

10. Buscar pedido inexistente (404 Not Found)

- curl -i http://localhost:5086/api/orders/00000000-0000-0000-0000-000000000000
  Explicação: GUID válido porém não encontrado.

11. Novo pedido acima do estoque restante (400 Bad Request)

- curl -i -X POST http://localhost:5086/api/orders -H "Content-Type: application/json" -d '{"customerName":"Gina","items":[{"productId":"'$PRODUCT_ID'","quantity":9}]}'
  Explicação: Após pedidos anteriores, o estoque remanescente pode ser insuficiente; retorna 400.

## Observações

- Middleware de erro:
  - BusinessException -> 400 Bad Request (JSON com mensagem)
  - Exceções não tratadas -> 500 Internal Server Error
- InMemory não aplica constraints relacionais/DDL; validações ocorrem na camada de negócio.
- Para persistência real e validações de banco, seria necessário trocar para SQLite/SQL Server e criar migrations.
