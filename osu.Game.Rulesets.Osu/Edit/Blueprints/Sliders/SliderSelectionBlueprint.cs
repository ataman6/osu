﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        protected readonly SliderBodyPiece BodyPiece;
        protected readonly SliderCircleSelectionBlueprint HeadBlueprint;
        protected readonly SliderCircleSelectionBlueprint TailBlueprint;
        protected readonly PathControlPointVisualiser ControlPointVisualiser;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.Start),
                TailBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.End),
                ControlPointVisualiser = new PathControlPointVisualiser(sliderObject, true)
            };
        }

        private IBindable<int> pathVersion;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            pathVersion = HitObject.Path.Version.GetBoundCopy();
            pathVersion.BindValueChanged(_ => updatePath());
        }

        protected override void Update()
        {
            base.Update();

            BodyPiece.UpdateFrom(HitObject);
        }

        private Vector2 rightClickPosition;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    rightClickPosition = e.MouseDownPosition;
                    return false; // Allow right click to be handled by context menu

                case MouseButton.Left when e.ControlPressed && IsSelected:
                    placementControlPointIndex = addControlPoint(e.MousePosition);
                    return true; // Stop input from being handled and modifying the selection
            }

            return false;
        }

        private int? placementControlPointIndex;

        protected override bool OnDragStart(DragStartEvent e) => placementControlPointIndex != null;

        protected override bool OnDrag(DragEvent e)
        {
            Debug.Assert(placementControlPointIndex != null);

            HitObject.Path.ControlPoints[placementControlPointIndex.Value].Position.Value = e.MousePosition - HitObject.Position;

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e)
        {
            placementControlPointIndex = null;
            return true;
        }

        private int addControlPoint(Vector2 position)
        {
            position -= HitObject.Position;

            int insertionIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < HitObject.Path.ControlPoints.Count - 2; i++)
            {
                float dist = new Line(HitObject.Path.ControlPoints[i].Position.Value, HitObject.Path.ControlPoints[i + 1].Position.Value).DistanceToPoint(position);

                if (dist < minDistance)
                {
                    insertionIndex = i + 1;
                    minDistance = dist;
                }
            }

            // Move the control points from the insertion index onwards to make room for the insertion
            HitObject.Path.ControlPoints.Insert(insertionIndex, new PathControlPoint { Position = { Value = position } });

            return insertionIndex;
        }

        private void updatePath()
        {
            HitObject.Path.ExpectedDistance.Value = composer?.GetSnappedDistanceFromDistance(HitObject.StartTime, (float)HitObject.Path.CalculatedDistance) ?? (float)HitObject.Path.CalculatedDistance;
            UpdateHitObject();
        }

        public override MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, () => addControlPoint(rightClickPosition)),
        };

        public override Vector2 SelectionPoint => HeadBlueprint.SelectionPoint;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => BodyPiece.ReceivePositionalInputAt(screenSpacePos);

        protected virtual SliderCircleSelectionBlueprint CreateCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position) => new SliderCircleSelectionBlueprint(slider, position);
    }
}
