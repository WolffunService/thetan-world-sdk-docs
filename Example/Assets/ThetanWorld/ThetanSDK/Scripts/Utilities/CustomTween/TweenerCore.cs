namespace Wolffun.Tweening
{
    public abstract class TweenerCore
    {
        private bool _isActive = false;

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }
        public abstract bool IsStarted { get; }
        public abstract void ResetValue();
        public abstract bool IsComplete { get; }
        public abstract float Duration { get; }
        public abstract float CurrentTime { get; }
        public abstract bool IsIndependentTimeScale { get; }

        public abstract void UpdateWithDeltaTime(float dt);
    }
}