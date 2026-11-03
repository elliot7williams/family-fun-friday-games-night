        private void ViewportFontSize(S style, string value)
        {
            int number, offset; char unit, offsetUnit;
            if (!ContainerLength(value, out number, out unit, out offset, out offsetUnit)) return;
            bool viewport = unit == 'w' || unit == 'h' || unit == 'n' || unit == 'x';
            bool viewportOffset = offsetUnit == 'w' || offsetUnit == 'h' || offsetUnit == 'n' || offsetUnit == 'x';
            if ((!viewport && unit != 'p') || (offsetUnit != '\0' && !viewportOffset && offsetUnit != 'p') || (!viewport && !viewportOffset)) return;
            long pixels, adjustment;
            if (!ContainerLengthValue(null, number, unit, out pixels)) return;
            if (offsetUnit != '\0')
            {
                if (!ContainerLengthValue(null, Math.Abs(offset), offsetUnit, out adjustment)) return;
                pixels += offset < 0 ? -adjustment : adjustment;
            }
            int size = (int)Math.Round(pixels / 10000D, MidpointRounding.AwayFromZero);
            if (size > 0 && size < 200) style.Fs = size;
        }

