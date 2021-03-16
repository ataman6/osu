// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Handlers.Tablet;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class TabletAreaSelection : CompositeDrawable
    {
        private readonly ITabletHandler handler;

        private Container tabletContainer;
        private Container usableAreaContainer;

        private readonly Bindable<Size> areaOffset = new BindableSize();
        private readonly Bindable<Size> areaSize = new BindableSize();
        private readonly Bindable<Size> tabletSize = new BindableSize();

        public TabletAreaSelection(ITabletHandler handler)
        {
            this.handler = handler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(5);

            InternalChild = tabletContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 5,
                BorderThickness = 2,
                BorderColour = Color4.Black,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    },
                    usableAreaContainer = new Container
                    {
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Yellow,
                            },
                            new OsuSpriteText
                            {
                                Text = "usable area",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = Color4.Black,
                                Font = OsuFont.Default.With(size: 12)
                            }
                        }
                    },
                }
            };

            areaOffset.BindTo(handler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                usableAreaContainer.MoveTo(new Vector2(val.NewValue.Width, val.NewValue.Height), 100, Easing.OutQuint);
            }, true);

            areaSize.BindTo(handler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                usableAreaContainer.ResizeTo(new Vector2(val.NewValue.Width, val.NewValue.Height), 100, Easing.OutQuint);
            }, true);

            ((IBindable<Size>)tabletSize).BindTo(handler.TabletSize);
            tabletSize.BindValueChanged(val =>
            {
                tabletContainer.Size = new Vector2(val.NewValue.Width, val.NewValue.Height);
            });
        }

        protected override void Update()
        {
            base.Update();

            var size = tabletSize.Value;

            float fitX = size.Width / DrawWidth;
            float fitY = size.Height / DrawHeight;

            float adjust = MathF.Max(fitX, fitY);

            tabletContainer.Scale = new Vector2(1 / adjust);
        }
    }
}
