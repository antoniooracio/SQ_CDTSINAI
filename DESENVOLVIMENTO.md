# Manual Técnico e de Desenvolvimento - SQ.CDT_SINAI

## Visão Geral
Este documento serve como guia definitivo para entender, manter e evoluir o sistema **SQ.CDT_SINAI**. O projeto é um sistema de Gestão de Qualidade e Legalização para rede hospitalar, focado em controle de incidentes, conformidade legal de estabelecimentos e gestão de acessos.

## 1. Arquitetura e Tecnologias
O sistema segue uma arquitetura de microsserviços simplificada (Frontend e Backend separados), rodando em containers.

- **Linguagem:** C# (.NET 9.0)
- **Frontend (Web):** ASP.NET Core MVC com Razor Views. Utiliza o template **AdminLTE 3** (Bootstrap 4) para a interface administrativa.
- **Backend (API):** ASP.NET Core Web API. Centraliza toda a regra de negócio, acesso a dados e segurança.
- **Banco de Dados:** MariaDB 10.6 (via Docker).
- **ORM:** Entity Framework Core (Code-First).
- **Autenticação:** Híbrida. JWT para a API e Cookie Authentication para o Frontend.
- **Relatórios:** QuestPDF para geração de documentos PDF.
- **Infraestrutura:** Docker Compose orquestrando Web, API e Banco.

## 2. Estrutura da Solução

### `SQ.CDT_SINAI.Shared`
Biblioteca de classes compartilhada entre Web e API. Evita duplicação de código.
- **DTOs:** Objetos de transferência de dados (ex: `LoginDto`, `IncidentDto`).
- **Models:** Entidades do banco de dados (ex: `Collaborator`, `Incident`, `Establishment`).
- **Validations:** Atributos de validação customizados (ex: validação de CPF).

### `SQ.CDT_SINAI.API` (Backend)
O "cérebro" do sistema.
- **Controllers:** Expostos via HTTP REST.
- **Data:** Contexto do EF Core (`AppDbContext`) e Migrations.
- **Segurança:** Validação de Tokens JWT e Policies.
- **HealthChecks:** Monitoramento de saúde (`/health`).

### `SQ.CDT_SINAI.Web` (Frontend)
A interface visual.
- **Services:** Camada que consome a API usando `HttpClient`.
- **Controllers:** Recebem a interação do usuário e chamam os Services.
- **Views:** Telas Razor (.cshtml).
- **Filtros:** `PermissionFilter` para controle de acesso granular nas telas.

## 3. Documentação Técnica dos Módulos (Controllers)

Abaixo, explicamos a responsabilidade de cada Controller principal no sistema.

### Módulo de Autenticação e Segurança
| Controller (Web) | Controller (API) | Função |
| :--- | :--- | :--- |
| `AccountController` | `AuthController` | Gerencia Login, Logout e "Esqueci minha senha". No Web, cria o Cookie de sessão. Na API, gera o JWT. |
| `CollaboratorController` | `CollaboratorController` | CRUD de usuários. Gerencia dados pessoais, endereço (ViaCEP) e vínculos (Perfil/Estabelecimentos). |
| `RoleController` | `RoleController` | CRUD de Perfis de Acesso (ex: Admin, Coordenador). |
| `PermissionController` | `PermissionController` | Gerencia a **Matriz de Permissões**. Permite definir o que cada perfil pode fazer (Visualizar, Criar, Editar) em cada módulo. |

### Módulo de Qualidade (Incidentes)
| Controller (Web) | Controller (API) | Função |
| :--- | :--- | :--- |
| `IncidentController` | `IncidentController` | Centraliza a gestão de ocorrências. <br> - **Externas:** Abertas via tela de login (público). <br> - **Internas:** Abertas por usuários logados. <br> - **SLA:** Calcula prazos automaticamente (Baixa: 72h, Média: 48h, Alta: 24h). |

### Módulo de Relatórios
| Controller (Web) | Controller (API) | Função |
| :--- | :--- | :--- |
| `ReportController` | `ReportController` | Gera dashboards analíticos de ocorrências. Suporta filtros avançados (múltipla seleção) e exibe gráficos via Chart.js. |

### Módulo de Legalização (Compliance)
| Controller (Web) | Controller (API) | Função |
| :--- | :--- | :--- |
| `EstablishmentController` | `EstablishmentController` | CRUD de Estabelecimentos. É o ponto de entrada para a tela de gestão de documentos (`Legalization`). |
| - | `LegalizationController` | Gerencia o upload de arquivos, download, controle de vencimentos e geração de relatórios PDF. |
| `DocumentTypeController` | `DocumentTypeController` | CRUD de Tipos de Documentos (ex: Alvará, AVCB). |
| `BrandController` | `BrandController` | CRUD de Marcas (ex: Sinai, CDT). |
| `EstablishmentTypeController` | `EstablishmentTypeController` | CRUD de Tipos de Unidade (ex: Hospital, Clínica). |

