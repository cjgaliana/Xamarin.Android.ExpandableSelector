using Android.Annotation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using Xamarin.Android.ExpandableSelector.Animation;

namespace Xamarin.Android.ExpandableSelector
{
    /// <summary>
    ///     FrameLayout extension used to show a list of ExpandableItems instances represented with Button
    ///     or ImageButton widgets which can be collapsed and expanded using an animation. The configurable
    ///     elements of the class are:
    ///     - List of items to show represented with ExpandableItem instances.
    ///     - Time used to perform the collapse/expand animations. Expressed in milliseconds.
    ///     - Show or hide the view background when the List of ExpandaleItems are collapsed.
    ///     - Configure a ExpandableSelectorListeners to be notified when the view is going to be
    ///     collapsed/expanded or has been collapsed/expanded.
    ///     - Configure a OnExpandableItemClickListener to be notified when an item is clicked.
    /// </summary>
    public class ExpandableSelector : FrameLayout
    {
        private static readonly int DEFAULT_ANIMATION_DURATION = 300;

        private List<View> _buttons = new List<View>();
        private ExpandableSelectorAnimator _expandableSelectorAnimator;
        private Drawable _expandedBackground;
        private bool _hideBackgroundIfCollapsed;

        public ExpandableSelector(Context context)
            : this(context, null)
        {
        }

