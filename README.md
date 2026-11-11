# TravessIA - Semaforo Inteligente e Mobilidade Urbana Inclusiva (CitizenTech GS 2026/2)

## 1. Visao Geral da Solucao e Impacto CitizenTech

O **TravessIA** e uma plataforma CitizenTech de mobilidade urbana inclusiva e seguranca viaria preditiva projetada para proteger cidadaos com mobilidade reduzida, idosos, cadeirantes e pessoas com deficiencia visual em travessias de pedestres.

Nos grandes centros urbanos, o tempo de fase verde para pedestres nos semaforos tradicionais e calculado com base na velocidade media de deslocamento de um adulto sem restricoes fisicas (1,2 m/s) sob condicoes climaticas ideais. Quando ocorrem chuvas moderadas ou tempestades, ou quando um idoso ou pessoa com deficiencia realiza a travessia em avenidas largas, esse tempo fixo torna-se insuficiente, elevando o risco de atropelamentos e quedas.

O **TravessIA** integra sensores fisicos de proximidade na calcada (Beacons BLE/IoT), perfis de mobilidade do cidadao (mediante consentimento expresso da LGPD) e telemetria meteorologica regional em tempo real (`SECO`, `CHUVA_MODERADA`, `TEMPESTADE`) para calcular dinamicamente e conceder segundos extras de travessia segura, registrando justificativas estruturadas de Inteligencia Artificial e trilha completa de auditoria para os gestores municipais de transito.

### Alinhamento aos Objetivos de Desenvolvimento Sustentavel da ONU (ODS)
- **ODS 3 (Saude e Bem-Estar - Meta 3.6)**: Reducao de acidentes de transito, quedas em pista molhada e atropelamentos de pedestres vulneraveis nas travessias urbanas.
- **ODS 9 (Industria, Inovacao e Infraestrutura - Meta 9.1)**: Modernizacao da infraestrutura viaria publica com sensores IoT, APIs resilientes em nuvem e automacao DevSecOps.
- **ODS 10 (Reducao das Desigualdades - Meta 10.2)**: Inclusao social e autonomia de locomocao urbana para idosos, cadeirantes e pessoas com deficiencia visual ou mobilidade reduzida.
- **ODS 11 (Cidades e Comunidades Sustentaveis - Meta 11.2)**: Sistemas de transporte e travessia seguros, acessiveis e sustentaveis com foco em grupos em situacao de vulnerabilidade.
- **ODS 13 (Acao Contra a Mudanca Global do Clima - Meta 13.1)**: Adaptacao ativa da infraestrutura semaforica a eventos climaticos extremos, chuvas intensas e alagamentos urbanos.

---

## 2. Integrantes do Grupo (2TDSPJ - FIAP)

| Nome Completo | RM | E-mail Institucional |
|---|---|---|
| Joao Henrique Batista Leal | RM564361 | RM564361@fiap.com.br |
| Erik Naoki Miyasato | RM565771 | RM565771@fiap.com.br |
| Juliana da Silva Stigliani | RM561171 | RM561171@fiap.com.br |
| Gutemberg Rocha Silva | RM562267 | RM562267@fiap.com.br |
| Gustavo Arthur Carvalho Sartori | RM561650 | RM561650@fiap.com.br |

---

## 3. Arquitetura Macro da Solucao (Cloud Azure, Key Vault e CI/CD)

