<div align="center">
  
  [![Gmail](https://img.shields.io/badge/Gmail-D14836?style=for-the-badge&logo=gmail&logoColor=white)](mailto:pinheiros.dev@gmail.com)
  [![LinkedIn](https://img.shields.io/badge/linkedin-%230077B5.svg?style=for-the-badge&logo=linkedin&logoColor=white)](https://linkedin.com/in/lucas-p-5b1585265)
  [![YouTube](https://img.shields.io/badge/YouTube-%23FF0000.svg?style=for-the-badge&logo=YouTube&logoColor=white)](https://www.youtube.com/@PinheirosDev)

  <br />
  <br />

  <h1 align="center">ClienteApp - Sistema de Gerenciamento de Clientes</h1>

  > Este é um sistema de gerenciamento de clientes (CRUD - Create, Read, Update, Delete) desenvolvido em C#. Ele permite cadastrar, listar, atualizar e excluir registros de clientes, com validações robustas e controle de concorrência utilizando PostgreSQL.

  <a href="https://github.com/p1nheiros/crud-clientes"><strong>➥ Visualizar Projeto</strong></a>

</div>

<br />



### 🛠️ Funcionalidades

- **Cadastro de Clientes:** Insira ID, Nome e Limite de Crédito.
- **Listagem de Clientes:** Exibe os registros cadastrados em uma tabela com suporte à ordenação por colunas.
- **Atualização de Dados:** Permite alterar informações de um cliente existente, com confirmação do usuário.
- **Exclusão de Clientes:** Remove clientes cadastrados, após confirmação.
- **Controle de Concorrência:** Bloqueia registros em edição para evitar alterações simultâneas.

### 📂 Estrutura do Projeto

- **Frontend:** Desenvolvido com Windows Forms para interface gráfica.
- **Backend:** Lógica de negócios e transações utilizando PostgreSQL.

### 📋 Pré-requisitos

Antes de começar, verifique se você atendeu aos seguintes requisitos:

* [Git](https://git-scm.com/downloads "Download Git") deve estar instalado em seu sistema operacional.
- .NET Framework instalado.
- PostgreSQL configurado e acessível.

### 📍 Executar localmente

Para executar o repositório localmente, execute este comando no seu git bash:

Linux and macOS:

```bash
sudo git clone https://github.com/p1nheiros/crud-clientes.git
```

Windows:

```bash
git clone https://github.com/p1nheiros/crud-clientes.git
```

### 🚀 Como Usar

1. **Baixar o Executável**
   - Baixe o arquivo `ClienteApp.exe` clicando [aqui](code/ClienteApp/ClienteApp/bin/Debug/ClienteApp.exe).
   - Ou clone o repositório e compile no Visual Studio.

2. **Configurar Banco de Dados**
   - Certifique-se de que o PostgreSQL está instalado e rodando.
   - Altere a lihha de conexão com o Postgres com suas credenciais:
     ```csharp
     private readonly string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=clientes_db";
     ```
   - Crie o banco de dados:
     ```sql
     CREATE DATABASE clientes_db;
     ```
   - Crie a tabela `clientes`:
     ```sql
     CREATE TABLE clientes (
         idcliente INT PRIMARY KEY,
         nome_cliente VARCHAR(100) NOT NULL,
         limite_credito DECIMAL(10, 2) NOT NULL
     );
     ```

3. **Executar a Aplicação**
   - Abra o arquivo `ClienteApp.exe`.
   - Insira os dados no formulário e use os botões para realizar as operações.

### 👨‍💻 Desenvolvedor

<table>
  <tr>
    <td>
      <a href="#">
        <img src="https://avatars.githubusercontent.com/u/124714182?v=4" width="100px;" alt="Lucas Pinheiro"/><br>
        <sub>
          <b>Lucas Pinheiro</b>
        </sub>
      </a>
    </td>
  </tr>
</table>

### ☎️ Contato

Se você quiserentrar em contato comigo, pode me encontrar no [Gmail](mailto:pinheiros.dev@gmail.com).

### 📝 Licença

Esse projeto está sob licença. Veja o arquivo [LICENÇA](LICENSE.md) para mais detalhes.

[⬆ Voltar ao topo](README.md)<br>
