using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

[Serializable]
public class TableColumn
{
    [SqlField("name")]
    public string Name { get; set; }
    [SqlField("type")]
    public string Type { get; set; }
    [SqlField("notnull")]
    public bool NotNull { get; set; }
    [SqlField("defaultvalue")]
    public string DefaultValue { get; set; }
}

public class MetaDataItem
{
    [SqlField("value")]
    public int? Value { get; set; }

    [SqlField("display")]
    public string Display { get; set; }

    public MetaDataItem()
    {
    }

    public MetaDataItem(int? value, string display)
    {
        Value = value;
        Display = display;
    }

    public static MetaDataItem CreateNullItem(string display = "")
    {
        return new MetaDataItem() { Display = display };
    }

    public static MetaDataItem Default(string display = "")
    {
        return new MetaDataItem() { Display = display };
    }
}

