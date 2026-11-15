# TravessIA API

API RESTful corporativa construida com **Minimal APIs em .NET 10.0**, **Clean Architecture**, **Entity Framework Core 9 (`UNIQUEIDENTIFIER` / `Guid`)**, autenticacao e autorizacao **JWT RBAC** (`CIDADAO` e `GESTOR`), **Idempotencia** (`Idempotency-Key` + `X-Idempotent-Replayed`), **Rate Limiting** com `FixedWindowLimiter` (`HTTP 429`), **Health Check em 3 camadas** (`/health`), integracao nativa com **Azure Key Vault** (`DefaultAzureCredential`) e **35 testes automatizados xUnit** (`27` unitarios + `8` de integracao).

**Turma:** 2TDSPG  
**Repositorio:** `https://github.com/JoaoHBatistaLeal/TravessIA`  
**URL Base em Producao:** `https://webapp-travessia-rm564361.azurewebsites.net`  
**Swagger UI:** `https://webapp-travessia-rm564361.azurewebsites.net/swagger/index.html`  
**Health Check:** `https://webapp-travessia-rm564361.azurewebsites.net/health`  

---

## 1. Integrantes do Grupo

- **Joao Henrique Batista Leal** — RM: `564361` 
- **Erik Naoki Miyasato** — RM: `565771` 
- **Juliana da Silva Stigliani** — RM: `561171`
- **Gutemberg Rocha Silva** — RM: `562267` 
- **Gustavo Arthur Carvalho Sartori** — RM: `561650`

---

## 2. Visao Geral da Solucao CitizenTech e Beneficios para o Negocio

### Problema Urbano e Solucao Proposta
Nos grandes centros urbanos, a temporizacao semaforica tradicional utiliza como referencia a velocidade media de caminhada de um adulto sem limitacoes fisicas (`1,2 m/s`) em pista seca. Essa premissa compromete a seguranca de idosos, cadeirantes, pessoas com deficiencia visual e cidadaos com mobilidade reduzida em travessias largas e durante chuvas intensas.

O **TravessIA** atua como camada de inteligencia viaria: sensores de proximidade instalados na calcada validam a presenca real do pedestre no intervalo fisico de **`0.1m` a `5.0m`**, consultam seu perfil de mobilidade cadastrado mediante **consentimento expresso LGPD (`'S'`)** e combinam a largura da via (`largura_via_m`) com telemetria climatica (`SECO`, `CHUVA_MODERADA`, `TEMPESTADE`) para estender dinamicamente o tempo de travessia segura e gerar uma justificativa auditavel (`justificativa_ia`).

### Stack Tecnologica

- **Linguagem e Framework:** C#, .NET 10.0 
- **Persistencia e Banco:** Entity Framework Core 9.0, Azure SQL Database, In-Memory
- **Seguranca e Segredos:** JWT Bearer, BCrypt, Azure Key Vault
- **Resiliencia e Docs:** Rate Limiting, Idempotencia, Health Checks, Swagger / OpenAPI 3.0
- **Testes Automatizados:** xUnit, WebApplicationFactory
- **Container e Cloud:** Docker, Azure App Service, Azure Pipelines
---


## 3. Estrutura da Clean Architecture, SOLID e Modelagem Relacional (5 Tabelas ERD)

### Camadas da Solucao (`TravessIA.sln`)
- **`TravessIA.Domain`**: Entidades ricas (`PerfilMobilidade`, `Usuario`, `Semaforo`, `Acionamento`, `AuditoriaSemaforo`), invariantes de negocio (`AplicarRegrasDeTravessia`, `CalcularExtensaoInteligente`, `EnsureLgpdConsent`), excecao `DomainException` e `HashHelper` (`BCrypt`). Zero dependencias externas de infraestrutura.
- **`TravessIA.Application`**: Contratos de repositorios (`I*Repository`), interfaces e implementacoes de servicos (`I*Service`), geracao de tokens (`JwtTokenIssuer`) e DTOs imutaveis (`record`) com metadados `[Description]` para OpenAPI.
- **`TravessIA.Infrastructure`**: `TravessIAContext`, mapeamentos Fluent API (`IEntityTypeConfiguration<T>` com `.ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()")`), `TravessIASeeder` e repositorios concretos EF Core.
- **`TravessIA.API`**: Ponto de composicao (`Program.cs`), extensoes de DI e Swagger (`SwaggerExtensions`, `TravessIAServiceCollectionExtensions`), tratamento global `GlobalExceptionHandler` (RFC 7807 `ProblemDetails`), Rate Limiter, Health Checks (`HealthCheckResponseWriter`, `ExternalWeatherAndKeyVaultHealthCheck`) e 6 modulos Minimal API (`/api/v1/*`).
- **`TravessIA.Tests`**: Projeto de testes com `CrosswalkDomainTests` (`27` casos unitarios) e `TravessIAIntegrationTests` (`8` cenarios integrados com `WebApplicationFactory<Program>`).

