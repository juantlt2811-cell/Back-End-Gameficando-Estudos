# Gameficando Estudos — Back-End

API de uma plataforma de estudos gamificada: professores cadastram matérias e sobem material de estudo (PDF, DOCX, texto, código); o sistema usa RAG (busca vetorial + LLM) para gerar atividades automaticamente a partir desse material; alunos respondem atividades em sessões de prática e acumulam uma "ofensiva" (streak) diária.

> O front-end (React + Vite) vive em um repositório separado e consome esta API.

## Stack

- **.NET 10** / ASP.NET Core Web API
- **PostgreSQL** com a extensão **pgvector**, hospedado no [Supabase](https://supabase.com/)
- **Entity Framework Core** (Npgsql) para acesso a dados e migrations
- **JWT** (access token curto + refresh token com rotação) para autenticação
- **Argon2id** para hash de senha
- **Google Gemini** (`gemini-embedding-001`) para embeddings
- **Groq** (`openai/gpt-oss-20b`) para geração de atividades e avaliação de respostas dissertativas
- **SendGrid (SMTP)** para e-mails transacionais (redefinição de senha)
- **xUnit + Moq + FluentAssertions** para testes

## Arquitetura

O projeto segue Clean Architecture, com a dependência sempre apontando para dentro:

```
src/
  GameficandoEstudos.Domain/          Entidades, enums e invariantes de negócio. Sem dependências externas.
  GameficandoEstudos.Application/     Casos de uso (Services), DTOs e interfaces (Application define, Infrastructure implementa).
  GameficandoEstudos.Infrastructure/  EF Core, repositórios, integrações externas (LLM, e-mail, extração de texto), migrations.
  GameficandoEstudos.Api/             Controllers, autenticação, configuração (Program.cs), Swagger.
tests/
  GameficandoEstudos.Application.Tests/      Testes unitários dos Services (com mocks das interfaces).
  GameficandoEstudos.Infrastructure.Tests/    Testes de componentes de infraestrutura.
```

### Principais entidades de domínio

| Entidade | Responsabilidade |
|---|---|
| `Usuario` | Aluno ou Professor; dono do `PerfilGamificacao`; suporta redefinição de senha e anonimização (LGPD). |
| `PerfilGamificacao` | Ofensiva atual/recorde e limite diário de atividades (3/dia). |
| `Materia` | Matéria cadastrada por um professor. |
| `Modulo` | Divisão opcional de uma matéria (ex.: "Condicionais", "Laços"). Agrupa atividades e materiais; o que não tem módulo fica em "Sem módulo" e vale para a matéria inteira. |
| `CaixaFontes` / `FonteChunks` | Material de estudo enviado pelo professor, já dividido em chunks com embedding. |
| `Atividade` | Questão gerada por IA ou criada manualmente — `Conteudo` (enunciado) e `Alternativas` são separados, sem precisar de parsing no cliente. |
| `SessaoPratica` / `TentativaResposta` | Uma sessão de prática do aluno numa matéria (ou num módulo dela) e cada resposta dada dentro dela. A sessão guarda os ids das atividades sorteadas, e só elas podem ser respondidas. |
| `RefreshToken` | Token opaco (hash SHA-256) com rotação, usado no fluxo de refresh/logout. |
| `LogAuditoria` | Trilha de auditoria de ações sensíveis; não tem FK para `Usuario` de propósito, para sobreviver à anonimização da conta. |

## Autenticação

- Login devolve um **access token** JWT de curta duração (15 min, configurável) e um **refresh token** opaco (7 dias, configurável), com rotação a cada uso.
- `POST /api/auth/refresh` troca um refresh token válido por um novo par de tokens; o refresh usado é revogado.
- `POST /api/auth/logout` revoga o refresh token atual.
- Senhas usam Argon2id (`IPasswordService`); redefinição de senha usa token hash + expiração, comparado com `CryptographicOperations.FixedTimeEquals`.
- Login tem tentativas limitadas (`ILoginAttemptGuard`) e os endpoints de autenticação têm rate limiting (`AddRateLimiting`, policy `auth`).

## Endpoints

Todos os endpoints (exceto `auth/cadastrar` e `auth/login`) exigem `Authorization: Bearer <token>`. Onde indicado, exigem também o papel (`Professor` ou `Aluno`).

### Auth (`/api/auth`)
| Método | Rota | Descrição |
|---|---|---|
| POST | `cadastrar` | Cria um novo usuário (Aluno ou Professor). |
| POST | `login` | Autentica e devolve access + refresh token. |
| POST | `esqueci-senha` | Dispara e-mail de redefinição de senha. |
| POST | `redefinir-senha` | Define nova senha a partir do token recebido por e-mail. |
| PUT | `alterar-senha` | Troca a senha do usuário autenticado. |
| POST | `refresh` | Troca um refresh token válido por um novo par. |
| POST | `logout` | Revoga o refresh token atual. |

### Matéria (`/api/materia`)
| Método | Rota | Papel | Descrição |
|---|---|---|---|
| POST | `criar` | Professor | Cria uma matéria. |
| PUT | `atualizar` | Professor | Atualiza uma matéria. |
| DELETE | `{id}` | Professor | Remove uma matéria. |
| GET | `{id}` | — | Detalhe de uma matéria. |
| GET | `minhas` | Professor | Matérias do professor logado. |
| GET | `disponiveis` | — | Matérias disponíveis para alunos. |
| GET | `pesquisar` | — | Busca de matérias. |
| GET | `{id}/estatisticas` | Professor | Estatísticas de desempenho dos alunos na matéria. |

### Módulo (`/api/modulo`)
| Método | Rota | Papel | Descrição |
|---|---|---|---|
| GET | `materia/{idMateria}` | — | Lista os módulos da matéria, com quantidade de atividades ativas e de materiais de cada um. |
| POST | (raiz) | Professor | Cria um módulo: `{ idMateria, nome }`. Nome único por matéria. |
| PUT | `{id}` | Professor | Renomeia um módulo: `{ nome }`. |
| DELETE | `{id}` | Professor | Exclui o módulo. Atividades e materiais dele voltam para "Sem módulo" (nada é apagado). |

### Caixa de Fontes (`/api/caixafontes`) — todos exigem papel Professor
| Método | Rota | Descrição |
|---|---|---|
| POST | `processar-texto` | Processa um texto colado diretamente. Aceita `idModulo` opcional. |
| POST | `upload` | Sobe um arquivo (PDF, DOCX, texto ou código) como material de estudo. Aceita o campo de formulário `idModulo` opcional. |
| PUT | `mover-material` | Move um material já enviado para outro módulo (ou para nenhum, com `idModulo: null`), sem reprocessar o arquivo. |
| GET | `materia/{idMateria}/materiais` | Lista o material de estudo de uma matéria, com o `idModulo` de cada um. |
| DELETE | `remover-material` | Remove um material de estudo (e seus chunks). |

### Atividade (`/api/atividade`)
| Método | Rota | Papel | Descrição |
|---|---|---|---|
| GET | `materia/{idMateria}` | — | Lista atividades de uma matéria. |
| POST | `gerar` | Professor | Gera uma atividade via RAG + LLM a partir do material da matéria (ou só do módulo, se `idModulo` for informado). |
| PUT | `{id}/modulo` | Professor | Move a atividade para outro módulo: `{ idModulo }` (`null` = "Sem módulo"). |
| PUT | `atualizar` | Professor | Edita uma atividade existente. |
| DELETE | `{id}` | Professor | Remove (ou inativa, se já tiver respostas) uma atividade. |
| POST | `{id}/testar` | Professor | Testa uma atividade com uma resposta, sem contar como tentativa de aluno. |

### Sessão Prática (`/api/sessaopratica`) — todos exigem papel Aluno
| Método | Rota | Descrição |
|---|---|---|
| GET | `confirmar-inicio/{idMateria}?idModulo=&tamanho=` | Confirma se o aluno pode iniciar uma sessão (respeita o limite diário) e quantas questões ela terá. |
| POST | `iniciar/{idMateria}?idModulo=&tamanho=` | Inicia uma sessão de prática com questões sorteadas do módulo (ou da matéria inteira, sem `idModulo`). |
| POST | `{idSessao}/responder/{idAtividade}` | Envia a resposta de uma questão da sessão. |
| POST | `{idSessao}/concluir` | Conclui a sessão e atualiza a ofensiva. |

`tamanho` aceita `Curta` (5), `Media` (10), `Longa` (20) ou `Todas` (também como 1 a 4). Sem query string, a sessão continua como antes: 5 questões da matéria inteira. O limite diário (3 sessões concluídas por dia) não muda com o tamanho.

### Ranking (`/api/ranking`)
| Método | Rota | Descrição |
|---|---|---|
| GET | `ofensiva` | Ranking global por ofensiva. |
| GET | `materia/{idMateria}` | Ranking de % de acerto numa matéria. |

### Usuário (`/api/usuario`) — direitos LGPD
| Método | Rota | Descrição |
|---|---|---|
| GET | `meus-dados` | Exporta os dados pessoais do usuário autenticado. |
| DELETE | `minha-conta` | Anonimiza a conta (direito ao esquecimento) — exige confirmação de senha. |

## Geração de atividades (RAG)

1. O professor chama `POST /api/atividade/gerar` com `idMateria`, `tema`, `tipoAtividade` e, opcionalmente, `idModulo`.
2. O texto é embeddado (`gemini-embedding-001`, truncado para 768 dimensões) e os chunks mais relevantes são buscados por distância de cosseno no pgvector — só no material do módulo, quando há módulo, ou em todo o material da matéria. Chunks acima do limiar de distância (`LimiarDistanciaMaxima` em `VectorSearchService`) são descartados; se nenhum sobrar, a geração é recusada sem chamar o LLM.
3. Esses chunks viram contexto de um prompt para o Groq (`openai/gpt-oss-20b`), que deve usar **apenas** esse conteúdo e responder em JSON estruturado: `{ enunciado, alternativas, gabarito, temaForaDoConteudo }`. Se o tema pedido não estiver no material, o LLM marca `temaForaDoConteudo` e a API devolve erro em vez de inventar uma questão.
4. Para múltipla escolha, o backend valida que vieram pelo menos 2 alternativas e **embaralha a ordem delas**, recalculando a letra do gabarito — isso remove o viés de posição do LLM (tendência de sempre colocar a resposta certa na mesma letra).
5. A atividade é salva com `Conteudo` (só o enunciado) e `Alternativas` separados, para o cliente não precisar fazer parsing de texto.

## Segurança e LGPD

- Controle de acesso por papel (`[Authorize(Roles = "...")]`) em todas as operações de escrita sensíveis.
- Checagem de propriedade (anti-IDOR) em matéria/atividade/caixa de fontes: um professor só altera o que é dele.
- Rate limiting dedicado para login (`auth`) e para endpoints que chamam LLM (`ia`).
- HSTS fora de ambiente de desenvolvimento, handler de exceção global (não vaza stack trace), CORS restrito a origens configuradas.
- Log de auditoria para ações sensíveis, sem vínculo de FK com `Usuario` (sobrevive à anonimização).
- Exportação de dados pessoais e anonimização de conta (direito de acesso e ao esquecimento).

## Rodando localmente

### Pré-requisitos
- .NET 10 SDK
- Um banco PostgreSQL com a extensão `vector` habilitada (o projeto usa o Supabase, mas qualquer Postgres com pgvector funciona)
- Chaves de API: Google Gemini (embeddings) e Groq (geração/avaliação)
- Uma conta SMTP (ex.: SendGrid) para e-mails — opcional em desenvolvimento, onde `FakeEmailService` é usado

### Configuração

Nunca commite segredos em `appsettings.json`. Use o `dotnet user-secrets` a partir da pasta `src/GameficandoEstudos.Api`:

```bash
cd src/GameficandoEstudos.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=..."
dotnet user-secrets set "Jwt:ChaveSecreta" "uma-chave-bem-grande-e-aleatoria"
dotnet user-secrets set "LlmApi:GroqKey" "sua-chave-groq"
dotnet user-secrets set "LlmApi:EmbeddingsKey" "sua-chave-google-gemini"
dotnet user-secrets set "Smtp:Senha" "sua-api-key-do-sendgrid"
```

As demais configurações (emissor/duração do JWT, host/porta do SMTP, origens de CORS) ficam em `appsettings.json` / `appsettings.Development.json` e podem ser sobrescritas por ambiente.

### Banco de dados

```bash
dotnet ef database update --project src/GameficandoEstudos.Infrastructure --startup-project src/GameficandoEstudos.Api
```

### Executando a API

```bash
dotnet run --project src/GameficandoEstudos.Api
```

O Swagger fica disponível em `/swagger` no ambiente de desenvolvimento.

### Testes

```bash
dotnet test
```

## Convenções do projeto

- Nomes de domínio, DTOs, métodos e mensagens de erro em **português** (o domínio do negócio é educação no Brasil).
- Soft delete: uma atividade só é apagada de verdade se nunca recebeu resposta (`PossuiRespostasAsync`); caso contrário é apenas inativada (`Inativar()`), preservando o histórico de quem já respondeu.
- Regra de cascade nas FKs: `Restrict` para dados históricos/de auditoria que precisam sobreviver à remoção do "dono" (ex.: `SessaoPratica → Usuario`, `TentativaResposta → Atividade`); `Cascade` só para artefatos de sessão que devem mesmo desaparecer com o dono (ex.: `RefreshToken → Usuario`); `LogAuditoria` não tem FK nenhuma para `Usuario`, de propósito, para sobreviver à anonimização.