```mermaid
flowchart LR
    subgraph ClientLayer["Camada Cliente e IoT"]
        Mobile["App Mobile Cidadao / Painel Gestor"]
        Beacon["Sensor Calcada Beacon BLE (0.1m a 5.0m)"]
    end

    subgraph AzureCloud["Microsoft Azure Cloud (rg-travessia-gs2026 - brazilsouth)"]
        WebApp["Azure App Service Linux (SKU F1 Free)\nwebapp-travessia-rm564361\nASP.NET Core 10 Minimal API"]
        KeyVault["Azure Key Vault (Standard RBAC)\nkv-travessia-564361\nDefaultAzureCredential / Managed Identity"]
        SqlDb["Azure SQL Database (sqldb-travessia)\nsql-travessia-rm564361\n5 Tabelas Relacionais ERD"]
    end

    subgraph DevOpsGovernance["Azure DevOps (OrgJJEGG / TravessIA)"]
        Boards["Azure Boards (Scrum)\nEpics, Features, PBIs, Tasks (Sprint 1)"]
        Repos["Azure Repos + GitHub\nBranch Policy on main + PR Review"]
        Pipelines["Azure Pipelines (azure-pipeline.yml)\nStage 1: Build + xUnit Test + PublishTestResults\nStage 2: Release CD to Azure Web App"]
    end

    Beacon -->|"Proximidade 0.1m-5.0m"| Mobile
    Mobile -->|"HTTPS + JWT Bearer + Idempotency-Key"| WebApp
    WebApp -->|"System-Assigned Managed Identity"| KeyVault
    WebApp -->|"EF Core 9 / T-SQL"| SqlDb
    Boards -->|"Work Item Linking"| Repos
    Repos -->|"Trigger on PR Merge to main"| Pipelines
    Pipelines -->|"Continuous Deployment (drop.zip)"| WebApp
```

### Componentes de Infraestrutura Provisionados na Nuvem (Azure)
- **Assinatura Azure**: `2026-2TDSPJ-JOAO-H-BATISTA-LEAL-RM564361`
- **Resource Group**: `rg-travessia-gs2026` (`brazilsouth`)
- **App Service Plan**: `plan-travessia-free` (`F1` Free Tier, Linux)
- **Azure Web App**: `webapp-travessia-rm564361` (`https://webapp-travessia-rm564361.azurewebsites.net`) com **System-Assigned Managed Identity**
- **Azure Key Vault**: `kv-travessia-564361` (`https://kv-travessia-564361.vault.azure.net/`) armazenando os segredos `ConnectionStrings--DefaultConnection` e `JwtSettings--SecretKey` lidos em tempo de execucao via `DefaultAzureCredential`
- **Azure SQL Server & Database**: `sql-travessia-rm564361.database.windows.net` / `sqldb-travessia` populado via `/scripts/script-bd.sql`
- **Azure DevOps Organization & Project**: `https://dev.azure.com/OrgJJEGG/TravessIA` (Processo `Scrum`, Azure Boards `Sprint 1`, Pipeline `TravessIA-CI-CD` via `/azure-pipeline.yml`)
- **URLs Publicas em Producao**:
  - **Swagger UI (Producao)**: `https://webapp-travessia-rm564361.azurewebsites.net/swagger`
  - **OpenAPI JSON**: `https://webapp-travessia-rm564361.azurewebsites.net/swagger/v1/swagger.json`
  - **Health Check (`/health`)**: `https://webapp-travessia-rm564361.azurewebsites.net/health`

---

## 4. Modelagem Relacional de Dados (5 Tabelas ERD e `/scripts/script-bd.sql`)

O banco de dados relacional implementa fielmente as 5 tabelas do diagrama entidade-relacionamento no script `/scripts/script-bd.sql` e no mapeamento Fluent API do Entity Framework Core (`TravessIA.Infrastructure`):

1. **`TB_PERFIL_MOBILIDADE`**: Cadastro de perfis de mobilidade (`id_perfil` PK Identity, `codigo` UK, `descricao`, `seg_extra_seco`, `seg_extra_chuva`) com carga inicial para `PADRAO`, `IDOSO`, `CADEIRANTE`, `DEFICIENCIA_VISUAL` e `MOBILIDADE_REDUZIDA`.
2. **`TB_USUARIO`**: Cadastro de cidadaos e gestores (`id_usuario` PK Identity, `id_perfil` FK, `nome`, `email` UK, `senha_hash` BCrypt, `role` `CIDADAO`/`GESTOR`, `ano_nascimento`, `consentimento_lgpd` `'S'`/`'N'`, `dt_cadastro`).
3. **`TB_SEMAFORO`**: Cadastro de cruzamentos semaforicos monitorados por beacons (`id_semaforo` PK Identity, `codigo_beacon` UK, `endereco`, `latitude`, `longitude`, `largura_via_m`, `tempo_base_seg`, `status_operacional` `ATIVO`/`MANUTENCAO`/`MODO_CHUVA`).
4. **`TB_ACIONAMENTO`**: Registro de acionamentos de travessia inteligente (`id_acionamento` PK Identity, `id_usuario` FK, `id_semaforo` FK, `chave_idempotencia` UK, `dt_hora`, `distancia_sensor_m`, `condicao_clima` `SECO`/`CHUVA_MODERADA`/`TEMPESTADE`, `intensidade_chuva_mm`, `tempo_extra_seg`, `justificativa_ia`).
5. **`TB_AUDITORIA_SEMAFORO`**: Tabela de auditoria automatica sem restricao de chave estrangeira (`registra (sem FK)`) para preservar o historico mesmo apos delecao de semaforos (`id_auditoria` PK Identity, `id_semaforo`, `operacao` `INSERT`/`UPDATE`/`DELETE`, `campo_alterado`, `valor_antigo`, `valor_novo`, `usuario_bd`, `dt_operacao`).