### Principios SOLID Aplicados
- **Single Responsibility (SRP)**: Cada modulo `*Endpoints.cs`, `*Service.cs` e `*Repository.cs` possui responsabilidade unica; regras de calculo climatico residem exclusivamente em `Acionamento`.
- **Open/Closed (OCP)**: Novos perfis de mobilidade e mapeamentos de excecoes no `GlobalExceptionHandler` sao adicionados sem alterar fluxos existentes.
- **Liskov Substitution (LSP) & Interface Segregation (ISP)**: Interfaces especificas por agregado (`IPerfilMobilidadeRepository`, `IUsuarioRepository`, `ISemaforoRepository`, `IAcionamentoRepository`, `IAuditoriaSemaforoRepository`) substituiveis entre SQL Server, SQLite e InMemory.
- **Dependency Inversion (DIP)**: `TravessIA.API` e `TravessIA.Application` dependem apenas de abstracoes (`I*Repository`, `I*Service`), injetadas via DI container.

### Modelagem Relacional (5 Tabelas ERD - `/scripts/script-bd.sql`)
1. **`TB_PERFIL_MOBILIDADE`**: `id_perfil` (PK `UNIQUEIDENTIFIER`), `codigo` (UK), `descricao`, `seg_extra_seco`, `seg_extra_chuva`. Seed inicial: `PADRAO`, `IDOSO`, `CADEIRANTE`, `DEFICIENCIA_VISUAL`, `MOBILIDADE_REDUZIDA`.
2. **`TB_USUARIO`**: `id_usuario` (PK `UNIQUEIDENTIFIER`), `id_perfil` (FK), `nome`, `email` (UK), `senha_hash` (BCrypt), `role` (`CIDADAO`/`GESTOR`), `ano_nascimento`, `consentimento_lgpd` (`S`/`N`), `dt_cadastro`.
3. **`TB_SEMAFORO`**: `id_semaforo` (PK `UNIQUEIDENTIFIER`), `codigo_beacon` (UK), `endereco`, `latitude`, `longitude`, `largura_via_m`, `tempo_base_seg`, `status_operacional` (`ATIVO`/`MANUTENCAO`/`MODO_CHUVA`). Seed inicial: `BEACON-SP-001`, `BEACON-SP-002`, `BEACON-SP-003`.
4. **`TB_ACIONAMENTO`**: `id_acionamento` (PK `UNIQUEIDENTIFIER`), `id_usuario` (FK), `id_semaforo` (FK), `chave_idempotencia` (UK), `dt_hora`, `distancia_sensor_m` (`0.1` a `5.0`), `condicao_clima` (`SECO`/`CHUVA_MODERADA`/`TEMPESTADE`), `intensidade_chuva_mm`, `tempo_extra_seg`, `justificativa_ia`.
5. **`TB_AUDITORIA_SEMAFORO`**: `id_auditoria` (PK `UNIQUEIDENTIFIER`), `id_semaforo` (`UNIQUEIDENTIFIER` **sem FK**), `operacao` (`INSERT`/`UPDATE`/`DELETE`), `campo_alterado`, `valor_antigo`, `valor_novo`, `usuario_bd`, `dt_operacao`.

---

## 4. Documentacao dos Endpoints

Todos os endpoints de `/api/v1/*` utilizam a politica de Rate Limiting `"fixed"` e metadados fluentes OpenAPI (`.WithName()`, `.WithTags()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`, `.ProducesProblem()`).

