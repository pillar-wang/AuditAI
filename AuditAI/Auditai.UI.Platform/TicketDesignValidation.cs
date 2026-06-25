﻿﻿using System;
using System.Collections.Generic;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public class TicketDesignValidation
{
    public class TempField
    {
        public Id64 Field { get; set; }
        public string Text { get; set; }
        public string InputValue { get; set; }

        public void WriteTo(object ticketCell)
        {
            if (ticketCell == null) return;
            try
            {
                // 通过反射设置 cell 的 Value 属性
                var valueProp = ticketCell.GetType().GetProperty("Value");
                if (valueProp != null)
                {
                    valueProp.SetValue(ticketCell, InputValue);
                }
            }
            catch
            {
                // 写入失败时静默处理
            }
        }
    }

    public class FixedAndDynamicRowMixRange
    {
        public int RangeStartRowIndex { get; set; }
        public int RangeEndRowIndex { get; set; }
        public int RangeRowsCount { get; set; }
        public List<Tuple<int, int>> DynamicRowsList { get; set; }
        public List<int> FixedRowsList { get; set; }
    }

    public class FieldCellSetting
    {
        public bool IsInFixedDataRow { get; set; }
        public bool IsInDynamicDataRow { get; set; }
        public bool IsTicketKey { get; set; }
        public object ticketDesignCellVM { get; set; }
        public TempField TempField { get; set; }
        public object TicketMergeRange { get; set; }
    }

    public int GroupStartRow { get; set; }
    public int GroupEndRow { get; set; }
    public List<Tuple<int, int>> DataMergeCols { get; set; }
    public int Kind { get; set; }
    public bool Success => FailureReason == TicketDesignFailureReason.None;

    public IDictionary<object, TempField> DicField { get; } = new Dictionary<object, TempField>();
    public Dictionary<object, TempField> TitleDicField { get; } = new Dictionary<object, TempField>();
    public Dictionary<object, TempField> FooterDicField { get; } = new Dictionary<object, TempField>();
    public List<FixedAndDynamicRowMixRange> MixRangeList { get; } = new List<FixedAndDynamicRowMixRange>();
    public FieldCellSetting[,] MixTicketCellSettingList { get; set; }

    public TicketDesignFailureReason FailureReason { get; set; }
}