### Cadastros Gerais
| Controller (Web) | Controller (API) | Função |
| :--- | :--- | :--- |
| `SpecializationController` | `SpecializationController` | CRUD de Especializações médicas/técnicas. |

## 4. Guia de Menus e Navegação

O menu lateral do sistema está organizado por áreas funcionais:

1.  **Dashboard (`Home`)**
    *   Visão geral com gráficos de ocorrências.
    *   Cards de alerta para documentos vencidos ou a vencer.
    *   Lista de "Minhas Ocorrências" pendentes.

2.  **Módulo Segurança**
    *   **Colaboradores:** Listagem e cadastro de usuários. Aqui define-se o Perfil e os Estabelecimentos que o usuário pode acessar.
    *   **Perfis de Acesso:** Criação de roles (ex: "Gerente", "Auditor").
    *   **Matriz de Permissões:** Tela avançada para marcar/desmarcar o que cada perfil pode fazer. Possui função de "Copiar Permissões".

3.  **Módulo Qualidade**
    *   **Ocorrências Externas:** Listagem de todas as ocorrências abertas pelo público externo. Permite responder e atribuir responsáveis.

4.  **Módulo Gestão de Legalização**
    *   **Estabelecimentos:** Cadastro das unidades. Na listagem, há um botão "Documentos" que leva à tela de gestão de arquivos.
    *   **Marcas / Tipos de Estabelecimento / Tipos de Documentos:** Cadastros auxiliares para categorização.

5.  **Cadastros Gerais**
    *   **Especializações:** Cadastro de áreas médicas/técnicas.

## 5. Como Executar (Desenvolvimento)

### Pré-requisitos
- Docker Desktop instalado e rodando.

### Passo a Passo
1.  Abra o terminal na raiz da solução.
2.  Execute o comando para construir e subir os containers:
    ```bash
    docker-compose up --build
    ```
3.  Aguarde os logs indicarem que a API e o Web estão rodando.
4.  Acesse:
    - **Site:** http://localhost:5002
    - **API (Swagger):** http://localhost:5001/swagger
    - **HealthCheck:** http://localhost:5001/health

### Comandos Úteis
- **Parar:** `docker-compose down`
- **Limpar Cache de Build:** `docker builder prune -f`
- **Recriar Forçado:** `docker-compose up -d --build --force-recreate`

## 6. Banco de Dados e Migrations
O sistema usa **Entity Framework Core Code-First**.

- **Migrations Automáticas:** A API tenta aplicar as migrations pendentes toda vez que inicia.
- **Criar Nova Migration:** Se você alterar um Model, deve criar uma migration:
  ```bash
  dotnet ef migrations add NomeDaMudanca --project SQ.CDT_SINAI.API --startup-project SQ.CDT_SINAI.API
  ```
- **Aplicar:** Basta reiniciar o container da API.

## 7. Histórico de Versões e Funcionalidades

### v1.0 - Fundação
- Estrutura Web + API + Shared.
- Autenticação JWT e Login.
- Cadastro de Colaboradores e Especializações.

### v1.1 - Qualidade (Incidentes)
- Abertura de Ocorrências Externas (Anônimas) e Internas.
- Dashboard com gráficos e KPIs.
- SLA automático por gravidade.

### v1.2 - Legalização
- Cadastro de Estabelecimentos, Marcas e Tipos.
- Gestão de Documentos (Upload, Download, Status).
- Relatórios PDF com QuestPDF.
- Dashboard de Vencimentos.

### v1.3 - Segurança Avançada (Atual)
- Implementação de RBAC (Roles e Permissions).
- Matriz de Permissões dinâmica.
- Escopo de Dados (Usuário só vê documentos do seu estabelecimento).
- Auditoria e melhorias de UX (Select All, Copiar Permissões).

### v1.4 - Automação e Auditoria (Atual)
- **Renovação Automática:** Worker Service que verifica diariamente documentos vencidos configurados para renovação.
- **Agendamento:** Executa na inicialização do container e agendado para rodar todo dia às 02:00 AM.
- **Histórico:** Tela de logs para auditar todas as renovações automáticas realizadas pelo sistema.
- **Reversão:** Funcionalidade para desfazer uma renovação automática incorreta, restaurando a data anterior.

### v1.5 - Relatórios Gerenciais (Atual)
- Novo módulo de Relatórios de Ocorrências.
- Filtros avançados com múltipla seleção (Select2) para Categorias, Status, Gravidade e Tipo.
- Dashboard interativo com gráficos (Pizza, Barra, Linha) utilizando Chart.js.
- Indicadores de SLA (Tempo Médio) e volumetria por área/mês.

