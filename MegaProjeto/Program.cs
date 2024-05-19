using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("GERAÇÃO DO NÚMEROS MEGA");
        Console.WriteLine("Escolha a opção:");
        Console.WriteLine("1 - GERANDO NÚMEROS MAIS SORTEADOS");
        Console.WriteLine("PARA INICIAR DIGITE 1:");
        int opcao = Convert.ToInt32(Console.ReadLine());

        switch (opcao)
        {
            case 1:
                GeraNumerosMega();
                break;
        }
    }

    public static void GeraNumerosMega()
    {
        string caminhoArquivo = @"C:\mega\resultadosMegaSena.txt";

        try
        {
            if (File.Exists(caminhoArquivo))
            {
                var linhas = File.ReadAllLines(caminhoArquivo);
                var jogos = ProcessarLinhas(linhas);
                Console.WriteLine("Conteúdo do arquivo lido com sucesso.");


                if (jogos.Count == 0)
                {
                    Console.WriteLine("Nenhum jogo válido encontrado no arquivo.");
                    return;
                }

                // Exibir os números mais sorteados
                var numerosMaisSorteados = ObterNumerosMaisSorteados(jogos);
                Console.WriteLine("Números mais sorteados:");
                foreach (var numero in numerosMaisSorteados)
                {
                    Console.WriteLine($"Número: {numero.Key}, Frequência: {numero.Value}");
                }

                // Gerar jogos baseados nos números mais sorteados
                Console.WriteLine("Quantos jogos você deseja gerar?");
                int quantidadeJogos = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("Quantos números por jogo (6, 7 ou 9)?");
                int quantidadeNumeros = Convert.ToInt32(Console.ReadLine());

                if (quantidadeNumeros != 6 && quantidadeNumeros != 7 && quantidadeNumeros != 9)
                {
                    Console.WriteLine("Quantidade de números por jogo inválida.");
                    return;
                }

                var jogosGerados = GerarJogos(quantidadeJogos, quantidadeNumeros, numerosMaisSorteados);

                Console.WriteLine("Jogos gerados:");
                foreach (var jogo in jogosGerados)
                {
                    Console.WriteLine(string.Join(", ", jogo));
                }
            }
            else
            {
                Console.WriteLine("Arquivo não encontrado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {ex.Message}");
        }
    }

    public static List<int[]> ProcessarLinhas(string[] linhas)
    {
        var jogos = new List<int[]>();

        for (int i = 0; i < linhas.Length; i++)
        {
            var linha = linhas[i];
            var partes = linha.Split(' ');

            if (partes.Length >= 8)
            {
                try
                {
                    var numeros = partes[2..8].Select(int.Parse).ToArray();
                    jogos.Add(numeros);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Linha com formato inválido (não numérico): {linha}");
                }
            }
            else
            {
                Console.WriteLine($"Linha com formato inválido (partes insuficientes): {linha}");
            }

            if ((i + 1) % (linhas.Length / 10) == 0)
            {
                Console.WriteLine($"Processamento de linhas: {(i + 1) * 100 / linhas.Length}% concluído");
                Thread.Sleep(1);
            }

            
        }

        return jogos;
    }

    public static ConcurrentDictionary<int, int> ObterNumerosMaisSorteados(List<int[]> jogos)
    {
        var frequencia = new ConcurrentDictionary<int, int>();

        for (int i = 0; i < jogos.Count; i++)
        {
            var jogo = jogos[i];
            foreach (var numero in jogo)
            {
                frequencia.AddOrUpdate(numero, 1, (key, oldValue) => oldValue + 1);
            }

            if ((i + 1) % (jogos.Count / 10) == 0)
            {
                Console.WriteLine($"Cálculo de frequências: {(i + 1) * 100 / jogos.Count}% concluído");
                Thread.Sleep(1);
            }
        }

        var ordenado = frequencia.OrderByDescending(n => n.Value)
                                 .ToDictionary(n => n.Key, n => n.Value);

        return new ConcurrentDictionary<int, int>(ordenado);
    }

    public static List<int[]> GerarJogos(int quantidadeJogos, int quantidadeNumeros, ConcurrentDictionary<int, int> numerosMaisSorteados)
    {
        var jogosGerados = new List<int[]>();
        var numerosOrdenados = numerosMaisSorteados.Keys.ToList();

        var random = new Random();

        for (int i = 0; i < quantidadeJogos; i++)
        {
            var jogo = new int[quantidadeNumeros];

            for (int j = 0; j < quantidadeNumeros; j++)
            {
                int indice = random.Next(numerosOrdenados.Count);
                jogo[j] = numerosOrdenados[indice];
                numerosOrdenados.RemoveAt(indice);
            }

            jogosGerados.Add(jogo);
            numerosOrdenados = numerosMaisSorteados.Keys.ToList(); // Reset para o próximo jogo

            Console.WriteLine($"Geração de jogos: {(i + 1) * 100 / quantidadeJogos}% concluído");
        }

        return jogosGerados;
    }
}
