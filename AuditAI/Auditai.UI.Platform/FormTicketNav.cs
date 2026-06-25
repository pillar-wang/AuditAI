using System.Windows.Forms;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class FormTicketNav : Form
{
    public string Title { get; set; }
    public object InputVM { get; set; }
    public object Ticket { get; set; }
    public dynamic Result { get; set; }
}