using System.Windows;

namespace MapLoaderCore
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
