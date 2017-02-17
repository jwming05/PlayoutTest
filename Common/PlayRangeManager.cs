using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutTest
{
    public abstract class PlayRangeManager
    {
        [System.ThreadStatic]
        private static PlayRangeManagerWithMaxDuration _withMaxDurationInstance = new PlayRangeManagerWithMaxDuration();

        [System.ThreadStatic]
        private static PlayRangeManagerWithoutMaxDuration _withoutMaxDurationInstance = new PlayRangeManagerWithoutMaxDuration();
        public static PlayRangeManager GetPlayRangeManager(TimeSpan? maxDuration)
        {
            if (maxDuration == null)
            {
                return _withoutMaxDurationInstance;
            }

            if (maxDuration.Value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("maxDuration", 
                    string.Format("<{0}>无效，不能小于TimeSpan.Zero。",maxDuration.Value));
            }


        }

        //public PlayRangeManager(IMediaSource mediaSource)
        //{
        //    this.MediaSource = mediaSource;
        //}

        //private IMediaSource MediaSource { get; set; }

        //internal PlayRange Adjust(PlayRange playRange)
        //{
        //    if(this.MediaSource.n)
        //}

        sealed class PlayRangeManagerWithMaxDuration : PlayRangeManager
        {
            public TimeSpan MaxDuration { get; set; }
        }

        sealed class PlayRangeManagerWithoutMaxDuration: PlayRangeManager
        {

        }
    }
}
