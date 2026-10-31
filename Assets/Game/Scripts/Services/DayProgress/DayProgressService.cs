namespace Game.Scripts.Services.DayProgress
{
    public class DayProgressService : IDayProgressService
    {
        private const int FirstDay = 1;

        public int CurrentDay { get; set; } = FirstDay;

        public void Restart()
        {
            CurrentDay = FirstDay;
        }
    }
}
