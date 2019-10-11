﻿using System;
using System.Collections.Generic;

namespace Minesweeper.Logic
{
    internal class Minefield
    {

        public interface ICell
        {
            bool IsBomb { get; }
            int Neighbors { get; }

        }

        private readonly struct Cell : ICell
        {

            public const int c_bomb = -1;

            private readonly int m_data;
            public Cell(int _data)
            {
                if (_data != c_bomb && (_data < 0 || _data > 8))
                {
                    throw new ArgumentOutOfRangeException(nameof(_data));
                }
                m_data = _data;
            }

            public bool IsBomb => m_data == c_bomb;

            public int Neighbors => IsBomb ? throw new InvalidOperationException() : m_data;
        }

        private const int c_maxSize = 64;
        private readonly int[,] m_cells;

        public Minefield(int _w, int _h, int _bombCount)
        {
            if (_w < 1 || _w > c_maxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(_w));
            }
            if (_h < 1 || _h > c_maxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(_h));
            }
            if (_bombCount < 0 || _bombCount > _w * _h)
            {
                throw new ArgumentOutOfRangeException(nameof(_bombCount));
            }
            BombCount = _bombCount;
            m_cells = new int[_w, _h];
            InitializeCells();
        }

        private void InitializeCells()
        {
            bool additive = BombCount * 2 < CellCount;
            // Fill
            {
                int initValue = additive ? 0 : Cell.c_bomb;
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
                int count = additive ? BombCount : (CellCount - BombCount);
                int influence = additive ? +1 : -1;
                int drop = additive ? Cell.c_bomb : 0;
                foreach ((int x, int y) in Random(count, _t => m_cells[_t.x, _t.y] == Cell.c_bomb ^ additive))
                {
                    m_cells[x, y] = drop;
                    int bombs = 0;
                    foreach ((int nx, int ny) in Neighborhood(x, y))
                    {
                        if (m_cells[nx, ny] != Cell.c_bomb)
                        {
                            m_cells[nx, ny] += influence;
                        }
                        else
                        {
                            bombs++;
                        }
                    }
                    if (!additive)
                    {
                        m_cells[x, y] = bombs;
                    }
                }
            }
        }

        private IEnumerable<(int x, int y)> Random(int _count, Predicate<(int x, int y)> _pickable)
        {
            var random = new Random();
            while (_count > 0)
            {
                int r = random.Next(CellCount);
                (int, int) c = (r % Width, r / Width);
                if (_pickable(c))
                {
                    yield return c;
                    _count--;
                }
            }
        }

        public IEnumerable<(int x, int y)> Neighborhood(int _x, int _y)
        {
            ValidateIndex(_x, _y);
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
        public int BombCount { get; }
        public ICell this[int _x, int _y] => new Cell(m_cells[_x, _y]);
        public ICell this[(int x, int y) _p] => this[_p.x, _p.y];

        public void ValidateIndex(int _x, int _y)
        {
            if (_x < 0 || _x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(_x));
            }
            if (_y < 0 || _y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(_y));
            }
        }

    }

}
