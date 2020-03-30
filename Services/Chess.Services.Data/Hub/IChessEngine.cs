namespace Chess.Services.Data
{
    public interface IChessEngine
    {
        void Start();

        void Save();

        void Load();

        void Clear();
    }
}
