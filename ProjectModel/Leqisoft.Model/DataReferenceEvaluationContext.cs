﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;

namespace Leqisoft.Model;

public class DataReferenceEvaluationContext
{
	public Project Project { get; set; }

	public TreeNodeBase CurrentTreeNode { get; set; }
	
	// 新增：跨项目解析器，根据 ProjectId 返回外部 Project 实例
	public Func<Guid, Project> ProjectResolver { get; set; }
	
	// 新增：外部项目缓存
	public Dictionary<Guid, Project> ExternalProjectCache { get; set; } = new Dictionary<Guid, Project>();
}
