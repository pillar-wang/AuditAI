using Google.Protobuf;

namespace Leqisoft.Util;

internal class ServerTaskInputFileStreamWriter
{
	protected CodedOutputStream _outputStream;

	public ServerTaskInputFileStreamWriter(CodedOutputStream codedOutputStream)
	{
		_outputStream = codedOutputStream;
	}

	public void WriteCount(int value)
	{
		_outputStream.WriteInt32(value);
	}

	public void WriteData(IMessage message)
	{
		_outputStream.WriteMessage(message);
	}

	public void WriteData(int actionType, IMessage message)
	{
		_outputStream.WriteEnum(actionType);
		_outputStream.WriteMessage(message);
	}

	public void WriteString(string value)
	{
		_outputStream.WriteString(value);
	}
}
