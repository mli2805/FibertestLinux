using System.Windows;

namespace Demo.WindowsPresentation
{
   /// <summary>
   /// Interaction logic for Message.xaml
   /// </summary>
   public partial class Message
   {
      public Message()
      {
         InitializeComponent();
      }

      private void button1_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;   
         Close();
      }
   }
}
