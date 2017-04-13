using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class SingleItem<T> where T : class, new()
{
    /// <summary>
    /// 单例对象
    /// </summary>
    private static T single = null;

    /// <summary>
    /// 单例
    /// </summary>
    public static T Single
    {
        get
        {
            if (single == null)
            {
                single = new T();
            }
            return single;
        }
    }
}