| Metodo | Rota | Descricao Funcional | Autorizacao |
|---|---|---|---|
| `GET` | `/health` | Verifica saude em 3 camadas (`api-self`, `database`, `external-weather-and-keyvault`) | Anonimo |
| `GET` | `/swagger/index.html` | Interface interativa Swagger UI (ativa em Development e Production) | Anonimo |
| `GET` | `/swagger/v1/swagger.json` | Especificacao OpenAPI 3.0 em formato JSON | Anonimo |
| `POST` | `/api/v1/auth/login` | Autentica email/senha (BCrypt) e emite token Bearer JWT | Anonimo |
| `POST` | `/api/v1/auth/register` | Auto-cadastro de usuario com resolucao automatica de `id_perfil` e emissao de JWT | Anonimo |
| `GET` | `/api/v1/auth/me` | Retorna identidade (`idUsuario`, `nome`, `email`, `role`) extraida dos claims do token | Bearer JWT - `CIDADAO`/`GESTOR` |
| `GET` | `/api/v1/perfis-mobilidade` | Lista todos os perfis de mobilidade de `TB_PERFIL_MOBILIDADE` | Anonimo |
| `GET` | `/api/v1/perfis-mobilidade/{id:guid}` | Consulta um perfil de mobilidade pelo `id_perfil` | Anonimo |
| `POST` | `/api/v1/perfis-mobilidade` | Cria novo perfil de mobilidade validando `segExtraChuva >= segExtraSeco` | Bearer JWT - `GESTOR` |
| `PUT` | `/api/v1/perfis-mobilidade/{id:guid}` | Atualiza parametros de um perfil de mobilidade existente | Bearer JWT - `GESTOR` |
| `DELETE` | `/api/v1/perfis-mobilidade/{id:guid}` | Remove um perfil de mobilidade pelo `id_perfil` | Bearer JWT - `GESTOR` |
| `GET` | `/api/v1/usuarios` | Lista todos os usuarios cadastrados sem expor `senha_hash` | Bearer JWT - `GESTOR` |
| `GET` | `/api/v1/usuarios/{id:guid}` | Consulta dados cadastrais e consentimento LGPD de um usuario | Bearer JWT - `CIDADAO`/`GESTOR` |
| `POST` | `/api/v1/usuarios` | Cadastra usuario com hash BCrypt e resolucao automatica de `id_perfil` | Anonimo |
| `PUT` | `/api/v1/usuarios/{id:guid}` | Atualiza perfil, dados cadastrais ou consentimento LGPD do usuario | Bearer JWT - `CIDADAO`/`GESTOR` |
| `DELETE` | `/api/v1/usuarios/{id:guid}` | Remove um usuario de `TB_USUARIO` pelo `id_usuario` | Bearer JWT - `GESTOR` |
| `GET` | `/api/v1/semaforos` | Lista todos os semaforos inteligentes em `TB_SEMAFORO` | Anonimo |
| `GET` | `/api/v1/semaforos/{id:guid}` | Consulta detalhes tecnicos e status operacional de um semaforo | Anonimo |
| `POST` | `/api/v1/semaforos` | Cadastra semaforo e grava evento `INSERT` em `TB_AUDITORIA_SEMAFORO` | Bearer JWT - `GESTOR` |
| `PUT` | `/api/v1/semaforos/{id:guid}` | Atualiza semaforo e grava evento `UPDATE` em `TB_AUDITORIA_SEMAFORO` | Bearer JWT - `GESTOR` |
| `DELETE` | `/api/v1/semaforos/{id:guid}` | Remove semaforo e preserva evento `DELETE` em `TB_AUDITORIA_SEMAFORO` | Bearer JWT - `GESTOR` |
| `GET` | `/api/v1/acionamentos` | Lista historico de acionamentos inteligentes com `justificativaIa` | Bearer JWT - `CIDADAO`/`GESTOR` |
| `GET` | `/api/v1/acionamentos/{id:guid}` | Consulta um acionamento especifico por `id_acionamento` | Bearer JWT - `CIDADAO`/`GESTOR` |
| `POST` | `/api/v1/acionamentos` | Solicita travessia inteligente com calculo seco/chuva e suporte a `Idempotency-Key` | Bearer JWT - `CIDADAO`/`GESTOR` |
| `PUT` | `/api/v1/acionamentos/{id:guid}` | Recalcula e atualiza telemetria climatica e tempo extra de um acionamento | Bearer JWT - `CIDADAO`/`GESTOR` |
| `DELETE` | `/api/v1/acionamentos/{id:guid}` | Remove registro de `TB_ACIONAMENTO` por `id_acionamento` | Bearer JWT - `GESTOR` |
| `GET` | `/api/v1/auditorias` | Lista toda a trilha imutavel de `TB_AUDITORIA_SEMAFORO` | Bearer JWT - `GESTOR` |
| `GET` | `/api/v1/auditorias/{id:guid}` | Consulta registro especifico de auditoria pelo `id_auditoria` | Bearer JWT - `GESTOR` |
| `GET` | `/api/v1/auditorias/semaforo/{idSemaforo:guid}` | Lista historico `INSERT`/`UPDATE`/`DELETE` por `id_semaforo` (mesmo apos exclusao) | Bearer JWT - `GESTOR` |

