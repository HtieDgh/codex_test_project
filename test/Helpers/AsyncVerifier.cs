namespace test.Helpers
{
    public static class AsyncVerifier
    {

        /// <summary>
        /// Проверка заключается в ожидании незавершенности задачи на момент проверки. 
        /// Если метод не завершил задачу до проверки, то это может указывать на асинхронное выполнение.
        /// </summary>
        /// <param name="asyncMethod"></param>
        /// <returns></returns>
        public static async Task<bool> IsMethodAsync(Func<CancellationToken,Task> asyncMethod)
        {

            // Запускаем метод
            var task = asyncMethod(CancellationToken.None);

            // Проверяем, что метод не завершился синхронно
            if (task.IsCompleted)
            {
                return false; // Метод выполнился синхронно
            }

            await task;
            return true;

        }
    }
}
