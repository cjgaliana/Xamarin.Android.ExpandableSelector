namespace Xamarin.Android.ExpandableSelector
{
    /// <summary>
    ///     Contains all the information needed to render a expandable item inside a ExpandableSelector
    ///     widget. The information you can render is a Drawable identifier, a String used as title and a
    ///     Drawable used as background.
    /// </summary>
    public class ExpandableItem
    {
        private const int NO_ID = -1;

        public string Title { get; set; }
        public int ResourceId { get;  set; }
        public int BackgroundId { get; set; }

        public ExpandableItem()
            : this(NO_ID, null)
        {
        }

        public ExpandableItem(int backgroundId)
            : this(backgroundId, null)
        {
        }

        public ExpandableItem(string title)
            : this(NO_ID, title)
        {
        }

        private ExpandableItem(int backgroundId, string title)
        {
            this.BackgroundId = backgroundId;
            this.Title = title;
        }

        public bool HasResourceId()
        {
            return this.ResourceId != NO_ID;
        }

        public bool HasBackgroundId()
        {
            return this.BackgroundId != NO_ID;
        }

        public bool HasTitle()
        {
            return !string.IsNullOrEmpty(this.Title);
        }
    }
}