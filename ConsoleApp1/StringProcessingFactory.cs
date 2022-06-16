using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace TextFrequencyAnalysis
{
    /// <summary>
    /// Класс реализации обработки строк
    /// </summary>
    public class StringProcessingFactory
    {
        /// <summary>
        /// Путь к файлу
        /// </summary>
        private string _filePath = string.Empty;

        /// <summary>
        /// Текст из файла
        /// </summary>
        private string _fileText = string.Empty;

        /// <summary>
        /// Словарь данных для разных потоков
        /// </summary>
        private Dictionary<string, string[]> _dictionaryDataForThread = new Dictionary<string, string[]>();

        /// <summary>
        /// Отсчёт времени
        /// </summary>
        Stopwatch stopwatch = new Stopwatch();
      
        public StringProcessingFactory(string filePath)
        {
            _filePath = filePath;
        }

        public void InitAsync()
        {
            stopwatch.Start();

            try
            {
                //Валидация
                if (!CheckValidation())
                {
                    return;
                }

                //Чтение из файла и удаление знаков пунктуации
                PunctuationHandler();

                //Нарезка данных для разных потоков
                CutWordsForThread();

                //Запуск нескольких потоков для подсчёта разделённых данных
                Threadlaunch();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка. {ex.Message}");
            }
        }

        /// <summary>
        /// Запускает 2 параллельных потока
        /// </summary>
        private void Threadlaunch()
        {
            for (int i = 1; i < 3; i++)
            {
                Thread myThread = new(MethodThread);

                myThread.Name = $"{i}";
                myThread.Start();
            }
        }

        /// <summary>
        /// Запуск вычисления данных в нескольких потоках
        /// </summary>
        private void MethodThread()
        {
            CalculationData();


            //1. Получаем данные из всех потоков в виде одного не уникального массив
            //2. Парсим данные по условному разделителю
            //3. Производим группировку, чтобы сделать массив уникальным
            //4. Формируем 

            var endData = _dictionaryDataForThread.SelectMany(w => w.Value)
                .Select(e => new
                {
                    Key = e.Split('|')[0],
                    Value = e.Split('|')[1]
                })
                .GroupBy(w => w.Key)
                .ToDictionary(q =>
                    q.Select(e => e.Key).FirstOrDefault(),
                    w => w.Sum(r => Convert.ToInt32(r.Value))
                ).OrderByDescending(e => e.Value)
                .Take(10);

            //Выводить данные только в главном потоке
            if (Thread.CurrentThread.Name == "1")
            {
                foreach (var e in endData)
                {
                    Console.WriteLine($"Триплет: [{e.Key}] Повторений: {e.Value}");
                }

                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Метод расчёта триплетов для участка данных
        /// </summary>
        private void CalculationData()
        {
            //Ключ потока. Он же ключ в общем контейнере с данными
            var keyThread = Thread.CurrentThread.Name;

            //Получаем контейнер слов соответствующий текущему потоку по принципу (KolLenght / KolThread)
            //KolLenght - Общее кол-во слов
            //KolThread - Кол-во потоков
            var words = _dictionaryDataForThread.Where(q => q.Key == Thread.CurrentThread.Name)
                .SelectMany(q => q.Value)
                .ToArray();

            List<string> strings = new List<string>();

            for (int i = 0; i < words.Length; i++)
            {
                while (words[i].Length > 2)
                {
                    strings.Add(words[i].Substring(0, 3));
                    words[i] = words[i].Substring(3);
                }

            }

            _dictionaryDataForThread[keyThread] = strings.GroupBy(q => q)
                .OrderByDescending(g => g.Count())
                .Select(gr => $"{gr.Key}|{gr.Count()}")
                .ToArray();
        }

        /// <summary>
        /// Нарезка данных для разных потоков
        /// </summary>
        private void CutWordsForThread()
        {
            string[] words = _fileText.Split(' ');
            var wordLength = words.Length;

            //Для нарезки данных на 3 и более потоков можно добавить цикл
            _dictionaryDataForThread.Add("1", words.Take(words.Length / 2).ToArray());

            if (wordLength % 2 != 0)
            {
                _dictionaryDataForThread.Add("2", words.Skip(wordLength / 2).Take(wordLength / 2 + 1).ToArray());
            }
            else
            {
                _dictionaryDataForThread.Add("2", words.Skip(wordLength / 2).Take(wordLength / 2).ToArray());
            }
        }

        /// <summary>
        /// Метод чтения данных из файла
        /// </summary>
        private void PunctuationHandler()
        {
            using (FileStream fstream = File.OpenRead(_filePath))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);

                _fileText = Encoding.Default.GetString(buffer);
                _fileText = Regex.Replace(_fileText, "[-.?!)(,:]", "");
            }
        }
        
        /// <summary>
        /// Валидация входных параметров
        /// </summary>
        private bool CheckValidation()
        {
            return CheckInputParam() && CheckingFileExists();
        }

        /// <summary>
        /// Проверка входных параметров
        /// </summary>
        /// <param name="filePath">Параметр</param>
        private bool CheckInputParam()
        {
            if (_filePath == String.Empty)
            {
                throw new Exception("Не валидный путь");
            }

            return true;
        }

        /// <summary>
        /// Проверка на существование файла
        /// </summary>
        /// <param name="filePath">Параметр</param>
        private bool CheckingFileExists()
        {
            if (!File.Exists(_filePath))
            {
                throw new Exception("Файл не существует");
            }

            return true;
        }

    }
}
