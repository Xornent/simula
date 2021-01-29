
using Simula.Editor.Rendering;
using Simula.Editor.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Simula.Editor.Editing
{
    internal sealed class CaretLayer : Layer
    {
        private readonly TextArea textArea;
        private bool isVisible;
        private Rect caretRectangle;
        private readonly DispatcherTimer caretBlinkTimer = new DispatcherTimer();
        private bool blink;

        public CaretLayer(TextArea textArea) : base(textArea.TextView, KnownLayer.Caret)
        {
            this.textArea = textArea;
            IsHitTestVisible = false;
            caretBlinkTimer.Tick += new EventHandler(caretBlinkTimer_Tick);
        }

        private void caretBlinkTimer_Tick(object sender, EventArgs e)
        {
            blink = !blink;
            InvalidateVisual();
        }

        public void Show(Rect caretRectangle)
        {
            this.caretRectangle = caretRectangle;
            isVisible = true;
            StartBlinkAnimation();
            InvalidateVisual();
        }

        public void Hide()
        {
            if (isVisible) {
                isVisible = false;
                StopBlinkAnimation();
                InvalidateVisual();
            }
        }

        private void StartBlinkAnimation()
        {
            TimeSpan blinkTime = Win32.CaretBlinkTime;
            blink = true; // the caret should visible initially
                          // This is important if blinking is disabled (system reports a negative blinkTime)
            if (blinkTime.TotalMilliseconds > 0) {
                caretBlinkTimer.Interval = blinkTime;
                caretBlinkTimer.Start();
            }
        }

        private void StopBlinkAnimation()
        {
            caretBlinkTimer.Stop();
        }

        internal Brush CaretBrush;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (isVisible && blink) {
                Brush caretBrush = CaretBrush;
                if (caretBrush == null)
                    caretBrush = (Brush)textView.GetValue(TextBlock.ForegroundProperty);

                if (textArea.OverstrikeMode) {
                    SolidColorBrush scBrush = caretBrush as SolidColorBrush;
                    if (scBrush != null) {
                        Color brushColor = scBrush.Color;
                        Color newColor = Color.FromArgb(100, brushColor.R, brushColor.G, brushColor.B);
                        caretBrush = new SolidColorBrush(newColor);
                        caretBrush.Freeze();
                    }
                }

                Rect r = new Rect(caretRectangle.X - textView.HorizontalOffset,
                                  caretRectangle.Y - textView.VerticalOffset,
                                  caretRectangle.Width,
                                  caretRectangle.Height);
                drawingContext.DrawRectangle(caretBrush, null, PixelSnapHelpers.Round(r, PixelSnapHelpers.GetPixelSize(this)));
            }
        }
    }
}
