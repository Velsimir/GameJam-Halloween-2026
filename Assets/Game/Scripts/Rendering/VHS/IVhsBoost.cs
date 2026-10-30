namespace Game.Scripts.Rendering.VHS
{
    public interface IVhsBoost
    {
        float Weight { get; }

        void Pulse(float peak, float attack, float hold, float release);

        void SetTarget(float weight, float smoothTime);

        void Clear();
    }
}