### Lógica do Relatório de Legalização
O relatório de documentos legais segue uma lógica "inteligente". Ele exibe apenas os documentos que são **obrigatórios** (configurados no Tipo de Estabelecimento) para as unidades filtradas.
- **Objetivo:** Evitar poluição visual com documentos irrelevantes para o tipo de unidade.
- **Comportamento:** Se um Tipo de Estabelecimento não tiver documentos configurados como necessários (Abertura ou Encerramento), o relatório não exibirá pendências, entendendo que "não há nada a cobrar".

## 8. Dicionário de Dados (Enums e Listas)

Para facilitar a manutenção e adição de novos itens nas listas suspensas (dropdowns), abaixo está o mapeamento de onde cada Enum está declarado e onde é utilizado no sistema.

### Localização dos Arquivos
Todos os Enums estão localizados no projeto **Shared**, dentro da pasta `Models`.

#### 1. Ocorrências (`SQ.CDT_SINAI.Shared/Models/Incident.cs`)
Define as opções utilizadas nos formulários de abertura e filtro de ocorrências.

| Enum | Descrição | Onde é usado (Views) |
| :--- | :--- | :--- |
| `ClientType` | Tipo de Cliente (Médico, Paciente, Fornecedor...) | `Account/Login.cshtml` (Externa)<br>`Incident/Create.cshtml` (Interna) |
| `ContactMethod` | Meio de Contato (Email, WhatsApp, Telefone...) | `Account/Login.cshtml`<br>`Incident/Create.cshtml` |
| `IncidentCategory` | Tipo de Ocorrência (Elogio, Reclamação...) | `Account/Login.cshtml`<br>`Incident/Create.cshtml` |
| `IncidentSeverity` | Gravidade (Baixa, Média, Alta) | `Incident/Create.cshtml`<br>`Home/Index.cshtml` (Dashboard) |
| `IncidentStatus` | Status (Aberto, Respondido, Fechado) | `Incident/ExternalIndex.cshtml`<br>`Home/Index.cshtml` |

#### 2. Legalização (`SQ.CDT_SINAI.Shared/Models/EstablishmentType.cs`)
| Enum | Descrição | Onde é usado (Views) |
| :--- | :--- | :--- |
| `ServiceLocationType` | Tipo de Local (Unidade, NTO, Suporte...) | `EstablishmentType/Create.cshtml`<br>`EstablishmentType/Edit.cshtml`<br>`EstablishmentType/Index.cshtml` |

#### 3. Documentos (`SQ.CDT_SINAI.Shared/Models/EstablishmentDocument.cs`)
| Enum | Descrição | Onde é usado (Views) |
| :--- | :--- | :--- |
| `DocumentStatus` | Situação (Pendente, Emitido, Vencido...) | `Establishment/Legalization.cshtml` |

## 9. Publicação e Deploy (Produção)

Para publicar o sistema em um servidor de produção (IIS, Linux com Nginx, ou Docker), siga os passos abaixo para gerar os arquivos compilados.

### 1. Gerar os Arquivos de Publicação
Abra o terminal na raiz da solução e execute os comandos para compilar a API e o Site:

```bash
# Publicar a API
dotnet publish SQ.CDT_SINAI.API/SQ.CDT_SINAI.API.csproj -c Release -o ./publish/api

# Publicar o Site (Web)
dotnet publish SQ.CDT_SINAI.Web/SQ.CDT_SINAI.Web.csproj -c Release -o ./publish/web
```

### 2. O que copiar para o Servidor?
Após executar os comandos acima, você terá uma pasta `publish` na raiz do projeto.
*   Copie todo o conteúdo de `./publish/api` para a pasta da API no servidor (ex: `C:\inetpub\wwwroot\sinai-api` ou `/var/www/sinai-api`).
*   Copie todo o conteúdo de `./publish/web` para a pasta do Site no servidor (ex: `C:\inetpub\wwwroot\sinai-web` ou `/var/www/sinai-web`).

### 3. Configurações Importantes (appsettings.json)
Antes de rodar, edite o arquivo `appsettings.json` em cada pasta no servidor para apontar para o ambiente de produção:

**Na API (`publish/api/appsettings.json`):**
*   `ConnectionStrings:DefaultConnection`: Aponte para o banco de produção.
*   `Jwt:Key`: Use uma chave forte e segura.

**No Site (`publish/web/appsettings.json`):**
*   `ApiSettings:BaseUrl`: Aponte para a URL onde a API está rodando (ex: `https://api.sinai.com.br/`).

---
*Última atualização: Reversão de Renovações.*