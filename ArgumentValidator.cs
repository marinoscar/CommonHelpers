﻿/*

Copyright (C) 2007-2011 by Gustavo Duarte and Bernardo Vieira.
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Helpers
{
    /// <summary>
    /// Offers helpful methods for argument validation
    /// </summary>
    public static class ArgumentValidator
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException" /> if <paramref name="argumentToValidate"/> is null.
        /// </summary>
        public static void ThrowIfNull(Object argumentToValidate, String argumentName)
        {
            if (null == argumentName)
            {
                throw new ArgumentNullException("argumentName");
            }

            if (null == argumentToValidate)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException" /> if <paramref name="argumentToValidate"/> is null or empty.
        /// </summary>
        public static void ThrowIfNullOrEmpty(String argumentToValidate, String argumentName)
        {
            ThrowIfNull(argumentToValidate, argumentName);

            if (argumentToValidate == String.Empty)
            {
                throw new ArgumentException(argumentName);
            }
        }

        public static void ThrowIfTrue(Boolean condition, String msg)
        {
            ThrowIfNullOrEmpty(msg, "msg");

            if (condition)
            {
                throw new ArgumentException(msg);
            }
        }

        public static void ThrowIfDoesNotExist(FileSystemInfo fileSytemObject, String argumentName)
        {
            ThrowIfNull(fileSytemObject, "fileSytemObject");
            ThrowIfNullOrEmpty(argumentName, "argumentName");

            if (!fileSytemObject.Exists)
            {
                throw new FileNotFoundException("'{0}' not found".Fi(fileSytemObject.FullName));
            }
        }

        public static void ThrowIfNotUtc(DateTime argumentToValidate, String argumentName)
        {
            ThrowIfNullOrEmpty(argumentName, "argumentName");

            if (argumentToValidate.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("You must pass an UTC DateTime value", argumentName);
            }
        }
    }
}