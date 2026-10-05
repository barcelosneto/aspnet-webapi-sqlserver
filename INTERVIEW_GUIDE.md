# Guia de entrevista — aspnet-webapi-sqlserver

## Problema, arquitetura e decisões

O problema é manter ordens de trabalho com um contrato REST claro. Controller recebe DTO; WorkOrderService aplica regras e usa AppDbContext; EF Core persiste em SQLite. C# e ASP.NET oferecem tipagem, DI e validação; EF fornece migrations e queries assíncronas. A tabela WorkOrders armazena título, descrição, situação, prioridade e prazo. GET lista/busca, POST cria com Location, PUT substitui campos, DELETE remove. Problem Details centraliza erros. Testes exercitam endpoints com SQLite em memória e migrations reais. Estude Service, Controller e ApiTests. O provider SQL Server é configurável mas não foi testado num servidor. Autenticação, concorrência, paginação e migração SQL Server são trabalho futuro.

## Roteiro de estudo

1. Execute os comandos do [README](README.md) e observe as saídas.
2. Execute os testes e leia os casos antes de alterar as regras.
3. Acompanhe uma entrada desde o contrato até a persistência ou saída.
4. Reproduza uma falha de validação e explique sua resposta.
5. Distinga o que foi implementado, o que foi validado e o que é melhoria futura.

## 15 perguntas com respostas

### 1. Por que usar DTOs?

DTOs definem o contrato HTTP e limitam campos editáveis. A entidade interna não é aceita diretamente, reduzindo acoplamento e evitando que o cliente altere IDs.

### 2. Qual é o papel de ApiController?

Aplica validação automática de DataAnnotations e respostas 400 para contratos inválidos. O serviço complementa regras como data obrigatória e conteúdo não vazio.

### 3. Por que não existe repository?

DbContext e DbSet já fornecem acesso e unidade de trabalho. O serviço mantém operações simples e compreensíveis; uma interface adicional sem uso concreto duplicaria abstrações.

### 4. Como as migrations funcionam?

Arquivos em Data/Migrations descrevem o schema inicial e snapshot do EF. `database update` aplica alterações pendentes; o app não migra automaticamente ao iniciar.

### 5. Por que SQLite é o padrão?

Um arquivo local evita exigir servidor para executar a demonstração. O EF também inclui provider SQL Server, mas migrations e diferenças de tipos devem ser tratadas por banco.

### 6. As migrations SQLite funcionam no SQL Server?

Não devem ser presumidas compatíveis. O README orienta gerar um conjunto específico em uma branch ou cópia dedicada. O provider SQL Server não foi validado com servidor real.

### 7. Como a configuração é obtida?

ASP.NET combina appsettings e variáveis de ambiente. `ConnectionStrings__Database` substitui a conexão. `.env.example` é documentação e não é carregado pelo runtime.

### 8. Como funciona a criação?

O controller recebe DTO, o serviço valida, cria entidade e chama SaveChangesAsync. Retorna DTO com ID. CreatedAtAction fornece 201 e Location para a consulta por ID.

### 9. Por que consultas usam AsNoTracking?

Leituras não precisam de detecção de mudanças. Isso reduz trabalho do EF. Atualização e exclusão usam entidades rastreadas para persistir mudanças.

### 10. Como a exclusão é definida?

Remove a linha fisicamente e retorna 204; um ID ausente retorna 404. Não há requisito de auditoria nesta entidade, portanto exclusão lógica não foi acrescentada.

### 11. Como erros inesperados são tratados?

UseExceptionHandler produz Problem Details genérico 500. BusinessRuleException vira 400 com mensagem de regra. Detalhes internos não entram na resposta.

### 12. Como os testes exercitam HTTP?

WebApplicationFactory hospeda o app para um HttpClient. O banco é substituído por SQLite em memória e migrations reais são aplicadas antes das requisições.

### 13. Por que manter a conexão em memória aberta?

SQLite em memória existe enquanto a conexão existe. ApiFactory abre uma conexão por instância e a mantém durante o teste para preservar dados entre requisições.

### 14. Como CancellationToken ajuda?

O token da requisição passa por consultas e gravações assíncronas, permitindo cancelamento quando o cliente desconecta. Ele não substitui timeout e políticas operacionais.

### 15. O que faltaria para produção?

Autenticação, autorização, concorrência otimista, paginação, observabilidade, migrations geridas e testes no banco escolhido. Hoje duas atualizações simultâneas podem sobrescrever alterações.

