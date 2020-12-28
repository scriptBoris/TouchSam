﻿using System;
using Xamarin.Forms;

namespace TouchSam
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    internal static class ConfigTouch
    {
        public static void Init()
        {
        }

        public static bool AutoChildrenInputTransparent { get; set; } = true;

        public static readonly BindableProperty ChildrenInputTransparentProperty =
            BindableProperty.CreateAttached(
                "ChildrenInputTransparent",
                typeof(bool),
                typeof(ConfigTouch),
                false,
                propertyChanged: (bindable, oldValue, newValue) => {
                    ConfigureChildrenInputTransparent(bindable);
                }
            );

        public static void SetChildrenInputTransparent(BindableObject view, bool value)
        {
            view.SetValue(ChildrenInputTransparentProperty, value);
        }

        public static bool GetChildrenInputTransparent(BindableObject view)
        {
            return (bool)view.GetValue(ChildrenInputTransparentProperty);
        }

        static void ConfigureChildrenInputTransparent(BindableObject bindable)
        {
            if (!(bindable is Layout layout))
                return;

            if (GetChildrenInputTransparent(bindable))
            {
                foreach (var layoutChild in layout.Children)
                    AddInputTransparentToElement(layoutChild);
                layout.ChildAdded += Layout_ChildAdded;
            }
            else
            {
                layout.ChildAdded -= Layout_ChildAdded;
            }
        }

        static void Layout_ChildAdded(object sender, ElementEventArgs e)
        {
            AddInputTransparentToElement(e.Element);
        }

        static void AddInputTransparentToElement(BindableObject obj)
        {
            if (obj is View view
                && Touch.GetColor(view) == Color.Default
                && Touch.GetTap(view) == null
                && Touch.GetLongTap(view) == null)
            {
                view.InputTransparent = true;
            }
        }
    }
}