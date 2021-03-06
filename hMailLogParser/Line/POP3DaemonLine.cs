﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hMailLogParser.Line
{
    public class POP3DaemonLine : SessionBasedLine
    {
        internal override void Parse(string[] columns)
        {
            if (columns.Length == 6)
            {
                this.ThreadID = int.Parse(columns[1].Sanitize());
                this.SessionID = int.Parse(columns[2].Sanitize());
                this.Date = DateTime.Parse(columns[3].Sanitize());
                this.IPAddress = columns[4].Sanitize();
                this.Message = columns[5].Sanitize();

                this.ParseSMTPMessage(this.Message);
            }
            else
            {
                throw new Exception("Malformed line");
            }
        }

        public const string LINE_TYPE = "POP3D";
        protected override string GetLineType()
        {
            return LINE_TYPE;
        }        
    }
}
