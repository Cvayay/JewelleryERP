using System.Windows.Controls;
using JewelleryERP.ViewModels;

namespace JewelleryERP.Views;

public partial class CustomerView : UserControl
{
    public CustomerView(CustomerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
