using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Simula.UI
{

    public partial class Load : UserControl
    {
        public Load()
        {
            InitializeComponent();
            var angleAnimation = new DoubleAnimationUsingKeyFrames();
            angleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            var keyFrames = angleAnimation.KeyFrames;

            keyFrames.Add(new DiscreteDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(30, TimeSpan.FromSeconds(0.05)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(60, TimeSpan.FromSeconds(0.1)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(90, TimeSpan.FromSeconds(0.15)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(120, TimeSpan.FromSeconds(0.2)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(150, TimeSpan.FromSeconds(0.25)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(180, TimeSpan.FromSeconds(0.3)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(210, TimeSpan.FromSeconds(0.35)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(240, TimeSpan.FromSeconds(0.4)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(270, TimeSpan.FromSeconds(0.45)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(300, TimeSpan.FromSeconds(0.5)));
            keyFrames.Add(new DiscreteDoubleKeyFrame(330, TimeSpan.FromSeconds(0.55)));

            rotation.BeginAnimation(RotateTransform.AngleProperty, angleAnimation);
        }

        public double Size {
            get { return scale.ScaleX; }
            set {
                scale.ScaleX = value;
                scale.ScaleY = value;
            }
        }
    }
}
