using System;

namespace DbAccess;

public class ConvertConfig
{
	public bool CreatePrimarykey { get; set; } = false;


	public bool CreateForignkey { get; set; } = false;


	public bool CreateCollate { get; set; } = false;


	public bool CreateIndex { get; set; } = false;


	public bool CreateTriggers { get; set; } = false;


	public bool CreateViews { get; set; } = false;


	public Predicate<string> tableSelectionHandler { get; set; }

	public SqlConversionHandler progressHandler { get; set; }

	public FailedViewDefinitionHandler viewFailedHandler { get; set; }

	public string UsePassword { get; set; }
}
