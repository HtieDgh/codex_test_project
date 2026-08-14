namespace codex.Services
{
    public interface IReader
    {
        public abstract IEnumerable<string[]> Read();
        public abstract IAsyncEnumerable<string[]> ReadAsync(CancellationToken cancellationToken = default);
    }
}
