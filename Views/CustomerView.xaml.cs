using JewelleryERP.ViewModels;
using System.Windows.Controls;

namespace JewelleryERP.Views;


public partial class CustomerView : UserControl
{
    public CustomerView()
    {
        InitializeComponent();
    }
    public CustomerView(CustomerViewModel viewModel)
{
    InitializeComponent();
    DataContext = viewModel;
}
}
