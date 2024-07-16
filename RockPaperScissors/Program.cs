using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ConsoleTables;

public class Game
{
    static void Main(string[] args)
    {
        var moves = args.Skip(2).ToArray();

       

        while (true)
        {
            var computerMoveIndex = new Random().Next(moves.Length);
            var computerMove = moves[computerMoveIndex];
            var key = HMACGenerator.GenerateKey();
            var hmac = HMACGenerator.GenerateHMAC(key, computerMove);

            Console.WriteLine($"HMAC: {hmac} \nMoves:");
            for (int i = 0; i < moves.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {moves[i]}");
            }
            Console.WriteLine("0 - exit\n? - help");

            Console.Write("Your move: ");
            var userInput = Console.ReadLine();

            if (userInput == "0")
            {
                Console.WriteLine("Game exited.");
                return;
            }
            else if (userInput == "?")
            {
                RulesTable.DisplayHelp(moves);
                continue;
            }

            if (int.TryParse(userInput, out int userMoveIndex) && userMoveIndex >= 1 && userMoveIndex <= moves.Length)
            {
                var userMove = moves[userMoveIndex - 1];
                Console.WriteLine($"Your move: {userMove}");
                Console.WriteLine($"Computer move: {computerMove}");

                var result = RulesTable.DetermineWinner(moves, userMoveIndex - 1, computerMoveIndex);
                Console.WriteLine(result);
                Console.WriteLine($"HMAC key: {BitConverter.ToString(key).Replace("-", "")}");
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
                break;
            }

            Console.WriteLine("Play again? (yes/no)");
            var playAgain = Console.ReadLine().Trim().ToLower();

            if (playAgain != "yes")
            {
                Console.WriteLine("Game exited.");
                return;
            }

        }
    }
}

class HMACGenerator
{
    public static byte[] GenerateKey()
    {
        var key = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return key;
    }

    public static string GenerateHMAC(byte[] key, string message)
    {
        using (var hmacsha256 = new HMACSHA256(key))
        {
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}

class MoveValidator
{
    public static bool Validate(string[] args)
    {
        if (args.Length < 3 || args.Length % 2 == 0)
        {
            Console.WriteLine("Error: Please provide an odd number (>= 3) of non-repeating moves.");
            Console.WriteLine("Example: dotnet run rock paper scissors");
            return false;
        }

        if (args.Distinct().Count() != args.Length)
        {
            Console.WriteLine("Error: Moves should be non-repeating.");
            Console.WriteLine("Example: dotnet run rock paper scissors");
            return false;
        }

        return true;
    }
}

class RulesTable
{
    public static void DisplayHelp(string[] moves)
    {
        var table = new ConsoleTable(new[] { "Move" }.Concat(moves).ToArray());

        for (int i = 0; i < moves.Length; i++)
        {
            var row = new string[moves.Length + 1];
            row[0] = moves[i];

            for (int j = 0; j < moves.Length; j++)
            {
                if (i == j)
                {
                    row[j + 1] = "Draw";
                }
                else if ((j - i + moves.Length) % moves.Length <= moves.Length / 2)
                {
                    row[j + 1] = "Win";
                }
                else
                {
                    row[j + 1] = "Lose";
                }
            }

            table.AddRow(row);
        }

        table.Write();
    }

    public static string DetermineWinner(string[] moves, int userMoveIndex, int computerMoveIndex)
    {
        if (userMoveIndex == computerMoveIndex)
        {
            return "It's a draw!";
        }

        var moveDifference = (computerMoveIndex - userMoveIndex + moves.Length) % moves.Length;

        if (moveDifference <= moves.Length / 2)
        {
            return "You lose!";
        }
        else
        {
            return "You win!";
        }
    }
}
