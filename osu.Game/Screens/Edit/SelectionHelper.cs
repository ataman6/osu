﻿using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Screens.Edit
{
    public class SelectionHelper : Component
    {
        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private EditorClock clock { get; set; }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public void CopySelectionToClipboard()
        {
            host.GetClipboard().SetText(formatSelectionAsString());
        }

        private string formatSelectionAsString()
        {
            const string separator = " - ";
            var builder = new StringBuilder();

            if (!editorBeatmap.SelectedHitObjects.Any())
            {
                builder.Append($"{clock.CurrentTime.ToEditorFormattedString()}{separator}");
                return builder.ToString();
            };

            builder.Append(editorBeatmap.SelectedHitObjects.First().StartTime.ToEditorFormattedString());
            builder.Append($" ({string.Join(',', composer.ConvertSelectionToString())}){separator}");
            return builder.ToString();
        }
    }
}
