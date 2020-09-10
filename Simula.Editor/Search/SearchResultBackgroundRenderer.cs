﻿
using System;
using System.Linq;
using System.Windows.Media;

using Simula.Editor.Document;
using Simula.Editor.Rendering;

namespace Simula.Editor.Search
{
	class SearchResultBackgroundRenderer : IBackgroundRenderer
	{
		TextSegmentCollection<SearchResult> currentResults = new TextSegmentCollection<SearchResult>();

		public TextSegmentCollection<SearchResult> CurrentResults {
			get { return currentResults; }
		}

		public KnownLayer Layer {
			get {
				// draw behind selection
				return KnownLayer.Selection;
			}
		}

		public SearchResultBackgroundRenderer()
		{
			markerBrush = Brushes.LightGreen;
			markerPen = new Pen(markerBrush, 1);
		}

		Brush markerBrush;
		Pen markerPen;

		public Brush MarkerBrush {
			get { return markerBrush; }
			set {
				this.markerBrush = value;
				markerPen = new Pen(markerBrush, 1);
			}
		}

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			if (textView == null)
				throw new ArgumentNullException("textView");
			if (drawingContext == null)
				throw new ArgumentNullException("drawingContext");

			if (currentResults == null || !textView.VisualLinesValid)
				return;

			var visualLines = textView.VisualLines;
			if (visualLines.Count == 0)
				return;

			int viewStart = visualLines.First().FirstDocumentLine.Offset;
			int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;

			foreach (SearchResult result in currentResults.FindOverlappingSegments(viewStart, viewEnd - viewStart)) {
				BackgroundGeometryBuilder geoBuilder = new BackgroundGeometryBuilder();
				geoBuilder.AlignToWholePixels = true;
				geoBuilder.BorderThickness = markerPen != null ? markerPen.Thickness : 0;
				geoBuilder.CornerRadius = 3;
				geoBuilder.AddSegment(textView, result);
				Geometry geometry = geoBuilder.CreateGeometry();
				if (geometry != null) {
					drawingContext.DrawGeometry(markerBrush, markerPen, geometry);
				}
			}
		}
	}
}
