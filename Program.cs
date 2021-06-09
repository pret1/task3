using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StonePaperScissorsGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!ValidateGeneralInput(args))
            {
                return;
            }

            PlayGame(args);
        }

        private static void PlayGame(string[] args)
        {
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                byte[] key = new byte[16];

                generator.GetBytes(key);

                Random random = new Random();
                int computerMoveIndex = random.Next(0, args.Length);
                string computerMove = args[computerMoveIndex];
                byte[] computerMoveBytes = Encoding.UTF8.GetBytes(computerMove);

                using (HMACSHA256 hMac = new HMACSHA256(key))
                {
                    byte[] moveHMac = hMac.ComputeHash(computerMoveBytes);

                    Console.WriteLine($"HMAC: {BytesToHexString(moveHMac)}");

                    int userMoveIndex = ReadUserInput(args);

                    if (userMoveIndex >= 0)
                    {
                        DetectWinner(args, computerMoveIndex, userMoveIndex);

                        Console.WriteLine($"HMAC key: {BytesToHexString(key)}");
                    }
                }
            }
        }

        private static bool ValidateGeneralInput(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Game moves can not be null or empty.");
                return false;
            }

            if (args.Length < 3 || args.Length % 2 == 0)
            {
                Console.WriteLine("Game moves count should be a valid odd number >= 3.");
                return false;
            }

            HashSet<string> set = args.ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (set.Count != args.Length)
            {
                Console.WriteLine("Game moves should be unique.");
                return false;
            }

            return true;
        }

        private static int ReadUserInput(string[] args)
        {
            int userMoveIndex;

            while (true)
            {
                Console.WriteLine("Available moves:");

                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine($"{i + 1} - {args[i]}");
                }

                Console.Write($"0 - exit{Environment.NewLine}Enter your move: ");

                string userMoveStr = Console.ReadLine();

                if (int.TryParse(userMoveStr, out int userMove) && userMove >= 0 && userMove <= args.Length)
                {
                    userMoveIndex = userMove - 1;
                    break;
                }
            }

            return userMoveIndex;
        }

        private static string BytesToHexString(byte[] bytes)
        {
            return string.Join("", bytes.Select(c => c.ToString("X2")));
        }

        private static void DetectWinner(string[] args, int computerMoveIndex, int userMoveIndex)
        {
            Console.WriteLine($"Your move: {args[userMoveIndex]}{Environment.NewLine}Computer move: {args[computerMoveIndex]}");

            if (computerMoveIndex == userMoveIndex)
            {
                Console.WriteLine("Draw!");

                return;
            }

            int half = args.Length / 2;

            for (int i = 1; i <= args.Length - 1; i++)
            {
                int itemIndex = (computerMoveIndex + i) % args.Length;

                if (itemIndex == userMoveIndex)
                {
                    string message = i <= half ? "You win!" : "Computer win!";
                    Console.WriteLine(message);

                    break;
                }
            }
        }
    }
}
