# Validação local

## Atualização de 27/10/2026

SDK .NET 10.0.401 reutilizado no Windows. `dotnet test tests/WorkOrders.Api.Tests/WorkOrders.Api.Tests.csproj`: **12 aprovados, 0 falhas**, com SQLite em memória e banco deliberadamente inacessível em um cenário de readiness. Os quatro testes novos verificam health checks, indisponibilidade do banco, limite de chamadas com HTTP 429/Retry-After/Problem Details e erro de validação estruturado.

A restauração identificou avisos na dependência transitiva System.Security.Cryptography.Pkcs 9.0.11. Foi adicionada referência explícita à versão corrigida 9.0.21. A consulta `dotnet list src/WorkOrders.Api/WorkOrders.Api.csproj package --include-transitive --vulnerable --no-restore` deixou de encontrar pacotes vulneráveis nas fontes consultadas; isso não substitui auditoria de segurança ou verificação futura. A suíte foi repetida após essa mudança e passou.

Sem teste de carga, DDoS, coordenação entre instâncias ou integração real SQL Server. O relatório abaixo é o histórico anterior.


Data: 05/10/2026. Ambiente: Windows x64, Python 3.14.7 e SDK .NET 10.0.401 conforme aplicável.

**8 testes aprovados, sem falhas.** Instalação e comandos de execução foram conferidos conforme o README. Testes Python usam ambientes separados por projeto; testes C# usam migrations reais em SQLite em memória.

As três APIs também foram iniciadas como processos locais e verificadas por HTTP; o extrator executou os três documentos fictícios e exportou JSON, CSV e relatório (1 válido, 2 inválidos).

Links internos dos documentos foram verificados. Uma busca por padrões comuns de chaves não encontrou segredos nos fontes, e regras de exclusão do Git foram testadas. Essa busca é uma verificação limitada, não uma certificação de segurança.

Integrações reais PostgreSQL, SQL Server e LLM externo não foram executadas. Funcionalidades e limitações específicas estão no README. Workflows de CI foram incluídos, mas não executados no GitHub porque os repositórios ainda não foram publicados.
