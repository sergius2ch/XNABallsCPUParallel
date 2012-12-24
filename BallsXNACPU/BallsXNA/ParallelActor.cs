using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallsXNA
{
    class ParallelActor
    {
        private Cell[,] partOfgrid;
        private int gridColumns;
        private int gridRows;
        private Ball[] balls;

        public Action Work;

        public ParallelActor(Ball[] balls, Cell[,] grid, int start, int end, bool last)
        {
            this.balls = balls;
            gridColumns = grid.GetLength(1);
            gridRows = end - start;
            
            if (last)
            {
                partOfgrid = new Cell[gridRows, gridColumns];
                for (int i = start; i < end; i++)
                {
                    for (int j = 0; j < gridColumns; j++)
                    {
                        partOfgrid[i - start, j] = grid[i, j];
                    }
                }
                Work = LastWork;
            } else
            {
                Work = UsualWork;
                partOfgrid = new Cell[gridRows+1, gridColumns];
                for (int i = start; i < end+1; i++)
                {
                    for (int j = 0; j < gridColumns; j++)
                    {
                        partOfgrid[i - start, j] = grid[i, j];
                    }
                }
            }           
        }

        protected List<int> list = new List<int>(64);
        public void LastWork()
        {
            for (int i = 0; i < gridRows; i++)
            {
                for (int j = 0; j < gridColumns; j++)
                {
                    Cell cell = partOfgrid[i, j];
                    if (cell.items.Count == 0) continue;
                    list.Clear();
                    #region проверка столкновений внутри ячейки
                    if (cell.items.Count > 1)
                        for (int ic = 0; ic < cell.items.Count - 1; ic++)
                        {
                            for (int jc = ic + 1; jc < cell.items.Count; jc++)
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
                        Cell cellright = partOfgrid[i, j + 1];
                        list.AddRange(cellright.items);
                    }
                    if (i < gridRows - 1)
                    {
                        Cell celldown = partOfgrid[i + 1, j];
                        list.AddRange(celldown.items);
                        if (j < gridColumns - 1)
                        {
                            Cell cellcross = partOfgrid[i + 1, j + 1];
                            list.AddRange(cellcross.items);
                        }
                        if (j > 0)
                        {
                            Cell cellcross = partOfgrid[i + 1, j - 1];
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

        public void UsualWork()
        {
            for (int i = 0; i < gridRows; i++)
            {
                for (int j = 0; j < gridColumns; j++)
                {
                    Cell cell = partOfgrid[i, j];
                    if (cell.items.Count == 0) continue;
                    list.Clear();
                    #region проверка столкновений внутри ячейки
                    if (cell.items.Count > 1)
                        for (int ic = 0; ic < cell.items.Count - 1; ic++)
                        {
                            for (int jc = ic + 1; jc < cell.items.Count; jc++)
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
                        Cell cellright = partOfgrid[i, j + 1];
                        list.AddRange(cellright.items);
                    }                   
                    Cell celldown = partOfgrid[i + 1, j];
                    list.AddRange(celldown.items);
                    if (j < gridColumns - 1)
                    {
                        Cell cellcross = partOfgrid[i + 1, j + 1];
                        list.AddRange(cellcross.items);
                    }
                    if (j > 0)
                    {
                        Cell cellcross = partOfgrid[i + 1, j - 1];
                        list.AddRange(cellcross.items);
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
            if (dx < Ball.Diameter && dy < Ball.Diameter)
            {
                float distance = (float)(Math.Sqrt(dx * dx + dy * dy));
                if (distance < Ball.Diameter - 1)
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
                    if (distance < Ball.Diameter - 2)
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
    }
}
