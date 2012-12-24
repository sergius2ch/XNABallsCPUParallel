using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BallsXNA
{
    /// <summary>
    /// Класс-менеджер для управления шариками
    /// </summary>
    class Manager
    {
        Logger log;
        Cell[,] grid;
        /// <summary>
        /// Массив шариков
        /// </summary>
        Ball[] balls = null;
        /// <summary>
        /// Количество шариков
        /// </summary>
        int NumberOfBalls;
        int Diameter;
        int Radius;
        /// <summary>
        /// Поле для шариков (прямоугольное)
        /// </summary>
        Rectangle field;

        private int gridRows;
        private int gridColumns;

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
            this.Diameter = Diameter;
            Radius = Diameter / 2;
            this.field = field;
            InitBalls();
            log = new Logger(System.Environment.CurrentDirectory);
            log.Save(balls);
        }

        /// <summary>
        /// Инициализация шариков
        /// </summary>
        protected void InitBalls()
        {
            float x, y, V, angle;
            Random rand = new Random();
            #region Создадим сетку для расстановки шариков
            int columns = field.Width / Diameter;
            int rows = field.Height / Diameter;
            List<int> listX = new List<int>(columns * rows);
            List<int> listY = new List<int>(columns * rows);
            //----------------------------------------------
            gridRows = rows+1;
            gridColumns = columns+1;
            grid = new Cell[rows+1, columns+1];
            //----------------------------------------------
            x = field.Left + Radius;
            y = field.Top + Radius;
            for (int i = 0; i <= rows; i++)
            {
                for (int j = 0; j <= columns; j++)
                {
                    grid[i, j] = new Cell();
                    listX.Add((int)x);
                    listY.Add((int)y);
                    x += Diameter;
                }
                #region Переход на начало новой строки
                y += Diameter;
                x = field.Left + Radius;
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
                int row = (int) Math.Truncate(y / Diameter);
                int column = (int) Math.Truncate(x / Diameter);
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
            CheckCollisions4Grid();
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
                float x = (float) ball.x/Diameter;
                float y = (float) ball.y/Diameter;
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
                    if (ball.x - Radius <= field.Left)
                    { // шарик отскакивает от левой границы
                        ball.vx = -ball.vx;
                        if (ball.x - Radius < field.Left)
                        {
                            float dx = field.Left - (ball.x - Radius);
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
                    if (ball.x + Radius >= field.Right)
                    { // шарик отскакивает от правой границы
                        ball.vx = -ball.vx;
                        if (ball.x + Radius > field.Right)
                        {
                            float dx = (ball.x + Radius) - field.Right;
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
                    if (ball.y - Radius <= field.Top)
                    { // шарик отскакивает от верхней границы
                        ball.vy = -ball.vy;
                        if (ball.y - Radius < field.Top)
                        {
                            float dy = field.Top - (ball.y - Radius);
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
                    if (ball.y + Radius >= field.Bottom)
                    { // шарик отскакивает от нижней границы
                        ball.vy = -ball.vy;
                        if (ball.y + Radius > field.Bottom)
                        {
                            float dy = (ball.y + Radius) - field.Bottom;
                            ball.y -= dy + 1;
                        }
                        ball.OnEdge = true;
                    }
                }
                #endregion
            }
        }


        /// <summary>
        /// Проверка на столкновения с границами
        /// </summary>
        protected void CheckBorders()
        {
            for (int i = 0; i < NumberOfBalls; i++)
            {
                balls[i].OnEdge = false;
                if (balls[i].x - Radius <= field.Left)
                { // шарик отскакивает от левой границы
                    balls[i].vx = -balls[i].vx;
                    if (balls[i].x - Radius < field.Left)
                    {
                        float dx = field.Left - (balls[i].x - Radius);
                        balls[i].x += dx+1;
                    }
                    balls[i].OnEdge = true;
                }
                if (balls[i].x + Radius >= field.Right)
                { // шарик отскакивает от правой границы
                    balls[i].vx = -balls[i].vx;
                    if (balls[i].x + Radius > field.Right)
                    {
                        float dx = (balls[i].x + Radius) - field.Right;
                        balls[i].x -= dx+1;
                    }
                    if (balls[i].OnEdge) return;
                    balls[i].OnEdge = true;
                } 
                if (balls[i].y - Radius <= field.Top)
                { // шарик отскакивает от верхней границы
                    balls[i].vy = -balls[i].vy;
                    if (balls[i].y - Radius < field.Top)
                    {
                        float dy = field.Top - (balls[i].y - Radius);
                        balls[i].y += dy+1;
                    }
                    if (balls[i].OnEdge) return;
                    balls[i].OnEdge = true;
                } 
                if (balls[i].y + Radius >= field.Bottom)
                { // шарик отскакивает от нижней границы
                    balls[i].vy = -balls[i].vy;
                    if (balls[i].y + Radius > field.Bottom)
                    {
                        float dy = (balls[i].y + Radius) - field.Bottom;
                        balls[i].y -= dy+1;
                    }
                    balls[i].OnEdge = true;
                }
            }
        }

        protected List<int> list = new List<int>(64);
        protected void CheckCollisions4Grid()
        {
            for (int i = 0; i < gridRows; i++)
            {
                for (int j = 0; j < gridColumns; j++)
                {
                    Cell cell = grid[i, j];
                    if (cell.items.Count == 0) continue;
                    list.Clear();   
                    #region проверка столкновений внутри ячейки
                    if (cell.items.Count > 1)
                    for (int ic = 0; ic < cell.items.Count-1; ic++)
                    {
                        for (int jc = ic+1; jc < cell.items.Count; jc++)
                        {
                            int i1 = cell.items[ic];
                            int j1 = cell.items[jc];
                            CheckCollisions2Ball(i1, j1);
                        }
                    }
                    #endregion
                    #region собираем списки шариков из соседних ячеек
                    if (j < gridColumns - 1)
                    {
                        Cell cellright = grid[i, j+1];
                        list.AddRange(cellright.items);
                    }
                    if (i < gridRows - 1)
                    {
                        Cell celldown = grid[i+1, j];
                        list.AddRange(celldown.items);
                        if (j < gridColumns - 1)
                        {
                            Cell cellcross = grid[i + 1, j + 1];
                            list.AddRange(cellcross.items);
                        }
                        if (j > 0)
                        {
                            Cell cellcross = grid[i + 1, j - 1];
                            list.AddRange(cellcross.items);
                        }
                    }
                    #endregion
                    #region проверка столкновений с шариками из соседних ячеек
                    for (int im = 0; im < cell.items.Count; im++)
                    {
                        for (int jn = 0; jn < list.Count; jn++)
                        {
                            int i1 = cell.items[im];
                            int j1 = list[jn];
                            CheckCollisions2Ball(i1, j1);
                        }
                    }
                    #endregion
                }
            }
        }

        protected void CheckCollisions2Ball(int i, int j)
        {
            float dx = (int)(balls[i].x - balls[j].x);
            float dy = (int)(balls[i].y - balls[j].y);
            if (dx < Diameter && dy < Diameter)
            {
                float distance = (float)(Math.Sqrt(dx * dx + dy * dy));
                if (distance < Diameter - 1)
                {   // шарики столкнулись
                    // Честная физика столкновений:
                    #region 1) Замена переменных для скоростей
                    float vx1 = balls[i].vx;
                    float vx2 = balls[j].vx;
                    float vy1 = balls[i].vy;
                    float vy2 = balls[j].vy;
                    #endregion
                    #region 2) Вычиляем единичный вектор столкновения
                    float ex = (dx / distance);
                    float ey = (dy / distance);
                    #endregion
                    #region 3) Проецируем вектора скоростей шариков на вектор столкновения
                    // первый шарик
                    float vex1 = (vx1 * ex + vy1 * ey);
                    float vey1 = (-vx1 * ey + vy1 * ex);
                    // второй шарик
                    float vex2 = (vx2 * ex + vy2 * ey);
                    float vey2 = (-vx2 * ey + vy2 * ex);
                    #endregion
                    #region 4) Вычисляем скорости после столкновения в проекции на вектор столкновения
                    float vPex = vex1 + (vex2 - vex1);
                    float vPey = vex2 + (vex1 - vex2);
                    #endregion
                    #region 5) Отменяем проецирование
                    vx1 = vPex * ex - vey1 * ey;
                    vy1 = vPex * ey + vey1 * ex;
                    vx2 = vPey * ex - vey2 * ey;
                    vy2 = vPey * ey + vey2 * ex;
                    #endregion
                    #region 6) Укажем шарикам их новые скорости
                    balls[i].vx = vx1;
                    balls[i].vy = vy1;
                    balls[j].vx = vx2;
                    balls[j].vy = vy2;
                    #endregion
                    #region 7) Устраним эффект залипания
                    if (distance < Diameter - 2)
                    {
                        if (!balls[i].OnEdge)
                        {
                            balls[i].x += ex;
                            balls[i].y += ey;
                        }
                        if (!balls[j].OnEdge)
                        {
                            balls[j].x -= ex;
                            balls[j].y -= ey;
                        }                
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Проверка столкновений шариков др/др
        /// </summary>
        protected void CheckCollisions()
        {
            // пробегаем по всем шарикам, кроме последнего
            for (int i = 0; i < NumberOfBalls; i++)
            {
                // сравниваем со всеми последующими
                for (int j = i + 1; j < NumberOfBalls; j++)
                {
                    CheckCollisions2Ball(i, j);
                }
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
                    new Vector2(ball.x - Radius, ball.y - Radius),
                    Color.White);
 
            }
        }
    }
}
