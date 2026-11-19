namespace Tetris
{
    
    // КЛАСС 1: ФИГУРА (ТЕТРОМИНО)
    
    public class Tetromino
    {
        // Матрица фигуры (0 - пустота, 1 - блок)
        public int[,] Shape { get; set; }
        public int X { get; set; } // Позиция по горизонтали
        public int Y { get; set; } // Позиция по вертикали
        public ConsoleColor Color { get; set; }

        public Tetromino(int[,] shape, ConsoleColor color)
        {
            Shape = shape;
            Color = color;
            X = 3; // Начальная позиция по центру (10 ширина / 2 - размер фигуры)
            Y = 0;
        }

        // Метод вращения фигуры (поворот матрицы на 90 градусов)
        public void Rotate()
        {
            int rows = Shape.GetLength(0);
            int cols = Shape.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotated[j, rows - 1 - i] = Shape[i, j];
                }
            }
            Shape = rotated;
        }

        // Возврат копии фигуры (нужно для отката вращения, если оно невозможно)
        public Tetromino Clone()
        {
            return new Tetromino((int[,])Shape.Clone(), Color) { X = this.X, Y = this.Y };
        }
    }

        // КЛАСС 2: ИГРОВОЕ ПОЛЕ
    
    public class GameField
    {
        public int Width { get; private set; } = 10;
        public int Height { get; private set; } = 20;
        public int[,] Grid { get; private set; }

        public GameField()
        {
            Grid = new int[Height, Width];
        }

        // Проверка на столкновение
        public bool IsCollision(Tetromino piece)
        {
            for (int i = 0; i < piece.Shape.GetLength(0); i++)
            {
                for (int j = 0; j < piece.Shape.GetLength(1); j++)
                {
                    if (piece.Shape[i, j] == 1)
                    {
                        int newX = piece.X + j;
                        int newY = piece.Y + i;

                        // Выход за границы поля
                        if (newX < 0 || newX >= Width || newY >= Height || newY < 0)
                            return true;

                        // Столкновение с уже упавшими блоками
                        if (Grid[newY, newX] == 1)
                            return true;
                    }
                }
            }
            return false;
        }

        // Закрепление фигуры на поле
        public void Merge(Tetromino piece)
        {
            for (int i = 0; i < piece.Shape.GetLength(0); i++)
            {
                for (int j = 0; j < piece.Shape.GetLength(1); j++)
                {
                    if (piece.Shape[i, j] == 1)
                    {
                        Grid[piece.Y + i, piece.X + j] = 1;
                    }
                }
            }
        }

        // Очистка заполненных линий
        public int ClearLines()
        {
            int linesCleared = 0;
            for (int i = Height - 1; i >= 0; i--)
            {
                bool isFull = true;
                for (int j = 0; j < Width; j++)
                {
                    if (Grid[i, j] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    linesCleared++;
                    // Сдвигаем все линии выше вниз
                    for (int k = i; k > 0; k--)
                    {
                        for (int j = 0; j < Width; j++)
                        {
                            Grid[k, j] = Grid[k - 1, j];
                        }
                    }
                    // Верхнюю линию делаем пустой
                    for (int j = 0; j < Width; j++)
                        Grid[0, j] = 0;

                    i++; // Проверяем эту же строку еще раз (так как она сдвинулась)
                }
            }
            return linesCleared;
        }

        // Отрисовка поля и текущей падающей фигуры
        public void Draw(Tetromino currentPiece)
        {
            Console.SetCursorPosition(0, 0); // Возвращаем курсор в начало, чтобы не было мерцания

            for (int i = 0; i < Height; i++)
            {
                Console.Write("|"); // Левая стена
                for (int j = 0; j < Width; j++)
                {
                    // Проверяем, есть ли блок текущей фигуры в этой клетке
                    bool isPieceBlock = false;
                    if (currentPiece != null)
                    {
                        for (int pi = 0; pi < currentPiece.Shape.GetLength(0); pi++)
                        {
                            for (int pj = 0; pj < currentPiece.Shape.GetLength(1); pj++)
                            {
                                if (currentPiece.Shape[pi, pj] == 1 && currentPiece.Y + pi == i && currentPiece.X + pj == j)
                                {
                                    isPieceBlock = true;
                                }
                            }
                        }
                    }

                    if (isPieceBlock)
                    {
                        Console.ForegroundColor = currentPiece.Color;
                        Console.Write("[]");
                        Console.ResetColor();
                    }
                    else if (Grid[i, j] == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray; // Цвет упавших блоков
                        Console.Write("[]");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(" .");
                    }
                }
                Console.WriteLine("|"); // Правая стена
            }
            Console.WriteLine(new string('-', Width * 2 + 2)); // Пол
        }
    }

        // КЛАСС 3: ОСНОВНАЯ ИГРА
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool playAgain = true;
            while (playAgain)
            {
                playAgain = RunGame();
            }
        }

        static bool RunGame()
        {
            GameField field = new GameField();
            Tetromino currentPiece = GenerateRandomPiece();
            int score = 0;
            int speed = 300; // Задержка в мс (чем меньше, тем быстрее падает)

            while (true)
            {
                field.Draw(currentPiece);
                Console.SetCursorPosition(0, field.Height + 1);
                Console.WriteLine($"Счет: {score}   ");
                Console.WriteLine("Управление: A - влево, D - вправо, W - поворот, S - ускорить");

                // Обработка ввода (неблокирующая)
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.A: // Влево
                            currentPiece.X--;
                            if (field.IsCollision(currentPiece)) currentPiece.X++; // Откат
                            break;
                        case ConsoleKey.D: // Вправо
                            currentPiece.X++;
                            if (field.IsCollision(currentPiece)) currentPiece.X--; // Откат
                            break;
                        case ConsoleKey.W: // Поворот
                            Tetromino backup = currentPiece.Clone();
                            currentPiece.Rotate();
                            if (field.IsCollision(currentPiece)) currentPiece.Shape = backup.Shape; // Откат
                            break;
                        case ConsoleKey.S: // Ускорение
                            currentPiece.Y++;
                            if (field.IsCollision(currentPiece)) currentPiece.Y--; // Откат
                            break;
                    }
                }

                // Автоматическое падение (гравитация)
                currentPiece.Y++;
                if (field.IsCollision(currentPiece))
                {
                    currentPiece.Y--; // Возвращаем на место
                    field.Merge(currentPiece); // Фиксируем фигуру
                    int cleared = field.ClearLines(); // Проверяем на заполненные линии
                    if (cleared > 0) score += cleared * 100;

                    // Создаем новую фигуру
                    currentPiece = GenerateRandomPiece();

                    // Проверка на конец игры
                    if (field.IsCollision(currentPiece))
                    {
                        field.Draw(null);
                        Console.SetCursorPosition(0, field.Height + 4);
                        Console.WriteLine("\n=== ИГРА ОКОНЧЕНА! ===");
                        Console.WriteLine($"Ваш итоговый счет: {score}");
                        Console.WriteLine("Нажмите 'Y' для игры заново или любую другую клавишу для выхода.");

                        ConsoleKeyInfo restartKey = Console.ReadKey(true);
                        return restartKey.Key == ConsoleKey.Y;
                    }
                }

                Thread.Sleep(speed);
            }
        }

        // Метод для генерации случайной фигуры (7 классических форм)
        static Tetromino GenerateRandomPiece()
        {
            Random rand = new Random();
            int type = rand.Next(7);

            switch (type)
            {
                case 0: // I
                    return new Tetromino(new int[,] { { 1, 1, 1, 1 } }, ConsoleColor.Cyan);
                case 1: // O
                    return new Tetromino(new int[,] { { 1, 1 }, { 1, 1 } }, ConsoleColor.Yellow);
                case 2: // T
                    return new Tetromino(new int[,] { { 0, 1, 0 }, { 1, 1, 1 } }, ConsoleColor.Magenta);
                case 3: // S
                    return new Tetromino(new int[,] { { 0, 1, 1 }, { 1, 1, 0 } }, ConsoleColor.Green);
                case 4: // Z
                    return new Tetromino(new int[,] { { 1, 1, 0 }, { 0, 1, 1 } }, ConsoleColor.Red);
                case 5: // J
                    return new Tetromino(new int[,] { { 1, 0, 0 }, { 1, 1, 1 } }, ConsoleColor.Blue);
                case 6: // L
                    return new Tetromino(new int[,] { { 0, 0, 1 }, { 1, 1, 1 } }, ConsoleColor.DarkYellow);
                default:
                    return new Tetromino(new int[,] { { 1 } }, ConsoleColor.White);
            }
        }
    }
}