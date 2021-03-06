﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hMailLogParser.Line
{
    public class ApplicationLine : LogLine
    {
        internal override void Parse(string[] columns)
        {
            if (columns.Length == 4)
            {
                this.ThreadID = int.Parse(columns[1].Sanitize());
                this.Date = DateTime.Parse(columns[2].Sanitize());
                this.Message = columns[3].Sanitize();
            }
            else
            {
                throw new Exception("Malformed line");
            }
        }

        public const string LINE_TYPE = "APPLICATION";
        protected override string GetLineType()
        {
            return LINE_TYPE;
        }
    }
}
