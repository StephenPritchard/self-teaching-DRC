using System;
using System.Collections.Generic;

namespace Learning_DRC
{
    internal class GPCRule
    {
        public enum RuleType
        {
            Body,
            Multi,
            Context,
            Two,
            Mphon,
            Single,
            Outrule,
        }
        public enum RulePosition
        {
            Beginning,
            Middle,
            End,
            All,
        }

        public RulePosition RPosition {get; set; }
        public RuleType RType { get; set; }
        public string RGrapheme { get; set; }
        public string RPhoneme {get; set; }
        public bool RProtection {get; set; }
        public float RWeight {get; set;}


        public GPCRule(IList<string> attributes)
        {
            switch (attributes[0])
            {
                case "b":
                    RPosition = RulePosition.Beginning;
                    break;
                case "m":
                    RPosition = RulePosition.Middle;
                    break;
                case "e":
                    RPosition = RulePosition.End;
                    break;
                case "A":
                    RPosition = RulePosition.All;
                    break;
                default:
                    System.Console.WriteLine("Invalid rule position {0} for grapheme {1}", attributes[0], attributes[2]);
                    break;
            }

            switch (attributes[1])
            {
                case "body":
                    RType = RuleType.Body;
                    break;
                case "multi":
                    RType = RuleType.Multi;
                    break;
                case "cs":
                    RType = RuleType.Context;
                    break;
                case "two":
                    RType = RuleType.Two;
                    break;
                case "mphon":
                    RType = RuleType.Mphon;
                    break;
                case "sing":
                    RType = RuleType.Single;
                    break;
                case "out":
                    RType = RuleType.Outrule;
                    break;
                default:
                    System.Console.WriteLine("Invalid rule type {0} for grapheme {1}", attributes[1], attributes[2]);
                    break;
            }

            RGrapheme = attributes[2];
            RPhoneme = attributes[3];

            switch (attributes[4])
            {
                case "u":
                    RProtection = false;
                    break;
                case "p":
                    RProtection = true;
                    break;
                default:
                    Console.WriteLine("Invalid rpotection status {0} for grapheme {1}", attributes[4], attributes[2]);
                    break;
            }

            RWeight = float.Parse(attributes[5]);
        }

        public char GetPositionCharacter()
        {
            switch (RPosition)
            {
                case (RulePosition.Beginning):
                    return 'b';

                case (RulePosition.Middle):
                    return 'm';
                case (RulePosition.End):
                    return 'e';
                case (RulePosition.All):
                    return 'A';
                default:
                    return '?';
            }
        }
    }
}
