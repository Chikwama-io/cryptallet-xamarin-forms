﻿/*
 * Copyright 2018 NAXAM CO.,LTD.
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */ 
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;

namespace Wallet.Controls
{
    public partial class Passcode : ContentView
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(Passcode),
            default(ICommand),
            BindingMode.OneWay);
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        static readonly Color ColorPrimary = Color.FromHex("#7C067C");
        static readonly Color ColorTextAccent = Color.FromHex("384951");
        static readonly Color ColorTextSecondary = Color.FromHex("b2b2b2");

        Stack<string> codes;

        public Passcode()
        {
            InitializeComponent();
            codes = new Stack<string>(6);
        }

        void PaddNumberTapped_Tapped(object sender, System.EventArgs e)
        {
            if (sender is Image img)
            {
                var sibling = ((Grid)img.Parent).Children[1];
                if (img == sibling)
                {
                    img = (Image)((Grid)img.Parent).Children[0];
                }

                UpdateBackground(img, sibling);
            }
            else if (sender is Label sibling)
            {
                var bg = ((Grid)sibling.Parent).Children[0];
                UpdateBackground((Image)bg, sibling);
            }
        }

        async void UpdateBackground(Image bg, View sibling)
        {
            if (sibling is Label label)
            {
                if (codes.Count < 6)
                {
                    ImageProperties.SetColor(grdPasscode.Children[codes.Count], ColorPrimary);
                    codes.Push(label.Text);

                    if (codes.Count == 6)
                    {
                        Command?.Execute(string.Join(string.Empty, codes.Reverse()));
                    }
                }

                label.TextColor = Color.White;
                ImageProperties.SetColor(bg, ColorPrimary);

                await Task.Delay(200);

                label.TextColor = ColorTextAccent;
                ImageProperties.SetColor(bg, Color.White);
            }
            else if (sibling is Image image)
            {
                if (codes.Count > 0)
                {
                    codes.Pop();
                    ImageProperties.SetColor(grdPasscode.Children[codes.Count], ColorTextSecondary);
                }

                ImageProperties.SetColor(bg, ColorPrimary);
                image.Source = "ic_pad_delete_white";

                await Task.Delay(200);

                ImageProperties.SetColor(bg, Color.White);
                image.Source = "ic_pad_delete";
            }
        }
    }
}
