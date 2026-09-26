using System;
using System.Collections.Generic;
using Auditai.DTO;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class TicketDesignValidation
{
    public class TempField
    {
        public Id64 Field { get; set; }
        public string Text { get; set; }
        public string InputValue { get; set; }

        // 原反编译实现用反射写 TicketCell 上并不存在的 "Value" 属性，
        // 静默失效导致字段单元格的静态文本/录入值（前缀与 ="..." 值）丢失，
        // 单据中残留原始标记如 [科目]="123"。这里直接写入 Text / InputValue。
        public void WriteTo(object ticketCell)
        {
            if (ticketCell is TicketCell tc)
            {
                tc.Text = Text ?? string.Empty;
                tc.InputValue = InputValue ?? string.Empty;
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