        public ExpandableSelector(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public ExpandableSelector(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            this.InitializeView(attrs);
        }

        [TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
        public ExpandableSelector(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
            this.InitializeView(attrs);
        }

        public List<ExpandableItem> ExpandableItems { get; set; }

        /// <summary>
        ///     Returns true if the view is collapsed and false if the view is expanded.
        /// </summary>
        public bool IsCollapsed
        {
            get { return this._expandableSelectorAnimator.IsCollapsed; }
        }

        /// <summary>
        ///     Returns true if the view is expanded and false if the view is collapsed.
        /// </summary>
        public bool IsExpanded
        {
            get { return this._expandableSelectorAnimator.IsExpanded; }
        }

        public event EventHandler<ExpandableItemClickEventArgs> ItemClick;

        protected virtual void OnItemClick(int index ,ExpandableItem item)
        {
            EventHandler<ExpandableItemClickEventArgs> handler = this.ItemClick;
            if (handler != null)
            {
                handler(this, new ExpandableItemClickEventArgs(index, item));
            }
        }

        public event EventHandler Expanded;

        protected virtual void OnExpanded()
        {
            EventHandler handler = this.Expanded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler Collapsed;

        protected virtual void OnCollapsed()
        {
            EventHandler handler = this.Collapsed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler StartExpand;

        protected virtual void OnStartExpand()
        {
            EventHandler handler = this.StartExpand;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler StartCollapse;

        protected virtual void OnStartCollapse()
        {
            EventHandler handler = this.StartCollapse;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Configures a List ExpandableItem to be shown. By default, the list of ExpandableItems is
        ///     going to be shown collapsed. Please take into account that this method creates
        ///     ImageButton/Button widgets based on the size of the list passed as parameter. Don't use this
        ///     library as a RecyclerView and take into account the number of elements to show.
        /// </summary>
        /// <param name="expandableItems"></param>
        public void ShowExpandableItems(List<ExpandableItem> expandableItems)
        {
            this.ValidateExpandableItems(expandableItems);

            this.Reset();

            this.ExpandableItems = expandableItems;
            this.RenderExpandableItems();
            this.HookEvents();
            this.BringChildsToFront(expandableItems);
        }

        /// <summary>
        ///     Performs different animations to show the previously configured ExpandableItems transformed
        ///     into Button widgets. Notifies the ExpandableSelectorListener instance there was previously
        ///     configured.
        /// </summary>
        public void Expand()
        {
            this._expandableSelectorAnimator.AnimationFinished += delegate
            {
                this.UpdateBackground();
                this.OnExpanded();
            };
            this.OnStartExpand();
            this._expandableSelectorAnimator.Expand();
        }

        /// <summary>
        ///     Performs different animations to hide the previously configured ExpandableItems transformed
        ///     into Button widgets. Notifies the ExpandableSelectorListener instance there was previously
        ///     configured.
        /// </summary>
        public void Collapse()
        {
            this._expandableSelectorAnimator.AnimationFinished += delegate
            {
                this.UpdateBackground();
                this.OnCollapsed();
            };
            this.OnStartCollapse();
            this._expandableSelectorAnimator.Collapse();
        }

        /// <summary>
        ///     Given a position passed as parameter returns the ExpandableItem associated.
        /// </summary>
        /// <param name="expandableItemPosition"></param>
        /// <returns></returns>
        public ExpandableItem GetExpandableItem(int expandableItemPosition)
        {
            return this.ExpandableItems[expandableItemPosition];
        }

        /// <summary>
        ///     Changes the ExpandableItem associated to a given position and updates the Button widget to
        ///     show the new ExpandableItem information.
        /// </summary>
        /// <param name="expandableItemPosition"></param>
        /// <param name="expandableItem"></param>
        public void UpdateExpandableItem(int expandableItemPosition, ExpandableItem expandableItem)
        {
            this.ValidateExpandableItem(expandableItem);
            this.ExpandableItems.RemoveAt(expandableItemPosition);
            this.ExpandableItems.Insert(expandableItemPosition, expandableItem);
            int buttonPosition = this._buttons.Count - 1 - expandableItemPosition;
            this.ConfigureButtonContent(this._buttons[buttonPosition], expandableItem);
        }

        private void InitializeView(IAttributeSet attrs)
        {
            TypedArray attributes =
                this.Context.ObtainStyledAttributes(attrs, Resource.Styleable.expandable_selector);
            this.InitializeAnimationDuration(attributes);
            this.InitializeHideBackgroundIfCollapsed(attributes);
            this.InitializeHideFirstItemOnCollapse(attributes);
            attributes.Recycle();
        }

        private void InitializeHideBackgroundIfCollapsed(TypedArray attributes)
        {
            this._hideBackgroundIfCollapsed =
                attributes.GetBoolean(Resource.Styleable.expandable_selector_hide_background_if_collapsed, false);
            this._expandedBackground = this.Background;
            this.UpdateBackground();
        }

        private void InitializeAnimationDuration(TypedArray attributes)
        {
            int animationDuration =
                attributes.GetInteger(Resource.Styleable.expandable_selector_animation_duration,
                    DEFAULT_ANIMATION_DURATION);
            int expandInterpolatorId =
                attributes.GetResourceId(Resource.Styleable.expandable_selector_expand_interpolator,
                    global::Android.Resource.Animation.AccelerateInterpolator);
            int collapseInterpolatorId =
                attributes.GetResourceId(Resource.Styleable.expandable_selector_collapse_interpolator,
                    global::Android.Resource.Animation.DecelerateInterpolator);
            int containerInterpolatorId =
                attributes.GetResourceId(Resource.Styleable.expandable_selector_container_interpolator,
                    global::Android.Resource.Animation.DecelerateInterpolator);
            this._expandableSelectorAnimator = new ExpandableSelectorAnimator(this, animationDuration,
                expandInterpolatorId,
                collapseInterpolatorId, containerInterpolatorId);
        }

        private void InitializeHideFirstItemOnCollapse(TypedArray attributes)
        {
            bool hideFirstItemOnCollapsed =
                attributes.GetBoolean(Resource.Styleable.expandable_selector_hide_first_item_on_collapse, false);
            this._expandableSelectorAnimator.HideFirstItemOnCollapse = hideFirstItemOnCollapsed;
        }

        private void UpdateBackground()
        {
            if (!this._hideBackgroundIfCollapsed)
            {
                return;
            }
            if (this.IsExpanded)
            {
                this.Background = this._expandedBackground;
            }
            else
            {
                this.SetBackgroundResource(global::Android.Resource.Color.Transparent);
            }
        }

        private void Reset()
        {
            this.ExpandableItems = new List<ExpandableItem>();

            foreach (View button in this._buttons)
            {
                this.RemoveView(button);
            }

            this._buttons = new List<View>();
            this._expandableSelectorAnimator.Reset();
        }

        private void RenderExpandableItems()
        {
            int numberOfItems = this.ExpandableItems.Count;
            for (int i = numberOfItems - 1; i >= 0; i--)
            {
                View button = this.InitializeButton(i);
                this.AddView(button);
                this._buttons.Add(button);
                this._expandableSelectorAnimator.InitializeButton(button);
                this.ConfigureButtonContent(button, this.ExpandableItems[i]);
            }
            this._expandableSelectorAnimator.Buttons = this._buttons;
        }

        private void HookEvents()
        {
            int numberOfButtons = this._buttons.Count;
            bool thereIsMoreThanOneButton = numberOfButtons > 1;
            if (thereIsMoreThanOneButton)
            {
                this._buttons[numberOfButtons - 1].Click += (s, e) =>
                {
                    if (this.IsCollapsed)
                    {
                        this.Expand();
                    }
                    else
                    {
                        var button = s as View;
                        this.OnItemClick(0, this.ExpandableItems[numberOfButtons - 1]);
                    }
                };
            }

            for (int i = 0; i < numberOfButtons - 1; i++)
            {
                int buttonPosition = i;
                this._buttons[i].Click += (s, e) =>
                {
                    int buttonIndex = numberOfButtons - 1 - buttonPosition;
                    this.OnItemClick(buttonIndex, this.ExpandableItems[buttonIndex]);
                };
            }
        }

        private View InitializeButton(int expandableItemPosition)
        {
            try
            {
                ExpandableItem expandableItem = this.ExpandableItems[expandableItemPosition];
                View button = null;

                LayoutInflater layoutInflater = LayoutInflater.From(this.Context);
                button =
                    layoutInflater.Inflate(
                        expandableItem.HasTitle()
                            ? Resource.Layout.expandable_item_button
                            : Resource.Layout.expandable_item_image_button,
                        this, false);

       
                button.Visibility = expandableItemPosition == 0 
                    ? ViewStates.Visible 
                    : ViewStates.Invisible;

                return button;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

        public void ConfigureButtonContent(View view, ExpandableItem expandableItem)
        {
            if (expandableItem.HasBackgroundId())
            {
                int backgroundId = expandableItem.BackgroundId;
                view.SetBackgroundResource(backgroundId);
            }
            if (expandableItem.HasTitle())
            {
                var button = view as Button;
                if (button==null)
                {
                    return;
                }
                string text = expandableItem.Title;
                button.Text = text;
            }
            if (expandableItem.HasResourceId())
            {
                var imageButton = view as ImageButton;
                if (imageButton== null)
                {
                    return;
                }
                int resourceId = expandableItem.ResourceId;
                imageButton.SetImageResource(resourceId);
            }
        }

        private void ValidateExpandableItem(ExpandableItem expandableItem)
        {
            if (expandableItem == null)
            {
                throw new ArgumentNullException("expandableItem",
                    "You can't use a null instance of ExpandableItem as parameter.");
            }
        }

        private void ValidateExpandableItems(List<ExpandableItem> expandableItems)
        {
            if (expandableItems == null)
            {
                throw new ArgumentNullException("expandableItems",
                    "The List<ExpandableItem> passed as argument can't be null");
            }
        }

        private void BringChildsToFront(List<ExpandableItem> expandableItems)
        {
            int childCount = this.ChildCount;

            int numberOfExpandableItems = expandableItems.Count;
            if (childCount > numberOfExpandableItems)
            {
                for (int i = 0; i < childCount - numberOfExpandableItems; i++)
                {
                    this.GetChildAt(i).BringToFront();
                }
            }
        }
    }

    public class ExpandableItemClickEventArgs : EventArgs
    {
        public int Index { get; private set; }
        public ExpandableItem Item { get; private set; }

        public ExpandableItemClickEventArgs(int index, ExpandableItem item)
        {
            this.Index = index;
            this.Item = item;
        }
    }
}