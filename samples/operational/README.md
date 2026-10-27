# Três exemplos operacionais em ASP.NET Core

Exemplos de estudo com dados fictícios, integrados à API existente. Não constituem implantação em produção nem teste de carga.

## Executar na raiz

```console
dotnet restore src/WorkOrders.Api/WorkOrders.Api.csproj
dotnet ef database update --project src/WorkOrders.Api
dotnet run --project src/WorkOrders.Api --urls http://localhost:5080
dotnet test tests/WorkOrders.Api.Tests/WorkOrders.Api.Tests.csproj
```

Antes de usar `dotnet ef`, execute `dotnet tool restore` na raiz. O banco de demonstração é SQLite.

## 1. Erros que o cliente consegue tratar

`POST /api/work-orders` com título vazio ou prioridade fora de 1–5 retorna HTTP 400 em `application/problem+json`, com os erros de validação. O teste `ValidationErrorHasStructuredProblemDetails` verifica o contrato. O tratamento de erros inesperados preserva detalhes internos no servidor.

## 2. Vivo não é o mesmo que pronto

- `/health/live`: processo consegue responder, sem consultar o banco.
- `/health/ready`: consulta a conectividade do banco por EF Core; retorna 503 quando ele está inacessível.
- `/health`: endpoint antigo preservado; não é um teste de dependências.

A resposta padrão é um estado simples, sem revelar conexão ou exceções. Readiness verifica conectividade, não presença de todas as tabelas ou regras de negócio. Em produção, defina exposição, limites e monitoramento conforme o ambiente.

## 3. Limite de chamadas com resposta explícita

Faça quatro chamadas seguidas a `/examples/limited`. A política aceita três por janela fixa de um minuto, sem fila. A quarta retorna 429, `Retry-After` e Problem Details.

É um único limite compartilhado em memória para esse endpoint demonstrativo, por instância da aplicação. Não é limite por usuário, coordenado entre servidores ou defesa completa contra DDoS. Os endpoints CRUD não recebem essa política; os valores foram escolhidos para facilitar a demonstração, não como recomendação de produção.

## Fontes

Implementação própria baseada nos recursos documentados pela Microsoft; sem copiar código de terceiros:

- [Problem Details](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)
- [Health checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0)
- [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)

Licença MIT do repositório.
