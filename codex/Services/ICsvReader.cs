namespace codex.Services
{
    /// <summary>
    /// Базовый класс для классов CsvReader
    /// </summary>
    public abstract class ICsvReader : IReader
    {
        protected TextReader reader_;
        public abstract IEnumerable<string[]> Read();

        public abstract IAsyncEnumerable<string[]> ReadAsync(CancellationToken cancellationToken = default);

        public ICsvReader(TextReader reader)
        {
            reader_ = reader;
            if(reader_.Peek() == -1)
            {
                throw new FileNotFoundException("reader is empty");
            }
        }
        ~ICsvReader() {
            reader_.Dispose();
        }
    }
}
