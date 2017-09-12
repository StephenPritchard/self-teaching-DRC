using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Learning_DRC
{
    class GPCRule
    {
        // ENUMERATED TYPES

        public enum RuleType
        {
            body,
            multi,
            context,
            two,
            mphon,
            single,
            outrule,
        }
        public enum RulePosition
        {
            beginning,
            middle,
            end,
            all,
        }

        // PROPERTIES

        public RulePosition RPosition { get; set; }
        public RuleType RType { get; set; }
        public string RGrapheme { get; set; }
        public string RPhoneme {get; set; }
        public bool RProtection {get; set; }
        public float RWeight {get; set;}


        // CONSTRUCTOR

        public GPCRule(string[] attributes)
        {
            switch (attributes[0])
            {
                case "b":
                    RPosition = RulePosition.beginning;
                    break;
                case "m":
                    RPosition = RulePosition.middle;
                    break;
                case "e":
                    RPosition = RulePosition.end;
                    break;
                case "A":
                    RPosition = RulePosition.all;
                    break;
                default:
                    System.Console.WriteLine("Invalid rule position {0} for grapheme {1}", attributes[0], attributes[2]);
                    break;
            }

            switch (attributes[1])
            {
                case "body":
                    RType = RuleType.body;
                    break;
                case "multi":
                    RType = RuleType.multi;
                    break;
                case "cs":
                    RType = RuleType.context;
                    break;
                case "two":
                    RType = RuleType.two;
                    break;
                case "mphon":
                    RType = RuleType.mphon;
                    break;
                case "sing":
                    RType = RuleType.single;
                    break;
                case "out":
                    RType = RuleType.outrule;
                    break;
                default:
                    System.Console.WriteLine("Invalid rule type {0} for grapheme {1}", attributes[1], attributes[2]);
                    break;
            }

            RGrapheme = attributes[2];
            RPhoneme = attributes[3];

            if (attributes[4] == "u")
            {
                RProtection = false;
            }
            else if (attributes[4] == "p")
            {
                RProtection = true;
            }
            else
            {
                System.Console.WriteLine("Invalid rpotection status {0} for grapheme {1}", attributes[4], attributes[2]);
            }

            RWeight = float.Parse(attributes[5]);
        }

        // Method to return a char (b, m, e, or A) for position, so that it can easily be printed
        // to the activations file
        public char GetPosChar()
        {
            switch (RPosition)
            {
                case (RulePosition.beginning):
                    return 'b';

                case (RulePosition.middle):
                    return 'm';
                case (RulePosition.end):
                    return 'e';
                case (RulePosition.all):
                    return 'A';
                default:
                    return '?';
            }
        }
    }
}
