using System;
using System.Collections.Generic;
using System.Linq;
using Android.Animation;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace Xamarin.Android.ExpandableSelector.Animation
{
    public class ExpandableSelectorAnimator
    {
        private const string Y_ANIMATION = "translationY";
        private const float CONTAINER_ANIMATION_OFFSET = 1.16f;

        private readonly int _animationDuration;
        private readonly int _collapseInterpolatorId;
        private readonly View _container;
        private readonly int _containerInterpolatorId;
        private readonly int _expandInterpolatorId;

        public ExpandableSelectorAnimator(View container, int animationDuration, int expandInterpolatorId,
            int collapseInterpolatorId, int containerInterpolatorId)
        {
            this._container = container;
            this._animationDuration = animationDuration;
            this._expandInterpolatorId = expandInterpolatorId;
            this._collapseInterpolatorId = collapseInterpolatorId;
            this._containerInterpolatorId = containerInterpolatorId;

            this.IsCollapsed = true;
        }

        /// <summary>
        ///     Configures the ExpandableSelectorAnimator to change the first item visibility to View.VISIBLE
        ///     View.INVISIBLE once the collapse/expand animation has been performed.
        /// </summary>
        public bool HideFirstItemOnCollapse { get; set; }

        /// <summary>
        ///     Configures the List of buttons used to calculate the animation parameters.
        /// </summary>
        public List<View> Buttons { get; set; }

        public bool IsCollapsed { get; private set; }

        /// <summary>
        ///     Returns true if the ExpandableSelector widget is expanded or false if is collapsed.
        /// </summary>
        public bool IsExpanded
        {
            get { return !this.IsCollapsed; }
        }

        public event EventHandler AnimationFinished;

        /// <summary>
        ///     Expands the ExpandableSelector performing a resize animation and at the same time moves the
        ///     buttons configures as childrens to the associated position given the order in the ListView
        ///     used to keep the reference to the buttons. The visibility of the buttons inside the
        ///     ExpandableSelector changes to View.VISIBLE before to perform the animation.
        /// </summary>
        public void Expand()
        {
            this.IsCollapsed = false;
            this.ChangeButtonsVisibility(ViewStates.Visible);
            this.ExpandButtons();
            this.ExpandContainer();
        }

        /// <summary>
        ///     Collapses the ExpandableSelector performing a resize animation and at the same time moves the
        ///     buttons configures as childrens to the associated position given the order in the List
        ///     View
        ///     used to keep the reference to the buttons. The visibility of the buttons inside the
        ///     ExpandableSelector changes to View.INVISIBLE after the resize animation.
        /// </summary>
        public void Collapse()
        {
            this.IsCollapsed = true;
            this.IsCollapsed = true;
            this.CollapseButtons();
            this.CollapseContainer();
        }

        /// <summary>
        ///     Configures the Button/ImageButton added to the ExpandableSelector to match with the initial
        ///     configuration needed by the component.
        /// </summary>
        /// <param name="button"></param>
        public void InitializeButton(View button)
        {
            this.ChangeGravityToBottomCenterHorizontal(button);
        }

        /// <summary>
        ///     Returns the component to the initial state without remove configuration related to animation
        ///     durations of if the first item visibility has to be changed.
        /// </summary>
        public void Reset()
        {
            this.Buttons = new List<View>();
            this.IsCollapsed = true;
        }

        private void ExpandButtons()
        {
            int numberOfButtons = this.Buttons.Count;
            var animations = new Animator[numberOfButtons];
            for (int i = 0; i < numberOfButtons; i++)
            {
                View button = this.Buttons[i];
                ITimeInterpolator interpolator = this.GetExpandAnimatorInterpolation();
                float toY = this.CalculateExpandedYPosition(i);
                animations[i] = this.CreateAnimatorForButton(interpolator, button, toY);
            }
            this.PlayAnimatorsTogether(animations);
        }

        private void CollapseButtons()
        {
            int numberOfButtons = this.Buttons.Count;
            ITimeInterpolator interpolator = this.GetCollapseAnimatorInterpolation();
            var animations = new Animator[numberOfButtons];
            for (int i = 0; i < numberOfButtons; i++)
            {
                View button = this.Buttons[i];
                int toY = 0;
                animations[i] = this.CreateAnimatorForButton(interpolator, button, toY);
            }
            this.PlayAnimatorsTogether(animations);
        }

        private void ExpandContainer()
        {
            int toWidth = this._container.Width;
            int toHeight = this.GetSumHeight();
            IInterpolator interpolator = this.GetContainerAnimationInterpolator();

            ResizeAnimation resizeAnimation = this.CreateResizeAnimation(toWidth, interpolator, toHeight);
            this._container.Animation.AnimationEnd += delegate
            {
                this.ChangeButtonsVisibility(ViewStates.Visible);
                this.OnAnimationFinished();
            };
            this._container.StartAnimation(resizeAnimation);
        }

        private void CollapseContainer()
        {
            int toWidth = this._container.Width;
            float toHeight = this.GetFirstItemHeight();
            IInterpolator interpolator = this.GetContainerAnimationInterpolator();

            ResizeAnimation resizeAnimation = this.CreateResizeAnimation(toWidth, interpolator, toHeight);

            this._container.Animation.AnimationEnd += delegate
            {
                this.ChangeButtonsVisibility(ViewStates.Invisible);
                this.OnAnimationFinished();
            };

            this._container.StartAnimation(resizeAnimation);
        }

        private ObjectAnimator CreateAnimatorForButton(ITimeInterpolator interpolator, View button, float toY)
        {
            ObjectAnimator objectAnimator = ObjectAnimator.OfFloat(button, Y_ANIMATION, toY);

            objectAnimator.SetInterpolator(interpolator);

            objectAnimator.SetDuration(this._animationDuration);
            return objectAnimator;
        }

        private ResizeAnimation CreateResizeAnimation(float toWidth, IInterpolator interpolator, float toHeight)
        {
            var resizeAnimation = new ResizeAnimation(this._container, toWidth, toHeight)
            {
                Interpolator = interpolator,
                Duration = (long)(this._animationDuration * CONTAINER_ANIMATION_OFFSET)
            };

            resizeAnimation.AnimationEnd += delegate { this.OnAnimationFinished(); };

            return resizeAnimation;
        }

        private void PlayAnimatorsTogether(Animator[] animations)
        {
            var animatorSet = new AnimatorSet();
            animatorSet.PlayTogether(animations);
            animatorSet.Start();
        }

        private float CalculateExpandedYPosition(int buttonPosition)
        {
            int numberOfButtons = this.Buttons.Count;
            int y = 0;
            for (int i = numberOfButtons - 1; i > buttonPosition; i--)
            {
                View button = this.Buttons[i];
                y = y - button.Height - this.GetMarginRight(button) - this.GetMarginLeft(button);
            }
            return y;
        }

        private void ChangeButtonsVisibility(ViewStates visibility)
        {
            int lastItem = this.HideFirstItemOnCollapse
                ? this.Buttons.Count
                : this.Buttons.Count - 1;
            for (int i = 0; i < lastItem; i++)
            {
                View button = this.Buttons[i];
                button.Visibility = visibility;
            }
        }

        private ITimeInterpolator GetExpandAnimatorInterpolation()
        {
			try {
				return AnimationUtils.LoadInterpolator(this._container.Context, this._expandInterpolatorId);

			} catch (Exception ex) {
				return null;
			}
        }

        private ITimeInterpolator GetCollapseAnimatorInterpolation()
        {
			try {
				return AnimationUtils.LoadInterpolator(this._container.Context, this._collapseInterpolatorId);

			} catch (Exception ex) {
				return null;
			}
        }

        private IInterpolator GetContainerAnimationInterpolator()
        {
			try {
				return AnimationUtils.LoadInterpolator(this._container.Context, this._containerInterpolatorId);

			} catch (Exception ex) {
				return null;
			}
        }

        private int GetSumHeight()
        {
            return this.Buttons.Sum(button => button.Height + this.GetMarginRight(button) + this.GetMarginLeft(button));
        }

        private int GetMarginRight(View view)
        {
            var layoutParams = (FrameLayout.LayoutParams)view.LayoutParameters;
            return layoutParams.RightMargin;
        }

        private int GetMarginLeft(View view)
        {
            var layoutParams = (FrameLayout.LayoutParams)view.LayoutParameters;
            return layoutParams.LeftMargin;
        }

        private float GetFirstItemHeight()
        {
            View firstButton = this.Buttons[0];
            int height = firstButton.Height;

            var layoutParams = (FrameLayout.LayoutParams)firstButton.LayoutParameters;
            int topMargin = layoutParams.TopMargin;
            int bottomMargin = layoutParams.BottomMargin;

            return height + topMargin + bottomMargin;
        }

        private void ChangeGravityToBottomCenterHorizontal(View view)
        {
            var layoutParams = (FrameLayout.LayoutParams)view.LayoutParameters;
            layoutParams.Gravity = GravityFlags.Bottom | GravityFlags.CenterHorizontal;

            //ViewGroup.LayoutParams parameters = view.LayoutParameters;
            //var newLayoutParameters = new FrameLayout.LayoutParams(parameters.Width, parameters.Height,
            //    GravityFlags.Bottom | GravityFlags.CenterHorizontal);

            //view.LayoutParameters = newLayoutParameters;
        }

        private void OnAnimationFinished()
        {
            EventHandler handler = this.AnimationFinished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}