using Android.Views;
using Android.Views.Animations;

namespace Xamarin.Android.ExpandableSelector.Animation
{
    /// <summary>
    /// Animation extension created to resize a widget in two dimensions given the from and to width and
    /// height. This Animation changes the width and the height associated to the widget and invokes
    /// requestLayout() method to redraw it.
    /// </summary>
    public sealed class ResizeAnimation : global::Android.Views.Animations.Animation
    {
        private const long DEFAULT_DURATION_IN_MS = 250;

        private readonly View _view;
        private readonly float _toHeight;
        private readonly float _fromHeight;
        private readonly float _toWidth;
        private readonly float _fromWidth;

        public ResizeAnimation(View view, float toWidth, float toHeight)
        {
            this._toHeight = toHeight;
            this._toWidth = toWidth;
            this._fromHeight = view.Height;
            this._fromWidth = view.Width;
            this._view = view;

            this.Duration = DEFAULT_DURATION_IN_MS;
        }

        protected override void ApplyTransformation(float interpolatedTime, Transformation t)
        {
            var height = (this._toHeight - this._fromHeight) * interpolatedTime + this._fromHeight;
            var width = (this._toWidth - this._fromWidth) * interpolatedTime + this._fromWidth;

            var layoutParams = this._view.LayoutParameters;
            layoutParams.Height = (int)height;
            layoutParams.Width = (int)width;

            this._view.RequestLayout();
        }

        public override bool WillChangeBounds()
        {
            return true;
        }
    }
}