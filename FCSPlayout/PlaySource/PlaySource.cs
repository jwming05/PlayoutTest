namespace FCSPlayout
{
    public abstract class PlaySource : IPlaySource
    {

        internal PlaySource()
        {
        }

        public IMediaSource MediaSource
        {
            get;protected set;
        }

        public PlayRange PlayRange
        {
            get; protected set;
        }

        public virtual string Title
        {
            get
            {
                return this.MediaSource.Title;
            }
        }
    }
}
