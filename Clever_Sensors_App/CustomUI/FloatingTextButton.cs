using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Clever_Sensors_App
{
    public class FloatingTextButton : FrameLayout
    {
        private CardView container;
        private ImageView leftIconView;
        private TextView titleView;
        private string text;
        private int textColor;
        private Drawable leftIcon;
        private int background;

        public FloatingTextButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            InflateLayout(context);
            InitAttributes(attrs);
            InitView();
        }

        public string Text
        {
            get { return text;  }
            set
            {
                text = value;
                titleView.Text = value;
                if (string.IsNullOrEmpty(value))
                    titleView.Visibility = ViewStates.Gone;
                else
                    titleView.Visibility = ViewStates.Visible;
            }            
        }

        public void SetTitleColor( int color)
        {
            textColor = color;
            titleView.SetTextColor(ConvertDecimalToColor(color));
        }

        public void SetBackgroundColor( int color)
        {
            background = color;
            container.SetCardBackgroundColor(ConvertDecimalToColor(color));
        }        

        public Drawable LeftIconDrawable
        {
            set
            {                
                if (value != null)
                {
                    leftIcon = value;
                    leftIconView.Visibility = ViewStates.Visible;
                    leftIconView.SetImageDrawable(value);
                }
                else
                    leftIconView.Visibility = ViewStates.Gone;
            }
            get
            {
                return leftIcon;
            }
        }

        public override void SetOnClickListener(IOnClickListener listener)
        {
            container.SetOnClickListener(listener);
        }

        public override void SetOnLongClickListener(IOnLongClickListener listener)
        {
            container.SetOnLongClickListener(listener);
        }

        private void InflateLayout(Context context)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.FloatingTextButtonView, this, true);
            container = view.FindViewById<CardView>(Resource.Id.layout_button_container);
            leftIconView = view.FindViewById<ImageView>(Resource.Id.layout_button_image_left);
            titleView = view.FindViewById<TextView>(Resource.Id.layout_button_text);
        }

        private void InitAttributes(IAttributeSet attrs)
        {
            TypedArray styleable = Context.ObtainStyledAttributes(attrs, Resource.Styleable.FloatingTextButton,0,0);
            text = styleable.GetString(Resource.Styleable.FloatingTextButton_floating_title);
            textColor = styleable.GetColor(Resource.Styleable.FloatingTextButton_floating_title_color, Color.Black);
            leftIcon = styleable.GetDrawable(Resource.Styleable.FloatingTextButton_floating_left_icon);
            background = styleable.GetColor(Resource.Styleable.FloatingTextButton_floating_background_color, Color.White);
            styleable.Recycle();
        }

        private void InitView()
        {
            Text= text;
            SetTitleColor(textColor);
            LeftIconDrawable = leftIcon;
            SetBackgroundColor(background);
            InitViewRadius();
            container.SetContentPadding(
                    GetHorizontalPaddingValue(16),
                    GetVerticalPaddingValue(8),
                    GetHorizontalPaddingValue(16),
                    GetVerticalPaddingValue(8)
            );            
        }

        private void InitViewRadius()
        {
            container.Post(() =>
            {
                container.Radius = container.Height / 2;
            });
        }
        
        private int GetVerticalPaddingValue(int dp)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                return DpToPx(Context, dp) / 4;
            else
                return DpToPx(Context, dp);
        }

        private int GetHorizontalPaddingValue(int dp)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                return DpToPx(Context, dp) / 2;
            else
                return DpToPx(Context, dp);
        }

        private int DpToPx(Context context, int dp)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip,dp, context.Resources.DisplayMetrics);
        }

        private Color ConvertDecimalToColor(int colorInt)
        {
            int a = (colorInt >> 24) & 0xff;
            int r = (colorInt >> 16) & 0xff;
            int g = (colorInt >> 8) & 0xff;
            int b = (colorInt) & 0xff;
            return Color.Argb(a, r, g, b);
        }
    }
}