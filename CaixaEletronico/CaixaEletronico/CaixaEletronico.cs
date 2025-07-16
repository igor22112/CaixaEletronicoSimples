using System;
public class CaixaEletronico
{
    public Dictionary<int, int> cedulas;   
    private List<INotificador> notificadores;
    
    public CaixaEletronico()
    {   //Dicionario para armazenar tipos de cedulas e quantidade
        cedulas = new Dictionary<int, int> {
                { 200, 2 },
                { 100, 1 },
                { 50, 0 },
                { 20, 0 },
                { 10, 0 },
                { 5, 0 },
                { 2, 0} 
            };
        notificadores = new List<INotificador>(1);
    }
    public void ExibirMenu()
    {
        // Exibe o menu principal do caixa eletrônico
        Console.Clear();
        Console.WriteLine("Bem-vindo ao Caixa Eletrônico!");
        Console.WriteLine("1. Consultar Saldo");
        Console.WriteLine("2. Sacar Dinheiro");
        Console.WriteLine("3. Depositar Dinheiro");
        Console.WriteLine("4. Exibir diretório do Log de Ações"); // mostra onde ta o arquivo de log
        Console.WriteLine("5. Sair");
        
        int opcaoMenu = int.Parse(Console.ReadLine()!);
        if (opcaoMenu < 1 || opcaoMenu > 4)
        {
            Console.WriteLine("Opção inválida. Tente novamente.");
            ExibirMenu();
            return;
        }
        notificadores.Add(new NotificadorArquivo());
        switch (opcaoMenu)
        {
            // recusividade para voltar ao menu
            case 1:
                ConsultarSaldo();
                ExibirMenu();
                break;
            case 2:
                SacarDinheiro();
                ExibirMenu();
                break;
            case 3:
                DepositarDinheiro();
                ExibirMenu();
                break;
            case 4:
                Console.Clear();
                Console.WriteLine(System.IO.Path.GetFullPath("log_caixa.txt"));
                Console.WriteLine("Tecle ENTER para voltar ao menu");
                Console.ReadLine();
                ExibirMenu();
                break;
            case 5:
                Console.WriteLine("Obrigado por usar o Caixa Eletrônico!");
                return;
        }
    }
    private void SacarDinheiro()
    {
        var original = new Dictionary<int, int>(cedulas);
        var resultado = new Dictionary<int, int>();

        Console.WriteLine("Digite o valor para saque: ");

        int valor = int.Parse(Console.ReadLine()!);
        int restante = valor;
        string msg = "";      

        // Verifica e consulta no dicionario a quantidade de notas de cada tipo para saque
        foreach (var c in cedulas.OrderByDescending(c => c.Key))
        {
            int nota = c.Key;
            int disponiveis = c.Value;
            int usar = Math.Min(restante / nota, disponiveis);

            if (usar > 0)
            {
                resultado[nota] = usar;
                restante -= usar * nota;
            }
        }
        // Realiza o saque e atualiza o dicionário de cédulas
        if (restante == 0)
        {
            foreach (var r in resultado)
                cedulas[r.Key] -= r.Value;

            msg = $"Saque realizado: R${valor}";
            Console.WriteLine(msg);
            Console.WriteLine("Tecle ENTER para voltar ao menu");
            Console.ReadLine();
            NotificarTodos(msg);
        }
        else
        {
            msg = $"Erro no saque: Não foi possível montar R${valor} com as cédulas disponíveis";
            Console.WriteLine(msg);
            SugerirSaqueAlternativo(valor);
            Console.WriteLine("Tecle ENTER para voltar ao menu");
            Console.ReadLine();
            NotificarTodos(msg);
            
        }
    }
    private void SugerirSaqueAlternativo(int valorOriginal)
    {
        for (int i = valorOriginal - 1; i > 0; i--)
        {
            if (PodeMontarValor(i))
            {
                Console.WriteLine($"Sugestão: você pode sacar R${i}");
                break;
            }
        }
    }
    private bool PodeMontarValor(int valor)
    {   // verifica se é possível montar o valor com as cédulas disponíveis, vai diminuindo o valor até encontar a nota mais baixa para saque
        int restante = valor;
        foreach (var c in cedulas.OrderByDescending(c => c.Key))
        {
            int usar = Math.Min(restante / c.Key, c.Value);
            restante -= usar * c.Key;
        }
        return restante == 0;
    }
    private void ConsultarSaldo()
    {        
        foreach (int i in cedulas.Keys)
        {
            Console.WriteLine($"Notas de R${i}: {cedulas[i]}");
        }
        int total = cedulas.Sum(c => c.Key * c.Value);
        Console.WriteLine($"Valor total: R${total}");
        Console.WriteLine("Tecle ENTER para voltar ao menu");
        Console.ReadLine();
    }
    private void DepositarDinheiro()
    {
        var deposito = new Dictionary<int, int>();
        foreach (var valor in new[] { 200, 100, 50, 20, 10, 5, 2 })
        {
            try
            {
                Console.WriteLine($"Quantidade de notas de R${valor}");
                int qtd = int.Parse(Console.ReadLine()!);
                deposito[valor] = qtd;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Valor inválido. Tente novamente." + ex.Message);
                Console.WriteLine("Tecle ENTER para voltar ao menu");                
                Console.ReadLine();
                ExibirMenu();
            }            
        }
        Depositar(deposito);
    }
    public void Depositar(Dictionary<int, int> deposito)
    {
        foreach (var item in deposito)
        {
            if (cedulas.ContainsKey(item.Key))
                cedulas[item.Key] += item.Value;
        }

        string resumo = string.Join(", ", deposito.Select(c => $"{c.Value}x R${c.Key}"));
        NotificarTodos($"Depósito realizado: {resumo}");
        Console.WriteLine("Deposito realizado com sucesso!" +
                          "\nTecle ENTER para voltar ao menu");
        Console.ReadLine();
        ExibirMenu();
    }
    // Sistema de Notificação com Interface
    interface INotificador
    {
        void Notificar(string mensagem);
    }
    class NotificadorArquivo : INotificador
    {   // diretorio do arquivo
        private string caminhoArquivo = "log_caixa.txt";

        public void Notificar(string mensagem)
        {
            string log = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {mensagem}";
            File.AppendAllText(caminhoArquivo, log + Environment.NewLine); // Cria arquivo de log e insere a msg da ação tomada
        }
    }
    private void NotificarTodos(string mensagem)
    {
        foreach (var n in notificadores)
            n.Notificar(mensagem);
    }
}