---

## 5. Requisitos Tecnicos .NET 10 e DevSecOps Implementados

1. **ASP.NET Core Minimal APIs + EF Core**:
   - Grupos de rotas versionados em `/api/v1/auth`, `/api/v1/perfis-mobilidade`, `/api/v1/usuarios`, `/api/v1/semaforos`, `/api/v1/acionamentos` e `/api/v1/auditorias`.
   - Regras de negocio de travessia inteligente (`POST /api/v1/acionamentos`): validacao de consentimento LGPD (`consentimento_lgpd == 'S'`), trava fisica de proximidade do sensor (`0.1m <= distancia_sensor_m <= 5.0m`), calculo adaptativo seco vs. chuva (`seg_extra_seco` vs. `seg_extra_chuva`), fator de largura da via (`largura_via_m`), bonus de seguranca para tempestade e geracao da explicacao em `justificativa_ia`.
2. **Documentacao OpenAPI / Swagger Fluente**:
   - Todos os endpoints sao documentados via metodos de extensao fluentes (`.WithName()`, `.WithTags()`, `.WithSummary()`, `.WithDescription()`, `.Accepts<T>()`, `.Produces<T>()`, `.ProducesProblem()`) e atributos `[Description]` nos DTOs, com suporte a autenticacao Bearer JWT na interface do Swagger em `Development` e `Production`.
3. **Autenticacao JWT e Autorizacao por Roles (`CIDADAO` e `GESTOR`)**:
   - Emissao de tokens JWT assinados com HMAC-SHA256 em `/api/v1/auth/login` e `/api/v1/auth/register`, protegendo operacoes administrativas com a politica `GestorOnly` (`GESTOR`) e operacoes de travessia com `CidadaoOrGestor`.
4. **Idempotencia (`Idempotency-Key`)**:
   - O endpoint `POST /api/v1/acionamentos` exige o cabecalho `Idempotency-Key` (ou `chaveIdempotencia`). Requisicoes repetidas com a mesma chave retornam o registro original com `HTTP 200 OK` e cabecalho `X-Idempotent-Replayed: true`, evitando acionamentos duplicados quando o pedestre pressiona o botao repetidamente.
5. **Rate Limiting com HTTP 429 (`ProblemDetails`)**:
   - Middleware `FixedWindowLimiter` configurado para retornar `HTTP 429 Too Many Requests` no padrao RFC 7807 `ProblemDetails` com cabecalho `Retry-After` quando a cota de requisicoes por janela e excedida.
6. **Health Checks (`/health`)**:
   - Endpoint `/health` inspecionando a conectividade com o banco de dados via `TravessIAContext` e retornando payload JSON estruturado.
7. **Suite de Testes Unitarios e de Integracao (`xUnit.net`)**:
   - Projeto `TravessIA.Tests` com 35 testes automatizados (`27` testes unitarios de dominio + `8` testes de integracao com `WebApplicationFactory<Program>`), validando calculo seco vs. molhado, trava de distancia 0.1m-5.0m, consentimento LGPD, CRUD completo, auditoria automatica, JWT `401`/`403`, replay de idempotencia, Rate Limiting `429` e `/health`.

---

## 6. Exemplos Completos de Payloads JSON para Testes CRUD

### 6.1. Autenticacao (`POST /api/v1/auth/register` e `POST /api/v1/auth/login`)

#### Request (`POST /api/v1/auth/login`)
```json
{
  "email": "gestor@travessia.sp.gov.br",
  "senha": "Password123!"
}
```

