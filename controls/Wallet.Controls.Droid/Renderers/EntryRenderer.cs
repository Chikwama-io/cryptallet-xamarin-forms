﻿using System;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Xamarin.Forms;
using Wallet.Controls.Droid.Renderers;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace Wallet.Controls.Droid.Renderers
{
    public class CustomEntryRenderer : EntryRenderer
    {
        public static void Preserve() { }

        public CustomEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null) return;

            Control.SetBackgroundResource(Resource.Drawable.bg_edittext);
            int padding = (int)Context.ToPixels(16);
            Control.SetPadding(padding, padding, padding, padding);
        }
    }
}
