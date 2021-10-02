namespace LD49
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface ICoroutineManager
    {
        void Start(IEnumerator coroutine);
    }

    public class Coroutine
    {
        private IEnumerator _coroutine;
        private Resume _resumeCondition;

        private Coroutine(IEnumerator coroutine)
        { 
            _coroutine = coroutine;
            _resumeCondition = Resume.NextFrame;
        }

        public bool IsCompleted { get; private set; }

        private void Update(GameTime gameTime)
        {
            if (IsCompleted) return;

            _resumeCondition.Update(gameTime);

            if (!_resumeCondition.IsMet) return;

            if (!_coroutine.MoveNext())
            {
                IsCompleted = true;
                return;
            }
            
            _resumeCondition = _coroutine.Current as Resume;
            if (_resumeCondition is null)
            {
                throw new Exception("Coroutines must provide a Coroutine.Resume directive");
            }            
        }

        public abstract class Resume
        {
            private Resume() { }

            public abstract bool IsMet { get; }

            public virtual void Update(GameTime gameTime) { }

            private class CoroutineResumeNextFrame : Resume
            {
                public override bool IsMet => true;
            }

            public static Resume NextFrame => new CoroutineResumeNextFrame();

            private class CoroutineResumeAfterDuration : Resume
            {
                private TimeSpan _duration;
                private TimeSpan _elapsed;

                public CoroutineResumeAfterDuration(TimeSpan duration) => _duration = duration;

                public override bool IsMet => _elapsed >= _duration;

                public override void Update(GameTime gameTime)
                {
                    if (IsMet) return;
                    _elapsed += TimeSpan.FromMilliseconds(gameTime.ElapsedGameTime.TotalMilliseconds);
                }
            }

            public static Resume AfterDuration(int milleseconds) =>
                new CoroutineResumeAfterDuration(TimeSpan.FromMilliseconds(milleseconds));

            private class CoroutineResumeWhen : Resume
            {
                private Func<bool> _condition;
                private bool _isMet;

                public CoroutineResumeWhen(Func<bool> condition) => _condition = condition;

                public override bool IsMet => _isMet;

                public override void Update(GameTime gameTime) => _isMet = _condition();
            }

            public static Resume When(Func<bool> condition) => new CoroutineResumeWhen(condition);
        }

        public class Manager : GameComponent, ICoroutineManager
        {
            private List<Coroutine> _coroutines = new List<Coroutine>();

            public Manager(Game game) : base(game) { }

            public void Start(IEnumerator coroutine) => _coroutines.Add(new Coroutine(coroutine));

            public override void Update(GameTime gameTime)
            {
                _coroutines.ForEach(c => c.Update(gameTime));
                _coroutines.RemoveAll(c => c.IsCompleted);
            }
        }
    }
}
