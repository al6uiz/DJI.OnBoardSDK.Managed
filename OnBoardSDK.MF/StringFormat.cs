// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  StringBuilder
**
**
** Purpose: implementation of the StringBuilder
** class.
**
===========================================================*/
using System;
namespace System.Text
{

    // This class represents a mutable string.  It is convenient for situations in
    // which it is desirable to modify a string, perhaps by removing, replacing, or 
    // inserting characters, without creating a new String subsequent to
    // each modification. 
    // 
    // The methods contained within this class do not return a new StringBuilder
    // object unless specified otherwise.  This class may be used in conjunction with the String
    // class to carry out modifications upon strings.
    // 
    // When passing null into a constructor in VJ and VC, the null
    // should be explicitly type cast.
    // For Example:
    // StringBuilder sb1 = new StringBuilder((StringBuilder)null);
    // StringBuilder sb2 = new StringBuilder((String)null);
    // Console.WriteLine(sb1);
    // Console.WriteLine(sb2);
    // 

    public static class StringBuilderExtension
    {

        public static StringBuilder AppendFormat(this StringBuilder sb, String format, Object arg0)
        {

            return AppendFormatHelper(sb, null, format, new object[] { arg0 });
        }

        public static StringBuilder AppendFormat(this StringBuilder sb, String format, Object arg0, Object arg1)
        {

            return AppendFormatHelper(sb, null, format, new object[] { arg0, arg1 });
        }

        public static StringBuilder AppendFormat(this StringBuilder sb, String format, Object arg0, Object arg1, Object arg2)
        {

            return AppendFormatHelper(sb, null, format, new object[] { arg0, arg1, arg2 });
        }

        public static StringBuilder AppendFormat(this StringBuilder sb, String format, params Object[] args)
        {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in AppendFormatHelper.
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }



            return AppendFormatHelper(sb, null, format, args);
        }

        public static StringBuilder AppendFormat(this StringBuilder sb, IFormatProvider provider, String format, Object arg0)
        {

            return AppendFormatHelper(sb, provider, format, new object[] { arg0 });
        }

        public static StringBuilder AppendFormat(this StringBuilder sb, IFormatProvider provider, String format, Object arg0, Object arg1)
        {

            return AppendFormatHelper(sb, provider, format, new object[] { arg0, arg1 });
        }

        public static StringBuilder AppendFormat(this StringBuilder sb, IFormatProvider provider, String format, Object arg0, Object arg1, Object arg2)
        {

            return AppendFormatHelper(sb, provider, format, new object[] { arg0, arg1, arg2 });
        }

        public static StringBuilder AppendFormat(this StringBuilder sb, IFormatProvider provider, String format, params Object[] args)
        {
            if (args == null)
            {
                // To preserve the original exception behavior, throw an exception about format if both
                // args and format are null. The actual null check for format is in AppendFormatHelper.
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }



            return AppendFormatHelper(sb, provider, format, new object[] { args });
        }

        private static void FormatError()
        {
            throw new FormatException("Invalid format string.");
        }

        internal static StringBuilder AppendFormatHelper(this StringBuilder sb, IFormatProvider provider, String format, object[] args)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }



            int pos = 0;
            int len = format.Length;
            char ch = '\x0';

            ICustomFormatter cf = null;
            if (provider != null)
            {
                cf = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
            }

            while (true)
            {
                int p = pos;
                int i = pos;
                while (pos < len)
                {
                    ch = format[pos];

                    pos++;
                    if (ch == '}')
                    {
                        if (pos < len && format[pos] == '}') // Treat as escape character for }}
                            pos++;
                        else
                            FormatError();
                    }

                    if (ch == '{')
                    {
                        if (pos < len && format[pos] == '{') // Treat as escape character for {{
                            pos++;
                        else
                        {
                            pos--;
                            break;
                        }
                    }

                    sb.Append(ch);
                }

                if (pos == len) break;
                pos++;
                if (pos == len || (ch = format[pos]) < '0' || ch > '9') FormatError();
                int index = 0;
                do
                {
                    index = index * 10 + ch - '0';
                    pos++;
                    if (pos == len) FormatError();
                    ch = format[pos];
                } while (ch >= '0' && ch <= '9' && index < 1000000);
                if (index >= args.Length) throw new FormatException("Index (zero based) must be greater than or equal to zero and less than the size of the argument list.");
                while (pos < len && (ch = format[pos]) == ' ') pos++;
                bool leftJustify = false;
                int width = 0;
                if (ch == ',')
                {
                    pos++;
                    while (pos < len && format[pos] == ' ') pos++;

                    if (pos == len) FormatError();
                    ch = format[pos];
                    if (ch == '-')
                    {
                        leftJustify = true;
                        pos++;
                        if (pos == len) FormatError();
                        ch = format[pos];
                    }
                    if (ch < '0' || ch > '9') FormatError();
                    do
                    {
                        width = width * 10 + ch - '0';
                        pos++;
                        if (pos == len) FormatError();
                        ch = format[pos];
                    } while (ch >= '0' && ch <= '9' && width < 1000000);
                }

                while (pos < len && (ch = format[pos]) == ' ') pos++;
                Object arg = args[index];
                StringBuilder fmt = null;
                if (ch == ':')
                {
                    pos++;
                    p = pos;
                    i = pos;
                    while (true)
                    {
                        if (pos == len) FormatError();
                        ch = format[pos];
                        pos++;
                        if (ch == '{')
                        {
                            if (pos < len && format[pos] == '{')  // Treat as escape character for {{
                                pos++;
                            else
                                FormatError();
                        }
                        else if (ch == '}')
                        {
                            if (pos < len && format[pos] == '}')  // Treat as escape character for }}
                                pos++;
                            else
                            {
                                pos--;
                                break;
                            }
                        }

                        if (fmt == null)
                        {
                            fmt = new StringBuilder();
                        }
                        fmt.Append(ch);
                    }
                }
                if (ch != '}') FormatError();
                pos++;
                String sFmt = null;
                String s = null;
                if (cf != null)
                {
                    if (fmt != null)
                    {
                        sFmt = fmt.ToString();
                    }
                    s = cf.Format(sFmt, arg, provider);
                }

                if (s == null)
                {
                    IFormattable formattableArg = arg as IFormattable;

                    if (formattableArg != null)
                    {
                        if (sFmt == null && fmt != null)
                        {
                            sFmt = fmt.ToString();
                        }

                        s = formattableArg.ToString(sFmt, provider);
                    }
                    else if (arg != null)
                    {
                        s = arg.ToString();
                    }
                }

                if (s == null) s = String.Empty;
                int pad = width - s.Length;
                if (!leftJustify && pad > 0) sb.Append(' ', pad);
                sb.Append(s);
                if (leftJustify && pad > 0) sb.Append(' ', pad);
            }
            return sb;
        }

        // Returns a reference to the current StringBuilder with all instances of oldString 
        // replaced with newString.  If startIndex and count are specified,
        // we only replace strings completely contained in the range of startIndex to startIndex + 
        // count.  The strings to be replaced are checked on an ordinal basis (e.g. not culture aware).  If 
        // newValue is null, instances of oldValue are removed (e.g. replaced with nothing.).
        //

    }

    internal class FormatException : Exception
    {
        public FormatException()
        {
        }

        public FormatException(string message) : base(message)
        {
        }

        public FormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