---

## 6. Exemplos de Payloads JSON, Idempotencia, Rate Limiting e ProblemDetails

### 6.1. Autenticacao (`POST /api/v1/auth/register` e `POST /api/v1/auth/login`)

#### Cadastro sem informar GUIDs (`POST /api/v1/auth/register`)
O backend gera `idUsuario` (`Guid.NewGuid()`) e resolve automaticamente `idPerfil` pelo `codigoPerfil` informado ou pela idade (`>= 60` anos -> perfil `IDOSO`):

**Request Body:**
```json
{
  "nome": "Maria Aparecida",
  "email": "maria.cidada@travessia.com.br",
  "senha": "TravessIA@2026",
  "anoNascimento": 1952,
  "codigoPerfil": "IDOSO",
  "role": "CIDADAO",
  "consentimentoLgpd": "S"
}
```

**Response (`201 Created`):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-11-11T22:49:48.0204027Z",
  "idUsuario": "f2eab64b-3b55-4b5c-9a9d-59d40020b2f8",
  "nome": "Maria Aparecida",
  "email": "maria.cidada@travessia.com.br",
  "role": "CIDADAO",
  "idPerfil": "11111111-1111-1111-1111-111111111102"
}
```

#### Login (`POST /api/v1/auth/login`)
**Request Body:**
```json
{
  "email": "gestor@travessia.com.br",
  "senha": "Admin@TravessIA2026"
}
```

**Response (`200 OK`):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-11-11T20:56:41Z",
  "idUsuario": "33333333-3333-3333-3333-333333333303",
  "nome": "Gestor Mobilidade Urbana",
  "email": "gestor@travessia.com.br",
  "role": "GESTOR",
  "idPerfil": "11111111-1111-1111-1111-111111111101"
}
```

### 6.2. Acionamento Inteligente Seco vs. Tempestade e Idempotencia (`POST /api/v1/acionamentos`)

Header enviado na requisicao: `Idempotency-Key: idem-rain-1794430182`

#### Cenario A — Clima `SECO` (Perfil `CADEIRANTE`, `BEACON-SP-001`, `larguraViaM = 18.50m`)
**Request Body:**
```json
{
  "codigoBeacon": "BEACON-SP-001",
  "distanciaSensorM": 1.5,
  "condicaoClima": "SECO",
  "intensidadeChuvaMm": 0
}
```

**Response (`201 Created` | Header `X-Idempotent-Replayed: false`):**
```json
{
  "idAcionamento": "4c2d2ce8-31eb-498f-bf98-5d10a3e0ae69",
  "idUsuario": "f2eab64b-3b55-4b5c-9a9d-59d40020b2f8",
  "idSemaforo": "22222222-2222-2222-2222-222222222201",
  "chaveIdempotencia": "idem-dry-1794430182",
  "dtHora": "2026-11-11T20:51:16.990005Z",
  "distanciaSensorM": 1.5,
  "condicaoClima": "SECO",
  "intensidadeChuvaMm": 0,
  "tempoExtraSeg": 13,
  "tempoBaseSeg": 15,
  "tempoTotalSeg": 28,
  "justificativaIa": "TravessIA Decision Engine: Perfil=CADEIRANTE (+8s) | Clima=SECO (0.00mm) | LarguraVia=18.50m (+5s) | BonusTempestade=+0s | TempoBase=15s | TempoExtraConcedido=13s | TempoTotalTravessia=28s."
}
```

#### Cenario B — Clima `TEMPESTADE` (`25.0mm`) e Replay Idempotente
**Request Body (com header `Idempotency-Key: idem-rain-1794430182`):**
```json
{
  "codigoBeacon": "BEACON-SP-001",
  "distanciaSensorM": 1.5,
  "condicaoClima": "TEMPESTADE",
  "intensidadeChuvaMm": 25.0
}
```

