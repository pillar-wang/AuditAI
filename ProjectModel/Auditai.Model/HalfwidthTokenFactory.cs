using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Auditai.Model;

public class HalfwidthTokenFactory : CommonTokenFactory
{
	public static HalfwidthTokenFactory Instance { get; } = new HalfwidthTokenFactory();


	private HalfwidthTokenFactory()
	{
	}

	public override CommonToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
	{
		CommonToken commonToken = base.Create(source, type, text, channel, start, stop, line, charPositionInLine);
		if (commonToken.Type == 18)
		{
			commonToken.Text = "(";
		}
		else if (commonToken.Type == 8)
		{
			commonToken.Text = ",";
		}
		else if (commonToken.Type == 19)
		{
			commonToken.Text = ")";
		}
		else if (commonToken.Type == 17)
		{
			commonToken.Text = "=";
		}
		else if (commonToken.Type == 12)
		{
			commonToken.Text = "+";
		}
		else if (commonToken.Type == 9)
		{
			commonToken.Text = "-";
		}
		else if (commonToken.Type == 10)
		{
			commonToken.Text = "*";
		}
		else if (commonToken.Type == 11)
		{
			commonToken.Text = "/";
		}
		return commonToken;
	}

	public override CommonToken Create(int type, string text)
	{
		return base.Create(type, text);
	}
}
