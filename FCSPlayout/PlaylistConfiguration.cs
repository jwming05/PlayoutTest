using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSPlayout
{
    public abstract class PlaylistConfiguration
    {
        internal PlaylistConfiguration()
        {
        }

        /// <summary>
        /// 初始化用于自动垫片的节目源。
        /// </summary>
        /// <param name="mediaSource">指定用于自动垫片的节目源。</param>
        /// <remarks>不能重复初始化。</remarks>
        public void InitAutoPaddingSource(IMediaSource mediaSource)
        {
            AutoPaddingMediaSource.Initialize(mediaSource);
        }

        /// <summary>
        /// 获取或设置播放项的最小时长。
        /// 该值必须大于TimeSpan.Zero。
        /// </summary>
        /// <remarks>
        /// 顺播的切片的时长可能小于该时长。
        /// </remarks>
        public TimeSpan MinPlayDuration { get; set; }
    }
}