**1a Chamada (`201 Created` | `X-Idempotent-Replayed: false`) e 2a Chamada Repetida (`200 OK` | `X-Idempotent-Replayed: true`):**
```json
{
  "idAcionamento": "d663b7b0-edc9-4a74-b45f-85fe43968195",
  "idUsuario": "f2eab64b-3b55-4b5c-9a9d-59d40020b2f8",
  "idSemaforo": "22222222-2222-2222-2222-222222222201",
  "chaveIdempotencia": "idem-rain-1794430182",
  "dtHora": "2026-11-11T20:51:17.2725172Z",
  "distanciaSensorM": 1.5,
  "condicaoClima": "TEMPESTADE",
  "intensidadeChuvaMm": 25.0,
  "tempoExtraSeg": 30,
  "tempoBaseSeg": 15,
  "tempoTotalSeg": 45,
  "justificativaIa": "TravessIA Decision Engine: Perfil=CADEIRANTE (+15s) | Clima=TEMPESTADE (25.00mm) | LarguraVia=18.50m (+10s) | BonusTempestade=+5s | TempoBase=15s | TempoExtraConcedido=30s | TempoTotalTravessia=45s."
}
```

### 6.3. Validacoes de Dominio (`400 Bad Request` - RFC 7807 `ProblemDetails`)

#### Distancia do Sensor Fora do Limite (`distanciaSensorM = 8.5m > 5.0m`)
```json
{
  "type": "about:blank",
  "title": "Regra de negocio violada",
  "status": 400,
  "detail": "Distancia do sensor fora do limite permitido (deve estar entre 0.1m e 5.0m da calcada).",
  "instance": "/api/v1/acionamentos"
}
```

#### Consentimento LGPD Negado (`consentimentoLgpd = "N"`)
```json
{
  "type": "about:blank",
  "title": "Regra de negocio violada",
  "status": 400,
  "detail": "Acionamento negado: usuario nao concedeu consentimento LGPD obrigatorio (S).",
  "instance": "/api/v1/usuarios/f2eab64b-3b55-4b5c-9a9d-59d40020b2f8"
}
```

### 6.4. Rate Limiting (`429 Too Many Requests` com Header `Retry-After: 60`)

```json
{
  "type": "about:blank",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Limite de requisicoes excedido. Tente novamente em instantes.",
  "instance": "/api/v1/semaforos"
}
```

### 6.5. Health Check em 3 Camadas (`GET /health` -> `200 OK`)

```json
{
  "status": "Healthy",
  "service": "TravessIA.API",
  "duration": "00:00:00.1267220",
  "checks": [
    {
      "name": "api-self",
      "status": "Healthy",
      "description": "TravessIA Minimal API runtime operacional e responsivo.",
      "duration": "00:00:00.0011735",
      "error": null
    },
    {
      "name": "database",
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0884055",
      "error": null
    },
    {
      "name": "external-weather-and-keyvault",
      "status": "Healthy",
      "description": "Motor de telemetria climatica/IA operacional e Azure Key Vault conectado (https://kv-travessia-564361.vault.azure.net/).",
      "duration": "00:00:00.0128626",
      "error": null
    }
  ]
}
```

---

## 7. Variaveis de Ambiente, Azure Key Vault e Seguranca

Nenhuma senha, chave JWT ou connection string existe em `appsettings.json` ou `appsettings.Development.json`. Em nuvem, a aplicacao utiliza `DefaultAzureCredential` associada a System-Assigned Managed Identity para ler o cofre `https://kv-travessia-564361.vault.azure.net/`.

| Chave / Variavel | Descricao | Origem em Producao |
|---|---|---|
| `KEYVAULT_URI` | URI do Azure Key Vault (`https://kv-travessia-564361.vault.azure.net/`) | App Settings do Web App |
| `ConnectionStrings__DefaultConnection` | String de conexao SQL Server (ou SQLite local quando vazia) | Azure Key Vault (`ConnectionStrings--DefaultConnection`) |
| `JwtSettings__SecretKey` | Chave HMAC-SHA256 de assinatura JWT | Azure Key Vault (`JwtSettings--SecretKey`) |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execucao (`Development` / `Production`) | App Settings (`Production`) |
| `RateLimiting__PermitLimit` | Limite de requisicoes por janela (`30` por padrao) | `appsettings.json` / Env |
| `RateLimiting__WindowSeconds` | Duracao da janela fixa em segundos (`60` por padrao) | `appsettings.json` / Env |

### Configuracao Segura Local via `dotnet user-secrets`

