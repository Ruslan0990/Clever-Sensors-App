using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.View.Animation;
using Android.Util;
using Android.Views;
using System;

namespace Clever_Sensors_App
{
    public class SnackbarBehavior : CoordinatorLayout.Behavior  {

        private static readonly FastOutSlowInInterpolator HIDE_INTERPOLATOR = new FastOutSlowInInterpolator();
        private const long HIDE_DURATION = 250L;
        private ViewPropertyAnimatorCompat animation = null;

        public SnackbarBehavior(Context context, IAttributeSet attrs): base(context, attrs)
        {
        }
        public override bool LayoutDependsOn(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
        {
            return dependency is Snackbar.SnackbarLayout;
        }

        public override bool OnDependentViewChanged(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
        {
            View _child = child as View;
            if (_child.TranslationY > 0)
            {
                return true;
            }
            if (animation != null)
            {
                animation.Cancel();
                animation = null;
            }
            _child.TranslationY = Math.Min(0f, dependency.TranslationY - dependency.Height);
            return true;
        }

        public override void OnDependentViewRemoved(  CoordinatorLayout parent, Java.Lang.Object child,View dependency)
        {
            View _child = child as View;
            if (dependency is Snackbar.SnackbarLayout) {

                animation = ViewCompat.Animate(_child)
                        .TranslationY(0f)
                        .SetInterpolator(HIDE_INTERPOLATOR)
                        .SetDuration(HIDE_DURATION);

                animation.Start();
            }
            base.OnDependentViewRemoved(parent, child, dependency);
        }
    }
}