using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    internal sealed class NormalPlaySource : PlaySource //IPlaySource
    {
        internal static NormalPlaySource CreateAutoPadding(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("duration", string.Format("<{0}>值无效，必须大于TimeSpan.Zero。", duration));
            }

            return new NormalPlaySource(AutoPaddingMediaSource.Instance, new PlayRange(duration));
        }

        internal NormalPlaySource(IPlaySource playSource)
        {
            if (playSource == null)
            {
                throw new ArgumentNullException("playSource");
            }

            var playRange = playSource.Adjust(playSource.PlayRange);

            this.MediaSource = playSource.MediaSource;
            this.PlayRange = playRange;
        }

        internal NormalPlaySource(IMediaSource mediaSource, PlayRange range)
        {
            if (mediaSource == null)
            {
                throw new ArgumentNullException("mediaSource");
            }

            range = mediaSource.Adjust(range);

            this.MediaSource = mediaSource;
            this.PlayRange = range;
        }
        internal NormalPlaySource(MarkableMediaSource markableSource)
        {
            if (markableSource == null)
            {
                throw new ArgumentNullException("markableSource");
            }

            this.MediaSource = markableSource.MediaSource;
            this.PlayRange = markableSource.PlayRange;
        }
    }
}
