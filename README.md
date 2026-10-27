# ASP.NET Work Orders API

API REST para ordens de trabalho fictícias com persistência EF Core, validação de DTOs, migrations e documentação Swagger. O nome do repositório referencia a opção SQL Server; SQLite é o banco padrão da demonstração.

## Tecnologias

C#, ASP.NET Core .NET 10 LTS, Entity Framework Core, SQLite, provider SQL Server, Swashbuckle/Swagger, xUnit e WebApplicationFactory. .NET 10 é uma versão estável LTS, conforme a [política oficial](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).

## Funcionalidades

### Exemplos operacionais adicionados em 27/10/2026

- `/health/live` verifica a resposta do processo e `/health/ready` verifica a conectividade do banco.
- `/examples/limited` demonstra uma janela fixa compartilhada por instância: três chamadas por minuto, sem fila; excedentes recebem HTTP 429, Retry-After e Problem Details.
- Testes verificam o contrato de erros de validação e a distinção entre processo vivo e banco indisponível.

[Guia de execução e limites dos exemplos](samples/operational/). A suíte local completa passou em **12 testes**, usando SQLite; não houve teste de carga ou implantação em produção. A política de limite se aplica somente ao endpoint demonstrativo.


CRUD de ordens com título, descrição, prioridade de 1 a 5, situação e prazo. DTOs limitam campos aceitos; datas devem ser informadas; situações permitidas são `open`, `in_progress` e `completed`. Erros de validação retornam 400, recursos ausentes 404, exclusão 204 e falhas inesperadas 500 com Problem Details.

## Instalação e execução

Instale o [SDK .NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0). Na raiz deste repositório, execute no PowerShell:

```powershell
dotnet restore src/WorkOrders.Api
dotnet tool restore
dotnet ef database update --project src/WorkOrders.Api
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/WorkOrders.Api --urls http://127.0.0.1:15000
```

As migrations versionadas criam a tabela `WorkOrders`. Não são aplicadas automaticamente ao iniciar a API. O caminho SQLite é resolvido pelo content root da aplicação: `src/WorkOrders.Api/workorders.db`. Mantenha os comandos na raiz do repositório. Swagger fica em [Swagger UI](http://127.0.0.1:15000/swagger) e só é publicado em `Development`.

## Exemplos de requisição e resposta

```powershell
$body = Get-Content samples/work-order.json -Raw
$order = Invoke-RestMethod -Method Post -Uri http://127.0.0.1:15000/api/work-orders -ContentType 'application/json' -Body $body
Invoke-RestMethod -Uri "http://127.0.0.1:15000/api/work-orders/$($order.id)"
Invoke-RestMethod -Uri http://127.0.0.1:15000/api/work-orders
Invoke-RestMethod -Method Put -Uri "http://127.0.0.1:15000/api/work-orders/$($order.id)" -ContentType 'application/json' -Body '{"title":"FICTITIOUS completed review","description":"FICTITIOUS maintenance finished","status":"completed","priority":2,"dueDate":"2025-01-20"}'
Invoke-RestMethod -Method Delete -Uri "http://127.0.0.1:15000/api/work-orders/$($order.id)"
```

Exemplo de resposta 201 para banco vazio, incluindo header `Location`:

```json
{"id":1,"title":"FICTITIOUS database maintenance","description":"FICTITIOUS review of a demonstration database","status":"open","priority":2,"dueDate":"2025-01-20"}
```

Os IDs são gerados pelo banco. Todas as informações da amostra são inventadas. Não há seed automático; a requisição acima cria a amostra.

## Arquitetura

`Models/` define a entidade; `Dtos/` define validações e contratos; `Data/` configura EF Core e migrations; `Services/` aplica regras, transforma DTOs e persiste; `Controllers/` trata HTTP; `Program.cs` configura injeção de dependência, provider e tratamento global. EF Core já encapsula persistência; uma camada repository extra foi dispensada.

## Testes

```powershell
dotnet test tests/WorkOrders.Api.Tests
```

Os testes usam WebApplicationFactory e um SQLite em memória por fábrica, aplicando migrations reais. Cobrem CRUD completo, persistência, campos inválidos, prioridade, situação, prazo ausente, recursos ausentes e documento Swagger.

## SQL Server

O provider está implementado e selecionado por configuração, mas não foi validado com uma instância SQL Server nesta entrega. O `.env.example` é informativo; ASP.NET não o carrega automaticamente. Configure variáveis no ambiente:

```powershell
$env:DatabaseProvider='SqlServer'
$env:ConnectionStrings__Database='Server=localhost;Database=PortfolioDemo;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
```

A string usa autenticação integrada local, sem senha. `TrustServerCertificate=True` serve apenas ao ambiente local; em produção, use certificado válido. As migrations incluídas são do SQLite e não devem ser reaproveitadas sem revisão no SQL Server. Para migrar um novo banco SQL Server, em uma cópia ou branch dedicada, retire as migrations SQLite e gere `dotnet ef migrations add InitialSqlServer --project src/WorkOrders.Api`, seguido de `dotnet ef database update --project src/WorkOrders.Api`. Use uma identidade autorizada no servidor. Manter os dois bancos requer conjuntos separados de migrations.

## Limitações e melhorias futuras

Adicionar autenticação, autorização, paginação, concorrência otimista, índices e integração SQL Server em CI. A exclusão é física porque esta entidade não possui requisito de auditoria. Não há anexos, responsáveis, pagamento ou processamento assíncrono. Datas no passado são permitidas para cadastrar ordens históricas; não existe regra de SLA neste projeto.

## Documentação adicional

- [Guia de entrevista](INTERVIEW_GUIDE.md)
- [Comandos de publicação](GIT_SETUP.md)
- [Licença MIT](LICENSE)

Todos os dados de demonstração são fictícios. As aplicações foram projetadas para demonstração local; não possuem autenticação de usuários.

