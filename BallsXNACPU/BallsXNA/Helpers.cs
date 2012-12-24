using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallsXNA
{
    public class Cell
    {
        public List<int> items;
        public Cell()
        {
            items = new List<int>(9);
        }
    }

    /// <summary>
    /// Класс для шарика
    /// </summary>
    [Serializable]
    public class Ball
    {
        public static int Diameter;
        public static int Radius;
        public int _id;
        public Cell currentCell;

        public bool OnEdge = false;
        /// <summary>
        /// координаты шарика
        /// </summary>
        public float x, y;

        /// <summary>
        /// Проекции скорости шарика на оси координат
        /// </summary>
        public float vx, vy;

        /// <summary>
        /// Конструктор шарика
        /// </summary>
        public Ball(float x, float y, int id)
        {
            this.x = x;
            this.y = y;
            this._id = id;
        }
        public Ball()
        {
            this.x = 0;
            this.y = 0;
            this._id = -1;
        }
    }
}
