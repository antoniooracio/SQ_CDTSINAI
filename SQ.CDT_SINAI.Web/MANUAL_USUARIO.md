# Manual do Usuário - SQ.CDT_SINAI

Bem-vindo ao Sistema de Qualidade e Legalização (SQ.CDT_SINAI). Este manual guiará você através das principais funcionalidades do sistema.

## 1. Acesso ao Sistema

### Login
Para acessar o sistema, utilize suas credenciais (E-mail e Senha) fornecidas pelo administrador.

![Tela de Login](/images/manual/login.png)

1.  Acesse o endereço do sistema.
2.  Digite seu **E-mail** e **Senha**.
3.  Clique em **ACESSAR**.

> **Nota:** Se esqueceu sua senha, entre em contato com o administrador ou utilize a função de recuperação (se habilitada).

---

## 2. Dashboard (Visão Geral)

Ao entrar, você verá o **Dashboard**, que resume o estado atual da qualidade e legalização.

![Dashboard](/images/manual/dashboard.png)

*   **Alertas de Vencimento:** Cards coloridos no topo mostram documentos vencidos ou a vencer nos próximos dias.
*   **Indicadores:** Total de colaboradores, especializações e ocorrências.
*   **Minhas Ocorrências:** Lista rápida de incidentes atribuídos a você ou abertos por você.
*   **Gráfico:** Distribuição de ocorrências por gravidade.

---

## 3. Módulo de Qualidade (Ocorrências)

Este módulo gerencia incidentes, reclamações e elogios.

### Registrar Ocorrência Interna
Se você é um colaborador logado:
1.  No menu lateral, vá em **Qualidade > Nova Ocorrência** (ou use o botão "Ações Rápidas" no Dashboard).
2.  Preencha o formulário:
    *   **Tipo:** Elogio, Reclamação, etc.
    *   **Gravidade:** Baixa, Média ou Alta (define o prazo de resposta).
    *   **Área/Regional/Marca:** Dados de localização do incidente.
    *   **Alvo:** Quem deve receber/tratar essa ocorrência.
    *   **Descrição:** Detalhes do fato.
3.  Clique em **Registrar**.

### Ocorrências Externas
O público externo pode registrar ocorrências na tela de login (botão "Notificação Anônima"). Essas ocorrências aparecem no menu **Qualidade > Ocorrências Externas** para triagem.

### Responder Ocorrência
1.  Acesse a lista de ocorrências.
2.  Clique em **Responder** na ocorrência desejada.
3.  Descreva a solução ou tratativa e salve.

---

## 4. Módulo de Legalização

Gerencie a documentação regulatória das unidades (Alvarás, Licenças, AVCB).

### Consultar Estabelecimentos
1.  Vá em **Gestão de Legalização > Estabelecimentos**.
2.  Você verá uma lista com uma **Barra de Progresso** colorida indicando o status dos documentos daquela unidade.

### Gerenciar Documentos
1.  Na lista de estabelecimentos, clique no botão azul **Documentos**.
2.  Você verá a lista de documentos obrigatórios.
    *   **Vermelho:** Vencido.
    *   **Verde:** Em dia.
    *   **Amarelo:** Pendente.
3.  Para atualizar um documento (ex: novo Alvará):
    *   Clique em **Atualizar**.
    *   Faça o upload do arquivo PDF/Imagem.
    *   Defina a nova data de validade e status.
4.  **Relatório:** Clique em "Gerar Relatório" para baixar um PDF com o resumo da situação legal da unidade.

---

## 5. Módulo de Segurança

(Acesso restrito a Administradores e Gestores)

*   **Colaboradores:** Cadastre novos usuários, defina seus cargos e vincule-os a estabelecimentos específicos.
*   **Perfis de Acesso:** Crie perfis (ex: Auditor, Gerente) e defina permissões.
*   **Matriz de Permissões:** Marque exatamente o que cada perfil pode fazer no sistema.

---

## 6. Relatórios de Ocorrências

O sistema oferece uma ferramenta para análise de dados de qualidade através de gráficos e indicadores.

### Gerar Relatório
1.  No menu lateral, vá em **Qualidade > Relatórios**.
2.  Defina os filtros desejados:
    *   **Período:** Data inicial e final.
    *   **Filtros de Texto:** Regional, Marca, Unidade.
    *   **Filtros de Seleção Múltipla:** Permite selecionar vários itens para **Origem** (Interna/Externa), **Gravidade**, **Status** e **Categorias**.
3.  Clique em **Gerar Relatório**.

### Dashboard de Resultados
Uma nova aba será aberta exibindo:
*   **Totalizador:** Quantidade total de registros encontrados.
*   **Gráficos:** Status, Tipo de Ocorrência, Evolução Mensal, Ocorrências por Área e Tempo Médio de Resolução.
*   **Impressão:** Utilize o botão "Imprimir" no canto superior direito para salvar a visualização em PDF ou imprimir.