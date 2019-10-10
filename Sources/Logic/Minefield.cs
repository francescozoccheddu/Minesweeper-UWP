using System;
using System.Collections.Generic;

namespace Minesweeper.Logic
{
    internal class Minefield
    {

        public interface ICell
        {
            bool IsMine { get; }
            int Neighbors { get; }

        }

        private readonly struct Cell : ICell
        {

            public const int c_mine = -1;

            private readonly int m_data;
            public Cell(int _data)
            {
                if (_data != c_mine && (_data < 0 || _data > 8))
                {
                    throw new ArgumentOutOfRangeException("Invalid data");
                }
                m_data = _data;
            }

            public bool IsMine => m_data != -1;

            public int Neighbors => IsMine ? throw new InvalidOperationException() : m_data;
        }

        private const int c_maxSize = 64;
        private readonly int[,] m_cells;

        public Minefield(int _w, int _h, int _mineCount)
        {
            if (_w < 1 || _h < 1 || _w > c_maxSize || _h > c_maxSize)
            {
                throw new ArgumentOutOfRangeException("Invalid size");
            }
            if (_mineCount < 0 || _mineCount > _w * _h)
            {
                throw new ArgumentOutOfRangeException("Invalid mine count");
            }
            MineCount = _mineCount;
            m_cells = new int[_w, _h];
            InitializeCells();
        }

        private void InitializeCells()
        {
            bool additive = MineCount * 2 < CellCount;
            // Fill
            {
                int initValue = additive ? 0 : Cell.c_mine;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        m_cells[x, y] = initValue;
                    }
                }
            }
            // Add
            {
                int count = additive ? MineCount : (CellCount - MineCount);
                int influence = additive ? +1 : -1;
                int drop = additive ? Cell.c_mine : 0;
                foreach ((int x, int y) in Random(count, _t => m_cells[_t.x, _t.y] == Cell.c_mine ^ additive))
                {
                    m_cells[x, y] = drop;
                    foreach ((int nx, int ny) in Neighborhood(x, y))
                    {
                        if (m_cells[nx, ny] != Cell.c_mine)
                        {
                            m_cells[nx, ny] += influence;
                        }
                    }
                }
            }
        }

        private IEnumerable<(int x, int y)> Random(int _count, Predicate<(int x, int y)> _pickable)
        {
            Random random = new Random();
            for (int t = 0; t < _count; t++)
            {
                int r = random.Next(CellCount - t);
                for (int o = 0; o < CellCount; o++)
                {
                    int x = o % Width;
                    int y = o / Width;
                    if (_pickable((x, y)))
                    {
                        yield return (x, y);
                        goto Found;
                    }
                }
                throw new OverflowException();
                Found:
                ;
            }
        }

        public IEnumerable<(int x, int y)> Neighborhood(int _x, int _y)
        {
            if (_x < 0 || _x >= Width || _y < 0 || _y >= Height)
            {
                throw new ArgumentOutOfRangeException("Invalid indexes");
            }
            int minX = Math.Max(_x - 1, 0);
            int minY = Math.Max(_y - 1, 0);
            int maxX = Math.Min(_x + 1, Width - 1);
            int maxY = Math.Min(_y + 1, Height - 1);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    yield return (x, y);
                }
            }
        }

        public IEnumerable<(int x, int y)> Expand(int _x, int _y, Predicate<(int x, int y)> _filter)
        {
            HashSet<(int, int)> found = new HashSet<(int, int)>();
            Stack<(int, int)> stack = new Stack<(int, int)>();
            void Push((int x, int y) _c)
            {
                if (m_cells[_c.x, _c.y] == 0 && !found.Contains(_c) && _filter(_c))
                {
                    stack.Push(_c);
                    found.Add(_c);
                }
            }
            Push((_x, _y));
            while (stack.Count > 0)
            {
                (int x, int y) c = stack.Pop();
                yield return c;
                foreach ((int, int) n in Neighborhood(c.x, c.y))
                {
                    Push(n);
                }
            }
        }

        public IEnumerable<(int x, int y)> Expand(int _x, int _y) => Expand(_x, _y, _t => true);

        public int Width => m_cells.GetLength(0);
        public int Height => m_cells.GetLength(1);
        public int CellCount => Width * Height;
        public int MineCount { get; }
        public ICell this[int _x, int _y] => new Cell(m_cells[_x, _y]);
        public ICell this[(int x, int y) _p] => this[_p.x, _p.y];

    }

}
