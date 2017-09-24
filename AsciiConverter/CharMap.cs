using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsciiConverter
{
    class CharMap : IComparable
    {
        /// <summary>
        /// Символ ASCII
        /// </summary>
        private char symbol;
        /// <summary>
        /// Количество черных пикселей 
        /// </summary>
        private int value;

        public char Symbol { get => symbol; set => symbol = value; }
        public int Value { get => value; set => this.value = value; }

        public CharMap(char symbol, int value)
        {
            this.symbol = symbol;
            this.value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            CharMap otharCharMap = obj as CharMap;
            if (otharCharMap != null)
                //Отрицательное значение необходимо, чтобы инвертировать сортировку
                return -this.value.CompareTo(otharCharMap.value);
            else
                throw new ArgumentException("Object is not a CharMap");
        }
    }
}