#### Response (`200 OK`)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-11-11T20:56:41Z",
  "idUsuario": 1,
  "nome": "Gestor CET Sao Paulo",
  "email": "gestor@travessia.sp.gov.br",
  "role": "GESTOR",
  "idPerfil": 1
}
```

---

### 6.2. CRUD Tabela 1: `TB_SEMAFORO` (`/api/v1/semaforos`)

#### 1) Criar Semaforo - `POST /api/v1/semaforos` (Requer Bearer Token `GESTOR`)
**Request Body:**
```json
{
  "codigoBeacon": "BEACON-IBIRAPUERA-500",
  "endereco": "Av. Pedro Alvares Cabral, 500 - Vila Mariana, Sao Paulo - SP",
  "latitude": -23.5874162,
  "longitude": -46.6576338,
  "larguraViaM": 16.50,
  "tempoBaseSeg": 20,
  "statusOperacional": "ATIVO"
}
```

**Response (`201 Created`):**
```json
{
  "idSemaforo": 4,
  "codigoBeacon": "BEACON-IBIRAPUERA-500",
  "endereco": "Av. Pedro Alvares Cabral, 500 - Vila Mariana, Sao Paulo - SP",
  "latitude": -23.5874162,
  "longitude": -46.6576338,
  "larguraViaM": 16.50,
  "tempoBaseSeg": 20,
  "statusOperacional": "ATIVO"
}
```

#### 2) Listar Semaforos - `GET /api/v1/semaforos`
**Response (`200 OK`):**
```json
[
  {
    "idSemaforo": 1,
    "codigoBeacon": "BEACON-PAULISTA-1106",
    "endereco": "Av. Paulista, 1106 - Bela Vista, Sao Paulo - SP",
    "latitude": -23.5641095,
    "longitude": -46.6524099,
    "larguraViaM": 18.50,
    "tempoBaseSeg": 22,
    "statusOperacional": "ATIVO"
  },
  {
    "idSemaforo": 4,
    "codigoBeacon": "BEACON-IBIRAPUERA-500",
    "endereco": "Av. Pedro Alvares Cabral, 500 - Vila Mariana, Sao Paulo - SP",
    "latitude": -23.5874162,
    "longitude": -46.6576338,
    "larguraViaM": 16.50,
    "tempoBaseSeg": 20,
    "statusOperacional": "ATIVO"
  }
]
```

#### 3) Atualizar Semaforo - `PUT /api/v1/semaforos/4` (Requer Bearer Token `GESTOR`)
**Request Body:**
```json
{
  "endereco": "Av. Pedro Alvares Cabral, 500 - Portao 3 Ibirapuera, Sao Paulo - SP",
  "latitude": -23.5874162,
  "longitude": -46.6576338,
  "larguraViaM": 16.50,
  "tempoBaseSeg": 24,
  "statusOperacional": "MODO_CHUVA"
}
```

**Response (`200 OK`):**
```json
{
  "idSemaforo": 4,
  "codigoBeacon": "BEACON-IBIRAPUERA-500",
  "endereco": "Av. Pedro Alvares Cabral, 500 - Portao 3 Ibirapuera, Sao Paulo - SP",
  "latitude": -23.5874162,
  "longitude": -46.6576338,
  "larguraViaM": 16.50,
  "tempoBaseSeg": 24,
  "statusOperacional": "MODO_CHUVA"
}
```

#### 4) Remover Semaforo - `DELETE /api/v1/semaforos/4` (Requer Bearer Token `GESTOR`)
**Response:** `204 No Content` (Gera registro automatico de auditoria `DELETE` em `TB_AUDITORIA_SEMAFORO`).

---

### 6.3. CRUD Tabela 2: `TB_PERFIL_MOBILIDADE` (`/api/v1/perfis-mobilidade`)

#### 1) Criar Perfil de Mobilidade - `POST /api/v1/perfis-mobilidade` (Requer Bearer Token `GESTOR`)
**Request Body:**
```json
{
  "codigo": "GESTANTE_INFANTIL",
  "descricao": "Gestantes e responsaveis com carrinho de bebe",
  "segExtraSeco": 7,
  "segExtraChuva": 13
}
```

**Response (`201 Created`):**
```json
{
  "idPerfil": 6,
  "codigo": "GESTANTE_INFANTIL",
  "descricao": "Gestantes e responsaveis com carrinho de bebe",
  "segExtraSeco": 7,
  "segExtraChuva": 13
}
```

#### 2) Consultar Perfil por ID - `GET /api/v1/perfis-mobilidade/6`
**Response (`200 OK`):**
```json
{
  "idPerfil": 6,
  "codigo": "GESTANTE_INFANTIL",
  "descricao": "Gestantes e responsaveis com carrinho de bebe",
  "segExtraSeco": 7,
  "segExtraChuva": 13
}
```

#### 3) Atualizar Perfil de Mobilidade - `PUT /api/v1/perfis-mobilidade/6` (Requer Bearer Token `GESTOR`)
**Request Body:**
```json
{
  "codigo": "GESTANTE_INFANTIL",
  "descricao": "Gestantes, lactantes e responsaveis com bebe de colo",
  "segExtraSeco": 8,
  "segExtraChuva": 15
}
```

**Response (`200 OK`):**
```json
{
  "idPerfil": 6,
  "codigo": "GESTANTE_INFANTIL",
  "descricao": "Gestantes, lactantes e responsaveis com bebe de colo",
  "segExtraSeco": 8,
  "segExtraChuva": 15
}
```

#### 4) Remover Perfil de Mobilidade - `DELETE /api/v1/perfis-mobilidade/6` (Requer Bearer Token `GESTOR`)
**Response:** `204 No Content`

---

### 6.4. CRUD Tabela 3: `TB_ACIONAMENTO` (`/api/v1/acionamentos`) com `Idempotency-Key`

#### 1) Solicitar Travessia Inteligente - `POST /api/v1/acionamentos` (Requer Bearer Token `CIDADAO` ou `GESTOR`)
**Headers:**
- `Authorization: Bearer <token>`
- `Idempotency-Key: cloud-prod-travessia-2026-001`

**Request Body:**
```json
{
  "idUsuario": 2,
  "idSemaforo": 1,
  "chaveIdempotencia": "cloud-prod-travessia-2026-001",
  "distanciaSensorM": 1.80,
  "condicaoClima": "CHUVA_MODERADA",
  "intensidadeChuvaMm": 14.50
}
```

**Response (`201 Created` na 1a chamada com `X-Idempotent-Replayed: false` / `200 OK` com `X-Idempotent-Replayed: true` em chamadas subsequentes):**
```json
{
  "idAcionamento": 1,
  "idUsuario": 2,
  "idSemaforo": 1,
  "chaveIdempotencia": "cloud-prod-travessia-2026-001",
  "dtHora": "2026-11-11T18:58:17.0514376Z",
  "distanciaSensorM": 1.80,
  "condicaoClima": "CHUVA_MODERADA",
  "intensidadeChuvaMm": 14.50,
  "tempoExtraSeg": 22,
  "tempoBaseSeg": 16,
  "tempoTotalSeg": 38,
  "justificativaIa": "TravessIA Decision Engine: Perfil=IDOSO (+12s) | Clima=CHUVA_MODERADA (14.50mm) | LarguraVia=18.50m (+10s) | BonusTempestade=+0s | TempoBase=16s | TempoExtraConcedido=22s | TempoTotalTravessia=38s."
}
```

#### 2) Listar Acionamentos - `GET /api/v1/acionamentos` (Requer Bearer Token)
**Response (`200 OK`):**
```json
[
  {
    "idAcionamento": 1,
    "idUsuario": 2,
    "idSemaforo": 1,
    "chaveIdempotencia": "cloud-prod-travessia-2026-001",
    "dtHora": "2026-11-11T18:58:17.0514376Z",
    "distanciaSensorM": 1.80,
    "condicaoClima": "CHUVA_MODERADA",
    "intensidadeChuvaMm": 14.50,
    "tempoExtraSeg": 22,
    "tempoBaseSeg": 16,
    "tempoTotalSeg": 38,
    "justificativaIa": "TravessIA Decision Engine: Perfil=IDOSO (+12s) | Clima=CHUVA_MODERADA (14.50mm) | LarguraVia=18.50m (+10s) | BonusTempestade=+0s | TempoBase=16s | TempoExtraConcedido=22s | TempoTotalTravessia=38s."
  }
]
```

#### 3) Consultar Acionamento por ID - `GET /api/v1/acionamentos/1` (Requer Bearer Token)
**Response (`200 OK`):**
```json
{
  "idAcionamento": 1,
  "idUsuario": 2,
  "idSemaforo": 1,
  "chaveIdempotencia": "cloud-prod-travessia-2026-001",
  "dtHora": "2026-11-11T18:58:17.0514376Z",
  "distanciaSensorM": 1.80,
  "condicaoClima": "CHUVA_MODERADA",
  "intensidadeChuvaMm": 14.50,
  "tempoExtraSeg": 22,
  "tempoBaseSeg": 16,
  "tempoTotalSeg": 38,
  "justificativaIa": "TravessIA Decision Engine: Perfil=IDOSO (+12s) | Clima=CHUVA_MODERADA (14.50mm) | LarguraVia=18.50m (+10s) | BonusTempestade=+0s | TempoBase=16s | TempoExtraConcedido=22s | TempoTotalTravessia=38s."
}
```

#### 4) Remover Acionamento - `DELETE /api/v1/acionamentos/1` (Requer Bearer Token `GESTOR`)
**Response:** `204 No Content`

---

### 6.5. Consulta Tabela 5: `TB_AUDITORIA_SEMAFORO` (`/api/v1/auditorias`)

#### Listar Registros de Auditoria - `GET /api/v1/auditorias` (Requer Bearer Token `GESTOR`)
**Response (`200 OK`):**
```json
[
  {
    "idAuditoria": 1,
    "idSemaforo": 1,
    "operacao": "UPDATE",
    "campoAlterado": "tempo_base_seg",
    "valorAntigo": "15",
    "valorNovo": "16",
    "usuarioBd": "gestor@travessia.sp.gov.br",
    "dtOperacao": "2026-11-11T18:58:16.8976937Z"
  }
]
```

---

## 7. Como Executar Localmente e no Docker

### Pre-requisitos
- .NET SDK 10.0 (`10.0.100` ou superior)
- Docker Desktop (opcional, para execucao conteinerizada)

### Passos para Execucao Local
1. Clone o repositorio e acesse a pasta raiz:
   ```bash
   git clone https://github.com/JoaoHBatistaLeal/TravessIA.git
   cd TravessIA
   ```
2. Restaure os pacotes e compile a solucao:
   ```bash
   dotnet restore TravessIA.sln
   dotnet build TravessIA.sln --configuration Release
   ```
3. Execute a suite completa de testes automatizados (Unitarios + Integracao `xUnit`):
   ```bash
   dotnet test TravessIA.sln --configuration Release
   ```
4. Inicie a API:
   ```bash
   dotnet run --project TravessIA.API/TravessIA.API.csproj
   ```
5. Acesse o Swagger UI em `http://localhost:5000/swagger` e o Health Check em `http://localhost:5000/health`.

