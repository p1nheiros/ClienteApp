using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Text.RegularExpressions;

namespace ClienteApp
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=clientes_db";

        private string currentNome;
        private decimal currentLimite;

        public Form1()
        {
            InitializeComponent();

            // Configuração inicial dos botões
            buttonSave.Enabled = false;
            buttonUpdate.Enabled = false;
            buttonDelete.Enabled = false;

            // Conecta os eventos aos botões e à grid
            buttonSave.Click += ButtonSave_Click;
            buttonUpdate.Click += ButtonUpdate_Click;
            buttonDelete.Click += ButtonDelete_Click;
            dataGridView1.CellClick += DataGridView1_CellClick;

            // Adiciona validações aos campos de entrada
            textBoxID.TextChanged += ValidateInputs;
            textBoxNome.TextChanged += ValidateInputs;
            textBoxLimite.TextChanged += ValidateInputs;

            // Adiciona restrições de entrada nos campos
            textBoxID.KeyPress += (s, e) => RestrictToNumbers(e);
            textBoxNome.KeyPress += (s, e) => RestrictToLetters(e);
            textBoxLimite.KeyPress += (s, e) => RestrictToDecimal(textBoxLimite, e);

            // Carrega os dados ao iniciar
            LoadDataGrid();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Deseja realmente salvar este registro?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                SaveRecord();
            }
        }

        private void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(textBoxNome.Text) || string.IsNullOrWhiteSpace(textBoxLimite.Text) || string.IsNullOrWhiteSpace(textBoxID.Text))
            {
                MessageBox.Show("Preencha todos os campos antes de salvar!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBoxLimite.Text, out _))
            {
                MessageBox.Show("Insira um valor numérico válido para o limite de crédito!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        var query = "INSERT INTO clientes (idcliente, nome_cliente, limite_credito) VALUES (@id, @nome, @limite)";
                        using (var command = new NpgsqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", int.Parse(textBoxID.Text));
                            command.Parameters.AddWithValue("@nome", textBoxNome.Text);
                            command.Parameters.AddWithValue("@limite", decimal.Parse(textBoxLimite.Text));
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                MessageBox.Show("Registro salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearFields();
                LoadDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar registro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxID.Text))
            {
                MessageBox.Show("Selecione um registro para atualizar!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBoxLimite.Text, out _))
            {
                MessageBox.Show("Insira um valor numérico válido para o limite de crédito!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Bloquear o registro para evitar alterações simultâneas
                            var lockQuery = "SELECT nome_cliente, limite_credito FROM clientes WHERE idcliente = @id FOR UPDATE NOWAIT";
                            using (var lockCommand = new NpgsqlCommand(lockQuery, connection, transaction))
                            {
                                lockCommand.Parameters.AddWithValue("@id", int.Parse(textBoxID.Text));
                                using (var reader = lockCommand.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        var nomeDb = reader.GetString(0);
                                        var limiteDb = reader.GetDecimal(1);

                                        if (nomeDb != currentNome || limiteDb != currentLimite)
                                        {
                                            MessageBox.Show("Os dados foram alterados por outro usuário. Operação cancelada.", "Conflito de Dados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            transaction.Rollback();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Registro não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        transaction.Rollback();
                                        return;
                                    }
                                }
                            }

                            // Confirmar update
                            var confirmResult = MessageBox.Show("Deseja realmente atualizar este registro?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (confirmResult == DialogResult.No)
                            {
                                transaction.Rollback();
                                return;
                            }

                            var query = "UPDATE clientes SET nome_cliente = @nome, limite_credito = @limite WHERE idcliente = @id";
                            using (var command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", int.Parse(textBoxID.Text));
                                command.Parameters.AddWithValue("@nome", textBoxNome.Text);
                                command.Parameters.AddWithValue("@limite", decimal.Parse(textBoxLimite.Text));
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            MessageBox.Show("Registro atualizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            LoadDataGrid();
                        }
                        catch (PostgresException ex) when (ex.SqlState == "55P03") // 55P03 = Lock not available
                        {
                            MessageBox.Show("Este registro está sendo alterado por outro usuário. Tente novamente mais tarde.", "Conflito de Dados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erro ao atualizar registro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            transaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar registro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Deseja realmente excluir este registro?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                DeleteRecord();
            }
        }

        private void DeleteRecord()
        {
            if (string.IsNullOrWhiteSpace(textBoxID.Text))
            {
                MessageBox.Show("Selecione um registro para excluir!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        // Bloquear o registro para evitar alterações simultâneas
                        var lockQuery = "SELECT * FROM clientes WHERE idcliente = @id FOR UPDATE";
                        using (var lockCommand = new NpgsqlCommand(lockQuery, connection, transaction))
                        {
                            lockCommand.Parameters.AddWithValue("@id", int.Parse(textBoxID.Text));
                            lockCommand.ExecuteNonQuery();
                        }

                        var query = "DELETE FROM clientes WHERE idcliente = @id";
                        using (var command = new NpgsqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", int.Parse(textBoxID.Text));
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                MessageBox.Show("Registro excluído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearFields();
                LoadDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir registro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridView1.Rows[e.RowIndex];

                // Verificar se o registro é novo
                if (row.Cells["idcliente"].Value == null || string.IsNullOrWhiteSpace(row.Cells["idcliente"].Value.ToString()))
                {
                    ClearFields();
                    buttonSave.Enabled = false;
                    buttonUpdate.Enabled = false;
                    buttonDelete.Enabled = false;
                    return;
                }

                try
                {
                    textBoxID.Text = row.Cells["idcliente"].Value.ToString();
                    textBoxNome.Text = row.Cells["nome_cliente"].Value.ToString();

                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var query = "SELECT limite_credito FROM clientes WHERE idcliente = @id";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@id", int.Parse(textBoxID.Text));
                            textBoxLimite.Text = command.ExecuteScalar()?.ToString();
                        }
                    }

                    currentNome = textBoxNome.Text;
                    currentLimite = decimal.Parse(textBoxLimite.Text);

                    buttonSave.Enabled = false;
                    buttonUpdate.Enabled = true;
                    buttonDelete.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao processar o registro selecionado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ValidateInputs(object sender, EventArgs e)
        {
            buttonSave.Enabled = !string.IsNullOrWhiteSpace(textBoxID.Text) &&
                                 !string.IsNullOrWhiteSpace(textBoxNome.Text) &&
                                 !string.IsNullOrWhiteSpace(textBoxLimite.Text);
        }

        private void RestrictToNumbers(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void RestrictToLetters(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void RestrictToDecimal(TextBox textBox, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            // Permitir apenas uma vírgula
            if (e.KeyChar == ',' && textBox.Text.Contains(","))
            {
                e.Handled = true;
            }
        }

        private void ClearFields()
        {
            textBoxID.Clear();
            textBoxNome.Clear();
            textBoxLimite.Clear();

            buttonSave.Enabled = false;
            buttonUpdate.Enabled = false;
            buttonDelete.Enabled = false;
        }

        private void LoadDataGrid()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT idcliente, nome_cliente, limite_credito FROM clientes";
                    var adapter = new NpgsqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    dataGridView1.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}