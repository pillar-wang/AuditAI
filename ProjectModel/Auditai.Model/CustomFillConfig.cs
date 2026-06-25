﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Auditai.Model;

[Serializable]
public class CustomFillConfig
{
    [JsonProperty("R")]
    public List<CustomFillRule> Rules { get; set; } = new List<CustomFillRule>();

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static CustomFillConfig Deserialize(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return new CustomFillConfig();
        return JsonConvert.DeserializeObject<CustomFillConfig>(s) ?? new CustomFillConfig();
    }
}