using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    internal sealed class AutoPaddingMediaSource : IMediaSource
    {
        private static AutoPaddingMediaSource _instance = null;
        internal static AutoPaddingMediaSource Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("未被初始化。");
                }
                return _instance;
            }
        }

        internal static void Initialize(IMediaSource mediaSource)
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("已被初始化。");
            }

            _instance = new AutoPaddingMediaSource(mediaSource);
        }

        private AutoPaddingMediaSource(IMediaSource mediaSource)
        {
            if (mediaSource == null)
            {
                throw new ArgumentNullException("mediaSource");
            }

            this.MediaSource = mediaSource;
        }
        public TimeSpan? NativeDuration
        {
            get
            {
                return this.MediaSource.NativeDuration;
            }
        }

        public string Title
        {
            get
            {
                return this.MediaSource.Title;
            }
        }

        public IMediaSource MediaSource { get; private set; }

        

        //public PlayRange Adjust(PlayRange range)
        //{
        //    return this.MediaSource.Adjust(range);
        //}
    }
}
