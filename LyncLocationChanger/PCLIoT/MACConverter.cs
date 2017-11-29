using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PCLIoT
{
    static public class MACConverter // : IValueConverter
    {
            static public object ConvertToString(byte[] value)
            {
                return BitConverter.ToString(value);
            }

            static public byte[] ConvertBack(object value)
            {
                if(value is string)
                {
                    byte[] mac = new byte[6];
                    var str = ((string)value).Replace("-", "");
                    for (int i = 0; i < 2; i++)
                    {
                        var s = str.Substring(i * 6, 6);
                        uint num = Convert.ToUInt32(s, 16);
                        byte[] byt = BitConverter.GetBytes(num);
                        for (int j=2; j >-1;j--)
                        {
                             mac[i * 3 + j] = byt[2-j];
                        }
                    }
                    //      uint num = uint.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    return mac; 
                }
                return null;
            }

    }
}
