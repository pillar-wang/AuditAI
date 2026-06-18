using System.Windows.Forms;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class FormTicketNav : Form
{
    public string Title { get; set; }
    public object InputVM { get; set; }
    public object Ticket { get; set; }
    public dynamic Result { get; set; }
}