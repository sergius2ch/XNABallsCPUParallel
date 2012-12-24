using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BallsXNA
{
    /// <summary>
    /// Класс-менеджер для управления шариками
    /// </summary>
    class Manager
    {
        private Logger log;
        private Cell[,] grid;
        /// <summary>
        /// Массив шариков
        /// </summary>
        private Ball[] balls = null;
        /// <summary>
        /// Количество шариков
        /// </summary>
        private int NumberOfBalls;
        
        /// <summary>
        /// Поле для шариков (прямоугольное)
        /// </summary>
        private Rectangle field;

        private int gridRows;
        private int gridColumns;

        private ParallelActor[] actors;

        /// <summary>
        /// Конструктор менеджера
        /// </summary>
        /// <param name="NumberOfBalls">Кол-во шариков</param>
        /// <param name="Diameter">Диаметр</param>
        /// <param name="field">Ограничивающий приямоугольник</param>
        public Manager(int NumberOfBalls, int Diameter, Rectangle field)
        {
            this.NumberOfBalls = NumberOfBalls;
            balls = new Ball[NumberOfBalls];
            Ball.Diameter = Diameter;
            Ball.Radius = Diameter / 2;
            this.field = field;
            InitBalls();
            InitParallelActors();
            /*
            log = new Logger(System.Environment.CurrentDirectory);
            log.Save(balls);
             * */
        }

        protected  void InitParallelActors()
        {
            int counter = System.Environment.ProcessorCount;
            actors = new ParallelActor[counter];
            int shift = gridRows/counter;
            counter = 0;
            int i = 0;
            while (counter < gridRows)
            {
                if ( (gridRows-(counter+shift)) < shift)
                {
                    actors[i] = new ParallelActor(balls, grid, counter, gridRows, true);
                    break;
                }
                actors[i] = new ParallelActor(balls, grid, counter, counter+shift, false);
                counter += shift;
                i++;
            }
        }

        /// <summary>
        /// Инициализация шариков
        /// </summary>
        protected void InitBalls()
        {
            float x, y, V, angle;
            Random rand = new Random();
            #region Создадим сетку для расстановки шариков
            int columns = field.Width / Ball.Diameter;
            int rows = field.Height / Ball.Diameter;
            List<int> listX = new List<int>(columns * rows);
            List<int> listY = new List<int>(columns * rows);
            //----------------------------------------------
            gridRows = rows+1;
            gridColumns = columns+1;
            grid = new Cell[rows+1, columns+1];
            //----------------------------------------------
            x = field.Left + Ball.Radius;
            y = field.Top + Ball.Radius;
            for (int i = 0; i <= rows; i++)
            {
                for (int j = 0; j <= columns; j++)
                {
                    grid[i, j] = new Cell();
                    listX.Add((int)x);
                    listY.Add((int)y);
                    x += Ball.Diameter;
                }
                #region Переход на начало новой строки
                y += Ball.Diameter;
                x = field.Left + Ball.Radius;
                #endregion
            }
            #endregion

            #region Расставляем шарики на поле
            int index = 0;
            for (int i = 0; i < NumberOfBalls; i++)
            {
                // Выбираем случайную ячейку в сетке
                index = rand.Next(listX.Count);
                x = listX[index];
                y = listY[index];
                balls[i] = new Ball(x, y, i);
                // закрепляем шарик за ячейкой сетки...
                int row = (int)Math.Truncate(y / Ball.Diameter);
                int column = (int)Math.Truncate(x / Ball.Diameter);
                grid[row, column].items.Add(i);
                balls[i].currentCell = grid[row, column];
                // удаляем ячейку из списка незанятых
                listX.RemoveAt(index);
                listY.RemoveAt(index);
                // Возьмём случайную величину скорости 0..1
                V = (float)rand.NextDouble();
                // Возьмём случайную величину - направление движения
                angle = (float)(rand.NextDouble() * 2 * Math.PI);
                // Вычисляем проекции скорости на оси координат
                balls[i].vx = (float)(V * Math.Cos(angle));
                balls[i].vy = (float)(V * Math.Sin(angle));
            }
            #endregion
            listX.Clear(); listY.Clear();
        }

        /// <summary>
        /// Обновление системы
        /// </summary>
        public void Update()
        {
            MoveBalls();
            CheckBorders4Grid();
            CheckCollisionsParallel();
        }

        public void CheckCollisionsParallel()
        {
            Parallel.For(0, actors.Length, i => actors[i].Work());
        }

        /// <summary>
        /// Перемещаем все шарики
        /// </summary>
        protected void MoveBalls()
        {
            for (int i = 0; i < NumberOfBalls; i++)
            {
                Ball ball = balls[i];
                ball.x += ball.vx;
                ball.y += ball.vy;
                float x = (float) ball.x/Ball.Diameter;
                float y = (float) ball.y/Ball.Diameter;
                int row = (int) (y);
                int column = (int) (x);
                Cell cell = grid[row, column];
                if (ball.currentCell != cell)
                {
                    ball.currentCell.items.Remove(i);
                    cell.items.Add(i);
                    ball.currentCell = cell;
                }
            }
        }

        /// <summary>
        /// Проверка на столкновения с границами
        /// </summary>
        protected  void CheckBorders4Grid()
        {
            for (int ri = 0; ri < gridRows; ri++)
            {
                Cell leftcell = grid[ri, 0];
                #region Проверка только левой границы
                for (int j = 0; j < leftcell.items.Count; j++)
                {
                    Ball ball = balls[leftcell.items[j]];
                    ball.OnEdge = false;
                    if (ball.x - Ball.Radius <= field.Left)
                    { // шарик отскакивает от левой границы
                        ball.vx = -ball.vx;
                        if (ball.x - Ball.Radius < field.Left)
                        {
                            float dx = field.Left - (ball.x - Ball.Radius);
                            ball.x += dx + 1;
                        }
                        ball.OnEdge = true;
                    }
                }
                #endregion
                Cell rightcell = grid[ri, gridColumns-1];
                #region Проверка только правой границы
                for (int j = 0; j < rightcell.items.Count; j++)
                {
                    Ball ball = balls[rightcell.items[j]];
                    ball.OnEdge = false;
                    if (ball.x + Ball.Radius >= field.Right)
                    { // шарик отскакивает от правой границы
                        ball.vx = -ball.vx;
                        if (ball.x + Ball.Radius > field.Right)
                        {
                            float dx = (ball.x + Ball.Radius) - field.Right;
                            ball.x -= dx + 1;
                        }
                        if (ball.OnEdge) return;
                        ball.OnEdge = true;
                    }
                }
                #endregion
            }
            for (int ci = 0; ci < gridColumns; ci++)
            {
                Cell topcell = grid[0, ci];
                #region Проверка только верхней границы
                for (int j = 0; j < topcell.items.Count; j++)
                {
                    Ball ball = balls[topcell.items[j]];
                    ball.OnEdge = false;
                    if (ball.y - Ball.Radius <= field.Top)
                    { // шарик отскакивает от верхней границы
                        ball.vy = -ball.vy;
                        if (ball.y - Ball.Radius < field.Top)
                        {
                            float dy = field.Top - (ball.y - Ball.Radius);
                            ball.y += dy + 1;
                        }
                        if (ball.OnEdge) return;
                        ball.OnEdge = true;
                    }
                }
                #endregion
                Cell bottomcell = grid[gridRows-1, ci];
                #region Проверка только нижней границы
                for (int j = 0; j < bottomcell.items.Count; j++)
                {
                    Ball ball = balls[bottomcell.items[j]];
                    ball.OnEdge = false;
                    if (ball.y + Ball.Radius >= field.Bottom)
                    { // шарик отскакивает от нижней границы
                        ball.vy = -ball.vy;
                        if (ball.y + Ball.Radius > field.Bottom)
                        {
                            float dy = (ball.y + Ball.Radius) - field.Bottom;
                            ball.y -= dy + 1;
                        }
                        ball.OnEdge = true;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Прорисовка шариков
        /// </summary>
        public void Draw(SpriteBatch batch, Texture2D tex)
        {
            foreach (Ball ball in balls)
            {
                batch.Draw(tex, 
                    new Vector2(ball.x - Ball.Radius, ball.y - Ball.Radius),
                    Color.White);
 
            }
        }
    }
}
