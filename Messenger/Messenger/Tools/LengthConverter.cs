﻿using Messenger.Foundation;
using Messenger.Foundation.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Messenger.Tools
{
    /// <summary>
    /// 将大小转化为带单位的字符串
    /// </summary>
    class LengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var len = 0L;
            if (value is long obl)
                len = obl;
            else if (value is double obd)
                len = (long)obd;
            return Extension.GetLength(len);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
