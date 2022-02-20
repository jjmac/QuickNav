namespace Rowlan.QuickNav
{
    /// <summary>
    /// Indicate whether startup action has been performed or not
    /// </summary>
    public class Startup
    {
        private static Startup _instance;

        public static Startup Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Startup();
                }

                return _instance;
            }
        }

        public bool Initialized { get; set; } = false;

    }
}