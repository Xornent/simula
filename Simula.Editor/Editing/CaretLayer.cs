﻿
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using Simula.Editor.Rendering;
using Simula.Editor.Utils;

namespace Simula.Editor.Editing
{
	sealed class CaretLayer : Layer
	{
		TextArea textArea;

		bool isVisible;
		Rect caretRectangle;

		DispatcherTimer caretBlinkTimer = new DispatcherTimer();
		bool blink;

		public CaretLayer(TextArea textArea) : base(textArea.TextView, KnownLayer.Caret)
		{
			this.textArea = textArea;
			this.IsHitTestVisible = false;
			caretBlinkTimer.Tick += new EventHandler(caretBlinkTimer_Tick);
		}

		void caretBlinkTimer_Tick(object sender, EventArgs e)
		{
			blink = !blink;
			InvalidateVisual();
		}

		public void Show(Rect caretRectangle)
		{
			this.caretRectangle = caretRectangle;
			this.isVisible = true;
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

		void StartBlinkAnimation()
		{
			TimeSpan blinkTime = Win32.CaretBlinkTime;
			blink = true; // the caret should visible initially
						  // This is important if blinking is disabled (system reports a negative blinkTime)
			if (blinkTime.TotalMilliseconds > 0) {
				caretBlinkTimer.Interval = blinkTime;
				caretBlinkTimer.Start();
			}
		}

		void StopBlinkAnimation()
		{
			caretBlinkTimer.Stop();
		}

		internal Brush CaretBrush;

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			if (isVisible && blink) {
				Brush caretBrush = this.CaretBrush;
				if (caretBrush == null)
					caretBrush = (Brush)textView.GetValue(TextBlock.ForegroundProperty);

				if (this.textArea.OverstrikeMode) {
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