### Execucao via Docker (`/dockerfiles/Dockerfile`)
```bash
docker build -f dockerfiles/Dockerfile -t travessia-api:latest .
docker run -d -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production travessia-api:latest
```

---

## 8. Declaracao Obrigatoria de Uso de Inteligencia Artificial Generativa

Em conformidade com as diretrizes academicas da FIAP para a Global Solution 2026/2, declaramos que ferramentas de Inteligencia Artificial Generativa foram utilizadas como assistentes de apoio durante o desenvolvimento deste projeto nas seguintes frentes:
1. **Revisao e Estruturacao de Codigo**: Apoio na refatoracao de camadas Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`), mapeamento Fluent API do Entity Framework Core e padronizacao de descricoes OpenAPI/Swagger.
2. **Automacao de Testes e Infraestrutura como Codigo (IaC)**: Auxilio na elaboracao de cenarios de teste `xUnit` (`WebApplicationFactory<Program>`) e scripts de provisionamento Azure CLI (`scripts/provision-azure.sh`) e pipeline YAML (`azure-pipeline.yml`).
3. **Supervisao e Validacao Humana**: Todas as regras de negocio de travessia inteligente, modelagem relacional das 5 tabelas (`script-bd.sql`), configuracoes de seguranca com Azure Key Vault (`DefaultAzureCredential`) e validacoes em nuvem foram integralmente revisadas, testadas e auditadas pelos integrantes do grupo, que assumem total responsabilidade tecnica e autoral pela solucao entregue.
