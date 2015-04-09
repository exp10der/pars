using System;

namespace ConsoleApplication1
{
    /// <summary>
    /// Класс для извлечения нужной информации из разнородного текста
    /// </summary>
    public class TextScanner
    {
        //--- members ---------------------------------------------------------
        string mText;
        int mPosition;
        int mStartRead;


        //--- public ----------------------------------------------------------
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="text">Текст для обработки</param>
        public TextScanner(string text)
        {
            mText = text;
            mPosition = 0;
            mStartRead = -1;
        }
        public void refresh()
        {
            mPosition = 0;
        }
        /// <summary>
        /// Перемещение указателя в конец текста
        /// </summary>
        public void GoToEnd()
        {
            mPosition = mText.Length;
        }

        /// <summary>
        /// Перемещение указателя на начало искомого текста
        /// </summary>
        /// <param name="text">Искомый текст</param>
        public void GoTo(string text)
        {
            if (!TryGoTo(text))
                throw new Exception("Неверный формат текста");
        }

        /// <summary>
        /// Попытка перемещения указателя на начало искомого текста
        /// </summary>
        /// <param name="text">Искомый текст</param>
        /// <returns>true в случае удачи</returns>
        public bool TryGoTo(string text)
        {
            int p = mText.IndexOf(text, mPosition);

            if (p == -1)
                return false;

            mPosition = p;
            return true;
        }

        /// <summary>
        /// Перемещение указателя за искомый текст
        /// </summary>
        /// <param name="text">Искомый текст</param>
        public void Skip(string text)
        {
            if (!TrySkip(text))
                throw new Exception("Неверный формат текста");
        }

        /// <summary>
        /// Попытка перемещения указателя за искомый текст
        /// </summary>
        /// <param name="text">Искомый текст</param>
        /// <returns>true в случае удачи</returns>
        public bool TrySkip(string text)
        {
            int p = mText.IndexOf(text, mPosition);

            if (p == -1)
                return false;

            mPosition = p + text.Length;
            return true;
        }

        /// <summary>
        /// Начать чтение с текущего места
        /// </summary>
        public void BeginRead()
        {
            if (mPosition == mText.Length)
                throw new Exception("Указатель в конце текста");

            mStartRead = mPosition;
        }

        /// <summary>
        /// Завершить чтение
        /// </summary>
        /// <returns>Прочитанный текст</returns>
        public string EndRead()
        {
            if (mStartRead == -1)
                throw new Exception("Необходимо предварительно начать чтение");

            string r = mText.Substring(mStartRead, mPosition - mStartRead);
            mStartRead = -1;
            return r;
        }

        /// <summary>
        /// Прочитать текст с текущей позиции до указанного текста
        /// </summary>
        /// <param name="text">Стоп-текст</param>
        /// <returns>Прочитанный текст</returns>
        public string ReadTo(string text)
        {
            BeginRead();
            GoTo(text);
            return EndRead();
        }
    }
}