using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laba4IMG
{
    /// <summary>
    /// Класс для загрузки, сохранения и изменения изображений в формате BMP (24-бит).
    /// </summary>
    public class Image
    {
        // Приватное поле, где реально хранятся все пиксели.
        // Это и есть наше изображение в виде двумерного массива.
        private Pixel[,] _pixels;

        /// <summary>
        /// Ширина изображения в пикселях. Свойство "только для чтения".
        /// </summary>
        public int Width
        {
            get
            {
                return _pixels.GetLength(0);
            }
        }

        /// <summary>
        /// Высота изображения в пикселях. Свойство "только для чтения".
        /// </summary>
        public int Height
        {
            get
            {
                return _pixels.GetLength(1);
            }
        }

        /// <summary>
        /// Конструктор, который загружает изображение из BMP-файла.
        /// Обратите внимание, что к загрузке доступны только 24-битные BMP без сжатия.
        /// Это 3 канала по 8 бит на каждый цвет (R, G, B).
        /// 32-битные BMP (с альфа-каналом) не поддерживаются этим кодом.
        /// 16-битные BMP (с сдвигами битов) тоже не поддерживаются.
        /// Поддерживаются оба порядка строк: "снизу вверх" (обычный) и "сверху вниз".
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        public Image(string filePath)
        {
            // Используем BinaryReader для чтения двоичных данных из файла.
            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                // --- Чтение заголовка BMP ---
                // Первые 2 байта должны быть 'B' и 'M'. Если нет - это не BMP.
                if (reader.ReadChar() != 'B' || reader.ReadChar() != 'M')
                    throw new ArgumentException("Неверный формат файла. Ожидался BMP.");

                reader.ReadInt32(); // Размер файла (не используем)
                reader.ReadInt16(); // Зарезервировано
                reader.ReadInt16(); // Зарезервировано
                int dataOffset = reader.ReadInt32(); // Смещение до начала данных пикселей

                // --- Чтение информационного заголовка ---
                reader.ReadInt32(); // Размер инфо-заголовка (не используем)
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                reader.ReadInt16(); // Цветовые плоскости (должно быть 1)
                short bitsPerPixel = reader.ReadInt16();
                int compression = reader.ReadInt32(); // Тип сжатия (0 = без сжатия)

                // Этот код поддерживает только 24-битные изображения
                if (bitsPerPixel != 24)
                    throw new NotSupportedException("Поддерживаются только 24-битные BMP изображения.");

                if (compression != 0)
                    throw new NotSupportedException("Поддерживаются только BMP без сжатия.");

                // Обычно высота в BMP положительная, и строки хранятся снизу вверх.
                // Но некоторые редакторы сохраняют строки сверху вниз - тогда высота
                // записана ОТРИЦАТЕЛЬНЫМ числом. Запоминаем порядок и берём модуль.
                bool topDown = height < 0;
                height = Math.Abs(height);

                if (width <= 0 || height == 0)
                    throw new ArgumentException("Некорректные размеры изображения в заголовке BMP.");

                // Инициализируем наш массив пикселей
                _pixels = new Pixel[width, height];

                // В BMP каждая строка должна быть выровнена по 4 байта.
                // Вычисляем, сколько "мусорных" байт (padding) в конце каждой строки.
                int padding = (4 - (width * 3) % 4) % 4;

                // Перемещаемся к началу данных пикселей
                reader.BaseStream.Seek(dataOffset, SeekOrigin.Begin);

                // Читаем пиксели. Важный прикол BMP: обычно строки хранятся снизу вверх.
                for (int row = 0; row < height; row++)
                {
                    // Номер строки в нашем массиве (y = 0 - верхняя строка картинки).
                    int y = topDown ? row : height - 1 - row;

                    for (int x = 0; x < width; x++)
                    {
                        // Еще один прикол BMP: порядок цветов BGR, а не RGB.
                        byte b = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte r = reader.ReadByte();
                        _pixels[x, y] = new Pixel { R = r, G = g, B = b };
                    }
                    // Пропускаем "мусорные" байты в конце строки.
                    reader.ReadBytes(padding);
                }
            }
        }

        /// <summary>
        /// Сохраняет текущее изображение в новый BMP-файл.
        /// </summary>
        /// <param name="filePath">Путь для сохранения файла.</param>
        public void Save(string filePath)
        {
            int width = Width;
            int height = Height;
            int padding = (4 - (width * 3) % 4) % 4;
            int imageSize = (width * 3 + padding) * height;
            int fileSize = 54 + imageSize;

            using (var writer = new BinaryWriter(File.Create(filePath)))
            {
                // --- Запись заголовка BMP (14 байт) ---
                writer.Write('B');
                writer.Write('M');
                writer.Write(fileSize);
                writer.Write((short)0);
                writer.Write((short)0);
                writer.Write(54); // Смещение до данных

                // --- Запись информационного заголовка (40 байт) ---
                writer.Write(40); // Размер инфо-заголовка
                writer.Write(width);
                writer.Write(height);
                writer.Write((short)1); // Плоскости
                writer.Write((short)24); // Бит на пиксель
                writer.Write(0); // Без сжатия
                writer.Write(imageSize);
                writer.Write(0); // Разрешение по X (неважно)
                writer.Write(0); // Разрешение по Y (неважно)
                writer.Write(0); // Количество цветов в палитре
                writer.Write(0); // Количество важных цветов

                // --- Запись данных пикселей ---
                // Сохраняем всегда в обычном формате: строки снизу вверх.
                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Pixel p = _pixels[x, y];
                        // Пишем в порядке BGR
                        writer.Write(p.B);
                        writer.Write(p.G);
                        writer.Write(p.R);
                    }
                    // Записываем "мусорные" байты для выравнивания.
                    for (int i = 0; i < padding; i++)
                        writer.Write((byte)0);
                }
            }
        }

        /// <summary>
        /// Индексатор для получения и установки пикселя по координатам.
        /// Позволяет писать так: image[x, y] = new Pixel { ... };
        /// То есть, мы можем обращаться к пикселям как к элементам массива,
        /// за счет того, что в собственном классе реализован индексатор.
        /// Ключевое слово 'this' указывает на то, что это индексатор этого класса.
        /// </summary>
        public Pixel this[int x, int y]
        {
            get
            {
                // Блок 'get' срабатывает при чтении значения.
                // То есть, когда мы пишем var a = image[x, y]; - срабатывает этот блок.
                // ВНИМАНИЕ: Pixel - структура, поэтому здесь возвращается КОПИЯ пикселя.
                // Из-за этого строка image[x, y].R = 255; не скомпилируется (ошибка CS1612):
                // мы бы меняли временную копию, а не пиксель внутри изображения.
                // Правильно: Pixel p = image[x, y]; p.R = 255; image[x, y] = p;
                // или через ссылку: image.GetPixel(x, y).R = 255;
                return _pixels[x, y];
            }
            set
            {
                // Блок 'set' срабатывает при записи значения.
                // То есть, когда мы пишем image[x, y] = new Pixel { ... }; - срабатывает этот блок.
                // 'value' - это неявный параметр, который содержит то, что мы пытаемся записать.
                _pixels[x, y] = value;
            }
        }

        /// <summary>
        /// Возвращает ПРЯМУЮ ССЫЛКУ на пиксель. Необходимо для задания с 'ref'.
		/// Без 'ref' вы бы получили КОПИЮ пикселя, и все изменения пропали бы впустую.
        /// </summary>
        public ref Pixel GetPixel(int x, int y)
        {
            // Проверка на выход за границы массива для безопасности.
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new IndexOutOfRangeException($"Координаты ({x},{y}) выходят за пределы изображения.");

            // Возвращаем ссылку на пиксель. Использование 'ref' позволяет изменять пиксель напрямую.
            // В противном случае при попытке изменить возвращаемое значение мы бы изменяли копию, а не оригинал.
            // В свою очередь, это может быть полезно для оптимизации производительности при работе с большим количеством пикселей.
            // Вместо создания временной копии пикселя, мы можем работать с оригиналом напрямую.
            return ref _pixels[x, y];
        }
    }

}
