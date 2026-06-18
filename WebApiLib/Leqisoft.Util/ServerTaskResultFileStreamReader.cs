using Google.Protobuf;

namespace Leqisoft.Util;

internal class ServerTaskResultFileStreamReader
{
	protected CodedInputStream _codeInputStream;

	public ServerTaskResultFileStreamReader(CodedInputStream codedInputStream)
	{
		_codeInputStream = codedInputStream;
	}

	public int ReadCount()
	{
		return _codeInputStream.ReadInt32();
	}

	public void ReadDataBlockWithActionType(out int actionType, IMessage dataBlock)
	{
		actionType = _codeInputStream.ReadEnum();
		_codeInputStream.ReadMessage(dataBlock);
	}

	public void ReadDataBlock(IMessage dataBlock)
	{
		_codeInputStream.ReadMessage(dataBlock);
	}

	public string ReadString()
	{
		return _codeInputStream.ReadString();
	}
}
