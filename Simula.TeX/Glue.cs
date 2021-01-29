using Simula.TeX.Boxes;
using System.Collections.Generic;

namespace Simula.TeX
{
    // Represents glueElement for holding together boxes.
    internal class Glue
    {
        private static readonly IList<Glue> glueTypes;
        private static readonly int[,,] glueRules;

        static Glue()
        {
            var parser = new GlueSettingsParser();
            glueTypes = parser.GetGlueTypes();
            glueRules = parser.GetGlueRules();
        }

        public static Box CreateBox(TexAtomType leftAtomType, TexAtomType rightAtomType, TexEnvironment environment)
        {
            leftAtomType = leftAtomType > TexAtomType.Inner ? TexAtomType.Ordinary : leftAtomType;
            rightAtomType = rightAtomType > TexAtomType.Inner ? TexAtomType.Ordinary : rightAtomType;
            var glueType = glueRules[(int)leftAtomType, (int)rightAtomType, (int)environment.Style / 2];
            return glueTypes[glueType].CreateBox(environment);
        }

        public Glue(double space, double stretch, double shrink, string name)
        {
            Space = space;
            Stretch = stretch;
            Shrink = shrink;
            Name = name;
        }

        public double Space {
            get;
            private set;
        }

        public double Stretch {
            get;
            private set;
        }

        public double Shrink {
            get;
            private set;
        }

        public string Name {
            get;
            private set;
        }

        private Box CreateBox(TexEnvironment environment)
        {
            var texFont = environment.MathFont;
            var quad = texFont.GetQuad(texFont.GetMuFontId(), environment.Style);
            return new GlueBox((Space / 18.0f) * quad, (Stretch / 18.0f) * quad, (Shrink / 18.0f) * quad);
        }
    }
}
