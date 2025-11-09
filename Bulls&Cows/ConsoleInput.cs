namespace Bulls_Cows
{
    internal class ConsoleInput : IInput
    {
        public GameCommand GetUserChar(out char ch)
        {
            var userInput = Console.ReadKey(true);
            ch = userInput.KeyChar;
            switch (userInput.Key)
            {
                case ConsoleKey.LeftArrow:
                    return GameCommand.Left;
                case ConsoleKey.RightArrow:
                    return GameCommand.Right;
                case ConsoleKey.Backspace:
                    return GameCommand.Backspace;
                case ConsoleKey.Enter:
                    return GameCommand.Accept;
                case ConsoleKey.Escape:
                    return GameCommand.Esc;
                default:
                    return GameCommand.None;
            }
        }
        public GameCommand GetCommand()
        {
            var userInput = Console.ReadKey(true).Key;
            switch (userInput)
            {
                case ConsoleKey.UpArrow:
                    return GameCommand.Up;
                case ConsoleKey.DownArrow:
                    return GameCommand.Down;
                case ConsoleKey.LeftArrow:
                    return GameCommand.Left;
                case ConsoleKey.RightArrow:
                    return GameCommand.Right;
                case ConsoleKey.Enter:
                    return GameCommand.Accept;
                case ConsoleKey.Escape:
                    return GameCommand.Esc;
                default:
                    return GameCommand.None;
            }
        }
        public void WaitUserInput() => Console.ReadKey();
        public string GetUserString() => Console.ReadLine();
    }
}
