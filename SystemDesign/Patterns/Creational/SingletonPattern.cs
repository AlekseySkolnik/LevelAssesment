namespace Patterns.Creational
{
    // Реализация на основе Lazy<T>
    // Достоинства: простота + потокобезопасность + «ленивость»!
    public sealed class LazySingleton
    {
        private static readonly Lazy<LazySingleton> _instance =
            new(() => new LazySingleton());
        LazySingleton() { }
        public static LazySingleton Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }

    // Реализация на основе блокировки с двойной проверкой
    public sealed class DoubleCheckedLock
    {
        // Поле должно быть volatile!
        private static volatile DoubleCheckedLock? _instance;
        private static readonly object _syncRoot = new();
        DoubleCheckedLock() { }
        public static DoubleCheckedLock Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        _instance ??= new DoubleCheckedLock();
                    }
                }

                return _instance;
            }
        }
    }
}