```bash
dotnet user-secrets init --project TravessIA.API/TravessIA.API.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=travessia-local.db" --project TravessIA.API/TravessIA.API.csproj
dotnet user-secrets set "JwtSettings:SecretKey" "<CHAVE_LOCAL_DESENVOLVIMENTO_MIN_32_CHARS>" --project TravessIA.API/TravessIA.API.csproj
```

---

## 8. Instrucoes de Instalacao, Execucao Local e Testes Automatizados (xUnit)

### Pre-requisitos
- `.NET SDK 10.0`
- SQL Server / Azure SQL Database (opcional localmente; sem `DefaultConnection`, a API utiliza SQLite local `travessia-local.db` automaticamente)

### Passos de Instalacao, Build e Execucao

```bash
git clone https://github.com/JoaoHBatistaLeal/TravessIA.git
cd TravessIA
dotnet restore TravessIA.sln
dotnet build TravessIA.sln --configuration Release
```

```bash
dotnet run --project TravessIA.API/TravessIA.API.csproj
```

### Suite de Testes Automatizados (`xUnit`)

A solucao conta com **35 testes automatizados**

```bash
dotnet test TravessIA.sln --configuration Release
```

Cobertura validada pelos 35 testes:
- Calculo de extensao de travessia seco vs. chuva moderada vs. tempestade nos 5 perfis prioritarios.
- Fator de largura da via (`largura_via_m`) e bonus de seguranca `+5s` em tempestade.
- Trava fisica de distancia do sensor (`0.1m` a `5.0m`), bloqueio de semaforo em `MANUTENCAO` e negacao LGPD (`'N'`).
- Fluxo JWT completo (`/register`, `/login`, `/me`), barreiras `401 Unauthorized` e `403 Forbidden` (`CIDADAO` vs `GESTOR`).
- CRUD completo nas tabelas principais, trilha automatica `INSERT`/`UPDATE`/`DELETE` em `TB_AUDITORIA_SEMAFORO`, replay idempotente (`X-Idempotent-Replayed: true`), Rate Limiting (`429`) e Health Check em 3 camadas (`/health`).

---

## 9. Guia de Deploy, Monitoramento em Nuvem e Declaracao de Uso de IA Generativa

### Provisionamento e Deploy em Nuvem (Azure)
1. **Provisionamento IaC**: Execute `./scripts/provision-azure.sh` (com `SQL_ADMIN_PASSWORD`, `SQL_CONNECTION_STRING` e `JWT_SECRET_KEY` exportadas na sessao) para criar `rg-travessia-gs2026`, `plan-travessia-free` (`F1`), `webapp-travessia-rm564361` (com Managed Identity), `sql-travessia-rm564361` (`sqldb-travessia`), `kv-travessia-564361` (`Key Vault Secrets User`) e aplicar `/scripts/script-bd.sql`.
2. **Esteira CI/CD (`azure-pipeline.yml`)**: No repositorio de entrega DevOps (branch `master`), cada merge aprovado executa `Build`, testes `xUnit`, publicacao do artefato `drop` e deploy automatizado via `AzureWebApp@1`.
3. **Monitoramento em 3 Camadas (`/health`)**: A rota `https://webapp-travessia-rm564361.azurewebsites.net/health` inspeciona continuamente `"api-self"` (disponibilidade da Minimal API), `"database"` (`TravessIAContext` conectado ao `sqldb-travessia`) e `"external-weather-and-keyvault"` (motor de telemetria climatica/IA + URI configurada do Key Vault).

### Declaracao Obrigatoria de Uso de Inteligencia Artificial Generativa
Em conformidade com as diretrizes academicas da FIAP para a Global Solution 2026/2, declaramos que ferramentas de Inteligencia Artificial Generativa foram utilizadas como apoio assistido no desenvolvimento desta solucao:
1. **Arquitetura e Padronizacao de Codigo**: Auxilio na estruturacao da Clean Architecture em `.NET 10.0`, configuracao das Minimal APIs, Fluent API do Entity Framework Core e descritores Swagger.
2. **Qualidade, Testes e Automacao**: Apoio na escrita de cenarios de teste `xUnit`, scripts de provisionamento (`scripts/provision-azure.sh`, `scripts/script-bd.sql`) e documentacao.
3. **Validacao e Autoria Humana**: Todas as regras de dominio, modelagem relacional, politicas de seguranca JWT/LGPD, integracao com Azure Key Vault e testes reais contra a nuvem foram integralmente revisados, executados e validados pelo grupo, que assume total responsabilidade tecnica pelo projeto entregue.